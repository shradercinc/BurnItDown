using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyBox;
using UnityEngine.TextCore.Text;
using static MyBox.EditorTools.MyGUI;
using JetBrains.Annotations;
using Unity.VisualScripting;

public class AStarNode
{
    public TileData ATileData;
    public AStarNode Parent;
    public int GCost;
    public int HCost;
    public int FCost => GCost + HCost;
}

public class NewManager : MonoBehaviour
{
    public static NewManager instance;

    [Foldout("Storing Things", true)]
    [Tooltip("Reference to players")][ReadOnly] public List<PlayerEntity> listOfPlayers = new List<PlayerEntity>();
    [Tooltip("Reference to walls")][ReadOnly] public List<WallEntity> listOfWalls = new List<WallEntity>();
    [Tooltip("Reference to guards")][ReadOnly] public List<GuardEntity> listOfGuards = new List<GuardEntity>();
    [Tooltip("Reference to active environmental objects")][ReadOnly] public List<EnvironmentalEntity> listOfEnvironmentals = new List<EnvironmentalEntity>();
    [Tooltip("Current Selected Tile")]public TileData selectedTile;
    [Tooltip("Quick reference to current movable tile")] TileData CurrentAvalibleMoveTarget;

    [Foldout("Card Zones", true)]
    [Tooltip("Your hand in the canvas")] RectTransform handTransform;
    [Tooltip("Reference to card scripts")] List<Card> listOfHand = new List<Card>();
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
    [Tooltip("Regain energy/health/cards")] Button regainResources;
    [Tooltip("Button to stop moving")] Button stopMovingButton;
    [Tooltip("info on entities")] public EntityToolTip toolTip;

    [Foldout("GameOver", true)]
    [SerializeField] TMP_Text gameOverText;
    [SerializeField] GameObject gameOverButton;
    [SerializeField] GameObject gameOverScreen;

    [Foldout("Grid", true)]
    [Tooltip("Tiles in the inspector")] Transform gridContainer;
    [Tooltip("Storage of tiles")][ReadOnly] public TileData[,] listOfTiles;
    [Tooltip("Spacing between tiles")][SerializeField] float tileSpacing = 2;
    [Tooltip("Tile height")][SerializeField] float tileHeight = 2;

    [Foldout("Prefabs", true)]
    [Tooltip("Floor tile prefab")] public TileData floorTile;
    [Tooltip("Player prefab")] public PlayerEntity player;
    [Tooltip("Wall prefab")] public WallEntity wall;
    [Tooltip("Guard prefab")] public GuardEntity guard;

    [Foldout("Setup", true)]
    [Tooltip("Size of the level")] public Vector2Int gridSize;
    [Tooltip("Starting hand")] Transform startingHand;
    [Tooltip("Entities and their starting positions")][SerializeField] List<EntityAndPosition> entityStarts = new List<EntityAndPosition>();

    public enum TurnSystem { You, Resolving, Environmentals, Enemy };
    [Foldout("Turn System", true)]
    [Tooltip("What's happening in the game")][ReadOnly] public TurnSystem currentTurn;
    [Tooltip("Number of actions you can do before enemy's turn")][ReadOnly] public int numActions;

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

