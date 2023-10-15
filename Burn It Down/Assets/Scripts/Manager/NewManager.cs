using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyBox;

public class AStarNode
{
    public TileData ATileData;
    public AStarNode Parent;
    //travel distance from node to start node
    public int GCost;
    //travel distance from node to target node
    public int HCost;
    //Astar value of this tile, the lower it is, the better for the pathfinder.
    public int FCost => GCost + HCost;
}

public class NewManager : MonoBehaviour
{

#region Variables

    public static NewManager instance;

    [Foldout("Storing Things", true)]
        [Tooltip("Reference to players")][ReadOnly] public List<PlayerEntity> listOfPlayers = new List<PlayerEntity>();
        [Tooltip("Reference to walls")][ReadOnly] public List<WallEntity> listOfWalls = new List<WallEntity>();
        [Tooltip("Reference to guards")][ReadOnly] public List<GuardEntity> listOfGuards = new List<GuardEntity>();
        [Tooltip("Reference to active environmental objects")][ReadOnly] public List<EnvironmentalEntity> listOfEnvironmentals = new List<EnvironmentalEntity>();
        [Tooltip("Reference to objectives")][ReadOnly] public List<ObjectiveEntity> listOfObjectives = new List<ObjectiveEntity>();

    [Foldout("Movement", true)]
        [Tooltip("Current Selected Tile")][ReadOnly] public TileData selectedTile;
        [Tooltip("Quick reference to current movable tile")][ReadOnly] public TileData CurrentAvailableMoveTarget;

    [Foldout("Card Zones", true)]
        [Tooltip("Your hand in the canvas")] RectTransform handTransform;
        [Tooltip("Reference to card scripts")] [ReadOnly] public List<Card> listOfHand = new List<Card>();
        [Tooltip("Your deck in the canvas")] Transform deck;
        [Tooltip("Your discard pile in the canvas")] Transform discardPile;
        [Tooltip("Your exhausted cards in the canvas")] Transform exhausted;
        
    [Foldout("UI Elements", true)]
        [Tooltip("Your health")] int currentHealth;
        [Tooltip("Health bar")] Slider healthBar;
        [Tooltip("Health bar's textbox")] TMP_Text healthText;
        [Tooltip("Your energy")] int currentEnergy;
        [Tooltip("Energy bar")] Slider energyBar;
        [Tooltip("Energy bar's textbox")] TMP_Text energyText;
        [Tooltip("Movement bar")] Slider movementBar;
        [Tooltip("Movement bar's textbox")] TMP_Text movementText;
        [Tooltip("End the turn")] Button endTurnButton;
        [Tooltip("Use objective")] [ReadOnly] public Button objectiveButton;
        [Tooltip("info on entities")] [ReadOnly] public EntityToolTip toolTip;
        
    [Foldout("GameOver", true)]
        [SerializeField] TMP_Text gameOverText;
        [SerializeField] GameObject gameOverButton;
        [SerializeField] GameObject gameOverScreen;

    [Foldout("Grid", true)]
        [Tooltip("Tiles in the inspector")] Transform gridContainer;
        [Tooltip("Storage of tiles")][ReadOnly] public TileData[,] listOfTiles;
        [Tooltip("Spacing between tiles")][SerializeField] float tileSpacing;

    [Foldout("Prefabs", true)]
        [Tooltip("Floor tile prefab")] [SerializeField] TileData floorTilePrefab;
        [Tooltip("Player prefab")][SerializeField] PlayerEntity playerPrefab;
        [Tooltip("Wall prefab")][SerializeField] WallEntity wallPrefab;
        [Tooltip("Guard prefab")][SerializeField] GuardEntity guardPrefab;
        [Tooltip("Objective prefab")][SerializeField] ObjectiveEntity objectivePrefab;
        [Tooltip("Guard prefab")][SerializeField] ExitEntity exitPrefab;

    [Foldout("Setup", true)]
        [Tooltip("Name of the level tsv")] [SerializeField] string levelToLoad;
        [Tooltip("Starting hand")] Transform startingHand;

