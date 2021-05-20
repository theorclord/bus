using Assets.Scripts;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class GameController : MonoBehaviour
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
    #endregion

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
        Players.Add(new Player() { Name = "Bob Test" });
        Players.Add(new Player() { Name = "Alice Test" });
        //Players.Add(new Player() { Name = "Natasha Test" });
        //Players.Add(new Player() { Name = "Simba Test" });

        CurrentPlayerIndex = 0;

        SetActivePlayer();

        // initialize deck of cards
        ResetCards();
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
                Debug.Log("Selected card: " + cardSelected.CardInfo.ToString());
                if (!cardSelected.Turned)
                {
                    cardSelected.TurnCard();
                    if (cardSelected.Type == CardType.BusRide)
                    {
                        //Bus
                        CardsActiveOnBus.Add(cardSelected);
                        // handle drawing bus cards
                        if (cardSelected.CardInfo.Value >= 10 || cardSelected.CardInfo.Value == 1)
                        {
                            // they drink
                            var drinkCounter = 0;
                            foreach (var card in CardsActiveOnBus)
                            {
                                // TODO move the draw new card when turning to the card object class.
                                // Use public method to draw the card from singleton
                                card.CardInfo = DrawCard();
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
                        if (!actionTaken)
                        {
                            // draw new card
                            cardSelected.CardInfo = DrawCard();
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

    private void ShuffleDeck(List<Card> deck)
    {
        int n = deck.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            Card value = deck[k];
            deck[k] = deck[n];
            deck[n] = value;
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

    private Card DrawCard()
    {
        int lastIndex = Deck.Count - 1;
        var currentCard = Deck[lastIndex];
        Debug.Log(currentCard.ToString());
        Deck.RemoveAt(lastIndex);
        return currentCard;
    }

    private void ResetCards()
    {
        // initialize deck of cards
        Deck = new List<Card>();
        foreach (Suit s in Enum.GetValues(typeof(Suit)))
        {
            for (int i = 0; i < 12; i++)
            {
                Deck.Add(
                    new Card()
                    {
                        Suit = s,
                        Value = i + 1
                    });
            }
        }

        // shuffle the deck
        ShuffleDeck(Deck);
    }

    private void SetActivePlayer()
    {
        PlayerText.GetComponent<Text>().text = CurrentPlayer.Name;
        HandText.text = string.Join(",", CurrentPlayer.Hand);
    }

    private void CheckSuits(List<Suit> suits)
    {
        // check if the suit is red
        var currentCard = DrawCard();
        DispenseSips(CurrentPlayer, 1, suits.Contains(currentCard.Suit));
        CurrentPlayer.Hand.Add(currentCard);
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

    private void StartGuitar()
    {
        // initialize guitar
        var busCard = DrawCard();

        var length = 4;
        var startSize = length / 2;

        // assumes same size
        for (int i = 0; i < length; i++)
        {
            // TopCard
            var topCard = DrawCard();
            //Guitar.GiveCards[i] = topCard;
            PlaceCard(startSize * -5 + 5 * i, 0, topCard, i + 1, CardType.Give);

            // BottomCard
            var bottomCard = DrawCard();
            //Guitar.TakeCards[i] = bottomCard;
            PlaceCard(startSize * -5 + 5 * i, -7.5f, bottomCard, i + 1, CardType.Take);
        }
        PlaceCard(startSize * -5 + 5 * (length), -3.75f, busCard, 0, CardType.Bus);
    }

    private void AssignCardInfo(CardObject cardObj, Card card, int sips, CardType type)
    {
        cardObj.CardInfo = card;
        cardObj.Sips = sips;
        cardObj.Type = type;
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

    private void StartBus()
    {
        // clear the Guitar
        foreach (var gObj in CardsOnTable)
        {
            Destroy(gObj);
        }
        CardsOnTable = new List<GameObject>();
        ResetCards();
        // create the bus pattern
        var busSize = 5;
        var startSize = busSize / 2;
        var busHeights = new int[] { 1, 2, 3, 2, 1 };

        for (int i = 0; i < busSize; i++)
        {
            for (int j = 0; j < busHeights[i]; j++)
            {
                var busCard = DrawCard();
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

        if(PlayersOnBus.Count < 2)
        {
            // enable player selector
            BusPassengerPanel.SetActive(true);
            var busDropdown = BusPassengerPanel.GetComponentInChildren<Dropdown>();
            var availablePlayers = Players.Where(p => p.Name != PlayersOnBus.First().Name).Select(p => p.Name).ToList();
            busDropdown.ClearOptions();
            busDropdown.AddOptions(availablePlayers);
        }
    }

    private void PlaceCard(float xcoord, float ycoord, Card card, int sips, CardType type)
    {
        var cardObj = Instantiate(Resources.Load("HjerterTemplate") as GameObject, new Vector3(xcoord, ycoord), Quaternion.identity) as GameObject;
        var cardObjTop = cardObj.GetComponent<CardObject>();
        AssignCardInfo(cardObjTop, card, sips, type);
        CardsOnTable.Add(cardObj);
    }

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
        var currentCard = DrawCard();

        DispenseSips(CurrentPlayer, 4, currentCard.Suit == suit);
        CurrentPlayer.Hand.Add(currentCard);
        NextPlayer();
    }

    public void Red()
    {
        // check if the suit is red
        CheckSuits(new List<Suit>() { Suit.Diamond, Suit.Hearts });
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
        var currentCard = DrawCard();
        var exists = CurrentPlayer.Hand.Exists(c => c.Value == currentCard.Value);
        DispenseSips(CurrentPlayer, sips, exists);
        CurrentPlayer.Hand.Add(currentCard);
        NextPlayer();
    }

    public void AboveOrUnder(bool above)
    {
        var currentCard = DrawCard();
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
        NextPlayer();
    }

    public void InsideOrOutside(bool inside)
    {
        var currentCard = DrawCard();
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
        NextPlayer();
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
    #endregion


    #region DebugMethods

    public void ShowHand()
    {
        foreach (var card in CurrentPlayer.Hand)
        {
            Debug.Log(card.ToString());
        }
    }
    #endregion
}