        regainResources = GameObject.Find("Regain Resources Button").GetComponent<Button>();
        regainResources.onClick.AddListener(Regain);
        stopMovingButton = GameObject.Find("Stop Moving Button").GetComponent<Button>();
        stopMovingButton.onClick.AddListener(StopMoving);
    }
    void Start()
    {
        gameOverText.transform.parent.gameObject.SetActive(false);
        gridContainer.transform.localPosition = new Vector3(18, -1, 0);

        //since the right click script is under dontdestroyonload, we have to bring it back to the canvas
        RightClick.instance.transform.SetParent(this.transform.parent);

        //get all the cards in the game
        for (int i = 0; i < SaveManager.instance.allCards.Count; i++)
        {
            Card nextCard = SaveManager.instance.allCards[i];
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

        listOfTiles = new TileData[gridSize.x, gridSize.y];
        for (int i = 0; i < gridSize.x; i++) //create the tiles
        {
            for (int j = 0; j < gridSize.y; j++)
            {
                TileData nextTile = Instantiate(floorTile, gridContainer);
                nextTile.transform.localPosition = new Vector3(i * -tileSpacing, tileHeight, j * -tileSpacing);
                nextTile.name = $"Tile {i},{j}";
                listOfTiles[i, j] = nextTile;
                nextTile.gridPosition = new Vector2Int(i, j);
            }
        }
        for (int i = 0; i < gridSize.x; i++) //then find adjacent tiles
        {
            for (int j = 0; j < gridSize.y; j++)
            {
                FindAdjacent(listOfTiles[i, j]);
            }
        }

        for (int i = 0; i < entityStarts.Count; i++) //spawn all entities in their starting positions
        {
            TileData spawnHere = listOfTiles[(int)entityStarts[i].startingPosition.x, (int)entityStarts[i].startingPosition.y];
            Entity nextEntity = null;
            switch (entityStarts[i].entity)
            {
                case EntityAndPosition.EntityType.Player:
                    nextEntity = Instantiate(player, spawnHere.transform);
                    PlayerEntity thePlayer = nextEntity.GetComponent<PlayerEntity>();
                    thePlayer.movementLeft = thePlayer.movesPerTurn;
                    listOfPlayers.Add(thePlayer);
                    break;
                case EntityAndPosition.EntityType.Guard:
                    nextEntity = Instantiate(guard, spawnHere.transform);
                    GuardEntity theGuard = nextEntity.GetComponent<GuardEntity>();
                    theGuard.movementLeft = theGuard.movesPerTurn;
                    theGuard.direction = entityStarts[i].direction;
                    listOfGuards.Add(theGuard);
                    break;
                case EntityAndPosition.EntityType.Wall:
                    nextEntity = Instantiate(wall, spawnHere.transform);
                    listOfWalls.Add(nextEntity.GetComponent<WallEntity>());
                    break;
            }
            nextEntity.MoveTile(spawnHere);
        }

        SetEnergy(3);
        SetHealth(3);
        movementBar.value = 3;
        movementText.text = $"Movement: {3}";

        stopMovingButton.gameObject.SetActive(false);
        StartCoroutine(StartPlayerTurn());
    }
    void Update()
    {
        //regainResources.gameObject.SetActive(currentTurn == TurnSystem.You);
    }
    void FindAdjacent(TileData tile)
    {   //check each adjacent tile; if it's not null, add it to the list
        tile.adjacentTiles.Add(FindTile(new Vector2(tile.gridPosition.x + 1, tile.gridPosition.y)));
        tile.adjacentTiles.Add(FindTile(new Vector2(tile.gridPosition.x - 1, tile.gridPosition.y)));
        tile.adjacentTiles.Add(FindTile(new Vector2(tile.gridPosition.x, tile.gridPosition.y + 1)));
        tile.adjacentTiles.Add(FindTile(new Vector2(tile.gridPosition.x, tile.gridPosition.y - 1)));

        tile.adjacentTiles.RemoveAll(item => item == null); //delete all tiles that are null
    }
    public TileData FindTile(Vector2 vector) //find a tile based off Vector2
    {
        if (vector.x < 0 || vector.x >= gridSize.x || vector.y < 0 || vector.y >= gridSize.y)
            return null;
        else
            return listOfTiles[(int)vector.x, (int)vector.y];
    }
    public TileData FindTile(Vector2Int vector) //find a tile based off Vector2Int
    {
        if (vector.x < 0 || vector.x >= gridSize.x || vector.y < 0 || vector.y >= gridSize.y)
            return null;
        else
            return listOfTiles[(int)vector.x, (int)vector.y];
    }
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
    public void ChangeHealth(int n) //if you want to subtract 3 health, type SetHealth(-3);
    {
        currentHealth += n;
        healthText.text = $"Health: {currentHealth}";
        healthBar.value = currentHealth;
    }
    public void DrawCards(int num)
    {
        for (int i = 0; i < num; i++)
        {
            //if deck's empty, shuffle discard pile and add those cards to the deck
            //exhausted cards are not included in the shuffle

            if (deck.childCount > 0)
            {
                discardPile.Shuffle();
                while (discardPile.childCount > 0)
                    discardPile.GetChild(0).SetParent(deck);
            }

            if (deck.childCount > 0) //get the top card of the deck if there is one
                AddCardToHand(deck.GetChild(0).GetComponent<Card>());
        }
    }
    public void AddCardToHand(Card newCard)
    {
        //add the new card to your hand
        listOfHand.Add(newCard);
        newCard.transform.SetParent(handTransform);
        newCard.transform.localScale = new Vector3(1, 1, 1);
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
        currentTurn = TurnSystem.You;
        StartCoroutine(CanPlayCard());
        //StartCoroutine(ChoosePlayerToMove());
        yield return null;
    }
    void MadeDecision()
    {
        currentTurn = TurnSystem.Resolving;
        //ChoiceManager.instance.DisableCards();
        //ChoiceManager.instance.DisableTiles();
    }
    IEnumerator ChoosePlayerToMove()
    {
        if (listOfPlayers[0].movementLeft > 0)
        {
            List<TileData> playerTiles = new List<TileData>();

            for (int i = 0; i < listOfPlayers.Count; i++)
            {
                if (listOfPlayers[i].movementLeft > 0)
                    playerTiles.Add(listOfPlayers[i].currentTile);
            }

            ChoiceManager.instance.ChooseTile(playerTiles);

            while (ChoiceManager.instance.chosenTile == null)
            {
                if (currentTurn != TurnSystem.You)
                    yield break;
                else
                    yield return null;
            }
            yield return MovePlayer(listOfPlayers[0].currentTile);
        }
        
    }
    public IEnumerator MovePlayer(TileData currentTile)
    {
        //currentTurn = TurnSystem.Resolving;
        PlayerEntity currentPlayer = currentTile.myEntity.GetComponent<PlayerEntity>();
        List<TileData> possibleTiles = CalculateReachableGrids(currentTile, currentPlayer.movementLeft);

        
        ChoiceManager.instance.ChooseTile(possibleTiles);

        ChoiceManager.instance.chosenTile = null;
        while (ChoiceManager.instance.chosenTile == null)
        {
            if (selectedTile != currentTile)
            {
                ChoiceManager.instance.DisableMovement();
                yield break;
            }
            else
            {
                yield return null;
            }
            print("chosenTile " + ChoiceManager.instance.chosenTile);
        }
        print("Distance = " + GetDistance(currentTile, ChoiceManager.instance.chosenTile));
        currentPlayer.movementLeft -= GetDistance(currentTile, ChoiceManager.instance.chosenTile);
        currentPlayer.MoveTile(ChoiceManager.instance.chosenTile);
        ChoiceManager.instance.DisableMovement();
        movementBar.value = player.movementLeft;
        movementText.text = $"Movement: {player.movementLeft}";
        

        /*
        MadeDecision();
        //stopMovingButton.gameObject.SetActive(true);
        List<TileData> possibleTiles = CalculateReachableGrids(listOfPlayers[0].currentTile, listOfPlayers[0].movementLeft);
        ChoiceManager.instance.ChooseTile(possibleTiles);

        while (ChoiceManager.instance.chosenTile == null)
        {
            if (currentTurn != TurnSystem.Resolving)
                yield break;
            else
                yield return null;
        }

        PlayerEntity player = currentTile.myEntity.GetComponent<PlayerEntity>();
        player.movementLeft -= GetDistance(player.currentTile, ChoiceManager.instance.chosenTile);
        player.MoveTile(ChoiceManager.instance.chosenTile);
        ChoiceManager.instance.DisableTiles();

        movementBar.value = player.movementLeft;
        movementText.text = $"Movement: {player.movementLeft}";
        if (player.movementLeft <= 0)
            StopMoving();
        */
    }
    public void StopMoving()
    {
        //StartCoroutine(sta());
    }
    IEnumerator CanPlayCard() //choose a card to play
    {
        List<Card> canBePlayed = new List<Card>();
        for (int i = 0; i < listOfHand.Count; i++)
        {
            if (listOfHand[i].CanPlay())
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
            MadeDecision();
            DiscardCard(playMe);
            ChangeEnergy((int)energyBar.value - playMe.energyCost);
            yield return playMe.PlayEffect();
            //StartCoroutine(EndTurn());
        }
    }
    public void Regain()
    {
        MadeDecision();
        SetEnergy(3);
        DrawCards(5 - listOfHand.Count);
        for (int i = 0; i < listOfPlayers.Count; i++)
        {
            listOfPlayers[i].movementLeft = listOfPlayers[i].movesPerTurn;
            movementBar.value = player.movementLeft;
            movementText.text = $"Movement: {player.movementLeft}";
        }
        StartCoroutine(EnvironmentalPhase());
    }

    IEnumerator EnvironmentalPhase()
    {
        ChoiceManager.instance.DisableCards();
        ChoiceManager.instance.DisableTiles();
        stopMovingButton.gameObject.SetActive(false);

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
    IEnumerator EndTurn()       //Starts Guard Phase
    {
        ChoiceManager.instance.DisableCards();
        ChoiceManager.instance.DisableTiles();
        stopMovingButton.gameObject.SetActive(false);

        for (int i = 0; i < listOfPlayers.Count; i++)
            yield return listOfPlayers[i].EndOfTurn();

        //sets turn to the enemies, and counts through the grid activating all enemies simultaniously
        currentTurn = TurnSystem.Enemy;

        for (int i = 0; i < listOfGuards.Count; i++)
        {
            yield return null;
            yield return listOfGuards[i].EndOfTurn();
        }

        StartCoroutine(StartPlayerTurn());
    }

    public void SetTargetLocation(TileData targetLocation)
    {
        CalculatePathfinding(listOfPlayers[0].currentTile, targetLocation, listOfPlayers[0].movementLeft);
    }

    //gets the distance (in gridspaces) between two gridspaces
    private int GetDistance(TileData a, TileData b)
    {
        int distX = Mathf.Abs(a.gridPosition.x - b.gridPosition.x);
        int distY = Mathf.Abs(a.gridPosition.y - b.gridPosition.y);
        print("x distance = " + distX);
        print("y distance = " + distY);

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

    public void CalculatePathfinding(TileData startLocation, TileData targetLocation, int actionPoint)
    {
        // Create the open list containing the start node and closed set
        List<AStarNode> openList = new List<AStarNode>();
        HashSet<TileData> closedSet = new HashSet<TileData>();
        Dictionary<TileData, AStarNode> nodeLookup = new Dictionary<TileData, AStarNode>();

        AStarNode startNode = new AStarNode { ATileData = startLocation };
        openList.Add(startNode);
        nodeLookup[startLocation] = startNode;
        // Main loop, continues until there are no more nodes to explore
        while (openList.Count > 0)
        {
            // Find the node with the lowest F cost
            AStarNode currentNode = openList[0];

            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].FCost < currentNode.FCost || openList[i].FCost == currentNode.FCost && openList[i].HCost < currentNode.HCost)
                {
                    currentNode = openList[i];
                }
            }
            // Remove the current node from the open list and add it to the closed set
            openList.Remove(currentNode);
            closedSet.Add(currentNode.ATileData);
            // Check if the current node is the target location, if so, retrace the path
            if (currentNode.ATileData == targetLocation)
            {
                RetracePath(startNode, currentNode, actionPoint);
                return;
            }
            // Explore neighbors of the current node
            foreach (TileData neighbor in currentNode.ATileData.adjacentTiles)
            {
                // Ignore blocked nodes or nodes already in the closed set
                if (neighbor.myEntity != null)
                {
                    if (neighbor.myEntity.MoveCost >= 999 || closedSet.Contains(neighbor))
                    {
                        continue;
                    }
                }

                // Calculate the new G cost
                int newGCost = currentNode.GCost + GetDistance(currentNode.ATileData, neighbor) * neighbor.myEntity.MoveCost;
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
        }
    }

    private void RetracePath(AStarNode startNode, AStarNode endNode, int actionPoint)
    {
        // Create an empty list to store the path
        List<TileData> path = new List<TileData>();
        AStarNode currentNode = endNode;
        // Follow parent pointers from the end node to the start node, adding each node to the path
        while (currentNode != startNode)
        {
            path.Add(currentNode.ATileData);
            currentNode = currentNode.Parent;
        }
        // Reverse the path so it starts from the starting location
        path.Reverse();
        // Hide the pathfinding visualization for all grid units
        for (int i = 0; i < gridSize.x; i++)
        {
            for (int j = 0; j < gridSize.y; j++)
            {
                //remove indecator of pathfinding line
                //listOfTiles[i,j].hidepathfinding
            }
        }
        // Display the path and calculate its cost
        int pathCost = 0;
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
            CurrentAvalibleMoveTarget = CurrentTile;
            
            //Displays the pathfinding value to get to this point (but we don't have this yet)
            //CurrentTile.DisplayPathfinding(pathCost.ToString());
        }
    }

    /* to go when player is selected to show all movable tiles
    public void StartMovingMode()
    {
        inMovingMode = true;
        List<GridUnit> reachableGrids = CalculateReachableGrids(character.currentLocation, character.actionPoint);
        foreach (GridUnit gridUnit in reachableGrids)
        {
            gridUnit.DisplayReachableGrid();
        }
    }
    */

    /* to go when player is unselected and possible move spaces are no longer selected. 
    public void StopMovingMode()
    {
        inMovingMode = false;
        foreach (GridUnit grid in gridUnitList)
        {
            grid.ResetGrid();
        }
    }
    */

    /* Old code used for typing tiles, incompatable with fluid object/tile system use TileData.myEntity.cost (where a null entity = cost 1 or no movement tax)
    int GetCost(int tileType)
    {
        int cost = 0;
        switch (tileType)
        {
            case 0:
                cost = 1;
                break;
            case 1:
                cost = 2;
                break;
            case 2:
                cost = 99;
                break;
        }

        return cost;
    }
    */

    /* from old Astar, replaced by FindAdjacentTiles
    private List<TileData> FindAdjacentNeighborsList(TileData checkedTile)
    {
        List<TileData> neighbors = new List<TileData>();
        int mapX = gridSize.x;
        int mapY = gridSize.y;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                //Diagonal grids are not neighbor
                if (Mathf.Abs(x) == Mathf.Abs(y)) continue;

                int checkX = checkedTile.gridPosition.x + x;
                int checkY = checkedTile.gridPosition.y + y;

                if (checkX >= 0 && checkX < mapX && checkY >= 0 && checkY < mapY)
                {
                    neighbors.Add(listOfTiles[checkX,checkY]);
                }
            }
        }
        print("Core" + checkedTile.gridPosition);
        foreach (TileData tile in neighbors)
        {
            print(tile.gridPosition);
        }
        return neighbors;
        
    }
    */
}