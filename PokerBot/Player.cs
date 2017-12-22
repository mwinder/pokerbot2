using System.Collections.Generic;
using System.Linq;

namespace PokerBot
{
    public class Player
    {
        public string Name { get; set; }
        public int Chips { get; set; }
        public Card OurCard { get; set; }
        public List<string> OurMove { get; } = new List<string>();
        public string OurLastMove => OurMove.LastOrDefault();
    }
}