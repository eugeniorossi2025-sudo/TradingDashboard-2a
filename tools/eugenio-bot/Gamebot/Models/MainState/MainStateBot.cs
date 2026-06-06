using Gamebot.Communication.Firebase;
using Gamebot.Helpers;
using Gamebot.Models.Objects;
using Gamebot.Models.SubStates;
using Gamebot.Models.UI;
using Gamebot.UI.WindowForm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gamebot.Models.MainState
{
    internal class MainStateBot
    {
        private static bool IsPastEndDeckLimit()
        {
            return Runtime.number_deck >= Config.limitEndDeck;
        }

        public static void UpdateForm()
        {
            List<string> values = new List<string>();
            values.Clear();
            if (UpdateInterface.GetInstanceForm().progressUI != null)
            {
                values.Add(Number.FormatNumberDecimalEuro(Runtime.global_profit) ?? "");
                values.Add(Number.FormatNumberDecimalEuro(Runtime.sculping_profit) ?? "");
                values.Add($"{Runtime.numero_vincite}");
                values.Add($"{Runtime.numero_perdite}");
                UpdateInterface.GetInstanceForm().progressUI.Report(values);
            }
            UIForm.SetStatusBot();
        }

        public static void UpdateTimeElapsed()
        {
            List<string> values = new List<string>();
            values.Clear();
            if (UpdateInterface.GetInstanceForm().progressTimeElapsed != null && Runtime.runningStateMachineBot)
            {
                UpdateInterface.GetInstanceForm().progressTimeElapsed.Report(values);
            }
        }

        public static async Task<bool> CheckConnection()
        { return true;
            //return await FirestoreHelpers.CheckConnectionEnabled();
        }

        public static void updateAll()
        {
            UpdateNumberDeck();
            UpdateChangeColor();
            UpdateBalance();
            UpdateForm();
        }

        public static void StateMachine() 
        {
            updateAll();
            if (Runtime.martingala_counter + 1 == Config.index_alarm)
            {
                UIForm.SendAlert(Constants.EnumAlert.INDEX_ALARM);
            }
            Log.PrintInfo($"STATO BOT: {Runtime.current_state_bot} | DECK OCR: {OCReads.number_deck} | DECK CALCOLATO: {Runtime.number_deck}");
            Log.PrintInfo("ARRAY PAUSE SCULPING: " + string.Join(",", Runtime.color_pause_scalping_array));
            switch (Runtime.current_state_bot)
            {
                case Constants.EnumStateBot.IDLE:
                    CheckEnabled();
                    break;
                case Constants.EnumStateBot.FIRST_PLAY:
                    CheckEnabled();
                    if (IsPastEndDeckLimit())
                    {
                        Log.PrintInfo($"<!> FIRST_PLAY | MANO {Runtime.number_deck} OLTRE LIMITE {Config.limitEndDeck} | SALTO SCULPING → END_DECK <!>");
                        Runtime.current_state_bot = Constants.EnumStateBot.END_DECK;
                        break;
                    }
                    Log.PrintInfo("!!! FIRST_PLAY: ACTION !!!");
                    StateFirstPlay.Act();
                    if (Runtime.last_color != Config.start_color && Runtime.last_result != Constants.EnumColorBaccarat.TIE)
                    {
                        Runtime.current_state_bot = Constants.EnumStateBot.SCULPING;
                        Runtime.chosen_color = Config.start_color;
                        Log.PrintInfo($"<!> FIRST_PLAY | COLORE IMPOSTATO: {Runtime.chosen_color} <!>");
                    }
                    break;
                case Constants.EnumStateBot.SCULPING:
                case Constants.EnumStateBot.NEW_DECK:
                    CheckEnabled();
                    Log.PrintInfo("!!! SCULPING: ACTION !!!");
                    if (Runtime.first_giocata)
                    {
                        UIForm.SendAlert(Constants.EnumAlert.START_SCULPING);
                    }
                    if (Runtime.start_new_deck)
                    {
                        UIForm.SendAlert(Constants.EnumAlert.NEW_DECK);
                    }
                    StateSculping.Act();
                    if (Runtime.global_profit > (double)(float)Config.global_stop_win)
                    {
                        Log.PrintInfo("<!> SCULPING | RAGGIUNTO GLOBAL_STOP_WIN <!>");
                        Runtime.current_state_bot = Constants.EnumStateBot.GLOBAL_STOP_WIN;
                    }
                    else if (Runtime.global_profit < (double)(0f - (float)Config.global_stop_loss))
                    {
                        Log.PrintInfo("<!> SCULPING | RAGGIUNTO GLOBAL_STOP_LOSS <!>");
                        Runtime.current_state_bot = Constants.EnumStateBot.GLOBAL_STOP_LOSS;
                    }
                    else if (Runtime.number_deck >= Config.limitEndDeck)
                    {
                        Runtime.current_state_bot = Constants.EnumStateBot.END_DECK;
                    }
                    else if (Runtime.sculping_profit >= (double)(float)(Config.sculping_stop_win * (Config.safe_win / 100m)))
                    {
                        Runtime.current_state_bot = Constants.EnumStateBot.SAFE_WIN;
                    }
                    else if (Runtime.martingala_persa)
                    {
                        if (Config.modalita_alternata)
                        {
                            ChangeColorMartingalaPersa();
                        }
                        if (Config.skipPostSculping)
                        {
                            Log.PrintInfo("<!> SCULPING | MARTINGALA PERSA | RIPRENDO A GIOCARE <!>");
                            StateSculping.ResetRuntimeInfo();
                            Runtime.current_state_bot = Constants.EnumStateBot.SCULPING;
                        }
                        else
                        {
                            Log.PrintInfo("<!> SCULPING | MARTINGALA PERSA | VADO IN PAUSE SCULPING <!>");
                            Runtime.current_state_bot = Constants.EnumStateBot.PAUSE_SCALPING;
                        }
                        UIForm.SendAlert(Constants.EnumAlert.MARTINGALA_PERSA);
                    }
                    break;
                case Constants.EnumStateBot.SAFE_WIN:
                    CheckEnabled();
                    Runtime.ErasePauseScalpingArray();
                    Runtime.color_pause_scalping_array[0] = Runtime.chosen_color;
                    if (!Config.safe_win_enable)
                    {
                        Log.PrintInfo("<!> SAFE_WIN | SKIP SAFE WIN <!>");
                        UIForm.SendAlert(Constants.EnumAlert.END_SCULPING);
                        Runtime.current_state_bot = Constants.EnumStateBot.PAUSE_SCALPING;
                        break;
                    }
                    Log.PrintInfo("!!! SAFE_WIN: ACTION !!!");
                    StateSafeWin.Act();
                    if (Runtime.last_win)
                    {
                        if (Runtime.global_profit > (double)(float)Config.global_stop_win)
                        {
                            Log.PrintInfo("<!> SAFE_WIN | RAGGIUNTO GLOBAL_STOP_WIN <!>");
                            Runtime.current_state_bot = Constants.EnumStateBot.GLOBAL_STOP_WIN;
                        }
                        else if (Runtime.global_profit < (double)(0f - (float)Config.global_stop_loss))
                        {
                            Log.PrintInfo("<!> SAFE_WIN | RAGGIUNTO GLOBAL_STOP_LOSS <!>");
                            Runtime.current_state_bot = Constants.EnumStateBot.GLOBAL_STOP_LOSS;
                        }
                    }
                    else
                    {
                        Runtime.current_state_bot = Constants.EnumStateBot.PAUSE_SCALPING;
                        UIForm.SendAlert(Constants.EnumAlert.END_SCULPING);
                    }
                    break;
                case Constants.EnumStateBot.PAUSE_SCALPING:
                    {
                        CheckEnabled();
                        Log.PrintInfo("!!! PAUSE_SCULPING: ACTION !!!");
                        if (Runtime.global_profit > (double)(float)Config.global_stop_win)
                        {
                            Log.PrintInfo("<!> PAUSE_SCULPING | RAGGIUNTO GLOBAL_STOP_WIN <!>");
                            Runtime.current_state_bot = Constants.EnumStateBot.GLOBAL_STOP_WIN;
                            Runtime.ErasePauseScalpingArray();
                            break;
                        }
                        if (Runtime.global_profit < (double)(0f - (float)Config.global_stop_loss))
                        {
                            Log.PrintInfo("<!> PAUSE_SCULPING | RAGGIUNTO GLOBAL_STOP_LOSS <!>");
                            Runtime.current_state_bot = Constants.EnumStateBot.GLOBAL_STOP_LOSS;
                            Runtime.ErasePauseScalpingArray();
                            break;
                        }
                        if (Runtime.number_deck >= Config.limitEndDeck)
                        {
                            Runtime.current_state_bot = Constants.EnumStateBot.END_DECK;
                            Runtime.ErasePauseScalpingArray();
                            break;
                        }
                        StatePauseSculping.Act();
                        int indexArrayNay = StatePauseSculping.GetEmptyIndex(Runtime.color_pause_scalping_array);
                        Log.PrintInfo(string.Format("<!> PAUSE_SCULPING | INDICE ARRAY NAY: {0} | ARRAY: {1} <!>", indexArrayNay, string.Join(",", Runtime.color_pause_scalping_array)));
                        if (indexArrayNay < 0)
                        {
                            Runtime.current_state_bot = Constants.EnumStateBot.SCULPING;
                            if (Config.modalita_alternata)
                            {
                                Runtime.chosen_color = Runtime.color_pause_scalping_array[0];
                            }
                            Runtime.ErasePauseScalpingArray();
                            Log.PrintInfo($"<!> PAUSE_SCULPING | COLORE USCITO: {Runtime.last_color} | COLORE DA GIOCARE: {Runtime.chosen_color} | GIOCHERAI PROSSIMA MANO: SI <!>");
                        }
                        else
                        {
                            if (Config.modalita_alternata)
                            {
                                Runtime.chosen_color = Runtime.color_pause_scalping_array[0];
                            }
                            Log.PrintInfo($"<!> PAUSE_SCULPING | COLORE USCITO: {Runtime.last_color} | COLORE DA GIOCARE: {Runtime.chosen_color} | GIOCHERAI PROSSIMA MANO: NO <!>");
                        }
                        break;
                    }
                case Constants.EnumStateBot.END_DECK:
                    CheckEnabled();
                    Log.PrintInfo("!!! END_DECK: ACTION !!!");
                    Log.PrintInfo($"<!> END_DECK | NUMERO END DECK: {Config.limitEndDeck} <!>");
                    if (Runtime.global_profit > (double)(float)Config.global_stop_win)
                    {
                        Log.PrintInfo("<!> END_DECK | RAGGIUNTO GLOBAL_STOP_WIN <!>");
                        Runtime.current_state_bot = Constants.EnumStateBot.GLOBAL_STOP_WIN;
                        break;
                    }
                    if (Runtime.global_profit < (double)(0f - (float)Config.global_stop_loss))
                    {
                        Log.PrintInfo("<!> END_DECK | RAGGIUNTO GLOBAL_STOP_LOSS <!>");
                        Runtime.current_state_bot = Constants.EnumStateBot.GLOBAL_STOP_LOSS;
                        break;
                    }
                    StateFineMazzo.Act();
                    if (Runtime.martingala_counter == 0)
                    {
                        Runtime.current_state_bot = Constants.EnumStateBot.WAITING_NEW_DECK;
                        UIForm.SendAlert(Constants.EnumAlert.WAITING_NEW_DECK);
                    }
                    else if (Runtime.martingala_persa)
                    {
                        if (Config.modalita_alternata)
                        {
                            ChangeColorMartingalaPersa();
                        }
                        Runtime.current_state_bot = Constants.EnumStateBot.WAITING_NEW_DECK;
                        UIForm.SendAlert(Constants.EnumAlert.MARTINGALA_PERSA_FINE_MAZZO);
                    }
                    break;
                case Constants.EnumStateBot.WAITING_NEW_DECK:
                    CheckEnabled();
                    Log.PrintInfo("!!! WAITING_NEW_DECK: ACTION !!!");
                    if (Runtime.number_deck >= Constants.LIMIT_MIN_NEW_DECK
                        && Runtime.number_deck <= Constants.LIMIT_MAX_NEW_DECK)
                    {
                        UIForm.SendAlert(Constants.EnumAlert.WAITING_TO_START_SCALPING);
                        Runtime.chosen_color = Config.start_color;
                        Log.PrintInfo($"<!> WAITING_NEW_DECK | MAZZO NUOVO ({Runtime.number_deck}) | COLORE DA GIOCARE: {Runtime.chosen_color} <!>");
                        if (Config.skipPostSculping)
                        {
                            Log.PrintInfo("<!> WAITING_NEW_DECK | RICOMINCIO MAZZO E GIOCATA | VADO IN SCULPING | RIPRENDO A GIOCARE <!>");
                            Runtime.martingala_persa = false;
                            Runtime.start_new_deck = true;
                            Runtime.current_state_bot = Constants.EnumStateBot.NEW_DECK;
                        }
                        else
                        {
                            Log.PrintInfo("<!> WAITING_NEW_DECK | VADO IN PAUSE SCULPING | ATTENDO A GIOCARE <!>");
                            Runtime.current_state_bot = Constants.EnumStateBot.PAUSE_SCALPING;
                        }
                        Runtime.ErasePauseScalpingArray();
                        Runtime.color_pause_scalping_array[0] = Runtime.chosen_color;
                    }
                    else
                    {
                        if (Runtime.number_deck == 0)
                        {
                            Log.PrintInfo("<!> WAITING_NEW_DECK | MAZZO 0 | PROBE ROSSA MINIMA (attesa passaggio a mazzo 1+) <!>");
                        }
                        StateAttendiNuovoMazzo.Act();
                    }
                    break;
                case Constants.EnumStateBot.GLOBAL_STOP_WIN:
                    CheckEnabled();
                    Log.PrintInfo("!!! GLOBAL_STOP_WIN: ACTION !!!");
                    UIForm.SendAlert(Constants.EnumAlert.GLOBAL_STOP_WIN);
                    Runtime.current_state_bot = Constants.EnumStateBot.IDLE;
                    UpdateForm();
                    WorkerTask.Instance.StopGameBot();
                    break;
                case Constants.EnumStateBot.GLOBAL_STOP_LOSS:
                    CheckEnabled();
                    Log.PrintInfo("!!! GLOBAL_STOP_LOSS: ACTION !!!");
                    UIForm.SendAlert(Constants.EnumAlert.GLOBAL_STOP_LOSS);
                    Runtime.current_state_bot = Constants.EnumStateBot.IDLE;
                    UpdateForm();
                    WorkerTask.Instance.StopGameBot();
                    break;
            }
        }

        private static void ChangeColorMartingalaPersa()
        {
            if (Runtime.chosen_color == Constants.EnumColorBaccarat.BLU_PLAY)
            {
                Runtime.chosen_color = Constants.EnumColorBaccarat.RED_BANK;
            }
            else if (Runtime.chosen_color == Constants.EnumColorBaccarat.RED_BANK)
            {
                Runtime.chosen_color = Constants.EnumColorBaccarat.BLU_PLAY;
            }
            else
            {
                Runtime.chosen_color = Config.start_color;
            }
            Log.PrintInfo($"MARTINGALA PERSA | PROSSIMO COLORE DA GIOCARE: {Runtime.chosen_color}");
        }

        private static async void CheckEnabled()
        {
            Configuratore fm = UpdateInterface.GetInstanceForm();
            if (!(await CheckConnection()))
            {
                Runtime.current_state_bot = Constants.EnumStateBot.IDLE;
                WorkerTask.Instance.KillBot(fm);
                WorkerTask.Instance.StopGameBot();
                MessageBox.Show("Impossibile raggiungere il server di autenticazione.\nControllare la connessione ad internet.\nSe il problema persiste contattare l’assistenza.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        public static void fixMissingNuber(int value, double similarity)
        {
            if (value != 0 && Runtime.number_deck != -1 && Math.Abs(Runtime.number_deck - value) >= 3 &&  similarity < 0.8)
            {
                Runtime.number_deck++;
                if (Runtime.number_deck > 80) Runtime.number_deck = 0;
            }
            else
            {
                Runtime.number_deck = value;
            }
        }

        public static void UpdateNumberDeck()
        {
            DateTime temp = DateTime.Now;
            int deckTmp = OCReads.number_deck;
            double similarity = OCRResponse.Instance.Similarity;
            
            if (deckTmp == -1)
            {
                while (deckTmp == -1 && !Check.centralLabelContainsPUNTARE())
                {
                    deckTmp  = OCReads.number_deck;
                    similarity = OCRResponse.Instance.Similarity;
                }
                if (deckTmp == -1)
                {
                    Runtime.number_deck++;
                    if (Runtime.number_deck > 80) Runtime.number_deck = 0;
                    return;
                }
            }
            if (deckTmp >= 55)
            {
                while (deckTmp >= 55 && !Check.centralLabelContainsPUNTARE() &&
                       DateTime.Now - temp < TimeSpan.FromSeconds(20))
                {
                    deckTmp  = OCReads.number_deck;
                    similarity = OCRResponse.Instance.Similarity;
                }
            }
            
            fixMissingNuber(deckTmp, similarity);
            return;
            
            int tempNumberDeck = 0;
            Log.PrintInfo("INIZIO AGGIORNAMENTO NUMERO DECK");
            if (Runtime.number_deck == 0)
            {
                Runtime.number_deck = tempNumberDeck;
                Log.PrintInfo("ENTRO IN Runtime.number_deck == 0");
                Log.PrintInfo($"RUNTIME DECK: {Runtime.number_deck}");
                if (tempNumberDeck <= -1)
                {
                    Log.PrintInfo("tempNumberDeck: -1");
                    Runtime.number_deck = 0;
                }
                if (Runtime.current_state_bot == Constants.EnumStateBot.WAITING_NEW_DECK)
                {
                    Log.PrintInfo("Runtime.current_state_bot E' WAITING NEW DECK");
                    Runtime.number_deck = 0;
                }
            }
            else
            {
                Runtime.number_deck++;
                Log.PrintInfo("ENTRO IN Runtime.number_deck <> 0");
                Log.PrintInfo($"INCREMENTO Runtime.number_deck: {Runtime.number_deck}");
                Log.PrintInfo($"tempNumberDeck: {tempNumberDeck}");
                if (tempNumberDeck >= 0)
                {
                    Log.PrintInfo("tempNumberDeck > 0");
                    if (tempNumberDeck <= 9)
                    {
                        Runtime.number_deck = tempNumberDeck;
                        Log.PrintInfo($"tempNumberDeck <= 9: {tempNumberDeck}");
                    }
                    else
                    {
                        Log.PrintInfo($"Runtime.number_deck: {Runtime.number_deck}");
                        if (Runtime.number_deck >= 10)
                        {
                            Log.PrintInfo("Runtime.number_deck >= 10");
                            if (Math.Abs(tempNumberDeck - Runtime.number_deck) <= 1)
                            {
                                Log.PrintInfo("EGUAGLIO LETTURA OCR a Runtime.number_deck");
                                Runtime.number_deck = tempNumberDeck;
                            }
                        }
                        else
                        {
                            Log.PrintInfo("Runtime.number_deck < 10");
                            Log.PrintInfo("EGUAGLIO LETTURA OCR a Runtime.number_deck");
                            Runtime.number_deck = tempNumberDeck;
                        }
                    }
                }
                else
                {
                    Log.PrintInfo("tempNumberDeck < 0");
                }
            }
            Log.PrintInfo($"<!> OCREADS NUMBER_DECK: {OCReads.number_deck} | RUNTIME NUMBER_DECK: {Runtime.number_deck} | STATO: {Runtime.current_state_bot} <!>");
        }

        private static void UpdateBalance()
        {
            Runtime.balance = Runtime.balanceInit + Runtime.global_profit;
        }

        private static void UpdateChangeColor()
        {
            bool martingalaExist = false;
            int order = -1;
            int numberDeck = Runtime.number_deck;
            MartingalaInfoItem martingalaInfo = Config.MartingalaOptions.Where((MartingalaInfoItem item) => item.StartDeck <= numberDeck && item.EndDeck >= numberDeck).FirstOrDefault();
            if (martingalaInfo == null)
            {
                Config.cambio_colore = 0;
                Config.index_alarm = 0;
            }
            else
            {
                martingalaExist = true;
                order = martingalaInfo.Order;
                Config.cambio_colore = martingalaInfo.ChangeIndex;
                Config.index_alarm = martingalaInfo.AlarmIndex;
            }
            Log.PrintInfo($"INFO MARTINGALA | MARTINGALA #: {order} | ESISTE: {martingalaExist} | INDICE CAMBIO COLORE: {Config.cambio_colore} | INDICE ALLARME: {Config.index_alarm} | NUMBER_DECK: {Runtime.number_deck}");
        }
    }
}
