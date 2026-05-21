using Gamebot.Helpers;
using Gamebot.Models.MouseMove;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Gamebot.Models.Roulette.MouseMove
{
    internal class RouletteBets
    {
        public static async void Startup()
        {
            Runtime.game = 1;
            RouletteBets.m = Move.Instance;
            RouletteBets.m.MoveRiposoRoulette();
            RouletteBets.m.ActivateStartRiposaRoulette();
        }

        public static async Task DoTheBet(int handToPlay)
        {
            RouletteBets.m.DeactivateRiposaRoulette();
            List<BtnFiches> buttonFiches = Roulette.Instance.GetNumbersOfHand(handToPlay);
            string[] array = new string[5];
            array[0] = "--------------------- (R) BETTING ON (";
            array[1] = handToPlay.ToString();
            array[2] = "): [";
            array[3] = string.Join<int>(",", buttonFiches.Select((BtnFiches item) => item.Value));
            array[4] = "]----------------------";
            Log.PrintInfo(string.Concat(array));
            switch (handToPlay)
            {
                case 1:
                    RouletteBets.m.MoveRouletteHand1();
                    break;
                case 2:
                    RouletteBets.m.MoveRouletteHand2();
                    break;
                case 3:
                    RouletteBets.m.MoveRouletteHand3();
                    break;
            }
            Thread.Sleep(300);
            RouletteBets.m.Click();
            RouletteBets.m.MoveRiposoRoulette();
            RouletteBets.m.ActivateRiposaRoulette();
        }

        public static Move m;
    }
}