    public enum TurnSystem { You, Resolving, Environmentals, Enemy };
        [Foldout("Turn System", true)]
        [Tooltip("What's happening in the game")][ReadOnly] public TurnSystem currentTurn;

#endregion

#region Setup

    void Awake()
    {
        instance = this;

        deck = GameObject.Find("Deck").transform;
        discardPile = GameObject.Find("Discard Pile").transform;
        exhausted = GameObject.Find("Exhausted").transform;

        healthBar = GameObject.Find("Health Slider").GetComponent<Slider>();
        healthText = healthBar.transform.GetChild(2).GetComponent<TMP_Text>();
        energyBar = GameObject.Find("Energy Slider").GetComponent<Slider>();
        energyText = energyBar.transform.GetChild(2).GetComponent<TMP_Text>();
        movementBar = GameObject.Find("Movement Slider").GetComponent<Slider>();
        movementText = movementBar.transform.GetChild(2).GetComponent<TMP_Text>();

        handTransform = GameObject.Find("Player Hand").transform.GetChild(0).transform.GetChild(0).GetComponent<RectTransform>();
        gridContainer = GameObject.Find("Grid Container").transform;
        startingHand = GameObject.Find("Starting Hand").transform;

        endTurnButton = GameObject.Find("End Turn Button").GetComponent<Button>();
        endTurnButton.onClick.AddListener(Regain);
        objectiveButton = GameObject.Find("Objective Button").GetComponent<Button>();
        objectiveButton.onClick.AddListener(ResolveObjective);
    }

