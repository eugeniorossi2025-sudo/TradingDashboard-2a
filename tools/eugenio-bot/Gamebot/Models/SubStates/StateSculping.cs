using Gamebot.Helpers;
using Gamebot.Models.MouseMove;
using Gamebot.Models.Objects;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Gamebot.Models.SubStates
{
    internal class StateSculping
    {
        public static void Act()
        {
            StateSculping.exit = false;
            StateSculping.state = 0;
            string winner = string.Empty;
            Runtime.pause_sculping_counter = 0;
            while (!StateSculping.exit && Runtime.runningStateMachineBot)
            {
                int num = StateSculping.state;
                if (num != 0)
                {
                    if (num != 1)
                    {
                        StateSculping.exit = true;
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
                            StateSculping.CheckResult(winner);
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
                            StateSculping.exit = true;
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
                    StateSculping.FaiLaPuntata().Wait();
                    StateSculping.state = 1;
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

        public static void ResetRuntimeInfo()
        {
            Runtime.martingala_persa = false;
            Runtime.first_giocata = false;
            Runtime.last_color = Constants.EnumColorBaccarat.NAY;
            Runtime.martingala_counter = 0;
            if (!Config.modalita_alternata)
            {
                Runtime.chosen_color = Config.start_color;
            }
            Runtime.sculping_profit = 0.0;
            Runtime.last_win = false;
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
                    Runtime.global_profit += Config.martingala_array[Runtime.martingala_counter] * (double)Config.profit_multiplier;
                    Runtime.sculping_profit += Config.martingala_array[Runtime.martingala_counter] * (double)Config.profit_multiplier;
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
                    Runtime.global_profit -= Config.martingala_array[Runtime.martingala_counter];
                    Runtime.sculping_profit -= Config.martingala_array[Runtime.martingala_counter];
                    if (Runtime.martingala_counter < Config.martingala_array.Length)
                    {
                        if (Runtime.martingala_counter + 1 == Config.martingala_array.Length)
                        {
                            Runtime.martingala_persa = true;
                            Runtime.martingala_counter = 0;
                            if (!Config.modalita_alternata)
                            {
                                Runtime.chosen_color = Config.start_color;
                            }
                        }
                        else
                        {
                            Runtime.martingala_counter++;
                        }
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
                    Runtime.global_profit += Config.martingala_array[Runtime.martingala_counter];
                    Runtime.sculping_profit += Config.martingala_array[Runtime.martingala_counter];
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
                    Runtime.global_profit -= Config.martingala_array[Runtime.martingala_counter];
                    Runtime.sculping_profit -= Config.martingala_array[Runtime.martingala_counter];
                    if (Runtime.martingala_counter < Config.martingala_array.Length)
                    {
                        if (Runtime.martingala_counter + 1 == Config.martingala_array.Length)
                        {
                            Runtime.martingala_persa = true;
                            Runtime.martingala_counter = 0;
                            if (!Config.modalita_alternata)
                            {
                                Runtime.chosen_color = Config.start_color;
                            }
                        }
                        else
                        {
                            Runtime.martingala_counter++;
                        }
                    }
                    Runtime.numero_perdite++;
                    Runtime.last_win = false;
                }
            }
            else
            {
                Runtime.last_result = Constants.EnumColorBaccarat.TIE;
                Runtime.last_result_update = DateTime.Now;
            }
            StateSculping.exit = true;
            string haiVinto = (Runtime.last_win ? "SI" : "NO");
            Log.PrintInfo(string.Format("COLORE GIOCATO: {0} | COLORE USCITO: {1} | TU HAI VINTO: {2}", Runtime.chosen_color, Runtime.last_color, haiVinto));
            Log.PrintInfo(string.Format("PROFITTO GLOBALE: {0}€ | PROFITTO LOCALE: {1}€", Runtime.global_profit, Runtime.sculping_profit));
            Log.PrintInfo(string.Format("PROSSIMO INDICE MARTINGALA: {0}", Runtime.martingala_counter));
        }

        private static async Task FaiLaPuntata()
        {
            if (Runtime.start_new_deck)
            {
                Log.PrintInfo("<!> SCULPING | NUOVO MAZZO | GIOCATA RANDOM <!>");
                await StateSculping.GiocataRandomNewDeck();
            }
            else
            {
                await StateSculping.Giocata();
            }
        }

        private static async Task GiocataRandomNewDeck()
        {
            Runtime.puntata = 0;
            if (Runtime.number_deck >= 1 && Runtime.number_deck <= 3)
            {
                Log.PrintInfo("DENTRO GiocataRandomNewDeck()");
                Runtime.start_new_deck = false;
                try
                {
                    var fiches_array = Calcs
                        .GetBestCustomFichesAvailable(CustomFicheWidgetsContainer.getLowestFicheValueAvailable())
                        .ToArray();

                    //_ = Task.Run(async () =>  DashboardApiHelper.Send());
                    
                    await Bets.DoTheCustomBet(fiches_array);
                IL_00C7:
                    Log.PrintInfo(string.Format("GIOCATA RANDOM Waiting New Deck | NumberDeck: {0} | Runtime WaitingNewDeck: {1}", Runtime.number_deck, Runtime.waiting_deck_counter));
                    Runtime.current_state_bot = Constants.EnumStateBot.SCULPING;
                    return;
                    TaskAwaiter taskAwaiter2;
                    TaskAwaiter taskAwaiter = taskAwaiter2;
                    taskAwaiter2 = default(TaskAwaiter);
                    taskAwaiter.GetResult();
                    goto IL_00C7;
                }
                catch (Exception ex)
                {
                    Log.PrintInfo(ex.Message);
                }
            } else
            {
                //_ = Task.Run(async () =>  DashboardApiHelper.Send());
            }
        }

        private static async Task Giocata()
        {
            Runtime.puntata = 0;
            if (Runtime.first_giocata)
            {
                Runtime.first_giocata = false;
            }
            else
            {
                bool cambioColore = false;
                if (Config.cambio_colore > 0)
                {
                    int martingalaCounter = (Runtime.martingala_counter + 1) % Config.cambio_colore;
                    cambioColore = martingalaCounter == 0;
                    Log.PrintInfo(string.Format("DIVISIONE CAMBIO COLORE: {0} | CAMBIO COLORE: {1} | INDICE CAMBIO COLORE: {2} | CONFIG CAMBIO COLORE: {3}", new object[]
                    {
                        martingalaCounter,
                        cambioColore,
                        Runtime.martingala_counter + 1,
                        Config.cambio_colore
                    }));
                }
                bool cambioColorePerMartingala = Config.modalita_alternata && Runtime.martingala_counter == 0;
                if ((cambioColorePerMartingala || cambioColore) && Runtime.last_result != Constants.EnumColorBaccarat.TIE)
                {
                    Log.PrintInfo(string.Format("CAMBIO COLORE -> {0} | INDICE MARTINGALA: {1} | LAST RESULT: {2}", cambioColore, Runtime.martingala_counter, Runtime.last_result));
                    if (Runtime.chosen_color == Constants.EnumColorBaccarat.RED_BANK)
                    {
                        Runtime.chosen_color = Constants.EnumColorBaccarat.BLU_PLAY;
                    }
                    else
                    {
                        Runtime.chosen_color = Constants.EnumColorBaccarat.RED_BANK;
                    }
                }
            }
            
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

        private static bool exit;

        private static int state;
    }
}
