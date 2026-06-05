using Gamebot.Models;
using Gamebot.Models.MainState;
using Gamebot.Models.SubStates;
using Gamebot.Models.Roulette;
using Gamebot.Models.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gamebot.Models.Interfaces;
using Gamebot.UI.WindowForm;

namespace Gamebot.Helpers
{
    public class ProfitResponse
    {
        public decimal Margine { get; set; }
        public decimal SaldoIniziale { get; set; }
    }

    public static class DashboardApiHelper
    {
        private static readonly RequestApi apiCaller = new RequestApi();

        private static int lastDeck = -1;
        private static DateTime lastUpdate = DateTime.Now;
        private static int zeroCalls = 0;

        public static void LoadGlobalProfit()
        {
            try
            {
                Configuratore form = UpdateInterface.GetInstanceForm();

                var parametri = new Dictionary<string, string>
                {
                    { "USERNAME", form.username },
                    { "PASSWORD", form.password },
                    { "COMPUTER", form.computer }
                };

                string endpoint = form.url + "/api/proactive/get-global-profit";

                using var client = new HttpClient();
                var content = new FormUrlEncodedContent(parametri);

                var response = client.PostAsync(endpoint, content).Result;
                string risposta = response.Content.ReadAsStringAsync().Result;

                var data = JsonSerializer.Deserialize<ProfitResponse>(risposta);
                double saldoIniziale = decimal.ToDouble(
                    Math.Round(data.SaldoIniziale, 2, MidpointRounding.AwayFromZero)
                );

                
                double margine = decimal.ToDouble(
                    Math.Round(data.Margine, 2, MidpointRounding.AwayFromZero)
                );
                
                if (saldoIniziale > 0) 
                {
                    Runtime.global_profit = margine;
                    Runtime.balanceInit = saldoIniziale;
                    Runtime.balance = Runtime.balanceInit + Runtime.global_profit;
                }
                
            }
            catch
            {
                Runtime.global_profit = 0.0;
            }
        }



public static void SendSimple()
{
    try
    {
        Configuratore form = UpdateInterface.GetInstanceForm();

        DateTime startMethod = DateTime.Now;

        // -----------------------------
        // LETTURA STATO DAL FORM (uguale al tuo Send)
        // -----------------------------
        Runtime.balance = Runtime.balanceInit + Runtime.global_profit;

        string saldoIstantaneo = UIForm.PrintTotalBalance()
            .Replace("Saldo: ", "")
            .Replace("€", "")
            .Trim()
            .Replace(",", ".");

        string saldoIniziale = form.balanceStartValue.Value
            .ToString()
            .Replace(",", ".");

        saldoIniziale = Runtime.balanceInit.ToString().Replace(",", ".");
        saldoIstantaneo = Runtime.balance.ToString().Replace(",", ".");

        var chosen_color = Runtime.old_chosen_color;

        string mazzo = Runtime.number_deck.ToString();
        string margine = Runtime.global_profit.ToString();
        string colpoMartingala = Runtime.martingala_counter.ToString();
        string tempo = form.timeElapsedValueToChange.Text;
        string valoreGiocato = Runtime.puntata.ToString();
        string stato = Runtime.labelTextCurrentState
            .Replace("Stato Bot:", "")
            .Trim();

        // -----------------------------
        // CALCOLO ORE DA TEMPO (facoltativo)
        // -----------------------------
        string ore = "";
        if (!string.IsNullOrEmpty(tempo) && tempo.Contains(":"))
        {
            var parts = tempo.Split(':');
            if (parts.Length >= 2 &&
                int.TryParse(parts[0], out int h) &&
                int.TryParse(parts[1], out int m))
            {
                ore = ((h * 60 + m) / 60.0m)
                    .ToString()
                    .Replace(",", ".");
            }
        }
        
        if (mazzo == "-1")
        {
            mazzo = OCReads.number_deck.ToString();
        }

        // -----------------------------
        // COSTRUZIONE PARAMETRI
        // -----------------------------
        var parametri = new Dictionary<string, string>
        {
            { "USERNAME", form.username },
            { "PASSWORD", form.password },
            { "COMPUTER", form.computer },

            { "TAVOLO", form.tavolo },
            { "SALDO_INIZIALE", saldoIniziale },
            { "SALDO_ISTANTANEO", saldoIstantaneo },
            { "MARGINE", margine },
            { "VALORE_GIOCATO", valoreGiocato },
            { "COLPO_MARTINGALA", colpoMartingala },
            { "STATO", stato },
            { "MAZZO", mazzo },
            { "TEMPO", tempo },
            { "CHOSEN_COLOR", chosen_color }
        };

        Logger.WriteLog("SendSimple - " + JsonSerializer.Serialize(parametri));

        DateTime start = DateTime.Now;

        // -----------------------------
        // NUOVA CHIAMATA POST NATIVA (.NET 4.7.2)
        // -----------------------------
        string endpoint = form.url + "/api/proactive/update-params";

        using (var client = new HttpClient())
        {
            client.Timeout = TimeSpan.FromSeconds(30);

            var content = new FormUrlEncodedContent(parametri);

            HttpResponseMessage response = client
                .PostAsync(endpoint, content)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();

            string risposta = response.Content
                .ReadAsStringAsync()
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();

            Logger.WriteLog("SendSimple OK - ms:" +
                (int)DateTime.Now.Subtract(start).TotalMilliseconds);

            Logger.WriteLog("SendSimple response: " + risposta);
        }

        Logger.WriteLog("SendSimple totale metodo - ms:" +
            (int)DateTime.Now.Subtract(startMethod).TotalMilliseconds);
    }
    catch (Exception ex)
    {
        Logger.WriteLog("Errore SendSimple: " + ex.ToString());
    }
}

public static void SendDeck()
{
    try
    {
        Configuratore form = UpdateInterface.GetInstanceForm();

        DateTime startMethod = DateTime.Now;

        // -----------------------------
        // LETTURA DATI NECESSARI
        // -----------------------------

        string mazzo = Runtime.number_deck.ToString();
        if (mazzo == "-1")
        {
            mazzo = OCReads.number_deck.ToString();
        }

        // -----------------------------
        // COSTRUZIONE PARAMETRI POST
        // -----------------------------
        var parametri = new Dictionary<string, string>
        {
            { "USERNAME", form.username },
            { "PASSWORD", form.password },
            { "COMPUTER", form.computer },

            { "ACCOUNT", form.username },
            { "TAVOLO", form.tavolo },
            { "MAZZO", mazzo },
            { "MAZZO_CALCOLATO", mazzo }
        };

        Logger.WriteLog("SendDeck - " + JsonSerializer.Serialize(parametri));

        DateTime start = DateTime.Now;

        // -----------------------------
        // ENDPOINT API (DA ADATTARE SE SERVE)
        // -----------------------------
        string endpoint = form.url + "/api/proactive/update-deck";

        using (var client = new HttpClient())
        {
            client.Timeout = TimeSpan.FromSeconds(30);

            var content = new FormUrlEncodedContent(parametri);

            HttpResponseMessage response = client
                .PostAsync(endpoint, content)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();

            string risposta = response.Content
                .ReadAsStringAsync()
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();

            Logger.WriteLog("SendDeck OK - ms:" +
                (int)DateTime.Now.Subtract(start).TotalMilliseconds);

            Logger.WriteLog("SendDeck response: " + risposta);
        }

        Logger.WriteLog("SendDeck totale metodo - ms:" +
            (int)DateTime.Now.Subtract(startMethod).TotalMilliseconds);
    }
    catch (Exception ex)
    {
        Logger.WriteLog("Errore SendDeck: " + ex.ToString());
    }
}


