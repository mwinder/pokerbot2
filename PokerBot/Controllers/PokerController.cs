using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NLog;

namespace PokerBot.Controllers
{
    [Route("poker")]
    public class PokerController : Controller
    {
        //private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private static GameState GameState { get; set; }

        [HttpPost, Route("start")]
        public IActionResult Start(PokerStartingOptions options)
        {
            GameState = new GameState();
            GameState.OpponentName = options.OPPONENT_NAME;
            GameState.Chips = options.STARTING_CHIP_COUNT;

            return Content("");
        }

        [HttpPost, Route("update")]
        public IActionResult Update(PokerUpdate update)
        {
            //Log.Info($"command: {update.Command}, data: {update.Data}");

            if (update.Command == "CARD") GameState.Card = update.Data;
            if (update.Command == "POST_BLIND") { GameState.Blind = true; }
            if (update.Command == "RECEIVE_BUTTON") GameState.Button = true;
            if (update.Command == "RECEIVE_CHIPS") GameState.Chips += int.Parse(update.Data);
            if (update.Command == "OPPONENT_MOVE") GameState.Move.Add(update.Data);
            if (update.Command == "OPPONENT_CARD") GameState.OpponentCard = update.Data;

            return Content("");
        }


        [HttpGet, Route("move")]
        public IActionResult Move()
        {
            return Content("FOLD");
        }
    }

    public class GameState
    {
        public string OpponentName { get; set; }
        public string Card { get; set; }
        public string OpponentCard { get; set; }
        public int Chips { get; set; }
        public bool Blind { get; set; }
        public bool Button { get; set; }
        public List<string> Move { get; set; }
    }

    public class PokerStartingOptions
    {
        public string OPPONENT_NAME { get; set; }
        public int STARTING_CHIP_COUNT { get; set; }
        public int HAND_LIMIT { get; set; }
    }

    public class PokerUpdate
    {
        public string Command { get; set; }
        public string Data { get; set; }
    }
}