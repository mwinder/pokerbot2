namespace PokerBot
{
    public class PokerStartingOptions
    {
        public string OPPONENT_NAME { get; set; }
        public int STARTING_CHIP_COUNT { get; set; }
        public int HAND_LIMIT { get; set; }
        public int BIG_BLIND { get; set; }
        public int SMALL_BLIND { get; set; }
    }

    public class PokerUpdate
    {
        public string Command { get; set; }
        public string Data { get; set; }
    }
}