        public static void Send()
        {
            try
            {
                Configuratore form = UpdateInterface.GetInstanceForm();

                DateTime startMethod = DateTime.Now;

                // -----------------------------
                // LETTURA STATO DAL FORM
                // -----------------------------
                Runtime.balance = Runtime.balanceInit + Runtime.global_profit;
                string saldoIstantaneo = UIForm.PrintTotalBalance()
                    .Replace("Saldo: ", "")
                    .Replace("€", "")
                    .Trim()
                    .Replace(",", ".");

                string saldoIniziale = form.balanceStartValue.Value
                    .ToString()
                    .Replace(",", ".");
                
                saldoIniziale = Runtime.balanceInit.ToString().Replace(",", ".");
                saldoIstantaneo = Runtime.balance.ToString().Replace(",", ".");

                //string mazzo = form.numberDeckValueToChange.Text;
                /*
                var chosen_color_code = Runtime.chosen_color;
                string chosen_color = "";
                if (chosen_color_code == Constants.EnumColorBaccarat.BLU_PLAY)
                {
                    chosen_color = "P";
                } else if (chosen_color_code == Constants.EnumColorBaccarat.RED_BANK)
                {
                    chosen_color = "B";
                } else if (chosen_color_code == Constants.EnumColorBaccarat.TIE)
                {
                    chosen_color = "T";
                }
                */
                var chosen_color = Runtime.old_chosen_color;
                
                string mazzo = Runtime.number_deck.ToString(); //OCReads.number_deck.ToString();
                string margine = Runtime.global_profit.ToString();//form.labelNumberProfittoGlobale.Text.Replace("€", "").Replace(",", ".");
                string colpoMartingala = Runtime.old_martingala_counter.ToString();
                string tempo = form.timeElapsedValueToChange.Text;
                string valoreGiocato = Runtime.puntata.ToString();
                string stato = Runtime.labelTextCurrentState.Replace("Stato Bot:", "").Trim(); //form.labelStatus.Text.Replace("Stato Bot:", "").Trim());

                string vincebanker = form.textAreaBench.Text;
                string vinceplayer = form.textAreaPlayer.Text;
                string vincetie = form.textAreaTie.Text;

                string pbt = "";

                if (Runtime.last_result == Constants.EnumColorBaccarat.BLU_PLAY)
                {
                    pbt = "P";
                } else if (Runtime.last_result == Constants.EnumColorBaccarat.RED_BANK)
                {
                    pbt = "B";
                }
                else if (Runtime.last_result == Constants.EnumColorBaccarat.TIE)
                {
                    pbt = "T";
                }

                // -----------------------------
                // CALCOLO PBT
                // -----------------------------
                /*
                string pbt = "";
                
                if (OCReads.label_winner.Contains(vincebanker)) pbt = "B";
                else if (OCReads.label_winner.Contains(vinceplayer)) pbt = "P";
                else if (OCReads.label_winner.Contains(vincetie)) pbt = "T";
                
                if (pbt == "" && form._Old_Mazzo == mazzo)
                {
                    pbt = form._Old_PBT;
                }
                

                if (pbt == "" && DateTime.Now - Runtime.last_result_update < TimeSpan.FromSeconds(15))
                {
                    if (Runtime.last_result == Constants.EnumColorBaccarat.BLU_PLAY) pbt = "P";
                    else if (Runtime.last_result == Constants.EnumColorBaccarat.RED_BANK) pbt = "B";
                    else if (Runtime.last_result == Constants.EnumColorBaccarat.TIE) pbt = "T";
                }
                */

                // -----------------------------
                // CALCOLO VINCITA 1/0/-1
                // -----------------------------
                decimal nuovoSaldo, vecchioSaldo;
                decimal.TryParse(saldoIstantaneo.Replace(".", ","), out nuovoSaldo);
                decimal.TryParse(form._Old_SaldoIstantaneo.Replace(".", ","), out vecchioSaldo);

       
                string vincita = "0";
                if (nuovoSaldo > vecchioSaldo) vincita = "1";
                else if (nuovoSaldo < vecchioSaldo) vincita = "-1";
                
                form._Old_SaldoIstantaneo = saldoIstantaneo;
                form._Old_valoregiocato = valoreGiocato;
                form._Old_Mazzo = mazzo;
                form._Old_Stato = stato;
                form._Old_Margine = margine;
                form._Old_ColpoMartingala = colpoMartingala;
                form._Old_PBT = pbt;

                if (mazzo == "-1" && Runtime.current_state_bot == Constants.EnumStateBot.FIRST_PLAY)
                {
                    var mazzoInt = OCReads.number_deck - 1;
                    if (mazzoInt < 0) mazzoInt = 0;
                    mazzo = mazzoInt.ToString();
                }

                var parametri = new Dictionary<string, string>
                {
                    { "USERNAME", form.username },
                    { "PASSWORD", form.password },
                    { "COMPUTER", form.computer },
                    { "ACCOUNT", form.account },
                    
                    { "TAVOLO", form.tavolo },
                    { "SALDO_INIZIALE", saldoIniziale },
                    { "SALDO_ISTANTANEO", saldoIstantaneo },
                    { "MARGINE", margine },
                    { "STATO", stato },
                    { "COLPO_MARTINGALA", colpoMartingala },
                    { "VINCITA", vincita },
                    { "MAZZO", mazzo },
                    { "TEMPO", tempo },
                    { "AVVIO", form.avvio },
                    { "VALORE_GIOCATO", valoreGiocato },
                    { "PBT", pbt },
                    { "CHOSEN_COLOR", chosen_color}
                };

                Logger.WriteLog("Inizio invio - " + JsonSerializer.Serialize(parametri));

                DateTime start = DateTime.Now;

                /*
                if (mazzo != "0")
                {
                    zeroCalls = 0;
                } 
                if (mazzo == "0" && (start - lastUpdate < TimeSpan.FromSeconds(10)) && zeroCalls == 0)
                {
                    zeroCalls++;
                }
                if (mazzo == "0" && zeroCalls == 0)
                {
                    zeroCalls--;
                }

                if (zeroCalls > 0) return;
                */

                // -----------------------------
                // CHIAMATA SYNC
                // -----------------------------
                var risposta = apiCaller.GetAsync<int>(form.url + "/api/proactive/decide", parametri)
                                        .ConfigureAwait(false)
                                        .GetAwaiter()
                                        .GetResult();

                lastUpdate = DateTime.Now;
                Logger.WriteLog("Fine invio - ms:" +
                    (int)DateTime.Now.Subtract(start).TotalMilliseconds);

                // -----------------------------
                // INTERPRETA RISPOSTA
                // -----------------------------
                if (!int.TryParse(risposta, out int comando))
                    return;

                form.BeginInvoke((MethodInvoker)(() =>
                {
                    if (string.IsNullOrEmpty(form.txtComandiRicevuti.Text))
                        form.txtComandiRicevuti.Text =
                            DateTime.Now.ToString("HH:mm:ss") + " " + comando;
                    else
                        form.txtComandiRicevuti.Text =
                            DateTime.Now.ToString("HH:mm:ss") + " " + comando +
                            Environment.NewLine + form.txtComandiRicevuti.Text;
                }));

                // -----------------------------
                // LOGICA SPARATA NEL METODO DEDICATO
                // -----------------------------
                HandleActionAndState(form, comando, colpoMartingala, pbt, chosen_color);

                Logger.WriteLog("Totale metodo - ms:" +
                    (int)DateTime.Now.Subtract(startMethod).TotalMilliseconds);
            }
            catch (Exception ex)
            {
                Logger.WriteLog("Errore DashboardApiHelper: " + ex.ToString());
            }
        }

