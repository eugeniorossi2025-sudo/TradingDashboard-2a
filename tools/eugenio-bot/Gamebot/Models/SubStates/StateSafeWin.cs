using Gamebot.Helpers;
using Gamebot.Models.MouseMove;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Gamebot.Models.SubStates
{
    internal class StateSafeWin
    {
        public static void Act()
        {
            StateSafeWin.exit = false;
            StateSafeWin.state = 0;
            string winner = string.Empty;
            Runtime.pause_sculping_counter = 0;
            while (!StateSafeWin.exit && Runtime.runningStateMachineBot)
            {
                int num = StateSafeWin.state;
                if (num != 0)
                {
                    if (num != 1)
                    {
                        StateSafeWin.exit = true;
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
                            StateSafeWin.CheckResult(winner);
                            DashboardApiHelper.Send();
                            Thread.Sleep(750);
                        }
                        if (Check.centralLabelContainsTIE())
                        {
                            Log.PrintInfo("************* TIE ************");
                            string haiVinto = (Runtime.last_win ? "SI" : "NO");
                            Log.PrintInfo(string.Format("COLORE GIOCATO: {0} | COLORE USCITO: {1} | TU HAI VINTO: {2}", Runtime.chosen_color, Runtime.last_color, haiVinto));
                            Log.PrintInfo(string.Format("PROFITTO GLOBALE: {0}€ | PROFITTO LOCALE: {1}€", Runtime.global_profit, Runtime.sculping_profit));
                            Log.PrintInfo(string.Format("PROSSIMO INDICE MARTINGALA: {0}", Runtime.martingala_counter));
                            Runtime.last_result = Constants.EnumColorBaccarat.TIE;
                            Runtime.last_result_update = DateTime.Now;
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
                            StateSafeWin.exit = true;
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
                    StateSafeWin.FaiLaPuntata().Wait();
                    StateSafeWin.state = 1;
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
            Log.PrintInfo("CheckResult | Winner: " + winner);
            if (Check.centralLabelContainsBANCO(winner))
            {
                Runtime.last_color = Constants.EnumColorBaccarat.RED_BANK;
                Runtime.last_result = Constants.EnumColorBaccarat.RED_BANK;
                Runtime.last_result_update = DateTime.Now;
                if (Runtime.chosen_color == Constants.EnumColorBaccarat.RED_BANK)
                {
                    Runtime.global_profit += (double)((float)Config.martingala_array[Runtime.martingala_counter] * Config.profit_multiplier);
                    Runtime.sculping_profit += (double)((float)Config.martingala_array[Runtime.martingala_counter] * Config.profit_multiplier);
                    Runtime.martingala_counter = 0;
                    if (!Config.modalita_alternata)
                    {
                        Runtime.chosen_color = Config.start_color;
                    }
                    Runtime.last_win = true;
                    Runtime.numero_vincite++;
                }
                else if (Runtime.chosen_color == Constants.EnumColorBaccarat.BLU_PLAY)
                {
                    Runtime.global_profit -= (double)((float)Config.martingala_array[Runtime.martingala_counter]);
                    Runtime.sculping_profit -= (double)((float)Config.martingala_array[Runtime.martingala_counter]);
                    Runtime.martingala_counter = 0;
                    if (!Config.modalita_alternata)
                    {
                        Runtime.chosen_color = Config.start_color;
                    }
                    Runtime.last_win = false;
                    Runtime.numero_perdite++;
                }
            }
            else if (Check.centralLabelContainsGIOCATORE(winner))
            {
                Runtime.last_color = Constants.EnumColorBaccarat.BLU_PLAY;
                Runtime.last_result = Constants.EnumColorBaccarat.BLU_PLAY;
                Runtime.last_result_update = DateTime.Now;
                if (Runtime.chosen_color == Constants.EnumColorBaccarat.BLU_PLAY)
                {
                    Runtime.global_profit += (double)((float)Config.martingala_array[Runtime.martingala_counter]);
                    Runtime.sculping_profit += (double)((float)Config.martingala_array[Runtime.martingala_counter]);
                    Runtime.martingala_counter = 0;
                    if (!Config.modalita_alternata)
                    {
                        Runtime.chosen_color = Config.start_color;
                    }
                    Runtime.last_win = true;
                    Runtime.numero_vincite++;
                }
                else if (Runtime.chosen_color == Constants.EnumColorBaccarat.RED_BANK)
                {
                    Runtime.global_profit -= (double)((float)Config.martingala_array[Runtime.martingala_counter]);
                    Runtime.sculping_profit -= (double)((float)Config.martingala_array[Runtime.martingala_counter]);
                    Runtime.martingala_counter = 0;
                    if (!Config.modalita_alternata)
                    {
                        Runtime.chosen_color = Config.start_color;
                    }
                    Runtime.last_win = false;
                    Runtime.numero_perdite++;
                }
            }
            else
            {
                Runtime.last_result = Constants.EnumColorBaccarat.TIE;
                Runtime.last_result_update = DateTime.Now;
            }
            StateSafeWin.exit = true;
            string haiVinto = (Runtime.last_win ? "SI" : "NO");
            Log.PrintInfo(string.Format("COLORE GIOCATO: {0} | COLORE USCITO: {1} | TU HAI VINTO: {2}", Runtime.chosen_color, Runtime.last_color, haiVinto));
            Log.PrintInfo(string.Format("PROFITTO GLOBALE: {0}€ | PROFITTO LOCALE: {1}€", Runtime.global_profit, Runtime.sculping_profit));
            Log.PrintInfo(string.Format("PROSSIMO INDICE MARTINGALA: {0}", Runtime.martingala_counter));
        }

        private static async Task FaiLaPuntata()
        {
            Runtime.puntata = 0;
            Runtime.martingala_counter = 0;
            if (!Config.modalita_alternata)
            {
                Runtime.chosen_color = Config.start_color;
            }
            Runtime.chosen_color = Runtime.last_color;
            Runtime.valore_giocata = Config.martingala_array[Runtime.martingala_counter];
            Log.PrintInfo(string.Format("<!!> FAI LA PUNTATA | MARTINGALA INDICE: {0} | COLORE GIOCATO: {1} | VALORE GIOCATO: {2}€ <!!>", Runtime.martingala_counter, Runtime.chosen_color, Config.martingala_array[Runtime.martingala_counter]));
            try
            {
                var fiches_array = Calcs.GetBestCustomFichesAvailable((float)((int)Runtime.valore_giocata)).ToArray();

                /*
                var old_giocata = Runtime.valore_giocata;
                
                DashboardApiHelper.Send();
                Runtime.valore_giocata = Config.martingala_array[Runtime.martingala_counter];

                if (old_giocata != Runtime.valore_giocata)
                {
                    fiches_array = Calcs.GetBestCustomFichesAvailable((float)((int)Runtime.valore_giocata)).ToArray();
                    totale_puntata = 0.0;
                    for (int i = 0; i < fiches_array.Length; i++)
                    {
                        totale_puntata += fiches_array[i];  
                    }
                    Runtime.puntata = totale_puntata;
                    
                    _ = Task.Run(async () =>  DashboardApiHelper.Send());
                }
                */
                
                await Bets.DoTheCustomBet(fiches_array);
                return;
                TaskAwaiter taskAwaiter2;
                TaskAwaiter taskAwaiter = taskAwaiter2;
                taskAwaiter2 = default(TaskAwaiter);
                taskAwaiter.GetResult();
            }
            catch (Exception ex)
            {
                Log.PrintInfo(ex.Message);
            }
        }

        private const int STATE_WAIT_INIZIO_TURNO = 0;

        private const int STATE_WAIT_RISULTATO = 1;

        public static void RequestExit() => exit = true;

        private static bool exit;

        private static int state;
    }
}