    void Start()
    {
        gameOverText.transform.parent.gameObject.SetActive(false);
        gridContainer.transform.localPosition = new Vector3(18, -1, 0);

        //store all the cards here
        Transform emptyObject = new GameObject("EmptyObject").transform;
        for (int i = 0; i < SaveManager.instance.allCards.Count; i++)
            SaveManager.instance.allCards[i].transform.SetParent(emptyObject);

        //get all the cards in the deck
        for (int i = 0; i < SaveManager.instance.currentSaveData.chosenDeck.Count; i++)
        {
            Card nextCard = emptyObject.transform.Find(SaveManager.instance.currentSaveData.chosenDeck[i]).GetComponent<Card>();
            nextCard.transform.SetParent(deck);
            nextCard.transform.localPosition = new Vector3(10000, 10000, 0); //send the card far away where you can't see it anymore
            nextCard.choiceScript.DisableButton();
        }

        /*
        //get the cards in your starting hand
        for (int i = 0; i < startingHand.childCount; i++)
        {
            Card nextCard = startingHand.GetChild(i).GetComponent<Card>();
            nextCard.choiceScript.DisableButton();
            AddCardToHand(nextCard);
        }
        */

        deck.Shuffle(); //shuffle your deck
        DrawCards(5);

        //generate grids and entities from csv
        string[,] newGrid = LevelLoader.LoadLevelGrid(levelToLoad);
        listOfTiles = new TileData[newGrid.GetLength(0), newGrid.GetLength(1)];

        for (int i = 0; i < listOfTiles.GetLength(0); i++)
        {
            for (int j = 0; j < listOfTiles.GetLength(1); j++)
            {
                TileData nextTile = Instantiate(floorTilePrefab);
                nextTile.transform.SetParent(gridContainer);
                nextTile.transform.position = new Vector3(i * -tileSpacing, 0, j * -tileSpacing);
                nextTile.name = $"Tile {i},{j}";
                listOfTiles[i, j] = nextTile;
                nextTile.gridPosition = new Vector2Int(i, j);

                string[] numberPlusAddition = newGrid[i, j].Split("/");

                Entity thisTileEntity = null;
                switch (numberPlusAddition[0])
                {
                    case "1": //create player
                        thisTileEntity = Instantiate(playerPrefab, nextTile.transform);
                        thisTileEntity.name = "Player";
                        PlayerEntity player = thisTileEntity.GetComponent<PlayerEntity>();
                        player.movementLeft = player.movesPerTurn;
                        listOfPlayers.Add(player);
                        break;
                    case "2": //create exit
                        thisTileEntity = Instantiate(exitPrefab, nextTile.transform);
                        thisTileEntity.name = "Exit";
                        ObjectiveEntity exitObjective = thisTileEntity.GetComponent<ExitEntity>();
                        listOfObjectives.Add(exitObjective);
                        break;
                    case "3": //create objective
                        thisTileEntity = Instantiate(objectivePrefab, nextTile.transform);
                        thisTileEntity.name = numberPlusAddition[1];
                        ObjectiveEntity defaultObjective = thisTileEntity.GetComponent<ObjectiveEntity>();
                        listOfObjectives.Add(defaultObjective);
                        break;
                    case "10": //create weak wall
                        thisTileEntity = Instantiate(wallPrefab, nextTile.transform);
                        thisTileEntity.name = "Wall";
                        WallEntity weakWall = thisTileEntity.GetComponent<WallEntity>();
                        listOfWalls.Add(weakWall);
                        weakWall.direction = StringToDirection(numberPlusAddition[1]);
                        weakWall.health = 2;
                        break;
                    case "11": //create med wall
                        thisTileEntity = Instantiate(wallPrefab, nextTile.transform);
                        thisTileEntity.name = "Wall";
                        WallEntity medWall = thisTileEntity.GetComponent<WallEntity>();
                        listOfWalls.Add(medWall);
                        medWall.direction = StringToDirection(numberPlusAddition[1]);
                        medWall.health = 4;
                        break;
                    case "12": //create strong wall
                        thisTileEntity = Instantiate(wallPrefab, nextTile.transform);
                        thisTileEntity.name = "Wall";
                        WallEntity strongWall = thisTileEntity.GetComponent<WallEntity>();
                        listOfWalls.Add(strongWall);
                        strongWall.direction = StringToDirection(numberPlusAddition[1]);
                        strongWall.health = 6;
                        break;
                    case "20": //create guard
                        thisTileEntity = Instantiate(guardPrefab, nextTile.transform);
                        thisTileEntity.name = "Guard";
                        GuardEntity theGuard = thisTileEntity.GetComponent<GuardEntity>();
                        theGuard.movementLeft = theGuard.movesPerTurn;
                        theGuard.direction = StringToDirection(numberPlusAddition[1]);
                        listOfGuards.Add(theGuard);
                        break;
                }
                try
                {
                    thisTileEntity.MoveTile(nextTile);
                }
                catch (NullReferenceException)
                {
                    continue;
                }
            }
        }

        for (int i = 0; i < listOfTiles.GetLength(0); i++) //then find adjacent tiles
        {
            for (int j = 0; j < listOfTiles.GetLength(1); j++)
            {
                FindAdjacent(listOfTiles[i, j]);
            }
        }
        gridContainer.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);

        SetEnergy(3);
        SetHealth(3);
        SetMovement(3);

        StartCoroutine(StartPlayerTurn());
    }

    Vector2Int StringToDirection(string direction)
    {
        return direction[..1] switch
        {
            "u" => new Vector2Int(0, 1),
            "d" => new Vector2Int(0, -1),
            "l" => new Vector2Int(-1, 0),
            "r" => new Vector2Int(1, 0),
            _ => new Vector2Int(0, 0),
        };
    }

    void FindAdjacent(TileData tile) //check each adjacent tile; if it's not null, add it to the list
    {
        tile.adjacentTiles.Add(FindTile(new Vector2(tile.gridPosition.x + 1, tile.gridPosition.y)));
        tile.adjacentTiles.Add(FindTile(new Vector2(tile.gridPosition.x - 1, tile.gridPosition.y)));
        tile.adjacentTiles.Add(FindTile(new Vector2(tile.gridPosition.x, tile.gridPosition.y + 1)));
        tile.adjacentTiles.Add(FindTile(new Vector2(tile.gridPosition.x, tile.gridPosition.y - 1)));

        tile.adjacentTiles.RemoveAll(item => item == null); //delete all tiles that are null
    }

    public TileData FindTile(Vector2 vector) //find a tile based off Vector2
    {
        try
        {
            return listOfTiles[(int)vector.x, (int)vector.y];
        }
        catch (System.IndexOutOfRangeException)
        {
            return null;
        }
    }

    public TileData FindTile(Vector2Int vector) //find a tile based off Vector2Int
    {
        return FindTile(new Vector2(vector.x, vector.y));
    }

    public static IEnumerator Wait(float timer)
    {
        float wait = timer;
        while (wait > 0)
        {
            wait -= Time.deltaTime;
            yield return null;
        }
    }

    private void Update()
    {
        endTurnButton.gameObject.SetActive(currentTurn == TurnSystem.You);
    }

