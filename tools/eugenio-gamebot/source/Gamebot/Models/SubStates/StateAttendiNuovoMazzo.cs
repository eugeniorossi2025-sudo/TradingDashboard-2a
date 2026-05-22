using Gamebot.Helpers;
using Gamebot.Models.MouseMove;
using Gamebot.Models.Objects;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Gamebot.Models.SubStates
{
    internal class StateAttendiNuovoMazzo
    {
        public static void Act()
        {
            StateAttendiNuovoMazzo.exit = false;
            StateAttendiNuovoMazzo.state = 0;
            string winner = string.Empty;
            StateAttendiNuovoMazzo.randomBet = false;
            Runtime.martingala_counter = 0;
            while (!StateAttendiNuovoMazzo.exit && Runtime.runningStateMachineBot)
            {
                int num = StateAttendiNuovoMazzo.state;
                if (num != 0)
                {
                    if (num != 1)
                    {
                        StateAttendiNuovoMazzo.exit = true;
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
                            StateAttendiNuovoMazzo.CheckResult(winner);
                            DashboardApiHelper.Send();
                            
                            Thread.Sleep(750);
                        }
                        if (Check.centralLabelContainsTIE())
                        {
                            Log.PrintInfo("************* TIE ************");
                            Runtime.last_result = Constants.EnumColorBaccarat.TIE;
                            Runtime.last_result_update = DateTime.Now;
                            StateAttendiNuovoMazzo.exit = true;
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
                    StateAttendiNuovoMazzo.FaiLaPuntata().Wait();
                    StateAttendiNuovoMazzo.state = 1;
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
                
                if (StateAttendiNuovoMazzo.randomBet)
                {
                    if (Runtime.chosen_color == Constants.EnumColorBaccarat.RED_BANK)
                    {
                        Runtime.global_profit += 0.949999988079071;
                    }
                    else
                    {
                        Runtime.global_profit -= 1.0;
                    }
                }
            }
            else if (Check.centralLabelContainsGIOCATORE(winner))
            {
                Runtime.last_color = Constants.EnumColorBaccarat.BLU_PLAY;
                Runtime.last_result = Constants.EnumColorBaccarat.BLU_PLAY;
                Runtime.last_result_update = DateTime.Now;
                
                if (StateAttendiNuovoMazzo.randomBet)
                {
                    if (Runtime.chosen_color == Constants.EnumColorBaccarat.BLU_PLAY)
                    {
                        Runtime.global_profit += 1.0;
                    }
                    else
                    {
                        Runtime.global_profit -= 1.0;
                    }
                }
            }
            else
            {
                Runtime.last_result = Constants.EnumColorBaccarat.TIE;
                Runtime.last_result_update = DateTime.Now;
            }
            Runtime.fine_mazzo_counter++;
            StateAttendiNuovoMazzo.exit = true;
        }

        private static async Task FaiLaPuntata()
        {
            Runtime.puntata = 0;

            try
            {
                Runtime.chosen_color = Constants.EnumColorBaccarat.RED_BANK;
                var fiches_array = Calcs
                    .GetBestCustomFichesAvailable(CustomFicheWidgetsContainer.getLowestFicheValueAvailable())
                    .ToArray();
                double totale_puntata = 0.0;
                for (int i = 0; i < fiches_array.Length; i++)
                {
                    totale_puntata += fiches_array[i];
                }
                Runtime.puntata = totale_puntata;

                await Bets.DoTheCustomBet(fiches_array);
                Log.PrintInfo(string.Format("WAITING NEW DECK | PROBE ROSSA MINIMA | NUMBER_DECK: {0}", Runtime.number_deck));
                Runtime.waiting_deck_counter = 0;
                StateAttendiNuovoMazzo.randomBet = true;
                return;
            }
            catch (Exception ex)
            {
                Log.PrintInfo(ex.Message);
            }
        }

        private const int STATE_WAIT_INIZIO_TURNO = 0;

        private const int STATE_WAIT_RISULTATO = 1;

        private static bool exit;

        private static int state;

        private static bool randomBet;
    }
}