        // ----------------------------------------------------------
        // 🔹 LOGICA AZIONE + CAMBIO STATO EFFETTIVA ESTRATTA QUI
        // ----------------------------------------------------------
        private static void HandleActionAndState(Configuratore form, int comando, string colpoMartingala, string pbt, string chosen_color)
        {
            Runtime.bcomando_sf = false;
            switch ((Azione)comando)
            {
                case Azione.AzzeraMartingala:
                    Runtime.bcomando_sf = true;
                    break;

                case Azione.StopPc:
                    Runtime.bcomando_sf = false;
                    Runtime.game = 0;
                    Runtime.ocrBalanceCorrect = 0;
                    Runtime.ocrBalanceIncorrect = 0;
                    form.avvio = "0";

                    form.buttonStart.SafeInvoke(() => form.stop_all());
                    form.timerStart.Enabled = true;
                    break;

                /*
                case Azione.StartPc:
                    Runtime.bcomando_sf = false;
                    Runtime.game = 0;
                    Runtime.ocrBalanceCorrect = 0;
                    Runtime.ocrBalanceIncorrect = 0;
                    form.avvio = "1";

                    form.buttonStart.SafeInvoke(() =>
                    {
                        if (form.checkBoxAutoSaldo.Checked)
                            form.start_withPreScan();
                        else
                            form.start_all(bypass: true);
                    });
                    break;
                    */
                case Azione.PausaScalping:
                    Runtime.current_state_bot = Constants.EnumStateBot.PAUSE_SCALPING;
                    break;
            }

            // -----------------------------
            // SAFE WIN L5
            // -----------------------------

            if (Runtime.bcomando_sf && int.Parse(colpoMartingala) == 4) {
                Runtime.bcomando_sf = false;
                if (pbt.Equals("P") && chosen_color.Equals("B")) {
                    //Runtime.current_state_bot = Constants.EnumStateBot.PAUSE_SCALPING;
                    Runtime.martingala_counter = 0;
                }
                if (pbt.Equals("B") && chosen_color.Equals("P")) {
                    //Runtime.current_state_bot = Constants.EnumStateBot.PAUSE_SCALPING;
                    Runtime.martingala_counter = 0;
                }
            }

            // Post-AC2: dopo la logica storica, esci da scalping verso pausa (solo profitto locale).
            if ((Azione)comando == Azione.AzzeraMartingala)
            {
                Runtime.current_state_bot = Constants.EnumStateBot.PAUSE_SCALPING;
                Runtime.sculping_profit = 0.0;
                StateSculping.RequestExit();
                StateFirstPlay.RequestExit();
                StateSafeWin.RequestExit();
                StateFineMazzo.RequestExit();
                StateAttendiNuovoMazzo.RequestExit();
                StatePauseSculping.RequestExit();
            }
        }

        private static void SafeInvoke(this Control c, Action a)
        {
            if (c.InvokeRequired)
                c.BeginInvoke(a);
            else
                a();
        }
    }
}