#endregion

#region Changing Stats

    public bool EnoughEnergy(int n)//check if n is larger than current energy
    {
        return (energyBar.value >= n);
    }
    public void SetEnergy(int n) //if you want to set energy to 2, type SetEnergy(2);
    {
        ChangeEnergy(n - (int)currentEnergy);
    }
    public void ChangeEnergy(int n) //if you want to subtract 3 energy, type ChangeEnergy(-3);
    {
        currentEnergy += n;
        energyText.text = $"Energy: {currentEnergy}";
        energyBar.value = currentEnergy;
    }
    public void SetHealth(int n) //if you want to set health to 2, type SetHealth(2);
    {
        ChangeHealth(n - (int)currentHealth);
    }
    public void ChangeHealth(int n) //if you want to subtract 3 health, type ChangeHealth(-3);
    {
        currentHealth += n;
        healthText.text = $"Health: {currentHealth}";
        healthBar.value = currentHealth;
    }
    public void SetMovement(int n) //if you want to set movement to 2, type SetMovement(2);
    {
        ChangeMovement(n - (int)listOfPlayers[0].movementLeft);
    }
    public void ChangeMovement(int n) //if you want to subtract 3 movement, type ChangeMovement(-3);
    {
        listOfPlayers[0].movementLeft += n;
        movementText.text = $"Movement: {listOfPlayers[0].movementLeft}";
        movementBar.value = listOfPlayers[0].movementLeft;
    }
    #endregion

#region Cards
    public void DrawCards(int num)
    {
        for (int i = 0; i < num; i++)
        {
            try
            {
                AddCardToHand(GetTopCard());
            }
            catch (System.NullReferenceException)
            {
                break;
            }
        }
    }
    public Card GetTopCard()
    {
        if (deck.childCount > 0)
        {
            discardPile.Shuffle();
            while (discardPile.childCount > 0)
                discardPile.GetChild(0).SetParent(deck);
        }

        if (deck.childCount > 0) //get the top card of the deck if there is one
            return deck.GetChild(0).GetComponent<Card>();
        else
            return null;
    }
    public void AddCardToHand(Card newCard)
    {
        //add the new card to your hand
        if (newCard != null)
        {
            listOfHand.Add(newCard);
            newCard.transform.SetParent(handTransform);
            newCard.transform.localScale = new Vector3(1, 1, 1);
        }
    }
    public void DiscardCard(Card discardMe)
    {
        discardMe.transform.SetParent(discardPile);
        listOfHand.Remove(discardMe);
        discardMe.transform.localPosition = new Vector3(1000, 1000, 0); //send the card far away where you can't see it anymore
    }
    public void ExhaustCard(Card exhaustMe)
    {
        exhaustMe.transform.SetParent(exhausted);
        listOfHand.Remove(exhaustMe);
        exhaustMe.transform.localPosition = new Vector3(10000, 10000, 0); //send the card far away where you can't see it anymore
    }
#endregion

