using Gamebot.Helpers;
using Gamebot.Models.MouseMove;
using Gamebot.Models.Objects;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Gamebot.Models.SubStates
{
    internal class StatePauseSculping
    {
        public static void Act()
        {
            Runtime.first_giocata = true;
            Runtime.last_color = Constants.EnumColorBaccarat.NAY;
            Runtime.martingala_counter = 0;
            if (!Config.modalita_alternata)
            {
                Runtime.chosen_color = Config.start_color;
            }
            Runtime.sculping_profit = 0.0;
            Runtime.fine_mazzo_counter = 0;
            Runtime.last_win = false;
            Runtime.martingala_persa = false;
            StatePauseSculping.exit = false;
            StatePauseSculping.state = 0;
            string winner = string.Empty;
            StatePauseSculping.randomBet = false;
            while (!StatePauseSculping.exit && Runtime.runningStateMachineBot)
            {
                int num = StatePauseSculping.state;
                if (num != 0)
                {
                    if (num != 1)
                    {
                        StatePauseSculping.exit = true;
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
                            StatePauseSculping.CheckResultNew(winner);
                            DashboardApiHelper.Send();
                            Thread.Sleep(750);
                        }
                        if (Check.centralLabelContainsTIE())
                        {
                            Log.PrintInfo("************* TIE ************");
                            Runtime.last_result = Constants.EnumColorBaccarat.TIE;
                            Runtime.last_result_update = DateTime.Now;
                            StatePauseSculping.exit = true;
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
                    StatePauseSculping.FaiLaPuntata().Wait();
                    StatePauseSculping.state = 1;
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
                    
                if (StatePauseSculping.randomBet)
                {
                    if (Runtime.chosen_color == Constants.EnumColorBaccarat.RED_BANK)
                    {
                        Runtime.global_profit += 0.949999988079071;
                        return;
                    }
                    Runtime.global_profit -= 1.0;
                    return;
                }
            }
            else if (Check.centralLabelContainsGIOCATORE(winner))
            {
                Runtime.last_color = Constants.EnumColorBaccarat.BLU_PLAY;
                Runtime.last_result = Constants.EnumColorBaccarat.BLU_PLAY;
                Runtime.last_result_update = DateTime.Now;
                if (StatePauseSculping.randomBet)
                {
                    if (Runtime.chosen_color == Constants.EnumColorBaccarat.BLU_PLAY)
                    {
                        Runtime.global_profit += 1.0;
                        return;
                    }
                    Runtime.global_profit -= 1.0;
                    return;
                }
            }
            else
            {
                Runtime.last_result = Constants.EnumColorBaccarat.TIE;
                Runtime.last_result_update = DateTime.Now;
            }
        }

        private static void CheckResultNew(string winner)
        {
            StatePauseSculping.CheckResult(winner);
            int indexArrayNay = StatePauseSculping.GetEmptyIndex(Runtime.color_pause_scalping_array);
            if (indexArrayNay == 0)
            {
                Runtime.color_pause_scalping_array[indexArrayNay] = Runtime.chosen_color;
                StatePauseSculping.exit = true;
                return;
            }
            if (indexArrayNay < 0)
            {
                StatePauseSculping.exit = true;
                return;
            }
            if (Runtime.last_color != Runtime.color_pause_scalping_array[indexArrayNay - 1])
            {
                Runtime.color_pause_scalping_array[indexArrayNay] = Runtime.last_color;
                StatePauseSculping.exit = true;
                return;
            }
            StatePauseSculping.exit = true;
        }

        private static async Task FaiLaPuntata()
        {
            Runtime.puntata = 0;
            Runtime.pause_sculping_counter++;
            if (Runtime.pause_sculping_counter % 3 == 0)
            {
                try
                {
                    Log.PrintInfo(string.Format("GIOCATA RANDOM Pause Sculping | Runtime PauseScalping: {0}", Runtime.pause_sculping_counter));
                    
                    var fiches_array = Calcs
                        .GetBestCustomFichesAvailable(CustomFicheWidgetsContainer.getLowestFicheValueAvailable())
                        .ToArray();
                    double totale_puntata = 0.0;
                    for (int i = 0; i < fiches_array.Length; i++)
                    {
                        totale_puntata += fiches_array[i];  
                    }
                    Runtime.puntata = totale_puntata;
                    
                    //_ = Task.Run(async () =>  DashboardApiHelper.Send());
                    
                    await Bets.DoTheCustomBet(fiches_array);
                IL_00D5:
                    Runtime.pause_sculping_counter = 0;
                    StatePauseSculping.randomBet = true;
                    return;
                    TaskAwaiter taskAwaiter2;
                    TaskAwaiter taskAwaiter = taskAwaiter2;
                    taskAwaiter2 = default(TaskAwaiter);
                    taskAwaiter.GetResult();
                    goto IL_00D5;
                }
                catch (Exception ex)
                {
                    Log.PrintInfo(ex.Message);
                }
            }
            else
            {
                //_ = Task.Run(async () =>  DashboardApiHelper.Send());
            }
        }

        public static int GetEmptyIndex(Constants.EnumColorBaccarat[] array)
        {
            int index = -1;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == Constants.EnumColorBaccarat.NAY)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        private const int STATE_WAIT_INIZIO_TURNO = 0;

        private const int STATE_WAIT_RISULTATO = 1;

        public static void RequestExit() => exit = true;

        private static bool exit;

        private static int state;

        private static bool randomBet;
    }
}
