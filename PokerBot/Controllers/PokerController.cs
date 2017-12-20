using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace PokerBot.Controllers
{
    [Route("poker")]
    public class PokerController : Controller
    {
        private static GameState State { get; set; }

        [HttpPost, Route("start")]
        public IActionResult Start(PokerStartingOptions options)
        {
            State = new GameState
            {
                Pool = options.STARTING_CHIP_COUNT * 2,
                OpponentName = options.OPPONENT_NAME,
                OurChips = options.STARTING_CHIP_COUNT,
                HandLimit = options.HAND_LIMIT,
                BigBlind = options.BIG_BLIND,
                SmallBlind = options.SMALL_BLIND,
            };

            Log.Write($"[{State.OpponentName}] START chips: {State.OurChips}/{State.Pool}, handLimit: {State.HandLimit}, bigBlind: {State.BigBlind}, smallBlind: {State.SmallBlind}");

            return Content("");
        }

        [HttpPost, Route("update")]
        public IActionResult Update(PokerUpdate update)
        {
            try
            {
                Log.Write($"[{State.OpponentName}] {update.Command}: {update.Data}");

                if (update.Command == "CARD") State.OurCard = new Card(update.Data);
                if (update.Command == "POST_BLIND")
                {
                    State.Blind = true;
                    State.OurChips -= State.BigBlind;
                }
                if (update.Command == "RECEIVE_BUTTON")
                {
                    State.Button = true;
                    State.OurChips -= State.SmallBlind;
                }
                if (update.Command == "RECEIVE_CHIPS")
                {
                    State.ChipsReceived(int.Parse(update.Data));
                    Log.Write($"ROUND_OVER chips: {State.OurChips}");
                }
                if (update.Command == "OPPONENT_MOVE") State.OpponentMove.Add(update.Data);
                if (update.Command == "OPPONENT_CARD") State.OpponentCard = update.Data;

                return Content("");
            }
            catch (Exception exception)
            {
                Log.Write($"ERROR {exception}");
                return Content("");
            }
        }

        [HttpGet, Route("move")]
        public IActionResult Move()
        {
            var move = State.GetMove();
            Log.Write($"[{State.OpponentName}] MOVE {move}");
            return Content(move);
        }
    }

    public class Card
    {
        public string Name { get; }

        public Card(string name)
        {
            Name = name;
        }

        public bool HighValue() => Name == "9" || Name == "T" || Name == "J" || Name == "Q" || Name == "K" || Name == "A";
    }

    public class GameState
    {
        public int Pool { get; set; }

        public bool Blind { get; set; }
        public bool Button { get; set; }

        public int OurChips { get; set; }
        public Card OurCard { get; set; }
        public List<string> OurMove { get; } = new List<string>();
        public string OurLastMove => OurMove.LastOrDefault();

        public int OpponentChips => Pool - OurChips;
        public string OpponentCard { get; set; }
        public List<string> OpponentMove { get; } = new List<string>();
        public string OpponentLastMove => OpponentMove.LastOrDefault();
        public string OpponentName { get; set; }

        public int HandLimit { get; set; }
        public int BigBlind { get; set; }
        public int SmallBlind { get; set; }

        public void ChipsReceived(int chips)
        {
            OurChips += chips;
            OurMove.Clear();
            OpponentMove.Clear();
        }

        public string GetMove()
        {
            var move = CalculateMove();
            OurMove.Add(move);
            return move;
        }

        private string CalculateMove()
        {
            var opponentBet = 0;
            if (OpponentLastMove != null && OpponentLastMove.StartsWith("BET:"))
            {
                opponentBet = int.Parse(OpponentLastMove.Split(":")[1]);
            }
            if (IsHuman()
                && opponentBet > 0 && opponentBet < Pool * 0.25 && OurCard.HighValue())
                return "CALL";
            if (opponentBet >= OpponentChips && OpponentMove.Count == 1 && Blind)
                return "FOLD";
            if (OurLastMove != null && OurLastMove.StartsWith("BET")) return "CALL";
            if (OurCard.Name == "A") return $"BET:{OurChips}";
            if (OpponentLastMove == "BET" && OurCard.HighValue())
                return "CALL";
            if (OurCard.HighValue())
            {
                return "BET:{BigBlind}";
            }
            return "FOLD";
        }

        private bool IsHuman()
        {
            return (OpponentName == "allinordie" || OpponentName == "goker" || OpponentName == "chegwin2");
        }
    }

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