#region Turn System
    
    public void ResolveObjective()
    {
        StartCoroutine(listOfPlayers[0].adjacentObjective.ObjectiveComplete());
    }

    public void GameOver(string cause, string buttonTxt)
    {
        gameOverText.gameObject.SetActive(true);
        gameOverScreen.SetActive(true);
        gameOverButton.SetActive(true);
        gameOverText.text = cause;
        gameOverButton.GetComponentInChildren<TMP_Text>().text = buttonTxt;
    }

    IEnumerator StartPlayerTurn()
    {
        for (int i = 0; i < listOfPlayers.Count; i++)
            listOfPlayers[i].currentTile.moveable = true;

        currentTurn = TurnSystem.You;
        StartCoroutine(CanPlayCard());
        yield return null;
    }

    public IEnumerator MovePlayer(TileData currentTile)
    {
        PlayerEntity currentPlayer = currentTile.myEntity.GetComponent<PlayerEntity>();
        List<TileData> possibleTiles = CalculateReachableGrids(currentTile, currentPlayer.movementLeft);       
        ChoiceManager.instance.ChooseTile(possibleTiles);

        while (ChoiceManager.instance.chosenTile == null)
        {
            if (selectedTile != currentTile)
            {
                ChoiceManager.instance.ToggleMovement(false);
                yield break;
            }
            else
            {
                yield return null;
            }
        }

        int distanceTraveled = GetDistance(currentTile, ChoiceManager.instance.chosenTile);
        currentPlayer.movementLeft -= distanceTraveled;
        SetMovement(currentPlayer.movementLeft);
        currentPlayer.MoveTile(ChoiceManager.instance.chosenTile);
        ChoiceManager.instance.ToggleMovement(false);
    }

    IEnumerator CanPlayCard() //choose a card to play
    {
        ChoiceManager.instance.DisableCards();
        List<Card> canBePlayed = new List<Card>();
        for (int i = 0; i < listOfHand.Count; i++)
        {
            if (listOfHand[i].CanPlay(listOfPlayers[0]))
                canBePlayed.Add(listOfHand[i]);
        }
        ChoiceManager.instance.ChooseCard(canBePlayed);

        while (ChoiceManager.instance.chosenCard == null)
        {
            if (currentTurn != TurnSystem.You)
                yield break;
            else
                yield return null;
        }
        yield return PlayCard(ChoiceManager.instance.chosenCard);
    }

    IEnumerator PlayCard(Card playMe) //resolve that card
    {
        if (playMe != null)
        {
            DiscardCard(playMe);
            ChangeEnergy(-playMe.energyCost);
            yield return playMe.OnPlayEffect();
        }

        StartCoroutine(CanPlayCard());
    }

    public void Regain()
    {
        SetEnergy(3);
        DrawCards(5 - listOfHand.Count);
        for (int i = 0; i < listOfPlayers.Count; i++)
        {
            listOfPlayers[i].movementLeft = listOfPlayers[i].movesPerTurn;
            SetMovement(3);
        }
        StartCoroutine(EnvironmentalPhase());
    }

    IEnumerator EnvironmentalPhase()
    {
        currentTurn = TurnSystem.Environmentals;
        StopCoroutine(CanPlayCard());
        ChoiceManager.instance.DisableCards();

        for (int i = 0; i < listOfPlayers.Count; i++)
            yield return listOfPlayers[i].EndOfTurn();

        currentTurn = TurnSystem.Environmentals;
        for (int i = 0; i < listOfEnvironmentals.Count; i++)
        {
            yield return null;
            yield return listOfEnvironmentals[i].EndOfTurn();
        }
        StartCoroutine(EndTurn());
        yield return null;
    }

    IEnumerator EndTurn() //Starts Guard Phase
    {
        selectedTile = null;
        ChoiceManager.instance.DisableCards();
        ChoiceManager.instance.ToggleMovement(false);

        for (int i = 0; i < listOfPlayers.Count; i++)
            yield return listOfPlayers[i].EndOfTurn();

        //sets turn to the enemies, and counts through the grid activating all enemies simultaniously
        currentTurn = TurnSystem.Enemy;

        var group = new CoroutineGroup(this);
        for (int i = 0; i < listOfGuards.Count; i++)
            group.StartCoroutine(listOfGuards[i].EndOfTurn());
        while (group.AnyProcessing)
            yield return null;

        StartCoroutine(StartPlayerTurn());
    }

