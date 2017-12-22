using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace PokerBot
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
                Self = new Player
                {
                    Chips = options.STARTING_CHIP_COUNT,
                },
                Opponent = new Player
                {
                    Name = options.OPPONENT_NAME,
                },
                HandLimit = options.HAND_LIMIT,
                BigBlind = options.BIG_BLIND,
                SmallBlind = options.SMALL_BLIND,
            };

            Log.Write($"[{State.OpponentName}] START " +
                      $"chips: {State.Self.Chips}/{State.Pool}, " +
                      $"handLimit: {State.HandLimit}, " +
                      $"bigBlind: {State.BigBlind}, " +
                      $"smallBlind: {State.SmallBlind}");

            return Content("");
        }

        [HttpPost, Route("update")]
        public IActionResult Update(PokerUpdate update)
        {
            try
            {
                Log.Write($"[{State.OpponentName}] {update.Command}: {update.Data}");

                if (update.Command == "CARD") State.Self.OurCard = new Card(update.Data);
                if (update.Command == "POST_BLIND")
                {
                    State.BlindPosted();
                }
                if (update.Command == "RECEIVE_BUTTON")
                {
                    State.ButtonReceived();
                }
                if (update.Command == "RECEIVE_CHIPS")
                {
                    State.ChipsReceived(int.Parse(update.Data));
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

        private int OpponentChips => Pool - Self.Chips;
        public string OpponentCard { get; set; }
        public List<string> OpponentMove { get; } = new List<string>();
        private string OpponentLastMove => OpponentMove.LastOrDefault();
        public string OpponentName { get; set; }

        public int HandLimit { get; set; }
        public int BigBlind { get; set; }
        public int SmallBlind { get; set; }

        public Player Self { get; set; }
        public Player Opponent { get; set; }

        public void BlindPosted()
        {
            Blind = true;
            Self.Chips -= BigBlind;
        }

        public void ButtonReceived()
        {
            Button = true;
            Self.Chips -= SmallBlind;
        }

        public void ChipsReceived(int chips)
        {
            Self.Chips += chips;
            Self.OurMove.Clear();
            OpponentMove.Clear();
        }

        public string GetMove()
        {
            var move = CalculateMove();
            Self.OurMove.Add(move);
            return move;
        }

        private string CalculateMove()
        {
            var opponentBet = 0;
            if (OpponentLastMove != null && OpponentLastMove.StartsWith("BET:"))
            {
                opponentBet = Int32.Parse(OpponentLastMove.Split(":")[1]);
            }
            if (IsHuman()
                && opponentBet > 0 && opponentBet < Pool * 0.25 && Self.OurCard.HighValue())
                return "CALL";
            if (opponentBet >= OpponentChips && OpponentMove.Count == 1 && Blind)
                return "FOLD";
            if (Self.OurLastMove != null && Self.OurLastMove.StartsWith("BET")) return "CALL";
            if (Self.OurCard.Name == "A") return $"BET:{Self.Chips}";
            if (OpponentLastMove == "BET" && Self.OurCard.HighValue())
                return "CALL";
            if (Self.OurCard.HighValue())
            {
                return "BET:{BigBlind}";
            }
            return "FOLD";
        }

        private bool IsHuman()
        {
            return OpponentName == "allinordie" || OpponentName == "goker" || OpponentName == "chegwin2";
        }
    }
}