using Assets.Scripts;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using UnityEngine.SceneManagement;

public class BusGameController : MonoBehaviour
{
    // game objects
    public Camera main;

    private List<Card> Deck;
    public List<Player> Players { get; set; }

    private int CurrentPlayerIndex;
    private Player CurrentPlayer { get { return Players[CurrentPlayerIndex]; } }

    private int Round { get; set; }

    private List<PlayerCardPair> GiveActionPlayers { get; set; }

    private List<GameObject> CardsOnTable { get; set; }

    private List<CardObject> CardsActiveOnBus { get; set; }

    private List<Player> PlayersOnBus { get; set; }

    #region UIComponents
    // TODO handle this more gracefully
    public GameObject ActionText;
    public GameObject PlayerText;
    public Text HandText;

    // choice panels
    public GameObject First;
    public GameObject Second;
    public GameObject Third;
    public GameObject Fourth;

    // Action panels
    public GameObject GivePanel;
    public GameObject BusPassengerPanel;

    // misc
    public Text GiveCounter;

    // Add player objects
    public GameObject AddPlayerPanel;
    public Text CurrentPlayerText;

    #endregion

    // Holds the graphic of the last played card
    public GameObject LastCardPlayed;

    // Start is called before the first frame update
    void Start()
    {
        CardsOnTable = new List<GameObject>();
        GiveActionPlayers = new List<PlayerCardPair>();
        CardsActiveOnBus = new List<CardObject>();
        PlayersOnBus = new List<Player>();
        // TODO move to another place for init
        // Player init
        Players = new List<Player>();

        CurrentPlayerIndex = 0;

        // initialize deck of cards
        Deck = StaticHelperFunctions.ResetCards();
    }