#endregion

#region Pathfinding

    //gets the distance (in gridspaces) between two gridspaces
    public int GetDistance(TileData a, TileData b)
    {
        int distX = Mathf.Abs(a.gridPosition.x - b.gridPosition.x);
        int distY = Mathf.Abs(a.gridPosition.y - b.gridPosition.y);
        //print("x distance = " + distX);
        //print("y distance = " + distY);

        return distY + distX;
    }

    public List<TileData> CalculateReachableGrids(TileData startLocation, int movementSpeed)
    {
        List<TileData> reachableGrids = new List<TileData>();
        //First in first out
        Queue<(TileData, int)> queue = new Queue<(TileData, int)>();
        //HashSet is a simple collection without orders
        HashSet<TileData> visited = new HashSet<TileData>();

        queue.Enqueue((startLocation, 0));
        visited.Add(startLocation);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            TileData SelectTile = current.Item1;
            int cost = current.Item2;

            if (cost <= movementSpeed)
            {
                reachableGrids.Add(SelectTile);;
                FindAdjacent(SelectTile);
                foreach (TileData neighbor in SelectTile.adjacentTiles)
                {
                    int newCost;
                    if (neighbor.myEntity != null)
                    {
                        newCost = cost + neighbor.myEntity.MoveCost;
                    }
                    else
                    {
                        newCost = cost + 1;
                    }

                    if (!visited.Contains(neighbor) && newCost <= movementSpeed )
                    {
                        if (neighbor.myEntity == null)
                        {
                            queue.Enqueue((neighbor, newCost));
                            visited.Add(neighbor);
                        }
                        else if (neighbor.myEntity.MoveCost! >= 999)
                        {
                            queue.Enqueue((neighbor, newCost));
                            visited.Add(neighbor);
                        }

                    }
                }
            }
        }
        return reachableGrids;
    }

    public void CalculatePathfinding(TileData startLocation, TileData targetLocation, int actionPoint, bool singleMovement)
    {
        print("Starting from" + startLocation.gridPosition);
        // Create the open list containing the start node and closed set
        List<AStarNode> openList = new List<AStarNode>();
        HashSet<TileData> closedSet = new HashSet<TileData>();
        Dictionary<TileData, AStarNode> nodeLookup = new Dictionary<TileData, AStarNode>();

        AStarNode startNode = new AStarNode { ATileData = startLocation };
        openList.Add(startNode);
        nodeLookup[startLocation] = startNode;

        int iteration = 0;
        // Main loop, continues until there are no more nodes to explore
        while (openList.Count > 0 && iteration < 100)
        {
            iteration++;
            //print(iteration);
            // Find the node with the lowest F cost
            AStarNode currentNode = openList[0];

            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].FCost < currentNode.FCost || openList[i].FCost == currentNode.FCost && openList[i].HCost < currentNode.HCost)
                {
                    currentNode = openList[i];
                }
            }
            // Remov e the current node from the open list and add it to the closed set
            openList.Remove(currentNode);
            //print("before Count " + closedSet.Count);
            closedSet.Add(currentNode.ATileData);
            //print("after Count " + closedSet.Count);
            // Check if the current node is the target location, if so, retrace the path
            //print("Target: " + targetLocation.gridPosition);
            //print("Target: " + currentNode.ATileData.gridPosition);
            //print("");
            if (currentNode.ATileData.gridPosition == targetLocation.gridPosition)
            {
                RetracePath(startNode, currentNode, actionPoint, singleMovement);
                return;
            }
            // Explore neighbors of the current node
            foreach (TileData neighbor in currentNode.ATileData.adjacentTiles)
            {
                print("for neightbor: " + neighbor.gridPosition);
                // Ignore blocked nodes or nodes already in the closed set
                if (neighbor.myEntity != null)
                {
                    if (neighbor.gridPosition == targetLocation.gridPosition)
                    {

                    }
                    else if (neighbor.myEntity.MoveCost >= 999)
                    {
                        print("can't pass");
                        continue;
                    }
                    else if (closedSet.Contains(neighbor))
                    {
                        print("Tile already viewed");
                        continue;
                    }
                }

                // Calculate the new G cost
                int newGCost = 0;
                if (neighbor.myEntity != null)
                {
                    if (neighbor.gridPosition == targetLocation.gridPosition || neighbor.gridPosition == startLocation.gridPosition)
                    {
                        newGCost = currentNode.GCost + GetDistance(currentNode.ATileData, neighbor) * 1;
                    } else
                    {
                        newGCost = currentNode.GCost + GetDistance(currentNode.ATileData, neighbor) * neighbor.myEntity.MoveCost;
                    }

                }
                else
                {
                    newGCost = currentNode.GCost + GetDistance(currentNode.ATileData, neighbor) * 1;
                }
                AStarNode neighborNode;
                if (!nodeLookup.ContainsKey(neighbor))
                {
                    neighborNode = new AStarNode { ATileData = neighbor };
                    nodeLookup[neighbor] = neighborNode;
                }
                else
                {
                    neighborNode = nodeLookup[neighbor];
                }
                // Update neighbor's G, H, and Parent if the new G cost is lower, or if the neighbor is not in the open list
                if (newGCost < neighborNode.GCost || !openList.Contains(neighborNode))
                {
                    neighborNode.GCost = newGCost;
                    neighborNode.HCost = GetDistance(neighbor, targetLocation);
                    neighborNode.Parent = currentNode;
                    // Add neighbor to the open list if it's not already there
                    if (!openList.Contains(neighborNode))
                    {
                        openList.Add(neighborNode);
                    }
                }

            }
            /*
            print("");
            print("");
            print("openList length: " + openList.Count);
            for (int i = 0; i < openList.Count; i++)
            {
                print("");
                print("Open list contains: " + openList[i].ATileData.gridPosition);
                print("Distance to start, Gcost: " + openList[i].GCost);
                print("Distance to end, Hcost: " + openList[i].HCost);
                print("Node value: " + openList[i].FCost);
            }
            print("closedList Length: " + closedSet.Count);
            foreach(TileData Hash in closedSet)
            {
                print("ClosedSet Contains: " + Hash.gridPosition);
            }
            */
        }
    }

    private void RetracePath(AStarNode startNode, AStarNode endNode, int actionPoint, bool singleMovement)
    {

        // Create an empty list to store the path
        List<TileData> path = new List<TileData>();
        AStarNode currentNode = endNode;
        // Follow parent pointers from the end node to the start node, adding each node to the path
        int iteration = 0;
        while (currentNode != startNode && iteration < 3)
        {
            iteration++;
            print("Retrace iteration: " + iteration);
            path.Add(currentNode.ATileData);
            currentNode = currentNode.Parent;
            foreach (TileData data in path)
            {
                print("path contains " + data.gridPosition);
            }
            print("");
        }
        if (iteration == 5)
        {
            return;
        }
        // Reverse the path so it starts from the starting location
        path.Reverse();

        // Display the path and calculate its cost
        int pathCost = 0;
        if (!singleMovement)
        {
            foreach (TileData CurrentTile in path)
            {
                if (CurrentTile.myEntity != null)
                {
                    pathCost += CurrentTile.myEntity.MoveCost;
                }
                else
                {
                    pathCost++;
                }

                // If the path cost exceeds the action points available, stop displaying the path
                if (pathCost > actionPoint)
                {
                    break;
                }
                // Update the current available move target and display the pathfinding visualization with the path cost
                CurrentAvailableMoveTarget = CurrentTile;

                //Displays the pathfinding value to get to this point (but we don't have this yet)
                //CurrentTile.DisplayPathfinding(pathCost.ToString());
            }
        }
        else
        {
            CurrentAvailableMoveTarget = path[0];
        }
    }
#endregion
}