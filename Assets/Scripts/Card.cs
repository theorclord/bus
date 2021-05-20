using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public enum Suit
    {
        Spades = 0,
        Diamond = 1,
        Hearts = 2,
        Clubs = 3,
    }
    public class Card
    {
        public int Value { get; set; }
        public Suit Suit { get; set; }

        public override string ToString()
        {
            return "Color " + Suit + "; Value " + Value;
        }
    }

   
}