    // Update is called once per frame
    void Update()
    {
        // Mouse pointer section
        if (Input.GetMouseButtonDown(0) && GiveActionPlayers.Count == 0 && !BusPassengerPanel.activeSelf)
        {
            // Position of mouse pointer
            Vector3 pos = main.ScreenToWorldPoint(Input.mousePosition);

            RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero);
            if (hit.transform != null)
            {
                CardObject cardSelected = hit.transform.gameObject.GetComponent<CardObject>();
                if (cardSelected.CardInfo != null)
                {
                    Debug.Log("Selected card: " + cardSelected.CardInfo.ToString());
                    if (!cardSelected.Turned)
                    {
                        cardSelected.TurnCard();
                        if (cardSelected.Type == CardType.BusRide)
                        {
                            //Bus
                            CardsActiveOnBus.Add(cardSelected);
                            // display the turned card
                            DisplayCard(cardSelected.CardInfo);
                            // handle drawing bus cards
                            if (cardSelected.CardInfo.Value >= 10 || cardSelected.CardInfo.Value == 1)
                            {
                                // they drink
                                var drinkCounter = 0;
                                foreach (var card in CardsActiveOnBus)
                                {
                                    // TODO move the draw new card when turning to the card object class.
                                    // Use public method to draw the card from singleton
                                    card.CardInfo = DrawCard(Deck);
                                    card.TurnCard();
                                    drinkCounter++;
                                }
                                var playersToDrink = new StringBuilder();
                                foreach (var p in PlayersOnBus)
                                {
                                    playersToDrink.Append(p.Name + ", ");
                                }
                                playersToDrink.Append("skal drikke " + drinkCounter + " tåre");
                                CardsActiveOnBus = new List<CardObject>();
                                ActionText.GetComponent<Text>().text = playersToDrink.ToString();
                            }
                        }
                        else
                        {
                            //guitar
                            var actionTaken = CheckCardActionGuitar(cardSelected.CardInfo, cardSelected.Type, cardSelected.Sips);
                            DisplayCard(cardSelected.CardInfo);
                            if (!actionTaken)
                            {
                                // draw new card
                                cardSelected.CardInfo = DrawCard(Deck);
                                cardSelected.TurnCard();
                            }
                            else
                            {
                                if (GiveActionPlayers.Count > 0)
                                {
                                    GiveActionPlayers.OrderBy(pair => pair.Player.Name);
                                    // set active selection panel
                                    GivePanel.SetActive(true);
                                    // fill dropdown
                                    var dropDown = GivePanel.GetComponentInChildren<Dropdown>();
                                    List<string> playerNames = Players.Select(p => p.Name).ToList();
                                    dropDown.ClearOptions();
                                    dropDown.AddOptions(playerNames);
                                    SetActionPair();
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    #region HelperMethods
    /// <summary>
    /// Gets to next round in the initial game where you acquire cards
    /// </summary>
    private void NextRoundCardsAcquire()
    {
        Round++;
        switch (Round)
        {
            case 1:
                First.SetActive(false);
                Second.SetActive(true);
                Third.SetActive(false);
                Fourth.SetActive(false);
                break;
            case 2:
                First.SetActive(false);
                Second.SetActive(false);
                Third.SetActive(true);
                Fourth.SetActive(false);
                break;
            case 3:
                First.SetActive(false);
                Second.SetActive(false);
                Third.SetActive(false);
                Fourth.SetActive(true);
                break;
            case 4:
                First.SetActive(false);
                Second.SetActive(false);
                Third.SetActive(false);
                Fourth.SetActive(false);
                StartGuitar();
                break;
            default:
                break;
        }
    }

    private void NextPlayer()
    {
        CurrentPlayerIndex++;
        if (CurrentPlayerIndex > Players.Count - 1)
        {
            CurrentPlayerIndex = 0;
            NextRoundCardsAcquire();
        }
        SetActivePlayer();
    }

    private Card DrawCard(List<Card> deck)
    {
        int lastIndex = deck.Count - 1;
        // TODO handle empty deck
        var currentCard = deck[lastIndex];
        deck.RemoveAt(lastIndex);
        return currentCard;
    }
    
    private void CheckSuits(List<Suit> suits)
    {
        // check if the suit is red
        var currentCard = DrawCard(Deck);
        DispenseSips(CurrentPlayer, 1, suits.Contains(currentCard.Suit));
        CurrentPlayer.Hand.Add(currentCard);

        DisplayCard(currentCard);
    }

    private void StartGuitar()
    {
        // initialize guitar
        var busCard = DrawCard(Deck);

        var length = 4;
        var startSize = length / 2;

        // assumes same size
        for (int i = 0; i < length; i++)
        {
            // TopCard
            var topCard = DrawCard(Deck);
            PlaceCard(startSize * -5 + 5 * i, 0, topCard, i + 1, CardType.Give);

            // BottomCard
            var bottomCard = DrawCard(Deck);
            PlaceCard(startSize * -5 + 5 * i, -7.5f, bottomCard, i + 1, CardType.Take);
        }
        PlaceCard(startSize * -5 + 5 * (length), -3.75f, busCard, 0, CardType.Bus);
    }

    private bool CheckCardActionGuitar(Card selectedCard, CardType type, int sips)
    {
        StringBuilder actionsToTake = new StringBuilder();
        var playerFound = false;

        // The loop when playing the guitar
        var busCreated = false;
        foreach (var p in Players)
        {
            if (p.Hand.Exists(c => c.Value == selectedCard.Value))
            {
                playerFound = true;
                // flag all cards for actions
                switch (type)
                {
                    case CardType.Give:
                        // handle give 
                        // save player in action queue 
                        foreach (Card c in p.Hand)
                        {
                            if (c.Value == selectedCard.Value)
                            {
                                GiveActionPlayers.Add(new PlayerCardPair() { Card = c, Player = p, Sips = sips });
                            }
                        }
                        break;
                    case CardType.Take:
                        actionsToTake.AppendLine(p.Name + " skal tage " + sips + " tåre");
                        break;
                    case CardType.Bus:
                        // Handle bus
                        PlayersOnBus.Add(p);
                        // create the bus
                        if (!busCreated)
                        {
                            StartBus();
                            busCreated = true;
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        SetBusNames();

        ActionText.GetComponent<Text>().text = actionsToTake.ToString();
        return playerFound;
    }

    private void SetActionPair()
    {
        // set current player
        CurrentPlayerIndex = Players.IndexOf(GiveActionPlayers.Last().Player);
        // Set counter
        GiveCounter.text = GiveActionPlayers.Where(pair => pair.Player == CurrentPlayer).Count() + "";
        // set name
        SetActivePlayer();
    }
    
    #endregion


    #region in game methods
    public void GiveSipsToSelectedPlayer()
    {
        var actionPair = GiveActionPlayers.Last();
        var dropDown = GivePanel.GetComponentInChildren<Dropdown>();
        var intPlayerVal = dropDown.value;
        var targetPlayer = Players[intPlayerVal];
        var removedCard = actionPair.Card;
        // remove the card from the active player
        actionPair.Player.Hand = actionPair.Player.Hand.Where(c => c.Suit != removedCard.Suit || c.Value != removedCard.Value).ToList();

        // give the card to the targeted player
        targetPlayer.Hand.Add(removedCard);

        DispenseSips(targetPlayer, actionPair.Sips, false);
        // remove the card action from the list
        GiveActionPlayers.RemoveAt(GiveActionPlayers.Count - 1);


        if (GiveActionPlayers.Count == 0)
        {
            GivePanel.SetActive(false);
        }
        else
        {
            // set next player action
            SetActionPair();
        }
    }

    public void SuitCheck(int suitIndex)
    {
        Suit suit = (Suit)suitIndex;
        var currentCard = DrawCard(Deck);
        DisplayCard(currentCard);

        DispenseSips(CurrentPlayer, 4, currentCard.Suit == suit);
        CurrentPlayer.Hand.Add(currentCard);
        NextPlayer();
    }

    public void Red()
    {
        // check if the suit is red
        CheckSuits(new List<Suit>() { Suit.Diamonds, Suit.Hearts });
        NextPlayer();
    }

    public void Black()
    {
        // check if the suit is red
        CheckSuits(new List<Suit>() { Suit.Spades, Suit.Clubs });
        NextPlayer();
    }

    public void Equal(int sips)
    {
        var currentCard = DrawCard(Deck);
        var exists = CurrentPlayer.Hand.Exists(c => c.Value == currentCard.Value);
        DispenseSips(CurrentPlayer, sips, exists);
        CurrentPlayer.Hand.Add(currentCard);
        DisplayCard(currentCard);
        NextPlayer();
    }

    public void AboveOrUnder(bool above)
    {
        var currentCard = DrawCard(Deck);
        var currentPlayer = CurrentPlayer;
        if (currentPlayer.Hand.Count > 1)
        {
            // throw error
            Debug.LogError("To large hand size for current question");
            return;
        }
        // check for ace
        var ace = currentPlayer.Hand.Exists(c => c.Value == 1) || currentCard.Value == 1;

        if (above)
        {
            DispenseSips(currentPlayer, 2, ace ? false : currentPlayer.Hand[0].Value < currentCard.Value);
        }
        else
        {
            DispenseSips(currentPlayer, 2, ace ? false : currentPlayer.Hand[0].Value > currentCard.Value);
        }
        CurrentPlayer.Hand.Add(currentCard);
        DisplayCard(currentCard);
        NextPlayer();
    }

    public void InsideOrOutside(bool inside)
    {
        var currentCard = DrawCard(Deck);
        var currentPlayer = CurrentPlayer;
        if (currentPlayer.Hand.Count > 2)
        {
            // throw error
            Debug.LogError("To large hand size for current question");
            return;
        }
        // check for ace
        var ace = currentPlayer.Hand.Exists(c => c.Value == 1) || currentCard.Value == 1;
        var existAbove = currentPlayer.Hand.Exists(c => c.Value > currentCard.Value);
        var existBelow = currentPlayer.Hand.Exists(c => c.Value < currentCard.Value);
        var isInside = existAbove && existBelow;

        if (inside)
        {
            DispenseSips(currentPlayer, 3, ace ? false : isInside);
        }
        else
        {
            DispenseSips(currentPlayer, 3, ace ? false : (existAbove || existBelow) && !isInside);
        }
        CurrentPlayer.Hand.Add(currentCard);
        DisplayCard(currentCard);
        NextPlayer();
    }

    public void ResartBus()
    {
        SceneManager.LoadScene("Bus");
    }
    #endregion

    #region GameObjectsInteraction
    // This region contains all methods which interacts directly with gameobjects in the scene

    public void AddPlayer(InputField inputField)
    {
        // get the name from the input field
        var player = new Player() { Name = inputField.text };
        // add to the player list
        Players.Add(player);
        // update the UI with current player names
        CurrentPlayerText.text = CurrentPlayerText.text + "\n" + inputField.text;
        // clear the text field
        inputField.text = "";
    }

    public void AddPassenger()
    {
        var dropDown = BusPassengerPanel.GetComponentInChildren<Dropdown>();
        var intPlayerVal = dropDown.value;
        var playerName = dropDown.options[intPlayerVal];
        var targetPlayer = Players.Where(p => p.Name == playerName.text).First();
        PlayersOnBus.Add(targetPlayer);

        BusPassengerPanel.SetActive(false);
        SetBusNames();
    }

    public void StartGame()
    {
        AddPlayerPanel.SetActive(false);
        First.SetActive(true);
        SetActivePlayer();
    }

    /// <summary>
    /// Sets the names of the players who are on the bus
    /// </summary>
    private void SetBusNames()
    {
        // Set bus names
        string playersOnBusNames = "";
        foreach (var p in PlayersOnBus)
        {
            playersOnBusNames += p.Name + ", ";
        }
        if (!string.IsNullOrEmpty(playersOnBusNames))
        {
            PlayerText.GetComponent<Text>().text = playersOnBusNames;
        }
    }

    /// <summary>
    /// Places a card in the game scene
    /// </summary>
    /// <param name="xcoord">X coordinate of the card object</param>
    /// <param name="ycoord">Y coordinate of the card object</param>
    /// <param name="card">The card information to be set on the created object</param>
    /// <param name="sips">Number of sips associated with flipping the card. 0 for cards that have other functions</param>
    /// <param name="type">The type of action there is to be taken which flipping the card</param>
    private void PlaceCard(float xcoord, float ycoord, Card card, int sips, CardType type)
    {
        var cardObj = Instantiate(Resources.Load("CardTemplate") as GameObject, new Vector3(xcoord, ycoord), Quaternion.identity) as GameObject;
        var cardObjTop = cardObj.GetComponent<CardObject>();
        cardObjTop.SetCardInfo(card, sips, type);
        CardsOnTable.Add(cardObj);
    }

    private void StartBus()
    {
        // clear the Guitar
        foreach (var gObj in CardsOnTable)
        {
            Destroy(gObj);
        }
        CardsOnTable = new List<GameObject>();
        Deck = StaticHelperFunctions.ResetCards();
        // create the bus pattern
        var busSize = 5;
        var startSize = busSize / 2;
        var busHeights = new int[] { 1, 2, 3, 2, 1 };

        for (int i = 0; i < busSize; i++)
        {
            for (int j = 0; j < busHeights[i]; j++)
            {
                var busCard = DrawCard(Deck);
                var startYcoord = 0.0f;
                switch (busHeights[i])
                {
                    case 1:
                        startYcoord = 0.0f;
                        break;
                    case 2:
                        startYcoord = -4.0f;
                        break;
                    case 3:
                        startYcoord = -8.0f;
                        break;
                    default:
                        break;
                }
                PlaceCard(startSize * -5 + i * 5, startYcoord + j * 8, busCard, 0, CardType.BusRide);
            }
        }

        if (PlayersOnBus.Count < 2)
        {
            // enable player selector
            BusPassengerPanel.SetActive(true);
            var busDropdown = BusPassengerPanel.GetComponentInChildren<Dropdown>();
            var availablePlayers = Players.Where(p => p.Name != PlayersOnBus.First().Name).Select(p => p.Name).ToList();
            busDropdown.ClearOptions();
            busDropdown.AddOptions(availablePlayers);
        }
    }

    private void DispenseSips(Player player, int amount, bool give)
    {
        if (give)
        {
            // hand out one sip
            ActionText.GetComponent<Text>().text = player.Name + " må give " + amount + " tår";
        }
        else
        {
            // drink one sip
            ActionText.GetComponent<Text>().text = player.Name + " skal tage " + amount + " tår";
        }
    }

    private void SetActivePlayer()
    {
        PlayerText.GetComponent<Text>().text = CurrentPlayer.Name;
        HandText.text = string.Join(",", CurrentPlayer.Hand);
    }

    private void DisplayCard(Card card)
    {
        // Display last drawn card
        var lastPlayedCardObj = LastCardPlayed.GetComponent<CardObject>();
        lastPlayedCardObj.SetCardInfo(card, 0, CardType.Display);
        if (!lastPlayedCardObj.Turned)
        {
            lastPlayedCardObj.TurnCard();
        }
    }
    #endregion
}
