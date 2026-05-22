using Gamebot.Helpers;
using Gamebot.Models.Roulette.Funcs;
using Gamebot.Models.Roulette.MouseMove;
using Gamebot.Models.UI;
using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace Gamebot.Models.Roulette.Logic
{
    public static class SubStateRoulette
    {
        public static void MainCycle()
        {
            if (SubStateRoulette.state != SubStateRoulette.oldState)
            {
                SubStateRoulette.oldState = SubStateRoulette.state;
            }
            else
            {
                SubStateRoulette.oldState = SubStateRoulette.state;
                switch (SubStateRoulette.state)
                {
                    case 0:
                        if (SubStateRoulette.CheckPuoiPuntare())
                        {
                            SubStateRoulette.state = 1;
                        }
                        return;
                    case 1:
                        if (RouletteValues.Runtime.skip_next)
                        {
                            RouletteValues.Runtime.skip_next = false;
                            SubStateRoulette.playedHandLvl = 1;
                            SubStateRoulette.state = 0;
                            Thread.Sleep(12000);
                            return;
                        }
                        SubStateRoulette.FaiLaPuntata();
                        SubStateRoulette.state = 2;
                        return;
                    case 2:
                        if (SubStateRoulette.CheckRisultato())
                        {
                            SubStateRoulette.state = 3;
                        }
                        return;
                    case 3:
                        {
                            int num = SubStateRoulette.getNumFromResult(SubStateRoulette.lastResult);
                            Log.PrintInfo("*********************************************************");
                            Log.PrintInfo("*                                                       *");
                            Log.PrintInfo("*                                                       *");
                            Log.PrintInfo("************* NUMERO RISULTATO : -= " + num.ToString() + " =- ************");
                            Log.PrintInfo("*                                                       *");
                            Log.PrintInfo("*                                                       *");
                            Log.PrintInfo("*********************************************************");
                            if (SubStateRoulette.CheckHaiVinto(num))
                            {
                                SubStateRoulette.SegnaVincita();
                                Log.PrintInfo("************* ABBIAMO VINTO!!!1! ************");
                                SubStateRoulette.playedHandLvl = 1;
                                if (SubStateRoulette.CheckIsStopWin())
                                {
                                    Log.PrintInfo("************* STOP WIN ************");
                                    SubStateRoulette.state = 4;
                                }
                                else
                                {
                                    SubStateRoulette.state = 0;
                                }
                            }
                            else
                            {
                                SubStateRoulette.SegnaPerdita();
                                if (SubStateRoulette.playedHandLvl == 2)
                                {
                                    int arrayIndex = RouletteValues.Constants.nearby_number.IndexOf(num);
                                    try
                                    {
                                        int nextIndex = ((arrayIndex == RouletteValues.Constants.nearby_number.Count - 1) ? 0 : (arrayIndex + 1));
                                        int prevIndex = ((arrayIndex == 0) ? (RouletteValues.Constants.nearby_number.Count - 1) : (arrayIndex - 1));
                                        if (RouletteValues.Runtime.last_number == RouletteValues.Constants.nearby_number[arrayIndex] || RouletteValues.Runtime.last_number == RouletteValues.Constants.nearby_number[nextIndex] || RouletteValues.Runtime.last_number == RouletteValues.Constants.nearby_number[prevIndex])
                                        {
                                            Log.PrintInfo("SKIP TURNO PER NUMERO VICINO");
                                            RouletteValues.Runtime.skip_next = true;
                                            SubStateRoulette.state = 0;
                                            break;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.PrintInfo("RANGE SUPERATO: " + ex.Message);
                                    }
                                }
                                Log.PrintInfo("*************ABBIAMO PERSO :( ************");
                                SubStateRoulette.playedHandLvl++;
                                if (!SubStateRoulette.CheckWithinMartingala())
                                {
                                    UIForm.SendAlert(Constants.EnumAlert.ROULETTE_FINE_MANI_GIOCATE);
                                    Log.PrintInfo("*************OUT OF MARTINGALA :( ************");
                                    SubStateRoulette.playedHandLvl = 1;
                                }
                                if (SubStateRoulette.CheckIsStopLoss())
                                {
                                    Log.PrintInfo("************* STOP LOSS ************");
                                    SubStateRoulette.state = 5;
                                }
                                else
                                {
                                    SubStateRoulette.state = 0;
                                }
                            }
                            RouletteValues.Runtime.last_number = num;
                            return;
                        }
                    case 4:
                        UIForm.SendAlert(Constants.EnumAlert.GLOBAL_STOP_WIN);
                        SubStateRoulette.state = 6;
                        return;
                    case 5:
                        UIForm.SendAlert(Constants.EnumAlert.GLOBAL_STOP_LOSS);
                        SubStateRoulette.state = 6;
                        return;
                    case 6:
                        SubStateRoulette.SpegniTutto();
                        return;
                    default:
                        return;
                }
            }
        }

        public static bool CheckPuoiPuntare()
        {
            if (SubStateRoulette.centralLabelContainsPUNTARE())
            {
                Log.PrintInfo("************* (R) LETTO PUNTARE ************");
                return true;
            }
            return false;
        }

        private static bool centralLabelContainsPUNTARE()
        {
            return RouletteValues.OCReads.label_winner.Contains(Config.textAreaPuntare, StringComparison.OrdinalIgnoreCase);
        }

        public static async void FaiLaPuntata()
        {
            if (SubStateRoulette.playedHandLvl == 2 || SubStateRoulette.playedHandLvl == 3)
            {
                UIForm.SendAlert(Constants.EnumAlert.ROULETTE_MANO_GIOCATA_PERSA);
            }
            await RouletteBets.DoTheBet(SubStateRoulette.playedHandLvl);
        }

        public static bool CheckRisultato()
        {
            SubStateRoulette.lastResult = RouletteValues.OCReads.label_winner;
            if (SubStateRoulette.centralLabelContainsROSSO(SubStateRoulette.lastResult))
            {
                Log.PrintInfo("************* LETTO RISULTATO : ROSSO ************");
                return true;
            }
            if (SubStateRoulette.centralLabelContainsNERO(SubStateRoulette.lastResult))
            {
                Log.PrintInfo("************* LETTO RISULTATO : NERO ************");
                return true;
            }
            if (SubStateRoulette.centralLabelContainsVERDE(SubStateRoulette.lastResult))
            {
                Log.PrintInfo("************* LETTO RISULTATO : VERDE ************");
                return true;
            }
            return false;
        }

        private static bool centralLabelContainsROSSO(string s)
        {
            return s.Contains("ROSSO", StringComparison.OrdinalIgnoreCase);
        }

        private static bool centralLabelContainsNERO(string s)
        {
            return s.Contains("NERO", StringComparison.OrdinalIgnoreCase);
        }

        private static bool centralLabelContainsVERDE(string s)
        {
            return s.Contains("VERDE", StringComparison.OrdinalIgnoreCase);
        }

        private static int getNumFromResult(string lastResult)
        {
            if (lastResult.Contains("VERDE"))
            {
                return 0;
            }
            int num;
            try
            {
                num = int.Parse(Regex.Match(lastResult, "\\d+").Value);
            }
            catch (Exception)
            {
                Log.PrintInfo("F9 baby");
                num = 9;
            }
            return num;
        }

        public static bool CheckHaiVinto(int num)
        {
            return Roulette.Instance.CheckForNumberPresence(SubStateRoulette.playedHandLvl, num);
        }

        public static bool CheckIsStopWin()
        {
            Console.WriteLine("(R) CHECKING STOP WIN");
            Console.WriteLine("Global Profit : " + RouletteValues.Runtime.global_profit.ToString());
            Console.WriteLine("  Stop   Win  : " + RouletteValues.Config.stop_win.ToString());
            return RouletteValues.Runtime.global_profit > (float)RouletteValues.Config.stop_win;
        }

        public static bool CheckIsStopLoss()
        {
            Console.WriteLine("(R) CHECKING STOP LOSS");
            Console.WriteLine("Global Profit : " + RouletteValues.Runtime.global_profit.ToString());
            Console.WriteLine("  Stop  Loss  : " + RouletteValues.Config.stop_loss.ToString());
            return RouletteValues.Runtime.global_profit < -(float)RouletteValues.Config.stop_loss;
        }

        public static bool CheckWithinMartingala()
        {
            return Roulette.Instance.WithinBoundsOfMartingala(SubStateRoulette.playedHandLvl - 1);
        }

        private static void SegnaVincita()
        {
            RouletteValues.Runtime.numero_vincite++;
            float value = 0f;
            if (SubStateRoulette.playedHandLvl == 1)
            {
                value = (float)RouletteValues.Config.hand_value_1;
            }
            else if (SubStateRoulette.playedHandLvl == 2)
            {
                value = (float)RouletteValues.Config.hand_value_2;
            }
            else if (SubStateRoulette.playedHandLvl == 3)
            {
                value = (float)RouletteValues.Config.hand_value_3;
            }
            float netWin = value / (float)Roulette.Instance.GetNumOfNumbers(SubStateRoulette.playedHandLvl) * 36f - value;
            Log.PrintInfo("**** PROFITTO VINCITA: " + netWin.ToString() + " ****");
            RouletteValues.Runtime.global_profit += netWin;
            RouletteValues.Runtime.balance = RouletteValues.Runtime.balanceInit + RouletteValues.Runtime.global_profit;
            MainStateRoulette.UpdateForm();
        }

        private static void SegnaPerdita()
        {
            RouletteValues.Runtime.numero_perdite++;
            float value = 0f;
            if (SubStateRoulette.playedHandLvl == 1)
            {
                value = (float)RouletteValues.Config.hand_value_1;
            }
            else if (SubStateRoulette.playedHandLvl == 2)
            {
                value = (float)RouletteValues.Config.hand_value_2;
            }
            else if (SubStateRoulette.playedHandLvl == 3)
            {
                value = (float)RouletteValues.Config.hand_value_3;
            }
            Log.PrintInfo("**** PROFITTO PERDITA: " + value.ToString() + " ****");
            RouletteValues.Runtime.global_profit -= value;
            RouletteValues.Runtime.balance = RouletteValues.Runtime.balanceInit + RouletteValues.Runtime.global_profit;
            MainStateRoulette.UpdateForm();
        }

        public static void SpegniTutto()
        {
            RouletteValues.Runtime.current_state_bot = RouletteValues.Constants.EnumStateBot.END_DECK;
            RouletteTask.Instance.StopGameBot();
        }

        public static string GetState(int s)
        {
            string stateString;
            switch (s)
            {
                case 0:
                    stateString = "WAIT INIZIO TURNO";
                    break;
                case 1:
                    stateString = "FAI LA PUNTATA";
                    break;
                case 2:
                    stateString = "WAIT RISULTATO";
                    break;
                case 3:
                    stateString = "EVAL RISULTATO";
                    break;
                default:
                    stateString = "UNKNOWN";
                    break;
            }
            return stateString;
        }

        private static int FakePerdita(int num)
        {
            int i = new Random().Next(0, 1);
            Log.PrintInfo("FakePerdita | Random: " + i.ToString());
            switch (num)
            {
                case 0:
                    if (i != 0)
                    {
                        return 32;
                    }
                    return 26;
                case 1:
                    if (i != 0)
                    {
                        return 33;
                    }
                    return 20;
                case 2:
                    if (i != 0)
                    {
                        return 21;
                    }
                    return 25;
                case 3:
                    if (i != 0)
                    {
                        return 26;
                    }
                    return 35;
                case 4:
                    if (i != 0)
                    {
                        return 19;
                    }
                    return 21;
                case 5:
                    if (i != 0)
                    {
                        return 24;
                    }
                    return 10;
                case 6:
                    if (i != 0)
                    {
                        return 34;
                    }
                    return 27;
                case 7:
                    if (i != 0)
                    {
                        return 28;
                    }
                    return 29;
                case 8:
                    if (i != 0)
                    {
                        return 30;
                    }
                    return 23;
                case 9:
                    if (i != 0)
                    {
                        return 22;
                    }
                    return 31;
                case 10:
                    if (i != 0)
                    {
                        return 23;
                    }
                    return 5;
                case 11:
                    if (i != 0)
                    {
                        return 36;
                    }
                    return 30;
                case 12:
                    if (i != 0)
                    {
                        return 35;
                    }
                    return 28;
                case 13:
                    if (i != 0)
                    {
                        return 27;
                    }
                    return 36;
                case 14:
                    if (i != 0)
                    {
                        return 31;
                    }
                    return 20;
                case 15:
                    if (i != 0)
                    {
                        return 32;
                    }
                    return 19;
                case 16:
                    if (i != 0)
                    {
                        return 33;
                    }
                    return 24;
                case 17:
                    if (i != 0)
                    {
                        return 25;
                    }
                    return 34;
                case 18:
                    if (i != 0)
                    {
                        return 29;
                    }
                    return 22;
                case 19:
                    if (i != 0)
                    {
                        return 15;
                    }
                    return 4;
                case 20:
                    if (i != 0)
                    {
                        return 14;
                    }
                    return 1;
                case 21:
                    if (i != 0)
                    {
                        return 4;
                    }
                    return 2;
                case 22:
                    if (i != 0)
                    {
                        return 18;
                    }
                    return 9;
                case 23:
                    if (i != 0)
                    {
                        return 8;
                    }
                    return 10;
                case 24:
                    if (i != 0)
                    {
                        return 16;
                    }
                    return 5;
                case 25:
                    if (i != 0)
                    {
                        return 2;
                    }
                    return 17;
                case 26:
                    if (i != 0)
                    {
                        return 0;
                    }
                    return 3;
                case 27:
                    if (i != 0)
                    {
                        return 6;
                    }
                    return 13;
                case 28:
                    if (i != 0)
                    {
                        return 12;
                    }
                    return 7;
                case 29:
                    if (i != 0)
                    {
                        return 7;
                    }
                    return 18;
                case 30:
                    if (i != 0)
                    {
                        return 11;
                    }
                    return 8;
                case 31:
                    if (i != 0)
                    {
                        return 9;
                    }
                    return 14;
                case 32:
                    if (i != 0)
                    {
                        return 0;
                    }
                    return 15;
                case 33:
                    if (i != 0)
                    {
                        return 1;
                    }
                    return 16;
                case 34:
                    if (i != 0)
                    {
                        return 17;
                    }
                    return 6;
                case 35:
                    if (i != 0)
                    {
                        return 3;
                    }
                    return 12;
                case 36:
                    if (i != 0)
                    {
                        return 13;
                    }
                    return 11;
                default:
                    return -1;
            }
        }

        public const int STATE_WAIT_INIZIO_TURNO = 0;

        public const int STATE_FAI_LA_PUNTATA = 1;

        public const int STATE_WAIT_RISULTATO = 2;

        public const int STATE_EVAL_RISULTATO = 3;

        public const int STATE_STOP_WIN = 4;

        public const int STATE_STOP_LOSS = 5;

        public const int STATE_POST_STOP_IDLE = 6;

        private static string lastResult = string.Empty;

        public static int playedHandLvl = 1;

        private static bool exit = false;

        public static int state = 0;

        private static int oldState = 0;
    }
}
