using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardType
{
    Give = 0,
    Take = 1,
    Bus = 2,
    BusRide = 3,
    Display = 4,
}

public class CardObject : MonoBehaviour
{
    public Card CardInfo { get; set; }
    public bool Turned { get; set; }
    public int Sips { get; set; }
    public CardType Type { get; set; }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TurnCard()
    {
        if (!Turned)
        {
            Turned = true;
            SetSprite();
        } else
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = Resources.Load("Sprites/BackSide", typeof(Sprite)) as Sprite;
            Turned = false;
        }
    }

    public void SetCardInfo(Card card, int sips, CardType type)
    {
        CardInfo = card;
        Sips = sips;
        Type = type;
        if(Turned)
        {
            SetSprite();
        }
    }

    private void SetSprite()
    {
        // Load sprite
        // TODO add for clubs and spades
        string spriteName = "";
        switch (CardInfo.Suit)
        {
            case Suit.Clubs:
                spriteName = "Clubs/" + CardInfo.Value + "Klør";
                break;
            case Suit.Spades:
                spriteName = "Spades/" + CardInfo.Value + "Spar";
                break;
            case Suit.Diamonds:
                spriteName = "Diamonds/" + CardInfo.Value + "Ruder";
                break;
            case Suit.Hearts:
                spriteName = "Hearts/" + CardInfo.Value + "Hjerter";
                break;
            default:
                spriteName = "HjerterTemplate";
                break;
        }
        gameObject.GetComponent<SpriteRenderer>().sprite = Resources.Load("Sprites/" + spriteName, typeof(Sprite)) as Sprite;
    }
}
