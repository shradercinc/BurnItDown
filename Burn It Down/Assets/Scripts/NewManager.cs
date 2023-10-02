using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyBox;

public class NewManager : MonoBehaviour
{
    public static NewManager instance;

    [Foldout("Storing Things", true)]
         [Tooltip("Reference to players")] [ReadOnly] public List<PlayerEntity> listOfPlayers = new List<PlayerEntity>();
         [Tooltip("Reference to walls")] [ReadOnly] public List<WallEntity> listOfWalls = new List<WallEntity>();
         [Tooltip("Reference to guards")] [ReadOnly] public List<GuardEntity> listOfGuards = new List<GuardEntity>();
         [Tooltip("Reference to active environmental objects")][ReadOnly] public List<EnvironmentalEntity> listOfEnvironmentals = new List<EnvironmentalEntity>();

    [Foldout("Card Zones", true)]
        [Tooltip("Your hand in the canvas")]RectTransform handTransform;
        [Tooltip("Reference to card scripts")] List<Card> listOfHand = new List<Card>();
        [Tooltip("Your deck in the canvas")] Transform deck;
        [Tooltip("Your discard pile in the canvas")]Transform discardPile;
        [Tooltip("Your exhausted cards in the canvas")]Transform exhausted;
    
    [Foldout("UI Elements", true)]
        [Tooltip("Your health")]int currentHealth;
        [Tooltip("Health bar")]Slider healthBar;
        [Tooltip("Health bar's textbox")]TMP_Text healthText;
        [Tooltip("Your energy")]int currentEnergy; 
        [Tooltip("Energy bar")]Slider energyBar;
        [Tooltip("Energy bar's textbox")]TMP_Text energyText;
        [Tooltip("Movement bar")] Slider movementBar;
        [Tooltip("Movement bar's textbox")]TMP_Text movementText;
        [Tooltip("Regain energy/health/cards")] Button regainResources;
        [Tooltip("Button to stop moving")] Button stopMovingButton;
        [Tooltip("info on entities")] public EntityToolTip toolTip;

    [Foldout("GameOver", true)]
        [SerializeField] TMP_Text gameOverText;
        [SerializeField] GameObject gameOverButton;
        [SerializeField] GameObject gameOverScreen;

    [Foldout("Grid", true)]
        [Tooltip("Tiles in the inspector")] Transform gridContainer; 
        [Tooltip("Storage of tiles")] [ReadOnly] public TileData[,] listOfTiles;
        [Tooltip("Spacing between tiles")] [SerializeField] float tileSpacing = 2;
        [Tooltip("Tile height")] [SerializeField] float tileHeight = 2;
    
    [Foldout("Prefabs", true)]
        [Tooltip("Floor tile prefab")] public TileData floorTile;
        [Tooltip("Player prefab")] public PlayerEntity player;
        [Tooltip("Wall prefab")] public WallEntity wall;
        [Tooltip("Guard prefab")] public GuardEntity guard;

    [Foldout("Setup", true)]
        [Tooltip("Size of the level")] public Vector2Int gridSize;
        [Tooltip("Starting hand")] Transform startingHand;
        [Tooltip("Entities and their starting positions")] [SerializeField] List<EntityAndPosition> entityStarts = new List<EntityAndPosition>();
    
    public enum TurnSystem {You, Resolving, Environmentals, Enemy};
    [Foldout("Turn System", true)]
        [Tooltip("What's happening in the game")] [ReadOnly] public TurnSystem currentTurn;
        [Tooltip("Number of actions you can do before enemy's turn")] [ReadOnly] public int numActions;

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

        //get the cards in your starting hand
        for (int i = 0; i < startingHand.childCount; i++)
        {
            Card nextCard = startingHand.GetChild(i).GetComponent<Card>();
            nextCard.choiceScript.DisableButton();
            AddCardToHand(nextCard);
        }

        deck.Shuffle(); //shuffle your deck

        listOfTiles = new TileData[gridSize.x, gridSize.y];
        for (int i = 0; i<gridSize.x; i++) //create the tiles
        {
            for (int j = 0; j<gridSize.y; j++)
            {
                TileData nextTile = Instantiate(floorTile, gridContainer);
                nextTile.transform.localPosition = new Vector3(i*-tileSpacing, tileHeight, j*-tileSpacing);
                nextTile.name = $"Tile {i},{j}";
                listOfTiles[i, j] = nextTile;
                nextTile.gridPosition = new Vector2Int(i,j);
            }
        }
        for (int i = 0; i<gridSize.x; i++) //then find adjacent tiles
        {
            for (int j = 0; j<gridSize.y; j++)
            {
                FindAdjacent(listOfTiles[i, j]);
            }
        }

        for (int i = 0; i<entityStarts.Count; i++) //spawn all entities in their starting positions
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
        regainResources.gameObject.SetActive(currentTurn == TurnSystem.You);
    }
    void FindAdjacent(TileData tile)
    {   //check each adjacent tile; if it's not null, add it to the list
        tile.adjacentTiles.Add(FindTile(new Vector2(tile.gridPosition.x+1, tile.gridPosition.y)));
        tile.adjacentTiles.Add(FindTile(new Vector2(tile.gridPosition.x-1, tile.gridPosition.y)));
        tile.adjacentTiles.Add(FindTile(new Vector2(tile.gridPosition.x, tile.gridPosition.y+1)));
        tile.adjacentTiles.Add(FindTile(new Vector2(tile.gridPosition.x, tile.gridPosition.y-1)));
        
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
        StartCoroutine(ChoosePlayerToMove());
        yield return null;
    }
    void MadeDecision()
    {
        currentTurn = TurnSystem.Resolving;
        ChoiceManager.instance.DisableCards();
        ChoiceManager.instance.DisableTiles();
    }
    IEnumerator ChoosePlayerToMove()
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
        yield return MovePlayer(ChoiceManager.instance.chosenTile);
    }
    IEnumerator MovePlayer(TileData currentTile)
    {
        MadeDecision();
        stopMovingButton.gameObject.SetActive(true);
        List<TileData> possibleTiles = new List<TileData>();
        for (int i = 0; i < currentTile.adjacentTiles.Count; i++)
            possibleTiles.Add(currentTile.adjacentTiles[i]);
        ChoiceManager.instance.ChooseTile(possibleTiles);

        while (ChoiceManager.instance.chosenTile == null)
        {
            if (currentTurn != TurnSystem.Resolving)
                yield break;
            else
                yield return null;
        }

        PlayerEntity player = currentTile.myEntity.GetComponent<PlayerEntity>();
        player.movementLeft--;
        player.MoveTile(ChoiceManager.instance.chosenTile);

        movementBar.value = player.movementLeft;
        movementText.text = $"Movement: {player.movementLeft}";
            if (player.movementLeft <= 0)
            StopMoving();
        else
            yield return MovePlayer(ChoiceManager.instance.chosenTile);
    }
    public void StopMoving()
    {
        //StartCoroutine(EndTurn());
    }
    IEnumerator CanPlayCard() //choose a card to play
    {
        List<Card> canBePlayed = new List<Card>();
        for (int i = 0; i<listOfHand.Count; i++)
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

        for (int i = 0; i<listOfGuards.Count; i++)
        {
            yield return null;
            yield return listOfGuards[i].EndOfTurn();
        }

        StartCoroutine(StartPlayerTurn());
    }
}