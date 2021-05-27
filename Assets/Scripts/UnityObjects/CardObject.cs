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
            // Load sprite
            //TODO fix this to load correct sprite
            gameObject.GetComponent<SpriteRenderer>().sprite = Resources.Load("Sprites/HjerterTemplate", typeof(Sprite)) as Sprite;
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
    }
}
