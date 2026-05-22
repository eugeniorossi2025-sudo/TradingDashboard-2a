using System;
using Gamebot.Helpers;
using Gamebot.Models.MouseMove;
using Gamebot.Models.Objects;
using System.Threading;
using System.Threading.Tasks;

namespace Gamebot.Models.SubStates
{
    internal static class StateFirstPlay
    {
        public static void Act()
        {
            bool dashboardCalled = false;
            Runtime.ResetVariables();
            StateFirstPlay.exit = false;
            StateFirstPlay.state = 0;
            string winner = string.Empty;
            Log.PrintInfo(string.Format("VARIABILI INIZIALI | CHOSEN_COLOR: {0} | LAST COLOR: {1} | LAST_RESULT: {2}", Runtime.chosen_color, Runtime.last_color, Runtime.last_result));
            Log.PrintInfo(string.Format("VARIABILI INIZIALI | INDICE MARTINGALA: {0}", Runtime.martingala_counter));
            while (!StateFirstPlay.exit && Runtime.runningStateMachineBot)
            {
                int num = StateFirstPlay.state;
                if (num != 0)
                {
                    if (num != 1)
                    {
                        StateFirstPlay.exit = true;
                    }
                    else
                    {
                        if (Check.centralLabelContainsVINCE(ref winner))
                        {
                            Log.PrintInfo("************* CHECK RISULTATO ************");
                            Runtime.old_martingala_counter = Runtime.martingala_counter;
                            var chosen_color_code = Runtime.chosen_color;
                            string chosen_color = "";
                            if (chosen_color_code == Constants.EnumColorBaccarat.BLU_PLAY)
                            {
                                Runtime.old_chosen_color = "P";
                            } else if (chosen_color_code == Constants.EnumColorBaccarat.RED_BANK)
                            {
                                Runtime.old_chosen_color = "B";
                            } else if (chosen_color_code == Constants.EnumColorBaccarat.TIE)
                            {
                                Runtime.old_chosen_color = "T";
                            }
                            StateFirstPlay.CheckResult(winner);
                            DashboardApiHelper.Send();
                            Thread.Sleep(750);
                        }
                        if (Check.centralLabelContainsTIE())
                        {
                            Log.PrintInfo("************* TIE ************");
                            string giocheraiProssimaMano = ((Runtime.last_color != Config.start_color && Runtime.last_color != Constants.EnumColorBaccarat.NAY && Runtime.last_color != Constants.EnumColorBaccarat.TIE) ? "SI" : "NO");
                            Log.PrintInfo(string.Format("COLORE USCITO: {0} | COLORE CONFIG: {1} | GIOCHERAI PROSSIMA MANO: {2}", Runtime.last_color, Config.start_color, giocheraiProssimaMano));
                            Runtime.last_result = Constants.EnumColorBaccarat.TIE;
                            Runtime.last_result_update = DateTime.Now;
                            StateFirstPlay.exit = true;
                            Runtime.old_martingala_counter = Runtime.martingala_counter;
                            var chosen_color_code = Runtime.chosen_color;
                            string chosen_color = "";
                            if (chosen_color_code == Constants.EnumColorBaccarat.BLU_PLAY)
                            {
                                Runtime.old_chosen_color = "P";
                            } else if (chosen_color_code == Constants.EnumColorBaccarat.RED_BANK)
                            {
                                Runtime.old_chosen_color = "B";
                            } else if (chosen_color_code == Constants.EnumColorBaccarat.TIE)
                            {
                                Runtime.old_chosen_color = "T";
                            }
                            DashboardApiHelper.Send();
                            Thread.Sleep(750);
                        }
                    }
                }
                else if (Check.centralLabelContainsPUNTARE())
                {
                    _ = Task.Run(async () =>
                    {
                        DashboardApiHelper.SendDeck();
                    });
                    Log.PrintInfo("************* LETTO PUNTARE ************");
                    StateFirstPlay.FaiProbeRossaMinima().Wait();
                    StateFirstPlay.state = 1;
                    //_ = Task.Run(async () =>  DashboardApiHelper.Send());
                    Thread.Sleep(750);
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(2000);
                        DashboardApiHelper.SendSimple();
                    });
                }
                Thread.Sleep(250);
            }
        }

        private static void CheckResult(string winner)
        {
            if (Check.centralLabelContainsBANCO(winner))
            {
                Runtime.last_color = Constants.EnumColorBaccarat.RED_BANK;
                Runtime.last_result = Constants.EnumColorBaccarat.RED_BANK;
                Runtime.last_result_update = DateTime.Now;
            }
            else if (Check.centralLabelContainsGIOCATORE(winner))
            {
                Runtime.last_color = Constants.EnumColorBaccarat.BLU_PLAY;
                Runtime.last_result = Constants.EnumColorBaccarat.BLU_PLAY;
                Runtime.last_result_update = DateTime.Now;
            }
            else
            {
                Runtime.last_result = Constants.EnumColorBaccarat.TIE;
                Runtime.last_result_update = DateTime.Now;
            }
            StateFirstPlay.exit = true;
            string giocheraiProssimaMano = ((Runtime.last_color != Config.start_color && Runtime.last_color != Constants.EnumColorBaccarat.NAY) ? "SI" : "NO");
            Log.PrintInfo(string.Format("COLORE USCITO: {0} | COLORE CONFIG: {1} | GIOCHERAI PROSSIMA MANO: {2}", Runtime.last_color, Config.start_color, giocheraiProssimaMano));
        }

        private static async Task FaiProbeRossaMinima()
        {
            Runtime.chosen_color = Constants.EnumColorBaccarat.RED_BANK;
            var fiches_array = Calcs
                .GetBestCustomFichesAvailable(CustomFicheWidgetsContainer.getLowestFicheValueAvailable())
                .ToArray();

            await Bets.DoTheCustomBet(fiches_array);
            Log.PrintInfo("FIRST_PLAY | PROBE ROSSA MINIMA");
        }

        private const int STATE_WAIT_INIZIO_TURNO = 0;

        private const int STATE_WAIT_RISULTATO = 1;

        private static bool exit;

        private static int state;
    }
}
