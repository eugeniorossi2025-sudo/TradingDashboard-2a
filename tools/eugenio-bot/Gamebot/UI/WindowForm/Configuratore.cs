
using Gamebot.Communication.Firebase;
using Gamebot.Helpers;
using Gamebot.Models;
using Gamebot.Models.Communication;
using Gamebot.Models.Entity;
using Gamebot.Models.Interfaces;
using Gamebot.Models.MainState;
using Gamebot.Models.Objects;
using Gamebot.Models.Roulette;
using Gamebot.Models.Roulette.Funcs;
using Gamebot.Models.UI;
using Gamebot.Properties;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Media;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
namespace Gamebot.UI.WindowForm
{
    public enum Azione
    {
        Nulla = 0,
        StopPc = 1,
        AzzeraMartingala = 2,
        PausaScalping = 3,
    }
    public partial class Configuratore : Form
    {
        private TextBox TxtNote;
        private RequestApi apiCaller = new RequestApi();
        public string url = ConfigurationManager.AppSettings["Dashboard.Url"];
        public string username = ConfigurationManager.AppSettings["Dashboard.Username"];
        public string password = ConfigurationManager.AppSettings["Dashboard.Password"];
        public string account = ConfigurationManager.AppSettings["Value.Account"];
        public string computer = ConfigurationManager.AppSettings["Value.Computer"];
        public string tavolo = ConfigurationManager.AppSettings["Value.Tavolo"];

        string vincebanker = ""; //ConfigurationManager.AppSettings["Testo.BankerVince"];
        string vinceplayer = ""; //ConfigurationManager.AppSettings["Testo.PlayerVince"];
        string vincetie = ""; //ConfigurationManager.AppSettings["Testo.Tie"];

        public string avvio = "";

        public string _Mazzo = "";
        public string _SaldoIniziale = "";
        string _SaldoIstantaneo = " ";
        string _Stato = "";
        string _Margine = "";
        string _ColpoMartingala = "";
        string _Tempo = "";
        string _Vincita = "";
        string _Valoregiocato = "";
        string _PBT = "";

        public string _Old_Mazzo = " ";
        string _Old_SaldoIniziale = " ";
        public string _Old_SaldoIstantaneo = " ";
        public string _Old_Stato = "";
        public string _Old_Margine = "";
        public string _Old_ColpoMartingala = "";
        string _Old_Tempo = "";
        string _Old_avvio = "";
        public string _Old_valoregiocato = "";
        public string _Old_PBT = "";
        public System.Timers.Timer timerStart = new System.Timers.Timer(2000);




        //private async Task ControllaEInviaParametriApiAsync()
        //{
        //    Dictionary<string, string> parametri;
        //    //Se sono cambiati i valori dalla precedente lettura invio alla dashboard
        //    try
        //    {
        //        _SaldoIstantaneo = UIForm.PrintTotalBalance().Replace("Saldo: ", "").Replace("€", "").Replace(",", ".");
        //        _SaldoIniziale = balanceStartValue.Value.ToString().Replace(",", ".");
        //        _Mazzo = numberDeckValueToChange.Text;
        //        _Margine = labelNumberProfittoGlobale.Text.Replace("€", "").Replace(",", ".");
        //        _ColpoMartingala = Runtime.martingala_counter.ToString();
        //        _Stato = labelStatus.Text.Replace("Stato Bot:", "");
        //        _Tempo = timeElapsedValueToChange.Text;
        //        _Valoregiocato = Runtime.puntata.ToString();

        //        vincebanker = textAreaBench.Text; //BANCO VINCE  ConfigurationManager.AppSettings["Testo.BankerVince"];
        //        vinceplayer = textAreaPlayer.Text; //IL PLAYER VINCE ConfigurationManager.AppSettings["Testo.PlayerVince"];
        //        vincetie = textAreaTie.Text;//TIE ConfigurationManager.AppSettings["Testo.Tie"];

        //        if (OCReads.label_winner.Contains(vincebanker))
        //        { _PBT = "B"; }
        //        if (OCReads.label_winner.Contains(vinceplayer))
        //        { _PBT = "P"; }
        //        if (OCReads.label_winner.Contains(vincetie))
        //        { _PBT = "T"; }


        //        string _ColpoMartingalaADJ = "";

        //        if (Runtime.puntata > 0)
        //        { _ColpoMartingalaADJ = (Runtime.martingala_counter + 1).ToString(); }
        //        TxtNote.Text = "PUNTATA:" + _Valoregiocato.ToString() +
        //            "\r\n" +
        //            "MARTINGALA:" + _ColpoMartingala.ToString() +
        //            "\r\n" +
        //            OCReads.label_winner;
        //        if (_Mazzo != _Old_Mazzo || _Valoregiocato != _Old_valoregiocato/*||
        //            _SaldoIniziale != _Old_SaldoIniziale ||
        //            _SaldoIstantaneo != _Old_SaldoIstantaneo ||
        //            _Stato != _Old_Stato ||
        //            _Margine != _Old_Margine ||
        //            _ColpoMartingala != _Old_ColpoMartingala ||
        //            avvio!= _Old_avvio ||
        //            _Valoregiocato != _Old_valoregiocato*/
        //            )
        //        {

        //            decimal NuovoSaldo = 0;
        //            decimal VecchioSaldo = 0;
        //            try
        //            { NuovoSaldo = decimal.Parse(_SaldoIstantaneo.Replace(".", ",")); }
        //            catch (Exception)
        //            { }

        //            try
        //            { VecchioSaldo = decimal.Parse(_Old_SaldoIstantaneo.Replace(".", ",")); }
        //            catch (Exception)
        //            { }



        //            if (NuovoSaldo > VecchioSaldo)
        //            { _Vincita = "1"; }
        //            if (NuovoSaldo > VecchioSaldo)
        //            { _Vincita = "0"; }
        //            if (NuovoSaldo < VecchioSaldo)
        //            { _Vincita = "-1"; }

        //            _Old_Mazzo = _Mazzo;
        //            _Old_SaldoIniziale = _SaldoIniziale;
        //            _Old_SaldoIstantaneo = _SaldoIstantaneo;
        //            _Old_Stato = _Stato;
        //            _Old_Margine = _Margine;
        //            _Old_ColpoMartingala = _ColpoMartingala;
        //            _Old_avvio = avvio;
        //            _Old_valoregiocato = _Valoregiocato;
        //            _Old_PBT = _PBT;

        //            parametri = new Dictionary<string, string>
        //                {
        //                    { "USERNAME", username },
        //                    { "PASSWORD", password },
        //                    { "COMPUTER", computer },
        //                    { "ACCOUNT", account },
        //                    { "TAVOLO", tavolo },
        //                    { "SALDO_INIZIALE",  _SaldoIniziale } ,
        //                    { "SALDO_ISTANTANEO",_SaldoIstantaneo },
        //                    { "MARGINE", _Margine},
        //                    { "STATO", _Stato },
        //                    //{ "COLORE", "" },
        //                    { "COLPO_MARTINGALA", _ColpoMartingala },
        //                    { "VINCITA", _Vincita },
        //                    { "MAZZO", _Mazzo },
        //                    { "TEMPO", _Tempo },
        //                    { "AVVIO",  avvio  },
        //                    {"VALORE_GIOCATO",_Valoregiocato},
        //                    {"PBT",_PBT}
        //                };

        //            _PBT = "";
        //        }
        //        else
        //        {   //questa chiamata serve alla fine solo per ricevere i comandi non salva nella dasboard
        //            parametri = new Dictionary<string, string>
        //                {
        //                    { "USERNAME", username },
        //                    { "PASSWORD", password },
        //                    { "COMPUTER", computer }
        //                };
        //        }

        //        var risposta = await apiCaller.GetAsync<string>(url, parametri);

        //        //arriva comando safewin da dashboard
        //        if ((Azione)int.Parse(risposta) == Azione.AzzeraMartingala)
        //        {
        //            Runtime.bcomando_sf = true;

        //            //Runtime.current_state_bot = Constants.EnumStateBot.PAUSE_SCALPING; //.SAFE_WIN;
        //        }

        //        //arriva comando stop da dashboard
        //        if ((Azione)int.Parse(risposta) == Azione.StopPc)
        //        {
        //            Runtime.bcomando_sf = false;
        //            Runtime.game = 0;
        //            Runtime.ocrBalanceCorrect = 0;
        //            Runtime.ocrBalanceIncorrect = 0;
        //            avvio = "0";

        //            if (buttonStart.InvokeRequired)
        //            {
        //                // Uso BeginInvoke per passare al UI thread
        //                buttonStart.BeginInvoke((MethodInvoker)delegate
        //                {
        //                    stop_all();
        //                });
        //            }
        //            else
        //            {
        //                stop_all();
        //            }
        //            timerStart.Enabled = true;
        //        }

        //        //arriva comando start da dashboard
        //        if ((Azione)int.Parse(risposta) == Azione.StartPc)
        //        {
        //            Runtime.bcomando_sf = false;
        //            Runtime.game = 0;
        //            Runtime.ocrBalanceCorrect = 0;
        //            Runtime.ocrBalanceIncorrect = 0;
        //            avvio = "1";

        //            if (buttonStart.InvokeRequired)
        //            {
        //                // Uso BeginInvoke per passare al UI thread
        //                buttonStart.BeginInvoke((MethodInvoker)delegate
        //                {
        //                    if (checkBoxAutoSaldo.Checked)
        //                    {
        //                        start_withPreScan();
        //                    }
        //                    else
        //                    {
        //                        start_all(bypass: true);
        //                    }
        //                });
        //            }
        //            else
        //            {
        //                if (checkBoxAutoSaldo.Checked)
        //                {
        //                    start_withPreScan();
        //                }
        //                else
        //                {
        //                    start_all(bypass: true);
        //                }
        //            }
        //        }

        //        if (Runtime.bcomando_sf == true && int.Parse(_ColpoMartingala) == 5)
        //        {
        //            if (_PBT == "B")
        //            { //NON SUCCEDE NULLA
        //                Runtime.bcomando_sf = false;
        //            }

        //            if (_PBT == "P")
        //            {  /*
        //                //stop 
        //                Runtime.game = 0;
        //                Runtime.ocrBalanceCorrect = 0;
        //                Runtime.ocrBalanceIncorrect = 0;
        //                avvio = "0";

        //                if (buttonStart.InvokeRequired)
        //                {
        //                    // Uso BeginInvoke per passare al UI thread
        //                    buttonStart.BeginInvoke((MethodInvoker)delegate
        //                    {
        //                        stop_all();
        //                    });
        //                }
        //                else
        //                {
        //                    stop_all();
        //                }
        //                await Task.Delay(5000);

        //                timerStart.Enabled = true;
        //                //e poi start 
        //                Runtime.game = 0;
        //                Runtime.ocrBalanceCorrect = 0;
        //                Runtime.ocrBalanceIncorrect = 0;
        //                avvio = "1";

        //                if (buttonStart.InvokeRequired)
        //                {
        //                    // Uso BeginInvoke per passare al UI thread
        //                    buttonStart.BeginInvoke((MethodInvoker)delegate
        //                    {
        //                        if (checkBoxAutoSaldo.Checked)
        //                        {
        //                            start_withPreScan();
        //                        }
        //                        else
        //                        {
        //                            start_all(bypass: true);
        //                        }
        //                    });
        //                }
        //                else
        //                {
        //                    if (checkBoxAutoSaldo.Checked)
        //                    {
        //                        start_withPreScan();
        //                    }
        //                    else
        //                    {
        //                        start_all(bypass: true);
        //                    }
        //                }
        //                await Task.Delay(5000);
        //                Runtime.bcomando_sf = false;
        //                */
        //                Runtime.current_state_bot = Constants.EnumStateBot.PAUSE_SCALPING;
        //                Runtime.bcomando_sf = false;
        //            }

        //            if (_PBT == "T")
        //            {
        //                //attendo il prossimo comando
        //                Runtime.bcomando_sf = true;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //string a = ex.Message + " " + ex.StackTrace;
        //        //MessageBox.Show(a);
        //    }
        //}

        private async Task ControllaEInviaParametriApiAsync()
        {
            return;
            try
            {
                DateTime startMethod = DateTime.Now;

                // ------------------------------------------
                // LETTURA DATI DAL BOT
                // ------------------------------------------

                _SaldoIstantaneo = UIForm.PrintTotalBalance()
                    .Replace("Saldo: ", "")
                    .Replace("€", "")
                    .Trim()
                    .Replace(",", ".");

                _SaldoIniziale = balanceStartValue.Value
                    .ToString()
                    .Replace(",", ".");

                _Mazzo = numberDeckValueToChange.Text;
                _Margine = labelNumberProfittoGlobale.Text.Replace("€", "").Replace(",", ".");
                _ColpoMartingala = Runtime.martingala_counter.ToString();
                _Tempo = timeElapsedValueToChange.Text;
                _Valoregiocato = Runtime.puntata.ToString();
                _Stato = labelStatus.Text.Replace("Stato Bot:", "").Trim();


                // ------------------------------------------
                // CALCOLO P/B/T
                // ------------------------------------------

                vincebanker = textAreaBench.Text;
                vinceplayer = textAreaPlayer.Text;
                vincetie = textAreaTie.Text;

                if (OCReads.label_winner.Contains(vincebanker))
                    _PBT = "B";
                else if (OCReads.label_winner.Contains(vinceplayer))
                    _PBT = "P";
                else if (OCReads.label_winner.Contains(vincetie))
                    _PBT = "T";
                else
                    _PBT = "";


                // ------------------------------------------
                // CALCOLO VINCITA (1, 0, -1)
                // ------------------------------------------

                decimal nuovoSaldo = 0;
                decimal vecchioSaldo = 0;

                decimal.TryParse(_SaldoIstantaneo.Replace(".", ","), out nuovoSaldo);
                decimal.TryParse(_Old_SaldoIstantaneo.Replace(".", ","), out vecchioSaldo);

                if (nuovoSaldo > vecchioSaldo) _Vincita = "1";
                else if (nuovoSaldo < vecchioSaldo) _Vincita = "-1";
                else _Vincita = "0"; // pareggio


                // Aggiorno memoria interna
                _Old_SaldoIstantaneo = _SaldoIstantaneo;
                _Old_valoregiocato = _Valoregiocato;
                _Old_Mazzo = _Mazzo;
                _Old_Stato = _Stato;
                _Old_Margine = _Margine;
                _Old_ColpoMartingala = _ColpoMartingala;
                _Old_PBT = _PBT;


                // ------------------------------------------
                // COSTRUZIONE PARAMETRI PER L’API (SEMPRE COMPLETI)
                // ------------------------------------------

                string guid = Guid.NewGuid().ToString();

                var parametri = new Dictionary<string, string>
        {
            { "USERNAME", username },
            { "PASSWORD", password },
            { "COMPUTER", computer },
            { "ACCOUNT", account },
            { "TAVOLO", tavolo },
            { "SALDO_INIZIALE",  _SaldoIniziale },
            { "SALDO_ISTANTANEO", _SaldoIstantaneo },
            { "MARGINE", _Margine },
            { "STATO", _Stato },
            { "COLPO_MARTINGALA", _ColpoMartingala },
            { "VINCITA", _Vincita },
            { "MAZZO", _Mazzo },
            { "TEMPO", _Tempo },
            { "AVVIO", avvio },
            { "VALORE_GIOCATO", _Valoregiocato },
            { "PBT", _PBT },
                    {"guid", guid }
        };


                // ------------------------------------------
                // INVIO ALLA DASHBOARD/API
                // ------------------------------------------

                Logger.WriteLog("Inizio invio - " + JsonSerializer.Serialize(parametri));

                DateTime start = DateTime.Now;

                var risposta = await apiCaller.GetAsync<string>(url, parametri);

                var ts = (int)DateTime.Now.Subtract(start).TotalMilliseconds;

                Logger.WriteLog("Fine invio - ms:" + ts);

                // ------------------------------------------
                // GESTIONE COMANDI DA DASHBOARD
                // ------------------------------------------

                if (!int.TryParse(risposta, out int comando))
                    return;

                if (string.IsNullOrEmpty(txtComandiRicevuti.Text))
                {
                    txtComandiRicevuti.Text = DateTime.Now.ToString("HH:mm:ss") + " " + comando.ToString();
                }
                else
                {
                    txtComandiRicevuti.Text = DateTime.Now.ToString("HH:mm:ss") + " " + comando.ToString() + Environment.NewLine + txtComandiRicevuti.Text;
                }

                switch ((Azione)comando)
                {
                    // SAFE WIN
                    case Azione.AzzeraMartingala:
                        Runtime.bcomando_sf = true;
                        break;

                    // STOP
                    case Azione.StopPc:
                        Runtime.bcomando_sf = false;
                        Runtime.game = 0;
                        Runtime.ocrBalanceCorrect = 0;
                        Runtime.ocrBalanceIncorrect = 0;
                        avvio = "0";

                        if (buttonStart.InvokeRequired)
                            buttonStart.BeginInvoke((MethodInvoker)(() => stop_all()));
                        else
                            stop_all();

                        timerStart.Enabled = true;
                        break;

                    // START
                    /*
                    case Azione.StartPc:
                        Runtime.bcomando_sf = false;
                        Runtime.game = 0;
                        Runtime.ocrBalanceCorrect = 0;
                        Runtime.ocrBalanceIncorrect = 0;
                        avvio = "1";

                        if (buttonStart.InvokeRequired)
                        {
                            buttonStart.BeginInvoke((MethodInvoker)(() =>
                            {
                                if (checkBoxAutoSaldo.Checked)
                                    start_withPreScan();
                                else
                                    start_all(bypass: true);
                            }));
                        }
                        else
                        {
                            if (checkBoxAutoSaldo.Checked)
                                start_withPreScan();
                            else
                                start_all(bypass: true);
                        }

                        break;
                        */
                }


                // ------------------------------------------
                // SAFE WIN LOGICA L5
                // ------------------------------------------

                if (Runtime.bcomando_sf == true &&
                    int.Parse(_ColpoMartingala) == 5)
                {
                    if (_PBT == "B")
                    {
                        Runtime.bcomando_sf = false;
                    }
                    else if (_PBT == "P")
                    {
                        Runtime.current_state_bot = Constants.EnumStateBot.PAUSE_SCALPING;
                        Runtime.bcomando_sf = false;
                    }
                    else if (_PBT == "T")
                    {
                        // attendo il prossimo colpo
                    }
                }

                int ts_method = (int)DateTime.Now.Subtract(startMethod).TotalMilliseconds;

                Logger.WriteLog("Totale metodo - ms:" + ts_method);

            }
            catch
            {
                // errori silenziati per evitare crash del bot
            }
        }




        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            timerStart.Enabled = false;
            // Il codice che vuoi eseguire ad ogni intervallo del timer
            //Console.WriteLine($"Evento Scattato alle {e.SignalTime:HH:mm:ss.fff}");
            ControllaEInviaParametriApiAsync();
            timerStart.Enabled = true;
        }
        public Configuratore()
        {

            





            timerStart.Elapsed += OnTimedEvent;
            timerStart.Enabled = true;

            this._requestApiRepository = new RequestApi();
            this.InitializeComponent();
            this.Main();
            UIForm.SetStatusBot();


            balanceStartValue.Maximum = 10000000;
            globalStopWinValue.Maximum = 10000000;
            stopWinValue.Maximum = 10000000;
            stopLossValue.Maximum = 10000000;
            safeWinPerc.Maximum = 10000000;

            globalRouletteStopWin.Maximum = 10000000;
            globalRouletteStopLoss.Maximum = 10000000;
            numericRouletteValueHand1.Maximum = 10000000;
            numericRouletteValueHand2.Maximum = 10000000;
            numericRouletteValueHand3.Maximum = 10000000;



            this.progressUI = new Progress<List<string>>(delegate (List<string> update)
            {
                this.labelNumberProfittoGlobale.Text = update[0];
                this.labelNumberProfittoSculping.Text = update[1];
                this.labelNumerWin.Text = update[2];
                this.labelNumerLose.Text = update[3];
                this.buttonStart.Text = ((Runtime.current_state_bot == Constants.EnumStateBot.IDLE) ? "AVVIA ▶" : "STOP ■");
                this.balanceTotalValueText.Text = UIForm.PrintTotalBalance();
                this.labelStatus.Text = Runtime.labelTextCurrentState;
                this.numberDeckValueToChange.Text = Runtime.number_deck.ToString();
                this.timeElapsedValueToChange.Text = UIForm.GetTimeElapsed();

            });
            this.UpdateStats(this.progressUI);
            this.progressBalance = new Progress<List<string>>(delegate (List<string> update)
            {
                this.saldoLetto.Text = UIForm.PrintReadBalance();
                this.saldoLettoCorrect.Text = Runtime.ocrBalanceCorrect.ToString() + " / " + Runtime.ocrBalanceIncorrect.ToString();
                Runtime.readSaldo = this.readsaldo.Text;
            });
            this.UpdateStatsBalance(this.progressBalance);
            this.progressUIRoulette = new Progress<List<string>>(delegate (List<string> update)
            {
                this.lblRouletteGlobalProfitText.Text = update[0];
                this.lblRouletteHandWinText.Text = update[1];
                this.lblRouletteHandLossText.Text = update[2];
                this.btnRouletteStart.Text = ((RouletteValues.Runtime.current_state_bot == RouletteValues.Constants.EnumStateBot.IDLE || RouletteValues.Runtime.current_state_bot == RouletteValues.Constants.EnumStateBot.END_DECK) ? "AVVIA ▶" : "STOP ■");
                this.balanceRouletteTotalValueText.Text = UIForm.PrintTotalBalanceRoulette();
            });
            this.UpdateStatsRoulette(this.progressUIRoulette);

            this.progressTimeElapsed = new Progress<List<string>>(delegate (List<string> update)
            {
                this.timeElapsedValueToChange.Text = UIForm.GetTimeElapsed();

                ControllaEInviaParametriApiAsync();

            });
            this.UpdateTimeElapsed(this.progressTimeElapsed);

            //CAMPO NOTE PER SCOPI DI DEBUG
            this.TxtNote = new System.Windows.Forms.TextBox();
            this.tabPage1.Controls.Add(this.TxtNote);
            this.TxtNote.Location = new System.Drawing.Point(744, 537);
            this.TxtNote.Multiline = true;
            this.TxtNote.Name = "TxtNote";
            this.TxtNote.Size = new System.Drawing.Size(293, 44);
            this.TxtNote.TabIndex = 118;

        }

        private void Main()
        {
            this.labelVersion.Text = string.Format("v: L.{0}.{1}.{2}", 2, 8, 0);
            this.controlsToEnableDisable = UIForm.FindControlsByTag(this, "controlInput");
            this.controlsRouletteToEnableDisable = UIForm.FindControlsByTag(this, "controlInputRoulette");
            this.controlsRouletteToEnableDisableHand1 = UIForm.FindControlsByTag(this, "btnHand1Roulette");
            this.controlsRouletteToEnableDisableHand2 = UIForm.FindControlsByTag(this, "btnHand2Roulette");
            this.controlsRouletteToEnableDisableHand3 = UIForm.FindControlsByTag(this, "btnHand3Roulette");
            this.lblNameConfig.Text = "<<Nessuna configurazione caricata>>";
            this.balanceStartValue.Value = 1000m;
            this.balanceRouletteStartValue.Value = 1000m;
            this.txtZoomMonitor.Value = 100m;
            this.lblRouletteNameConfig.Text = "<<Nessuna configurazione caricata>>";
            this.customFichesPanel.Width = 469;
            this.customFichesPanel.Location = new Point(136, 375);
            this.noFichesLabel.Location = new Point(163, 39);
            this.showboxbtn.Visible = false;
            this.textAreaTie.Text = "TIE";
            this.textAreaWin.Text = "VINCE";
            this.textAreaBench.Text = "BANCO";
            this.textAreaPlayer.Text = "GIOCATORE";
            this.textAreaPuntare.Text = "PUNTARE";
            this.baccaratDemoBtnRadioDisabled.Checked = true;
            this.addEventsToRouletteButton();
            CancellationToken token = new CancellationTokenSource().Token;
            this.labelEnvironment.Visible = false;
            this.labelEnvironment.Text = Constants.EnumEnvironment.PRODUCTION.ToString();
        }

        #region Roulette

        private void buttonBalanceAreaRoulette_Click(object sender, EventArgs e)
        {
            this.GetZoom();
            UIForm.ClickButtonGreen(sender, "AREA_SALDO_ROULETTE");
        }

        private void roulettebalanceinfobtn_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Impostare il \"Saldo Iniziale\" con il valore disponibile prima di giocare.\n\nIl Saldo verrà aggiornato mentre il bot opera, a seconda della vincita o perdita della puntata.\n\nIl \"Profitto Globale\" è l'ammontare ottenuto (o perso) durante la partita. \n\nModalità lettura automatica: premere su \"Area Saldo\" per impostare l'area, e attivare la checkbox \"Saldo Autom.\".\nUna volta premuto \"AVVIA\" verrà effettuata una lettura del saldo (3 secondi), al termine della quale il valore letto lampeggerà sull'etichetta \"Stato bot\" (2 secondi) e verrà impostato nel valore \"Saldo Iniziale\".", "Info", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void roulettestopwinlossinfobtn_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Impostare \"Global Stop Win\" con il valore di vincita globale, raggiunto il quale il bot deve fermarsi.\n\nImpostare \"Global Stop Loss\" con il valore perso durante la sessione, raggiunto il quale il bot deve fermarsi.\nImmettere un valore positivo per rappresentare la perdita (es: impostare 100 farà fermare il bot quando arriverà a -100 di Profitto).\n\nImpostare il Riconoscimento Area \"Giocata\" #1, #2 e #3 sulla voce del pannello di gioco relativa a tale giocata.\nTale giocata deve essere preparata sia lato casinò che sul pannello del bot \"Numeri Giocata\" #1, #2 e #3, dove i numeri da giocare saranno visualizzati color tela/acquamarina anzichè rosso/nero/verde.\n\nImpostare \"Area Vincita\" sulla striscia che indica lo status della manche (\"Attendi la prossima partita\", \"Giocatore Vince\" ecc.).\n\nImpostare \"Area Riposo\" su una zona dell'interfaccia non interattiva, ma pur sempre nella schermata.\n\nImpostare \"Valore Giocata\" #1, #2 e #3 con i rispettivi valori *totali* della singola giocata (es: se in giocata #1 si punta su 4 numeri, con una fiche da 5, il valore giocata #1 sarà 20).", "Info", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void roulettemainhelpbtn_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Per mappare un'area, cliccare sul pulsante e trascinare un rettangolo sull'area da associare.\n\nAssicurarsi di aver mappato ogni area, prima di avviare il bot.\n(I pulsanti delle aree mappate appaiono verdi.)\n\nTutti i campi (Stop Win, Saldo Iniziale, Valore Giocata ecc.) devono essere configurati prima di avviare il bot.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private async void UpdateStatsRoulette(IProgress<List<string>> progress)
        {
            List<string> values = new List<string>();
            if (progress != null)
            {
                values.Add(Number.FormatNumberDecimalEuro(RouletteValues.Runtime.global_profit));
                values.Add(string.Format("{0}", RouletteValues.Runtime.numero_vincite));
                values.Add(string.Format("{0}", RouletteValues.Runtime.numero_perdite));
                progress.Report(values);
            }
        }

        public void SettingUIRouletteStart()
        {
            if (Runtime.game == 0)
            {
                UIForm.DisableAddButtonItem(this.btnRouletteStart);
            }
            else if (Runtime.game == 1)
            {
                this.btnRouletteStart.Text = "STOP ■";
            }
            UIForm.DisableAddButtonItem(this.btnRouletteLoadConfig);
            UIForm.DisableAddButtonItem(this.btnRouletteSaveConfig);
            foreach (Control control in this.controlsRouletteToEnableDisable)
            {
                UIForm.DisableRouletteItem(control);
            }
            foreach (Control control2 in this.controlsRouletteToEnableDisableHand1)
            {
                UIForm.DisableRouletteItem(control2);
            }
            foreach (Control control3 in this.controlsRouletteToEnableDisableHand2)
            {
                UIForm.DisableRouletteItem(control3);
            }
            foreach (Control control4 in this.controlsRouletteToEnableDisableHand3)
            {
                UIForm.DisableRouletteItem(control4);
            }
        }

        private void globalRouletteStopWin_ChangeDotToComma(object sender, EventArgs e)
        {
            this.globalRouletteStopWin.Text = UIForm.ReplaceDotIntoCommaValueText(this.globalRouletteStopWin.Text);
        }

        private void globalRouletteStopLoss_ChangeDotToComma(object sender, EventArgs e)
        {
            this.globalRouletteStopLoss.Text = UIForm.ReplaceDotIntoCommaValueText(this.globalRouletteStopLoss.Text);
        }

        private void numericRouletteValueHand1_ChangeDotToComma(object sender, EventArgs e)
        {
            this.numericRouletteValueHand1.Text = UIForm.ReplaceDotIntoCommaValueText(this.numericRouletteValueHand1.Text);
        }

        private void numericRouletteValueHand2_ChangeDotToComma(object sender, EventArgs e)
        {
            this.numericRouletteValueHand2.Text = UIForm.ReplaceDotIntoCommaValueText(this.numericRouletteValueHand2.Text);
        }

        private void numericRouletteValueHand3_ChangeDotToComma(object sender, EventArgs e)
        {
            this.numericRouletteValueHand3.Text = UIForm.ReplaceDotIntoCommaValueText(this.numericRouletteValueHand3.Text);
        }

        private void balanceRouletteStartValue_ChangeDotToComma(object sender, EventArgs e)
        {
            this.balanceRouletteStartValue.Text = UIForm.ReplaceDotIntoCommaValueText(this.balanceRouletteStartValue.Text);
        }

        private void addEventsToRouletteButton()
        {
            foreach (Control item in this.panelRoulettePlayed1.Controls.OfType<Control>().ToList<Control>())
            {
                if (item is Button)
                {
                    ((Button)item).FlatStyle = FlatStyle.Flat;
                    int value2 = Convert.ToInt32(item.Text);
                    item.Click += delegate (object sender, EventArgs EventArgs)
                    {
                        this.selectNumberClick(sender, value2, 1);
                    };
                }
            }
            foreach (Control item2 in this.panelRoulettePlayed2.Controls.OfType<Control>().ToList<Control>())
            {
                if (item2 is Button)
                {
                    ((Button)item2).FlatStyle = FlatStyle.Flat;
                    int value3 = Convert.ToInt32(item2.Text);
                    item2.Click += delegate (object sender, EventArgs EventArgs)
                    {
                        this.selectNumberClick(sender, value3, 2);
                    };
                }
            }
            foreach (Control item3 in this.panelRoulettePlayed3.Controls.OfType<Control>().ToList<Control>())
            {
                if (item3 is Button)
                {
                    ((Button)item3).FlatStyle = FlatStyle.Flat;
                    int value = Convert.ToInt32(item3.Text);
                    item3.Click += delegate (object sender, EventArgs EventArgs)
                    {
                        this.selectNumberClick(sender, value, 3);
                    };
                }
            }
        }

        private void selectNumberClick(object sender, int number, int play)
        {
            Button btn = (Button)sender;
            BtnFiches buttonFiches = Roulette.Instance.AddNumberToList(new BtnFiches(number, btn.BackColor, btn.ForeColor, btn.FlatAppearance.BorderColor), play);
            if (!buttonFiches.Removed)
            {
                UIForm.SelectButtonFichesRoulette(btn);
            }
            else
            {
                UIForm.DeselectButtonFichesRoulette(btn, buttonFiches);
                btn.BackColor = buttonFiches.BackCurrentColor;
            }
            Roulette.Instance.PrintAllHand();
        }

        private async void buttonRouletteStart_Click(object sender, EventArgs e)
        {
            Runtime.game = 1;
            Log.PrintInfo("(R) STARTING ROULETTE!!!1!");
            Log.PrintInfo("STATO BOT: " + RouletteValues.Runtime.current_state_bot);
            Runtime.ocrBalanceCorrect = 0;
            Runtime.ocrBalanceIncorrect = 0;
            if (RouletteValues.Runtime.current_state_bot != RouletteValues.Constants.EnumStateBot.IDLE)
            {
                stop_all_roulette();
            }
            else if (await MainStateBot.CheckConnection())
            {
                if (checkBoxAutoSaldoRoulette.Checked)
                {
                    start_roulette_withPreScan();
                }
                else
                {
                    start_all_roulette(bypass: false);
                }
            }
            else
            {
                MessageBox.Show("Impossibile raggiungere il server di autenticazione.\nControllare la connessione ad internet.\nSe il problema persiste contattare l’assistenza.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void stop_all_roulette()
        {
            RouletteValues.Runtime.current_state_bot = RouletteValues.Constants.EnumStateBot.IDLE;
            this.RouletteSettingUIStop();
            this.SettingUIStop();
            RoulettePlayer.Instance.Stop();
            this.labelStatusRoulette.Text = "Bot Inattivo";
        }

        private void start_all_roulette(bool bypass)
        {
            if (!this.CheckConfigRoulette(true))
            {
                return;
            }
            string projectPath = Constants.PathProject();
            Path.Combine(projectPath, "appData");
            if ((this.lblRouletteNameConfig.Text.Equals("<<Nessuna configurazione caricata>>") ? "" : this.lblRouletteNameConfig.Text).Equals(string.Empty))
            {
                MessageBox.Show("Caricare o salvare una configurazione", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
            string filenameToSave = Path.Combine(projectPath, "appData", this.lblRouletteNameConfig.Text);
            this.ReadRouletteParamForm();
            this.SaveRouletteDataForm(filenameToSave);
            RouletteValues.Runtime.current_state_bot = RouletteValues.Constants.EnumStateBot.RUNNING;
            this.labelStatusRoulette.Text = "Bot Attivo";
            this.SettingUIRouletteStart();
            this.SettingUIStart();
            RoulettePlayer.Instance.Start();
        }

        private void start_roulette_withPreScan()
        {
            AreaElement area = ListAreaElement.Instance.GetAreaByKey("AREA_SALDO_ROULETTE");
            if (area == null || area.startX == 0)
            {
                MessageBox.Show("Per poter leggere il saldo devi specificarne l'area!\n\nAlternativamente, disabilitare il checkbox \"Saldo Autom.\" e inserire manualmente il Saldo Iniziale prima di avviare il bot.", "AREA MANCANTE", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
            base.Opacity = 0.85;
            using (WaitScanBalanceWindow fw = new WaitScanBalanceWindow(new Action(this.scanSaldo)))
            {
                fw.ShowDialog(this);
            }
            base.Opacity = 1.0;
            try
            {
                this.balanceRouletteStartValue.Value = Convert.ToDecimal(OCReads.balance);
                this.labelStatusRoulette.Text = "start saldo : " + OCReads.balance;
                for (int i = 0; i < 8; i++)
                {
                    Thread.Sleep(250);
                    this.labelStatusRoulette.ForeColor = Color.Red;
                    base.Update();
                    Thread.Sleep(250);
                    this.labelStatusRoulette.ForeColor = SystemColors.ControlText;
                    base.Update();
                }
            }
            catch (Exception)
            {
                this.labelStatusRoulette.Text = "start saldo : NON RILEVATO!";
                for (int j = 0; j < 8; j++)
                {
                    Thread.Sleep(250);
                    this.labelStatusRoulette.ForeColor = Color.Red;
                    base.Update();
                    Thread.Sleep(250);
                    this.labelStatusRoulette.ForeColor = SystemColors.ControlText;
                    base.Update();
                }
            }
            this.start_all_roulette(true);
        }

        private void buttonLoadRouletteConfig_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Roulette files (*.rou)|*.rou";
            string folderToReadFile = Path.Combine(Constants.PathProject(), "appData");
            openFileDialog.InitialDirectory = folderToReadFile;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                this.lblRouletteNameConfig.Text = Path.GetFileName(openFileDialog.FileName);
                string configString = ManageFile.ReadFile(openFileDialog.FileName);
                if (string.IsNullOrEmpty(configString))
                {
                    MessageBox.Show("(R) NESSUNA CONFIGURAZIONE SALVATA", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }
                this.pulisciRoulette();
                JSONSingleConfig2 currentConfig = JsonSerializer.Deserialize<JSONConfig2>(configString, default(JsonSerializerOptions)).Configs[0];
                this._ReadJsonRoulette(currentConfig.ConfigRoulette);
                this._ReadJsonTelegram(currentConfig.ConfigTelegram);
            }
        }

        private void pulisciRoulette()
        {
            this.btnRoulettePlayed1Number0.BackColor = Color.SeaGreen;
            this.btnRoulettePlayed1Number0.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number1.BackColor = Color.Firebrick;
            this.btnRoulettePlayed1Number1.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number2.BackColor = Color.Black;
            this.btnRoulettePlayed1Number2.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number3.BackColor = Color.Firebrick;
            this.btnRoulettePlayed1Number3.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number4.BackColor = Color.Black;
            this.btnRoulettePlayed1Number4.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number5.BackColor = Color.Firebrick;
            this.btnRoulettePlayed1Number5.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number6.BackColor = Color.Black;
            this.btnRoulettePlayed1Number6.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number7.BackColor = Color.Firebrick;
            this.btnRoulettePlayed1Number7.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number8.BackColor = Color.Black;
            this.btnRoulettePlayed1Number8.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number9.BackColor = Color.Firebrick;
            this.btnRoulettePlayed1Number9.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number10.BackColor = Color.Black;
            this.btnRoulettePlayed1Number10.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number11.BackColor = Color.Black;
            this.btnRoulettePlayed1Number11.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number12.BackColor = Color.Firebrick;
            this.btnRoulettePlayed1Number12.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number13.BackColor = Color.Black;
            this.btnRoulettePlayed1Number13.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number14.BackColor = Color.Firebrick;
            this.btnRoulettePlayed1Number14.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number15.BackColor = Color.Black;
            this.btnRoulettePlayed1Number15.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number16.BackColor = Color.Firebrick;
            this.btnRoulettePlayed1Number16.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number17.BackColor = Color.Black;
            this.btnRoulettePlayed1Number17.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number18.BackColor = Color.Firebrick;
            this.btnRoulettePlayed1Number18.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number19.BackColor = Color.Firebrick;
            this.btnRoulettePlayed1Number19.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number20.BackColor = Color.Black;
            this.btnRoulettePlayed1Number20.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number21.BackColor = Color.Firebrick;
            this.btnRoulettePlayed1Number21.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number22.BackColor = Color.Black;
            this.btnRoulettePlayed1Number22.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number23.BackColor = Color.Firebrick;
            this.btnRoulettePlayed1Number23.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number24.BackColor = Color.Black;
            this.btnRoulettePlayed1Number24.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number25.BackColor = Color.Firebrick;
            this.btnRoulettePlayed1Number25.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number26.BackColor = Color.Black;
            this.btnRoulettePlayed1Number26.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number27.BackColor = Color.Firebrick;
            this.btnRoulettePlayed1Number27.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number28.BackColor = Color.Black;
            this.btnRoulettePlayed1Number28.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number29.BackColor = Color.Black;
            this.btnRoulettePlayed1Number29.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number30.BackColor = Color.Firebrick;
            this.btnRoulettePlayed1Number30.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number31.BackColor = Color.Black;
            this.btnRoulettePlayed1Number31.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number32.BackColor = Color.Firebrick;
            this.btnRoulettePlayed1Number32.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number33.BackColor = Color.Black;
            this.btnRoulettePlayed1Number33.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number34.BackColor = Color.Firebrick;
            this.btnRoulettePlayed1Number34.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number35.BackColor = Color.Black;
            this.btnRoulettePlayed1Number35.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number36.BackColor = Color.Firebrick;
            this.btnRoulettePlayed1Number36.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number0.BackColor = Color.SeaGreen;
            this.btnRoulettePlayed2Number0.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number1.BackColor = Color.Firebrick;
            this.btnRoulettePlayed2Number1.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number2.BackColor = Color.Black;
            this.btnRoulettePlayed2Number2.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number3.BackColor = Color.Firebrick;
            this.btnRoulettePlayed2Number3.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number4.BackColor = Color.Black;
            this.btnRoulettePlayed2Number4.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number5.BackColor = Color.Firebrick;
            this.btnRoulettePlayed2Number5.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number6.BackColor = Color.Black;
            this.btnRoulettePlayed2Number6.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number7.BackColor = Color.Firebrick;
            this.btnRoulettePlayed2Number7.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number8.BackColor = Color.Black;
            this.btnRoulettePlayed2Number8.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number9.BackColor = Color.Firebrick;
            this.btnRoulettePlayed2Number9.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number10.BackColor = Color.Black;
            this.btnRoulettePlayed2Number10.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number11.BackColor = Color.Black;
            this.btnRoulettePlayed2Number11.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number12.BackColor = Color.Firebrick;
            this.btnRoulettePlayed2Number12.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number13.BackColor = Color.Black;
            this.btnRoulettePlayed2Number13.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number14.BackColor = Color.Firebrick;
            this.btnRoulettePlayed2Number14.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number15.BackColor = Color.Black;
            this.btnRoulettePlayed2Number15.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number16.BackColor = Color.Firebrick;
            this.btnRoulettePlayed2Number16.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number17.BackColor = Color.Black;
            this.btnRoulettePlayed2Number17.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number18.BackColor = Color.Firebrick;
            this.btnRoulettePlayed2Number18.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number19.BackColor = Color.Firebrick;
            this.btnRoulettePlayed2Number19.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number20.BackColor = Color.Black;
            this.btnRoulettePlayed2Number20.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number21.BackColor = Color.Firebrick;
            this.btnRoulettePlayed2Number21.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number22.BackColor = Color.Black;
            this.btnRoulettePlayed2Number22.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number23.BackColor = Color.Firebrick;
            this.btnRoulettePlayed2Number23.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number24.BackColor = Color.Black;
            this.btnRoulettePlayed2Number24.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number25.BackColor = Color.Firebrick;
            this.btnRoulettePlayed2Number25.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number26.BackColor = Color.Black;
            this.btnRoulettePlayed2Number26.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number27.BackColor = Color.Firebrick;
            this.btnRoulettePlayed2Number27.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number28.BackColor = Color.Black;
            this.btnRoulettePlayed2Number28.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number29.BackColor = Color.Black;
            this.btnRoulettePlayed2Number29.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number30.BackColor = Color.Firebrick;
            this.btnRoulettePlayed2Number30.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number31.BackColor = Color.Black;
            this.btnRoulettePlayed2Number31.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number32.BackColor = Color.Firebrick;
            this.btnRoulettePlayed2Number32.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number33.BackColor = Color.Black;
            this.btnRoulettePlayed2Number33.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number34.BackColor = Color.Firebrick;
            this.btnRoulettePlayed2Number34.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number35.BackColor = Color.Black;
            this.btnRoulettePlayed2Number35.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number36.BackColor = Color.Firebrick;
            this.btnRoulettePlayed2Number36.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number0.BackColor = Color.SeaGreen;
            this.btnRoulettePlayed3Number0.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number1.BackColor = Color.Firebrick;
            this.btnRoulettePlayed3Number1.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number2.BackColor = Color.Black;
            this.btnRoulettePlayed3Number2.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number3.BackColor = Color.Firebrick;
            this.btnRoulettePlayed3Number3.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number4.BackColor = Color.Black;
            this.btnRoulettePlayed3Number4.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number5.BackColor = Color.Firebrick;
            this.btnRoulettePlayed3Number5.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number6.BackColor = Color.Black;
            this.btnRoulettePlayed3Number6.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number7.BackColor = Color.Firebrick;
            this.btnRoulettePlayed3Number7.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number8.BackColor = Color.Black;
            this.btnRoulettePlayed3Number8.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number9.BackColor = Color.Firebrick;
            this.btnRoulettePlayed3Number9.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number10.BackColor = Color.Black;
            this.btnRoulettePlayed3Number10.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number11.BackColor = Color.Black;
            this.btnRoulettePlayed3Number11.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number12.BackColor = Color.Firebrick;
            this.btnRoulettePlayed3Number12.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number13.BackColor = Color.Black;
            this.btnRoulettePlayed3Number13.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number14.BackColor = Color.Firebrick;
            this.btnRoulettePlayed3Number14.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number15.BackColor = Color.Black;
            this.btnRoulettePlayed3Number15.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number16.BackColor = Color.Firebrick;
            this.btnRoulettePlayed3Number16.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number17.BackColor = Color.Black;
            this.btnRoulettePlayed3Number17.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number18.BackColor = Color.Firebrick;
            this.btnRoulettePlayed3Number18.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number19.BackColor = Color.Firebrick;
            this.btnRoulettePlayed3Number19.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number20.BackColor = Color.Black;
            this.btnRoulettePlayed3Number20.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number21.BackColor = Color.Firebrick;
            this.btnRoulettePlayed3Number21.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number22.BackColor = Color.Black;
            this.btnRoulettePlayed3Number22.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number23.BackColor = Color.Firebrick;
            this.btnRoulettePlayed3Number23.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number24.BackColor = Color.Black;
            this.btnRoulettePlayed3Number24.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number25.BackColor = Color.Firebrick;
            this.btnRoulettePlayed3Number25.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number26.BackColor = Color.Black;
            this.btnRoulettePlayed3Number26.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number27.BackColor = Color.Firebrick;
            this.btnRoulettePlayed3Number27.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number28.BackColor = Color.Black;
            this.btnRoulettePlayed3Number28.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number29.BackColor = Color.Black;
            this.btnRoulettePlayed3Number29.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number30.BackColor = Color.Firebrick;
            this.btnRoulettePlayed3Number30.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number31.BackColor = Color.Black;
            this.btnRoulettePlayed3Number31.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number32.BackColor = Color.Firebrick;
            this.btnRoulettePlayed3Number32.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number33.BackColor = Color.Black;
            this.btnRoulettePlayed3Number33.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number34.BackColor = Color.Firebrick;
            this.btnRoulettePlayed3Number34.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number35.BackColor = Color.Black;
            this.btnRoulettePlayed3Number35.ForeColor = SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number36.BackColor = Color.Firebrick;
            this.btnRoulettePlayed3Number36.ForeColor = SystemColors.ControlLightLight;
        }

        private void buttonSaveRouletteConfig_Click(object sender, EventArgs e)
        {
            if (this.CheckConfigRoulette(false))
            {
                string projectPath = Constants.PathProject();
                string folderToReadFile = Path.Combine(projectPath, "appData");
                string filenameDialog = (this.lblRouletteNameConfig.Text.Equals("<<Nessuna configurazione caricata>>") ? "" : this.lblRouletteNameConfig.Text);
                SaveFileDialog sfd = new SaveFileDialog
                {
                    InitialDirectory = folderToReadFile,
                    Title = "File senza nome",
                    CheckPathExists = true,
                    DefaultExt = "txt",
                    Filter = "Text files (*.rou)|*.rou",
                    FilterIndex = 1,
                    RestoreDirectory = true,
                    FileName = filenameDialog
                };
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    string filenameToSave = Path.Combine(projectPath, "appData", sfd.FileName);
                    this.lblRouletteNameConfig.Text = Path.GetFileName(sfd.FileName);
                    this.ReadRouletteParamForm();
                    this.SaveRouletteDataForm(filenameToSave);
                }
            }
        }

        private void btnRouletteAreaHand1_Click(object sender, EventArgs e)
        {
            this.GetZoom();
            UIForm.ClickButtonRoulette(sender, "R_Hand1");
        }

        private void btnRouletteAreaHand2_Click(object sender, EventArgs e)
        {
            this.GetZoom();
            UIForm.ClickButtonRoulette(sender, "R_Hand2");
        }

        private void btnRouletteAreaHand3_Click(object sender, EventArgs e)
        {
            this.GetZoom();
            UIForm.ClickButtonRoulette(sender, "R_Hand3");
        }

        private void btnRouletteAreaWin_Click(object sender, EventArgs e)
        {
            this.GetZoom();
            UIForm.ClickButtonRoulette(sender, "R_Win");
        }

        private void btnRouletteAreaWait_Click(object sender, EventArgs e)
        {
            this.GetZoom();
            UIForm.ClickButtonRoulette(sender, "R_Wait");
        }

        public void RouletteSettingUIStop()
        {
            if (Runtime.game == 0)
            {
                UIForm.EnableAddButtonItem(this.btnRouletteStart);
            }
            else if (Runtime.game == 1)
            {
                UIForm.EnableAddButtonItem(this.buttonStart);
                this.btnRouletteStart.Text = "AVVIA ▶";
            }
            this.balanceRouletteStartValue.Value = (decimal)RouletteValues.Runtime.balance;
            UIForm.EnableAddButtonItem(this.btnRouletteLoadConfig);
            UIForm.EnableAddButtonItem(this.btnRouletteSaveConfig);
            foreach (Control control in this.controlsRouletteToEnableDisable)
            {
                UIForm.EnableRouletteItem(control);
            }
            foreach (Control control2 in this.controlsRouletteToEnableDisableHand1)
            {
                UIForm.EnableRouletteItem(control2);
            }
            foreach (Control control3 in this.controlsRouletteToEnableDisableHand2)
            {
                UIForm.EnableRouletteItem(control3);
            }
            foreach (Control control4 in this.controlsRouletteToEnableDisableHand3)
            {
                UIForm.EnableRouletteItem(control4);
            }
        }

        public void ReadRouletteParamForm()
        {
            RouletteValues.Config.stop_win = this.globalRouletteStopWin.Value;
            RouletteValues.Config.stop_loss = this.globalRouletteStopLoss.Value;
            RouletteValues.Config.hand_value_1 = this.numericRouletteValueHand1.Value;
            RouletteValues.Config.hand_value_2 = this.numericRouletteValueHand2.Value;
            RouletteValues.Config.hand_value_3 = this.numericRouletteValueHand3.Value;
            RouletteValues.Runtime.balanceInit = (float)this.balanceRouletteStartValue.Value;
            RouletteValues.Runtime.balance = (float)this.balanceRouletteStartValue.Value;
        }

        public void SaveRouletteDataForm(string filename)
        {
            Dictionary<string, AreaElement> areaElement = ListAreaElement.Instance.GetAllArea();
            List<int> h1Numbers = new List<int>();
            foreach (BtnFiches bf in Roulette.Instance.GetNumbersOfHand(1))
            {
                h1Numbers.Add(bf.Value);
            }
            List<int> h2Numbers = new List<int>();
            foreach (BtnFiches bf2 in Roulette.Instance.GetNumbersOfHand(2))
            {
                h2Numbers.Add(bf2.Value);
            }
            List<int> h3Numbers = new List<int>();
            foreach (BtnFiches bf3 in Roulette.Instance.GetNumbersOfHand(3))
            {
                h3Numbers.Add(bf3.Value);
            }
            if (!areaElement.ContainsKey("AREA_SALDO_ROULETTE"))
            {
                AreaElement nae = new AreaElement();
                nae.startX = 0;
                nae.startY = 0;
                nae.endX = 0;
                nae.endY = 0;
                ListAreaElement.Instance.AddArea("AREA_SALDO_ROULETTE", nae);
            }
            List<JSONSingleConfig2> listJson = new List<JSONSingleConfig2>
            {
                new JSONSingleConfig2
                {
                    ConfigRoulette = new JSONSingleConfigRoulette
                    {
                        RouletteStopWin = this.globalRouletteStopWin.Value,
                        RouletteStopLoss = this.globalRouletteStopLoss.Value,
                        RouletteHandArea1 = this.SingleArea(areaElement["R_Hand1"]),
                        RouletteHandArea2 = this.SingleArea(areaElement["R_Hand2"]),
                        RouletteHandArea3 = this.SingleArea(areaElement["R_Hand3"]),
                        RouletteWinArea = this.SingleArea(areaElement["R_Win"]),
                        RouletteAreaSaldo = this.SingleArea(areaElement["AREA_SALDO_ROULETTE"]),
                        RouletteWaitingArea = this.SingleArea(areaElement["R_Wait"]),
                        RouletteValueHand1 = this.numericRouletteValueHand1.Value,
                        RouletteValueHand2 = this.numericRouletteValueHand2.Value,
                        RouletteValueHand3 = this.numericRouletteValueHand3.Value,
                        RouletteHand1Numbers = h1Numbers,
                        RouletteHand2Numbers = h2Numbers,
                        RouletteHand3Numbers = h3Numbers,
                        Zoom = this.txtZoomMonitor.Text
                    },
                    ConfigTelegram = new JSONSingleConfigTelegram
                    {
                        VerifiedCode = this.textVerifiedCode.Text,
                        PhoneNumber = this.textActualPhone.Text,
                        GroupChatName = this.textChatName.Text
                    }
                }
            };
            string textFile = JsonSerializer.Serialize<JSONConfig2>(new JSONConfig2
            {
                User = "utente_1",
                Configs = listJson
            }, default(JsonSerializerOptions));
            ManageFile.SaveFile("appData", textFile, filename, false, true);
        }

        public void EnableCompleteUIForm()
        {
            this.RouletteSettingUIStop();
            this.SettingUIStop();
        }

        public void THREAD_MOD(string teste)
        {
            this.RouletteSettingUIStop();
            this.SettingUIStop();
        }

        private void _ReadJsonRoulette(JSONSingleConfigRoulette currentConfig)
        {
            this.globalRouletteStopWin.Value = currentConfig.RouletteStopWin;
            this.globalRouletteStopLoss.Value = currentConfig.RouletteStopLoss;
            this.numericRouletteValueHand1.Value = currentConfig.RouletteValueHand1;
            this.numericRouletteValueHand2.Value = currentConfig.RouletteValueHand2;
            this.numericRouletteValueHand3.Value = currentConfig.RouletteValueHand3;
            Roulette.Instance.CleanHands();
            ListAreaElement.Instance.ClearAll();
            ListAreaElement.Instance.AddArea("R_Hand1", currentConfig.RouletteHandArea1.GetArea());
            ListAreaElement.Instance.AddArea("R_Hand2", currentConfig.RouletteHandArea2.GetArea());
            ListAreaElement.Instance.AddArea("R_Hand3", currentConfig.RouletteHandArea3.GetArea());
            ListAreaElement.Instance.AddArea("R_Win", currentConfig.RouletteWinArea.GetArea());
            ListAreaElement.Instance.AddArea("R_Wait", currentConfig.RouletteWaitingArea.GetArea());
            if (currentConfig.RouletteAreaSaldo != null && currentConfig.RouletteAreaSaldo.startX != 0)
            {
                ListAreaElement.Instance.AddArea("AREA_SALDO_ROULETTE", currentConfig.RouletteAreaSaldo.GetArea());
                currentConfig.RouletteAreaSaldo.GetArea();
            }
            else
            {
                UIForm.SelectButtonStandard(this.buttonBalanceAreaRoulette);
            }
            UIForm.SelectButtonRoulette(this.btnRouletteOCRHand1);
            UIForm.SelectButtonRoulette(this.btnRouletteOCRHand2);
            UIForm.SelectButtonRoulette(this.btnRouletteOCRHand3);
            UIForm.SelectButtonRoulette(this.btnRouletteOCRWinArea);
            UIForm.SelectButtonRoulette(this.btnRouletteOCRWaitingArea);
            if (currentConfig.RouletteAreaSaldo != null)
            {
                UIForm.SelectButtonGreen(this.buttonBalanceAreaRoulette);
            }
            if (currentConfig.RouletteHand1Numbers != null && currentConfig.RouletteHand1Numbers.Count > 0)
            {
                foreach (int i in currentConfig.RouletteHand1Numbers)
                {
                    Button b = this.getButtonFromNumber(i, 1);
                    BtnFiches buttonFiches = Roulette.Instance.AddNumberToList(new BtnFiches(i, b.BackColor, b.ForeColor, b.FlatAppearance.BorderColor), 1);
                    if (!buttonFiches.Removed)
                    {
                        UIForm.SelectButtonFichesRoulette(b);
                    }
                    else
                    {
                        UIForm.DeselectButtonFichesRoulette(b, buttonFiches);
                        b.BackColor = buttonFiches.BackCurrentColor;
                    }
                }
            }
            if (currentConfig.RouletteHand2Numbers != null && currentConfig.RouletteHand2Numbers.Count > 0)
            {
                foreach (int j in currentConfig.RouletteHand2Numbers)
                {
                    Button b2 = this.getButtonFromNumber(j, 2);
                    BtnFiches buttonFiches2 = Roulette.Instance.AddNumberToList(new BtnFiches(j, b2.BackColor, b2.ForeColor, b2.FlatAppearance.BorderColor), 2);
                    if (!buttonFiches2.Removed)
                    {
                        UIForm.SelectButtonFichesRoulette(b2);
                    }
                    else
                    {
                        UIForm.DeselectButtonFichesRoulette(b2, buttonFiches2);
                        b2.BackColor = buttonFiches2.BackCurrentColor;
                    }
                }
            }
            if (currentConfig.RouletteHand3Numbers != null && currentConfig.RouletteHand3Numbers.Count > 0)
            {
                foreach (int k in currentConfig.RouletteHand3Numbers)
                {
                    Button b3 = this.getButtonFromNumber(k, 3);
                    BtnFiches buttonFiches3 = Roulette.Instance.AddNumberToList(new BtnFiches(k, b3.BackColor, b3.ForeColor, b3.FlatAppearance.BorderColor), 3);
                    if (!buttonFiches3.Removed)
                    {
                        UIForm.SelectButtonFichesRoulette(b3);
                    }
                    else
                    {
                        UIForm.DeselectButtonFichesRoulette(b3, buttonFiches3);
                        b3.BackColor = buttonFiches3.BackCurrentColor;
                    }
                }
            }
            this.txtZoomMonitor.Text = currentConfig.Zoom;
        }

        public NumericUpDown getStartButton()
        {
            return this.globalRouletteStopWin;
        }

        public Button getButtonFromNumber(int n, int h)
        {
            switch (h)
            {
                case 1:
                    switch (n)
                    {
                        case 0:
                            return this.btnRoulettePlayed1Number0;
                        case 1:
                            return this.btnRoulettePlayed1Number1;
                        case 2:
                            return this.btnRoulettePlayed1Number2;
                        case 3:
                            return this.btnRoulettePlayed1Number3;
                        case 4:
                            return this.btnRoulettePlayed1Number4;
                        case 5:
                            return this.btnRoulettePlayed1Number5;
                        case 6:
                            return this.btnRoulettePlayed1Number6;
                        case 7:
                            return this.btnRoulettePlayed1Number7;
                        case 8:
                            return this.btnRoulettePlayed1Number8;
                        case 9:
                            return this.btnRoulettePlayed1Number9;
                        case 10:
                            return this.btnRoulettePlayed1Number10;
                        case 11:
                            return this.btnRoulettePlayed1Number11;
                        case 12:
                            return this.btnRoulettePlayed1Number12;
                        case 13:
                            return this.btnRoulettePlayed1Number13;
                        case 14:
                            return this.btnRoulettePlayed1Number14;
                        case 15:
                            return this.btnRoulettePlayed1Number15;
                        case 16:
                            return this.btnRoulettePlayed1Number16;
                        case 17:
                            return this.btnRoulettePlayed1Number17;
                        case 18:
                            return this.btnRoulettePlayed1Number18;
                        case 19:
                            return this.btnRoulettePlayed1Number19;
                        case 20:
                            return this.btnRoulettePlayed1Number20;
                        case 21:
                            return this.btnRoulettePlayed1Number21;
                        case 22:
                            return this.btnRoulettePlayed1Number22;
                        case 23:
                            return this.btnRoulettePlayed1Number23;
                        case 24:
                            return this.btnRoulettePlayed1Number24;
                        case 25:
                            return this.btnRoulettePlayed1Number25;
                        case 26:
                            return this.btnRoulettePlayed1Number26;
                        case 27:
                            return this.btnRoulettePlayed1Number27;
                        case 28:
                            return this.btnRoulettePlayed1Number28;
                        case 29:
                            return this.btnRoulettePlayed1Number29;
                        case 30:
                            return this.btnRoulettePlayed1Number30;
                        case 31:
                            return this.btnRoulettePlayed1Number31;
                        case 32:
                            return this.btnRoulettePlayed1Number32;
                        case 33:
                            return this.btnRoulettePlayed1Number33;
                        case 34:
                            return this.btnRoulettePlayed1Number34;
                        case 35:
                            return this.btnRoulettePlayed1Number35;
                        case 36:
                            return this.btnRoulettePlayed1Number36;
                        default:
                            return null;
                    }
                    break;
                case 2:
                    switch (n)
                    {
                        case 0:
                            return this.btnRoulettePlayed2Number0;
                        case 1:
                            return this.btnRoulettePlayed2Number1;
                        case 2:
                            return this.btnRoulettePlayed2Number2;
                        case 3:
                            return this.btnRoulettePlayed2Number3;
                        case 4:
                            return this.btnRoulettePlayed2Number4;
                        case 5:
                            return this.btnRoulettePlayed2Number5;
                        case 6:
                            return this.btnRoulettePlayed2Number6;
                        case 7:
                            return this.btnRoulettePlayed2Number7;
                        case 8:
                            return this.btnRoulettePlayed2Number8;
                        case 9:
                            return this.btnRoulettePlayed2Number9;
                        case 10:
                            return this.btnRoulettePlayed2Number10;
                        case 11:
                            return this.btnRoulettePlayed2Number11;
                        case 12:
                            return this.btnRoulettePlayed2Number12;
                        case 13:
                            return this.btnRoulettePlayed2Number13;
                        case 14:
                            return this.btnRoulettePlayed2Number14;
                        case 15:
                            return this.btnRoulettePlayed2Number15;
                        case 16:
                            return this.btnRoulettePlayed2Number16;
                        case 17:
                            return this.btnRoulettePlayed2Number17;
                        case 18:
                            return this.btnRoulettePlayed2Number18;
                        case 19:
                            return this.btnRoulettePlayed2Number19;
                        case 20:
                            return this.btnRoulettePlayed2Number20;
                        case 21:
                            return this.btnRoulettePlayed2Number21;
                        case 22:
                            return this.btnRoulettePlayed2Number22;
                        case 23:
                            return this.btnRoulettePlayed2Number23;
                        case 24:
                            return this.btnRoulettePlayed2Number24;
                        case 25:
                            return this.btnRoulettePlayed2Number25;
                        case 26:
                            return this.btnRoulettePlayed2Number26;
                        case 27:
                            return this.btnRoulettePlayed2Number27;
                        case 28:
                            return this.btnRoulettePlayed2Number28;
                        case 29:
                            return this.btnRoulettePlayed2Number29;
                        case 30:
                            return this.btnRoulettePlayed2Number30;
                        case 31:
                            return this.btnRoulettePlayed2Number31;
                        case 32:
                            return this.btnRoulettePlayed2Number32;
                        case 33:
                            return this.btnRoulettePlayed2Number33;
                        case 34:
                            return this.btnRoulettePlayed2Number34;
                        case 35:
                            return this.btnRoulettePlayed2Number35;
                        case 36:
                            return this.btnRoulettePlayed2Number36;
                        default:
                            return null;
                    }
                    break;
                case 3:
                    switch (n)
                    {
                        case 0:
                            return this.btnRoulettePlayed3Number0;
                        case 1:
                            return this.btnRoulettePlayed3Number1;
                        case 2:
                            return this.btnRoulettePlayed3Number2;
                        case 3:
                            return this.btnRoulettePlayed3Number3;
                        case 4:
                            return this.btnRoulettePlayed3Number4;
                        case 5:
                            return this.btnRoulettePlayed3Number5;
                        case 6:
                            return this.btnRoulettePlayed3Number6;
                        case 7:
                            return this.btnRoulettePlayed3Number7;
                        case 8:
                            return this.btnRoulettePlayed3Number8;
                        case 9:
                            return this.btnRoulettePlayed3Number9;
                        case 10:
                            return this.btnRoulettePlayed3Number10;
                        case 11:
                            return this.btnRoulettePlayed3Number11;
                        case 12:
                            return this.btnRoulettePlayed3Number12;
                        case 13:
                            return this.btnRoulettePlayed3Number13;
                        case 14:
                            return this.btnRoulettePlayed3Number14;
                        case 15:
                            return this.btnRoulettePlayed3Number15;
                        case 16:
                            return this.btnRoulettePlayed3Number16;
                        case 17:
                            return this.btnRoulettePlayed3Number17;
                        case 18:
                            return this.btnRoulettePlayed3Number18;
                        case 19:
                            return this.btnRoulettePlayed3Number19;
                        case 20:
                            return this.btnRoulettePlayed3Number20;
                        case 21:
                            return this.btnRoulettePlayed3Number21;
                        case 22:
                            return this.btnRoulettePlayed3Number22;
                        case 23:
                            return this.btnRoulettePlayed3Number23;
                        case 24:
                            return this.btnRoulettePlayed3Number24;
                        case 25:
                            return this.btnRoulettePlayed3Number25;
                        case 26:
                            return this.btnRoulettePlayed3Number26;
                        case 27:
                            return this.btnRoulettePlayed3Number27;
                        case 28:
                            return this.btnRoulettePlayed3Number28;
                        case 29:
                            return this.btnRoulettePlayed3Number29;
                        case 30:
                            return this.btnRoulettePlayed3Number30;
                        case 31:
                            return this.btnRoulettePlayed3Number31;
                        case 32:
                            return this.btnRoulettePlayed3Number32;
                        case 33:
                            return this.btnRoulettePlayed3Number33;
                        case 34:
                            return this.btnRoulettePlayed3Number34;
                        case 35:
                            return this.btnRoulettePlayed3Number35;
                        case 36:
                            return this.btnRoulettePlayed3Number36;
                        default:
                            return null;
                    }
                    break;
                default:
                    return null;
            }
        }

        private bool CheckConfigRoulette(bool checkPlay = true)
        {
            string titleError = "Errore";
            if (Roulette.Instance.GetNumOfNumbers(1) < 1)
            {
                MessageBox.Show("Non ci sono numeri in Giocata #1", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (Roulette.Instance.GetNumOfNumbers(2) < 1)
            {
                MessageBox.Show("Non ci sono numeri in Giocata #2", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (Roulette.Instance.GetNumOfNumbers(3) < 1)
            {
                MessageBox.Show("Non ci sono numeri in Giocata #3", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (this.globalRouletteStopWin.Text == "")
            {
                MessageBox.Show("Inserire un valore nel campo \"Global Stop Win\"", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (this.globalRouletteStopLoss.Text == "")
            {
                MessageBox.Show("Inserire un valore nel campo \"Global Stop Loss\"", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (this.balanceRouletteStartValue.Text == "" && checkPlay)
            {
                MessageBox.Show("Inserire un valore nel campo \"Saldo iniziale\"", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            double num = Convert.ToDouble(this.globalRouletteStopWin.Text);
            double globalRouletteStopLossValue = Convert.ToDouble(this.globalRouletteStopLoss.Text);
            double numericRouletteValueHand1Value = Convert.ToDouble(this.numericRouletteValueHand1.Text);
            double numericRouletteValueHand2Value = Convert.ToDouble(this.numericRouletteValueHand2.Text);
            double numericRouletteValueHand3Value = Convert.ToDouble(this.numericRouletteValueHand3.Text);
            float balanceStart = (float)this.balanceRouletteStartValue.Value;
            if (num <= 0.0)
            {
                MessageBox.Show("Inserire un valore maggiore di 0 nel campo \"Global Stop Win\"", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (globalRouletteStopLossValue <= 0.0)
            {
                MessageBox.Show("Inserire un valore maggiore di 0 nel campo \"Global Stop Loss\"", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (ListAreaElement.Instance.GetAreaByKey("R_Hand1") == null)
            {
                MessageBox.Show("Definire l'area \"Giocata #1\"", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (ListAreaElement.Instance.GetAreaByKey("R_Hand2") == null)
            {
                MessageBox.Show("Definire l'area \"Giocata #2\"", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (ListAreaElement.Instance.GetAreaByKey("R_Hand3") == null)
            {
                MessageBox.Show("Definire l'area \"Giocata #3\"", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (ListAreaElement.Instance.GetAreaByKey("R_Win") == null)
            {
                MessageBox.Show("Definire l'area \"Area Vincita\"", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (ListAreaElement.Instance.GetAreaByKey("R_Wait") == null)
            {
                MessageBox.Show("Definire l'area \"Area Riposo\"", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (numericRouletteValueHand1Value <= 0.0)
            {
                MessageBox.Show("Inserire un valore maggiore di 0 nel campo \"Valore Giocata #1\"", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (numericRouletteValueHand2Value <= 0.0)
            {
                MessageBox.Show("Inserire un valore maggiore di 0 nel campo \"Valore Giocata #2\"", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (numericRouletteValueHand3Value <= 0.0)
            {
                MessageBox.Show("Inserire un valore maggiore di 0 nel campo \"Valore Giocata #3\"", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (balanceStart <= 0f && checkPlay)
            {
                MessageBox.Show("Inserire un valore maggiore di 0 nel campo \"Saldo iniziale\"", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            return true;
        }
        #endregion

        private void GetZoom()
        {
            int zoom = 100;
            if (!string.IsNullOrEmpty(this.txtZoomMonitor.Text))
            {
                zoom = Convert.ToInt32(this.txtZoomMonitor.Text);
            }
            Config.zoom = zoom;
        }

        private void buttonRed_Click(object sender, EventArgs e)
        {
            this.GetZoom();
            UIForm.ClickButtonGreen(sender, "ROSSO");
        }

        private void buttonBlu_Click(object sender, EventArgs e)
        {
            this.GetZoom();
            UIForm.ClickButtonGreen(sender, "BLU");
        }

        private void buttonAreaCentrale_Click(object sender, EventArgs e)
        {
            this.GetZoom();
            UIForm.ClickButtonGreen(sender, "AREA_CENTRALE");
        }

        private void buttonAreaVincita_Click(object sender, EventArgs e)
        {
            this.GetZoom();
            UIForm.ClickButtonGreen(sender, "AREA_VINCITA");
        }

        private void buttonDoubling_Click(object sender, EventArgs e)
        {
            this.GetZoom();
            UIForm.ClickButtonGreen(sender, "AREA_RADDOPPIO");
        }

        private void buttonDeckArea_Click(object sender, EventArgs e)
        {
            this.GetZoom();
            UIForm.ClickButtonGreen(sender, "AREA_MAZZO");
        }

        private void buttonBalanceArea_Click(object sender, EventArgs e)
        {
            this.GetZoom();
            UIForm.ClickButtonGreen(sender, "AREA_SALDO");
        }

        private void buttonFish1_Click(object sender, EventArgs e)
        {
            this.GetZoom();
            UIForm.ClickButtonGreen(sender, "FICHE_1");
        }

        private void buttonFish5_Click(object sender, EventArgs e)
        {
            this.GetZoom();
            UIForm.ClickButtonGreen(sender, "FICHE_5");
        }

        private void buttonFish25_Click(object sender, EventArgs e)
        {
            this.GetZoom();
            UIForm.ClickButtonGreen(sender, "FICHE_25");
        }

        private void buttonFish100_Click(object sender, EventArgs e)
        {
            this.GetZoom();
            UIForm.ClickButtonGreen(sender, "FICHE_100");
        }

        private void buttonFish250_Click(object sender, EventArgs e)
        {
            this.GetZoom();
            UIForm.ClickButtonGreen(sender, "FICHE_250");
        }

        private void buttonFish500_Click(object sender, EventArgs e)
        {
            this.GetZoom();
            UIForm.ClickButtonGreen(sender, "FICHE_500");
        }

        private async void buttonStart_Click(object sender, EventArgs e)
        {
            Logger.WriteLog("Test log");




            Runtime.game = 0;
            Log.PrintInfo("STARTING BACCARAT!!!1!");
            Runtime.ocrBalanceCorrect = 0;
            Runtime.ocrBalanceIncorrect = 0;
            if (Runtime.current_state_bot != Constants.EnumStateBot.IDLE)
            {
                avvio = "0";
                stop_all();
                timerStart.Enabled = true;
            }
            else if (await MainStateBot.CheckConnection())
            {
                avvio = "1";
                if (checkBoxAutoSaldo.Checked)
                {
                    start_withPreScan();
                }
                else
                {
                    start_all(bypass: false);
                }
            }
            else
            {
                MessageBox.Show("Impossibile raggiungere il server di autenticazione.\nControllare la connessione ad internet.\nSe il problema persiste contattare l’assistenza.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        public void stop_all()
        {
            this.SettingUIStop();
            this.RouletteSettingUIStop();
            Player.Instance.Stop();
        }

        public void start_all(bool bypass)
        {
            if (!this.CheckConfigBacarat(true))
            {
                return;
            }
            bool start = false;
            if (!bypass)
            {
                start = MessageBox.Show("Controllare che il numero del Deck sia inferiore a 50.\nL'avvio oltre tale numero potrebbe portare a comportamenti inaspettati\n\nAvviare il bot?", "ATTENZIONE", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes;
            }
            if (start || bypass)
            {
                string projectPath = Constants.PathProject();
                Path.Combine(projectPath, "appData");
                if ((this.lblNameConfig.Text.Equals("<<Nessuna configurazione caricata>>") ? "" : this.lblNameConfig.Text).Equals(string.Empty))
                {
                    MessageBox.Show("Caricare o salvare una configurazione", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }
                string filenameToSave = Path.Combine(projectPath, "appData", this.lblNameConfig.Text);
                this.ReadParamForm();
                this.SaveDataFormCustomFiches(filenameToSave);
                this.SettingUIStart();
                this.SettingUIRouletteStart();
                Player.Instance.Start();

                //stoppo il timer per fare ripartire da dashboard Eugenio
                timerStart.Enabled = false;
            }
        }

        private void globalStopWinValue_ChangeDotToComma(object sender, EventArgs e)
        {
            this.globalStopWinValue.Text = UIForm.ReplaceDotIntoCommaValueText(this.globalStopWinValue.Text);
        }

        private void stopWinValue_ChangeDotToComma(object sender, EventArgs e)
        {
            this.stopWinValue.Text = UIForm.ReplaceDotIntoCommaValueText(this.stopWinValue.Text);
        }

        private void stopLossValue_ChangeDotToComma(object sender, EventArgs e)
        {
            this.stopLossValue.Text = UIForm.ReplaceDotIntoCommaValueText(this.stopLossValue.Text);
        }

        private void safeWinPerc_ChangeDotToComma(object sender, EventArgs e)
        {
            this.safeWinPerc.Text = UIForm.ReplaceDotIntoCommaValueText(this.safeWinPerc.Text);
        }

        private void balanceStartValue_ChangeDotToComma(object sender, EventArgs e)
        {
            try
            {
                this.balanceStartValue.Text = UIForm.ReplaceDotIntoCommaValueText(this.balanceStartValue.Text);
            }
            catch (Exception)
            {

            }

        }



        private void txtZoomMonitor_RemoveDotAndComma(object sender, EventArgs e)
        {
            this.txtZoomMonitor.Text = UIForm.ReplaceDotAndCommaValueText(this.txtZoomMonitor.Text);
        }

        private void martingala1StartDeckValue_RemoveDotAndComma(object sender, EventArgs e)
        {
            this.martingala1StartDeckValue.Text = UIForm.ReplaceDotAndCommaValueText(this.martingala1StartDeckValue.Text);
        }

        private void martingala1EndDeckValue_RemoveDotAndComma(object sender, EventArgs e)
        {
            this.martingala1EndDeckValue.Text = UIForm.ReplaceDotAndCommaValueText(this.martingala1EndDeckValue.Text);
        }

        private void martingala2StartDeckValue_RemoveDotAndComma(object sender, EventArgs e)
        {
            this.martingala2StartDeckValue.Text = UIForm.ReplaceDotAndCommaValueText(this.martingala2StartDeckValue.Text);
        }

        private void martingala2EndDeckValue_RemoveDotAndComma(object sender, EventArgs e)
        {
            this.martingala2EndDeckValue.Text = UIForm.ReplaceDotAndCommaValueText(this.martingala2EndDeckValue.Text);
        }

        private void martingala3StartDeckValue_RemoveDotAndComma(object sender, EventArgs e)
        {
            this.martingala3StartDeckValue.Text = UIForm.ReplaceDotAndCommaValueText(this.martingala3StartDeckValue.Text);
        }

        private void martingala3EndDeckValue_RemoveDotAndComma(object sender, EventArgs e)
        {
            this.martingala3EndDeckValue.Text = UIForm.ReplaceDotAndCommaValueText(this.martingala3EndDeckValue.Text);
        }

        private void martingala4StartDeckValue_RemoveDotAndComma(object sender, EventArgs e)
        {
            this.martingala4StartDeckValue.Text = UIForm.ReplaceDotAndCommaValueText(this.martingala4StartDeckValue.Text);
        }

        private void martingala4EndDeckValue_RemoveDotAndComma(object sender, EventArgs e)
        {
            this.martingala4EndDeckValue.Text = UIForm.ReplaceDotAndCommaValueText(this.martingala4EndDeckValue.Text);
        }

        private void martingala1ChangeColorValue_RemoveDotAndComma(object sender, EventArgs e)
        {
            this.martingala1ChangeColorValue.Text = UIForm.ReplaceDotAndCommaValueText(this.martingala1ChangeColorValue.Text);
        }

        private void martingala2ChangeColorValue_RemoveDotAndComma(object sender, EventArgs e)
        {
            this.martingala2ChangeColorValue.Text = UIForm.ReplaceDotAndCommaValueText(this.martingala2ChangeColorValue.Text);
        }

        private void martingala3ChangeColorValue_RemoveDotAndComma(object sender, EventArgs e)
        {
            this.martingala3ChangeColorValue.Text = UIForm.ReplaceDotAndCommaValueText(this.martingala3ChangeColorValue.Text);
        }

        private void martingala4ChangeColorValue_RemoveDotAndComma(object sender, EventArgs e)
        {
            this.martingala4ChangeColorValue.Text = UIForm.ReplaceDotAndCommaValueText(this.martingala4ChangeColorValue.Text);
        }

        private void martingala1IndexAlarmValue_RemoveDotAndComma(object sender, EventArgs e)
        {
            this.martingala1IndexAlarmValue.Text = UIForm.ReplaceDotAndCommaValueText(this.martingala1IndexAlarmValue.Text);
        }

        private void martingala2IndexAlarmValue_RemoveDotAndComma(object sender, EventArgs e)
        {
            this.martingala2IndexAlarmValue.Text = UIForm.ReplaceDotAndCommaValueText(this.martingala2IndexAlarmValue.Text);
        }

        private void martingala3IndexAlarmValue_RemoveDotAndComma(object sender, EventArgs e)
        {
            this.martingala3IndexAlarmValue.Text = UIForm.ReplaceDotAndCommaValueText(this.martingala3IndexAlarmValue.Text);
        }

        private void martingala4IndexAlarmValue_RemoveDotAndComma(object sender, EventArgs e)
        {
            this.martingala4IndexAlarmValue.Text = UIForm.ReplaceDotAndCommaValueText(this.martingala4IndexAlarmValue.Text);
        }

        private bool CheckConfigBacarat(bool checkPlay = true)
        {
            string titleError = "Errore";
            int numberItemMartingala = 0;
            foreach (Control item in this.panelMartingala.Controls.OfType<Control>().ToList<Control>())
            {
                if (item is NumericUpDown)
                {
                    numberItemMartingala++;
                    if (item.Text == "")
                    {
                        MessageBox.Show("Un valore della Martingala non è corretto", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        return false;
                    }
                    if (Convert.ToDouble(item.Text) <= 0.0)
                    {
                        MessageBox.Show("Inserire un valore maggiore di 0 nei campi della Martingala", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        return false;
                    }
                }
            }
            if (numberItemMartingala == 0)
            {
                MessageBox.Show("Inserire almeno un valore nella Martingala", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (this.globalStopWinValue.Text == "")
            {
                MessageBox.Show("Inserire un valore nel campo \"Stop Win Glob\"", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (this.stopWinValue.Text == "")
            {
                MessageBox.Show("Inserire un valore nel campo \"Stop Win\"", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (this.stopLossValue.Text == "")
            {
                MessageBox.Show("Inserire un valore nel campo \"Stop Loss\"", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (this.safeWinPerc.Text == "")
            {
                MessageBox.Show("Inserire un valore nel campo \"Safe win\"", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (this.numberChangeEndDeck.Text == "")
            {
                MessageBox.Show("Inserire un valore nel campo \"Numero Cambio Fine Mazzo\"", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (this.balanceStartValue.Text == "" && checkPlay)
            {
                MessageBox.Show("Inserire un valore nel campo \"Saldo iniziale\"", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            double globalStopWin = Convert.ToDouble(this.globalStopWinValue.Text);
            double stopWin = Convert.ToDouble(this.stopWinValue.Text);
            double stopLoss = Convert.ToDouble(this.stopLossValue.Text);
            int safeWinCon = Convert.ToInt32(this.safeWinPerc.Text);
            float balanceStart = (float)this.balanceStartValue.Value;
            int numberOfDeck = Convert.ToInt32(this.numberChangeEndDeck.Text);
            if (globalStopWin <= 0.0)
            {
                MessageBox.Show("Inserire un valore maggiore di 0 nel campo \"Stop Win Glob\"", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (stopWin <= 0.0)
            {
                MessageBox.Show("Inserire un valore maggiore di 0 nel campo \"Stop Win\"", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (globalStopWin < stopWin)
            {
                MessageBox.Show("Il valore del campo \"Stop Win Glob\" deve essere maggiore del valore del campo \"Stop Win\"", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (stopLoss <= 0.0)
            {
                MessageBox.Show("Inserire un valore maggiore di 0 nel campo \"Stop Loss\"", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (safeWinCon <= 0)
            {
                MessageBox.Show("Inserire un valore maggiore di 0 nel campo \"Safe Win\"", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (numberOfDeck <= 0)
            {
                MessageBox.Show("Inserire un valore maggiore di 0 nel campo \"Numero Cambio Fine Mazzo\"", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (balanceStart <= 0f && checkPlay)
            {
                MessageBox.Show("Inserire un valore maggiore di 0 nel campo \"Saldo iniziale\"", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (ListAreaElement.Instance.GetAreaByKey("ROSSO") == null)
            {
                MessageBox.Show("Definire l'area \"Rosso\"", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (ListAreaElement.Instance.GetAreaByKey("BLU") == null)
            {
                MessageBox.Show("Definire l'area \"Blu\"", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (ListAreaElement.Instance.GetAreaByKey("AREA_CENTRALE") == null)
            {
                MessageBox.Show("Definire l'area \"Area Riposo\"", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (ListAreaElement.Instance.GetAreaByKey("AREA_VINCITA") == null)
            {
                MessageBox.Show("Definire l'area \"Area Vincita\"", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (ListAreaElement.Instance.GetAreaByKey("AREA_MAZZO") == null)
            {
                MessageBox.Show("Definire l'area \"Area Mazzo\"", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (ListAreaElement.Instance.GetAreaByKey("AREA_PUNTARE") == null)
            {
                MessageBox.Show("Definire l'area \"Area Puntare\"", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (ListAreaElement.Instance.GetAreaByKey("AREA_RADDOPPIO") == null)
            {
                MessageBox.Show("Definire l'area \"Area Raddoppio\"", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (Runtime.custom_fiches.Count < 1)
            {
                MessageBox.Show("Inserire almeno una fiche personalizzata", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            foreach (CustomFiche cf in Runtime.custom_fiches)
            {
                CustomFicheWidget cfw = CustomFicheWidgetsContainer.getCustomFicheWidgetByValue(cf.getValue());
                if (cfw == null)
                {
                    MessageBox.Show("Definire l'area \"" + cf.getDicitura() + "\"", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return false;
                }
                AreaElement ae = cfw.getArea();
                if (ae.startX == 0 || ae.endX == 0 || ae.startY == 0 || ae.endY == 0)
                {
                    MessageBox.Show("Definire l'area \"" + cfw.getTag() + "\"", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return false;
                }
            }
            if (!this.radioColorBlu.Checked && !this.radioColorRed.Checked)
            {
                MessageBox.Show("Selezionare un colore di partenza fra \"Rosso\" e \"Blu\"", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (!this.radioModeAlternata.Checked && !this.radioModeMonocolore.Checked)
            {
                MessageBox.Show("Selezionare una modalità di gioco fra \"Alternata\" e \"Monocolore\"", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            bool martingalaSet = true;
            if (this.martingala1StartDeckValue.Value == 0m && this.martingala1EndDeckValue.Value == 0m && this.martingala2StartDeckValue.Value == 0m && this.martingala2EndDeckValue.Value == 0m && this.martingala3StartDeckValue.Value == 0m && this.martingala3EndDeckValue.Value == 0m && this.martingala4StartDeckValue.Value == 0m && this.martingala4EndDeckValue.Value == 0m)
            {
                martingalaSet = false;
            }
            else
            {
                if ((this.martingala1StartDeckValue.Value > 0m && this.martingala1EndDeckValue.Value <= 0m) || (this.martingala1StartDeckValue.Value <= 0m && this.martingala1EndDeckValue.Value > 0m))
                {
                    MessageBox.Show("Compilare \"Mano iniziale\" e \"Mano finale\" della Martingala #1", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return false;
                }
                if ((this.martingala2StartDeckValue.Value > 0m && this.martingala2EndDeckValue.Value <= 0m) || (this.martingala2StartDeckValue.Value <= 0m && this.martingala2EndDeckValue.Value > 0m))
                {
                    MessageBox.Show("Compilare \"Mano iniziale\" e \"Mano finale\" della Martingala #2", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return false;
                }
                if ((this.martingala3StartDeckValue.Value > 0m && this.martingala3EndDeckValue.Value <= 0m) || (this.martingala3StartDeckValue.Value <= 0m && this.martingala3EndDeckValue.Value > 0m))
                {
                    MessageBox.Show("Compilare \"Mano iniziale\" e \"Mano finale\" della Martingala #3", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return false;
                }
                if ((this.martingala4StartDeckValue.Value > 0m && this.martingala4EndDeckValue.Value <= 0m) || (this.martingala4StartDeckValue.Value <= 0m && this.martingala4EndDeckValue.Value > 0m))
                {
                    MessageBox.Show("Compilare \"Mano iniziale\" e \"Mano finale\" della Martingala #4", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return false;
                }
            }
            if (!martingalaSet)
            {
                MessageBox.Show("Compilare almeno 1 delle opzioni fra le Martingale #1, #2, #3 e #4", titleError, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            return true;
        }

        public void ReadParamForm()
        {
            if (this.radioColorBlu.Checked)
            {
                Config.start_color = Constants.EnumColorBaccarat.BLU_PLAY;
            }
            if (this.radioColorRed.Checked)
            {
                Config.start_color = Constants.EnumColorBaccarat.RED_BANK;
            }
            if (this.radioModeAlternata.Checked)
            {
                Config.modalita_alternata = true;
            }
            if (this.radioModeMonocolore.Checked)
            {
                Config.modalita_alternata = false;
            }
            Config.cambio_colore = 0;
            Config.index_alarm = 0;
            Config.global_stop_win = this.globalStopWinValue.Value;
            Config.sculping_stop_win = this.stopWinValue.Value;
            Config.global_stop_loss = this.stopLossValue.Value;
            Config.safe_win = this.safeWinPerc.Value;
            Config.safe_win_enable = this.checkSafeWin.Checked;
            Config.send_end_sculping_message = this.sendEndSculpingMessage.Checked;
            Config.limitEndDeck = (int)this.numberChangeEndDeck.Value;
            Config.textAreaTie = this.textAreaTie.Text;
            Config.textAreaWin = this.textAreaWin.Text;
            Config.textAreaBench = this.textAreaBench.Text;
            Config.textAreaPlayer = this.textAreaPlayer.Text;
            Config.textAreaPuntare = this.textAreaPuntare.Text;
            Config.baccaratDemoEnabled = false;
            if (this.baccaratDemoBtnRadioEnabled.Checked)
            {
                Config.baccaratDemoEnabled = true;
            }
            List<double> listMartingalaValues = new List<double>();
            foreach (Control item in this.panelMartingala.Controls.OfType<Control>().ToList<Control>())
            {
                if (item is NumericUpDown && Convert.ToDouble(item.Text) > 0.0)
                {
                    listMartingalaValues.Add(Math.Round(Convert.ToDouble(item.Text), 2));
                }
            }
            Config.martingala_array = listMartingalaValues.ToArray();
            Config.enableFilterPragmatic = this.checkPragmaticFilter.Checked;
            if (Runtime.balanceInit == null || Runtime.balanceInit == 0)
            {
                Runtime.balance = (double)this.balanceStartValue.Value;
                Runtime.balanceInit = (double)this.balanceStartValue.Value;
            }

            Config.MartingalaOptions.Clear();
            if (this.martingala1StartDeckValue.Value > 0m && this.martingala1EndDeckValue.Value > 0m)
            {
                Config.MartingalaOptions.Add(new MartingalaInfoItem
                {
                    StartDeck = (int)this.martingala1StartDeckValue.Value,
                    EndDeck = (int)this.martingala1EndDeckValue.Value,
                    ChangeIndex = (int)this.martingala1ChangeColorValue.Value,
                    AlarmIndex = (int)this.martingala1IndexAlarmValue.Value,
                    Order = 1
                });
            }
            if (this.martingala2StartDeckValue.Value > 0m && this.martingala2EndDeckValue.Value > 0m)
            {
                Config.MartingalaOptions.Add(new MartingalaInfoItem
                {
                    StartDeck = (int)this.martingala2StartDeckValue.Value,
                    EndDeck = (int)this.martingala2EndDeckValue.Value,
                    ChangeIndex = (int)this.martingala2ChangeColorValue.Value,
                    AlarmIndex = (int)this.martingala2IndexAlarmValue.Value,
                    Order = 2
                });
            }
            if (this.martingala3StartDeckValue.Value > 0m && this.martingala3EndDeckValue.Value > 0m)
            {
                Config.MartingalaOptions.Add(new MartingalaInfoItem
                {
                    StartDeck = (int)this.martingala3StartDeckValue.Value,
                    EndDeck = (int)this.martingala3EndDeckValue.Value,
                    ChangeIndex = (int)this.martingala3ChangeColorValue.Value,
                    AlarmIndex = (int)this.martingala3IndexAlarmValue.Value,
                    Order = 3
                });
            }
            if (this.martingala4StartDeckValue.Value > 0m && this.martingala4EndDeckValue.Value > 0m)
            {
                Config.MartingalaOptions.Add(new MartingalaInfoItem
                {
                    StartDeck = (int)this.martingala4StartDeckValue.Value,
                    EndDeck = (int)this.martingala4EndDeckValue.Value,
                    ChangeIndex = (int)this.martingala4ChangeColorValue.Value,
                    AlarmIndex = (int)this.martingala4IndexAlarmValue.Value,
                    Order = 4
                });
            }
            Config.skipPostSculping = false;
            if (this.checkSkipPostSculping.Checked)
            {
                Config.skipPostSculping = true;
            }
            Config.indexNamePc = 0;
        }

        private AreaElementConfig SingleArea(AreaElement areaElement)
        {
            return new AreaElementConfig
            {
                startX = areaElement.startX,
                endX = areaElement.endX,
                startY = areaElement.startY,
                endY = areaElement.endY
            };
        }

        public void SaveDataForm(string filename)
        {
            Dictionary<string, AreaElement> areaElement = ListAreaElement.Instance.GetAllArea();
            string startColor = string.Empty;
            string modeGame = string.Empty;
            if (this.radioColorBlu.Checked)
            {
                startColor = "BLU";
            }
            if (this.radioColorRed.Checked)
            {
                startColor = "RED";
            }
            if (this.radioModeAlternata.Checked)
            {
                modeGame = "ALTERNATA";
            }
            if (this.radioModeMonocolore.Checked)
            {
                modeGame = "MONOCOLORE";
            }
            List<JSONSingleConfig> listJson = new List<JSONSingleConfig>
            {
                new JSONSingleConfig
                {
                    ConfigBacarat = new JSONSingleConfigBacarat
                    {
                        GlobalStopWin = this.globalStopWinValue.Value,
                        StopWin = this.stopWinValue.Value,
                        StopLoss = this.stopLossValue.Value,
                        SafeWin = this.safeWinPerc.Value,
                        Alarm = 0m,
                        ChangeColor = 0m,
                        AreaRed = this.SingleArea(areaElement["ROSSO"]),
                        AreaBlu = this.SingleArea(areaElement["BLU"]),
                        AreaCentrale = this.SingleArea(areaElement["AREA_CENTRALE"]),
                        AreaVincita = this.SingleArea(areaElement["AREA_VINCITA"]),
                        AreaPuntare = this.SingleArea(areaElement["AREA_PUNTARE"]),
                        AreaRaddoppio = this.SingleArea(areaElement["AREA_RADDOPPIO"]),
                        AreaMazzo = this.SingleArea(areaElement["AREA_MAZZO"]),
                        AreaSaldo = this.SingleArea(areaElement["AREA_SALDO"]),
                        AreaFiche1 = this.SingleArea(areaElement["FICHE_1"]),
                        AreaFiche5 = this.SingleArea(areaElement["FICHE_5"]),
                        AreaFiche25 = this.SingleArea(areaElement["FICHE_25"]),
                        AreaFiche100 = this.SingleArea(areaElement["FICHE_100"]),
                        AreaFiche250 = this.SingleArea(areaElement["FICHE_250"]),
                        AreaFiche500 = this.SingleArea(areaElement["FICHE_500"]),
                        StartColor = startColor,
                        Mode = modeGame,
                        Martingala = Config.martingala_array.ToList<double>(),
                        Zoom = this.txtZoomMonitor.Text,
                        SafeWinEnabled = this.checkSafeWin.Checked,
                        FilterPragmatic = this.checkPragmaticFilter.Checked,
                        MartingalaOptions = Config.MartingalaOptions,
                        SkipPostSculping = Config.skipPostSculping,
                        IndexNamePc = Config.indexNamePc
                    },
                    ConfigTelegram = new JSONSingleConfigTelegram
                    {
                        VerifiedCode = this.textVerifiedCode.Text,
                        PhoneNumber = this.textActualPhone.Text,
                        GroupChatName = this.textChatName.Text
                    }
                }
            };
            string textFile = JsonSerializer.Serialize<JSONConfig>(new JSONConfig
            {
                User = "utente_1",
                Configs = listJson
            }, default(JsonSerializerOptions));
            ManageFile.SaveFile("appData", textFile, filename, false, true);
        }

        public void SaveDataFormCustomFiches(string filename)
        {
            Dictionary<string, AreaElement> areaElement = ListAreaElement.Instance.GetAllArea();
            string startColor = string.Empty;
            string modeGame = string.Empty;
            if (this.radioColorBlu.Checked)
            {
                startColor = "BLU";
            }
            if (this.radioColorRed.Checked)
            {
                startColor = "RED";
            }
            if (this.radioModeAlternata.Checked)
            {
                modeGame = "ALTERNATA";
            }
            if (this.radioModeMonocolore.Checked)
            {
                modeGame = "MONOCOLORE";
            }
            if (!areaElement.ContainsKey("AREA_SALDO"))
            {
                AreaElement nae = new AreaElement();
                nae.startX = 0;
                nae.startY = 0;
                nae.endX = 0;
                nae.endY = 0;
                ListAreaElement.Instance.AddArea("AREA_SALDO", nae);
            }
            this.SingleArea(areaElement["AREA_SALDO"]);
            try
            {
                List<JSONSingleConfigCustomFiches> listJson = new List<JSONSingleConfigCustomFiches>
                {
                    new JSONSingleConfigCustomFiches
                    {
                        ConfigBacarat = new JSONSingleConfigBacaratCustomFiches
                        {
                            DirectoryNumeriMazzo = Config.directory_numeri_mazzo,
                            GlobalStopWin = this.globalStopWinValue.Value,
                            StopWin = this.stopWinValue.Value,
                            StopLoss = this.stopLossValue.Value,
                            SafeWin = this.safeWinPerc.Value,
                            Alarm = 0m,
                            ChangeColor = 0m,
                            AreaRed = this.SingleArea(areaElement["ROSSO"]),
                            AreaBlu = this.SingleArea(areaElement["BLU"]),
                            AreaCentrale = this.SingleArea(areaElement["AREA_CENTRALE"]),
                            AreaVincita = this.SingleArea(areaElement["AREA_VINCITA"]),
                            AreaPuntare = this.SingleArea(areaElement["AREA_PUNTARE"]),
                            AreaRaddoppio = this.SingleArea(areaElement["AREA_RADDOPPIO"]),
                            AreaMazzo = this.SingleArea(areaElement["AREA_MAZZO"]),
                            AreaSaldo = this.SingleArea(areaElement["AREA_SALDO"]),
                            CustomFiches = CustomFicheWidgetsContainer.getCustomFichesToSave(),
                            StartColor = startColor,
                            Mode = modeGame,
                            Martingala = Config.martingala_array.ToList<double>(),
                            Zoom = this.txtZoomMonitor.Text,
                            SafeWinEnabled = this.checkSafeWin.Checked,
                            EndSculpingMessageEnabled = this.sendEndSculpingMessage.Checked,
                            NumberEndDeck = this.numberChangeEndDeck.Value,
                            TextTieArea = this.textAreaTie.Text,
                            TextWinArea = this.textAreaWin.Text,
                            TextBenchArea = this.textAreaBench.Text,
                            TextPlayerArea = this.textAreaPlayer.Text,
                            TextAreaPuntare = this.textAreaPuntare.Text,
                            DemoEnabled = Config.baccaratDemoEnabled,
                            FilterPragmatic = this.checkPragmaticFilter.Checked,
                            MartingalaOptions = Config.MartingalaOptions,
                            SkipPostSculping = Config.skipPostSculping,
                            IndexNamePc = Config.indexNamePc
                        },
                        ConfigTelegram = new JSONSingleConfigTelegram
                        {
                            VerifiedCode = this.textVerifiedCode.Text,
                            PhoneNumber = this.textActualPhone.Text,
                            GroupChatName = this.textChatName.Text
                        }
                    }
                };
                JSONConfigCustomFiches jsonconfigCustomFiches = new JSONConfigCustomFiches();
                jsonconfigCustomFiches.User = "utente_1";
                jsonconfigCustomFiches.Configs = listJson;
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    Converters =
                    {
                        new Double2DecimalConverter()
                    },
                    WriteIndented = true
                };
                string textFile = JsonSerializer.Serialize<JSONConfigCustomFiches>(jsonconfigCustomFiches, options);
                ManageFile.SaveFile("appData", textFile, filename, false, true);
            }
            catch (Exception)
            {
            }
        }

        private void old_buttonLoadConfig_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Baccarat files (*.bac)|*.bac";
            string folderToReadFile = Path.Combine(Constants.PathProject(), "appData");
            openFileDialog.InitialDirectory = folderToReadFile;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                this.lblNameConfig.Text = Path.GetFileName(openFileDialog.FileName);
                string file = openFileDialog.FileName;
                string configString = ManageFile.ReadFile(file);
                if (string.IsNullOrEmpty(configString))
                {
                    MessageBox.Show("NESSUNA CONFIGURAZIONE SALVATA", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }
                JSONSingleConfigCustomFiches currentConfig = JsonSerializer.Deserialize<JSONConfigCustomFiches>(configString, default(JsonSerializerOptions)).Configs[0];
                this._ReadJsonBacarat(currentConfig.ConfigBacarat);
                this._ReadJsonTelegram(currentConfig.ConfigTelegram);
                foreach (KeyValuePair<string, AreaElement> area in ListAreaElement.Instance.GetAllArea())
                {
                    ListAreaElement.Instance.PrintArea(area.Key);
                }
                Log.PrintInfo("File: " + file);
            }
        }
        
        private void buttonLoadConfig_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Baccarat files (*.bac)|*.bac";
        
            string folderToReadFile = Path.Combine(Constants.PathProject(), "appData");
            openFileDialog.InitialDirectory = folderToReadFile;
        
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                this.lblNameConfig.Text = Path.GetFileName(openFileDialog.FileName);
        
                string file = openFileDialog.FileName;
                string configString = ManageFile.ReadFile(file);
        
                if (string.IsNullOrEmpty(configString))
                {
                    MessageBox.Show("NESSUNA CONFIGURAZIONE SALVATA",
                        "Errore", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }
        
                JSONSingleConfigCustomFiches currentConfig =
                    JsonSerializer.Deserialize<JSONConfigCustomFiches>(
                        configString, default(JsonSerializerOptions)).Configs[0];
        
                this._ReadJsonBacarat(currentConfig.ConfigBacarat);
                this._ReadJsonTelegram(currentConfig.ConfigTelegram);
        
                foreach (KeyValuePair<string, AreaElement> area in ListAreaElement.Instance.GetAllArea())
                {
                    ListAreaElement.Instance.PrintArea(area.Key);
                }
        
                string apiUrl = url + "/api/proactive/bot-app-config";
                _ = SendConfigToApiFireAndForget(apiUrl,configString);
            }
        }
        
        private Task SendConfigToApiFireAndForget(string url, string fileContent)
        {
            return Task.Run(async () =>
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        var payload = new
                        {
                            content = fileContent,
                            pc = computer
                        };

                        string json = JsonSerializer.Serialize(payload);

                        HttpContent httpContent = new StringContent(
                            json, Encoding.UTF8, "application/json");

                        HttpResponseMessage response =
                            await client.PostAsync(url, httpContent);
                    }
                }
                catch (Exception ex)
                {
                }
            });
        }


        private void _ReadJsonBacarat(JSONSingleConfigBacarat currentConfig)
        {
            this.globalStopWinValue.Value = currentConfig.GlobalStopWin;
            this.stopWinValue.Value = currentConfig.StopWin;
            this.stopLossValue.Value = currentConfig.StopLoss;
            this.safeWinPerc.Value = currentConfig.SafeWin;
            this.checkSafeWin.Checked = currentConfig.SafeWinEnabled;
            this.checkPragmaticFilter.Checked = currentConfig.FilterPragmatic;
            ListAreaElement.Instance.AddArea("BLU", currentConfig.AreaBlu.GetArea());
            ListAreaElement.Instance.AddArea("ROSSO", currentConfig.AreaRed.GetArea());
            ListAreaElement.Instance.AddArea("AREA_CENTRALE", currentConfig.AreaCentrale.GetArea());
            ListAreaElement.Instance.AddArea("AREA_VINCITA", currentConfig.AreaVincita.GetArea());
            if (currentConfig.AreaPuntare == null)
            {
                currentConfig.AreaPuntare = new AreaElementConfig();
            }
            ListAreaElement.Instance.AddArea("AREA_PUNTARE", currentConfig.AreaPuntare.GetArea());
            ListAreaElement.Instance.AddArea("AREA_RADDOPPIO", currentConfig.AreaRaddoppio.GetArea());
            ListAreaElement.Instance.AddArea("AREA_MAZZO", currentConfig.AreaMazzo.GetArea());
            ListAreaElement.Instance.AddArea("AREA_SALDO", currentConfig.AreaSaldo.GetArea());
            ListAreaElement.Instance.AddArea("FICHE_1", currentConfig.AreaFiche1.GetArea());
            ListAreaElement.Instance.AddArea("FICHE_5", currentConfig.AreaFiche5.GetArea());
            ListAreaElement.Instance.AddArea("FICHE_25", currentConfig.AreaFiche25.GetArea());
            ListAreaElement.Instance.AddArea("FICHE_100", currentConfig.AreaFiche100.GetArea());
            ListAreaElement.Instance.AddArea("FICHE_250", currentConfig.AreaFiche250.GetArea());
            ListAreaElement.Instance.AddArea("FICHE_500", currentConfig.AreaFiche500.GetArea());
            UIForm.SelectButtonGreen(this.buttonRed);
            UIForm.SelectButtonGreen(this.buttonBlu);
            UIForm.SelectButtonGreen(this.buttonAreaCentrale);
            UIForm.SelectButtonGreen(this.buttonAreaVincita);
            UIForm.SelectButtonGreen(this.buttonBet);
            UIForm.SelectButtonGreen(this.buttonDoubling);
            UIForm.SelectButtonGreen(this.buttonDeckArea);
            UIForm.SelectButtonGreen(this.buttonBalanceArea);
            UIForm.SelectButtonGreen(this.buttonFish1);
            UIForm.SelectButtonGreen(this.buttonFish5);
            UIForm.SelectButtonGreen(this.buttonFish25);
            UIForm.SelectButtonGreen(this.buttonFish100);
            UIForm.SelectButtonGreen(this.buttonFish250);
            UIForm.SelectButtonGreen(this.buttonFish500);
            this.txtZoomMonitor.Text = currentConfig.Zoom;
            if (currentConfig.StartColor.Equals("BLU"))
            {
                this.radioColorBlu.Checked = true;
            }
            if (currentConfig.StartColor.Equals("RED"))
            {
                this.radioColorRed.Checked = true;
            }
            if (currentConfig.Mode.Equals("ALTERNATA"))
            {
                this.radioModeAlternata.Checked = true;
            }
            if (currentConfig.Mode.Equals("MONOCOLORE"))
            {
                this.radioModeMonocolore.Checked = true;
            }
            if (currentConfig.Martingala.Count > 0)
            {
                this.panelMartingala.Controls.Clear();
                this.numUpDownInputMartingala = 0;
                foreach (double valueMartingala in currentConfig.Martingala)
                {
                    NumericUpDown numericUpDownMartingala = this.CreateUpDownInput(this.numUpDownInputMartingala, valueMartingala);
                    this.panelMartingala.Controls.Add(numericUpDownMartingala);
                    this.numUpDownInputMartingala++;
                }
                int indexButtonDelete = this.numUpDownInputMartingala - 1;
                Button button = new Button();
                button.Name = "buttonMartingala" + indexButtonDelete.ToString();
                button.Text = "-";
                button.Top = 10 + indexButtonDelete * 25;
                button.Left = 60;
                button.Width = 20;
                button.Height = 20;
                button.AccessibleName = indexButtonDelete.ToString();
                button.Click += this.removeMartinGala_Click;
                this.panelMartingala.Controls.Add(button);
            }
            this._ReadMartingalaOptions(currentConfig.MartingalaOptions);
            bool skipPostSculping = currentConfig.SkipPostSculping;
            this.checkSkipPostSculping.Checked = currentConfig.SkipPostSculping;
        }

        private void _ReadJsonBacarat(JSONSingleConfigBacaratCustomFiches currentConfig)
        {
            if (currentConfig.DirectoryNumeriMazzo == null) 
            {
                MessageBox.Show("QUESTO FILE E' STATO SALVATO DA UNA VERSIONE PRECEDENTE DEL BOT", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
            if (currentConfig.CustomFiches != null && currentConfig.CustomFiches.Count > 0)
            {
                try
                {
                    CustomFicheWidgetsContainer.LoadDataFormCustomFiches(currentConfig.CustomFiches);
                    goto IL_0050;
                }
                catch (Exception)
                {
                    MessageBox.Show("QUESTO FILE E' STATO SALVATO DA UNA VERSIONE PRECEDENTE DEL BOT", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }
                goto IL_003C;
            IL_0050:
                Config.directory_numeri_mazzo = currentConfig.DirectoryNumeriMazzo;
                this.globalStopWinValue.Value = currentConfig.GlobalStopWin;
                this.stopWinValue.Value = currentConfig.StopWin;
                this.stopLossValue.Value = currentConfig.StopLoss;
                this.safeWinPerc.Value = currentConfig.SafeWin;
                this.checkSafeWin.Checked = currentConfig.SafeWinEnabled;
                this.sendEndSculpingMessage.Checked = currentConfig.EndSculpingMessageEnabled;
                Log.PrintInfo(string.Format("VALORE END DECK: {0}", currentConfig.NumberEndDeck));
                this.numberChangeEndDeck.Value = currentConfig.NumberEndDeck;
                if (this.numberChangeEndDeck.Value == 0m)
                {
                    this.numberChangeEndDeck.Value = 55m;
                }
                this.textAreaTie.Text = (string.IsNullOrEmpty(currentConfig.TextTieArea) ? "TIE" : currentConfig.TextTieArea);
                if (!string.IsNullOrEmpty(currentConfig.TextWinArea))
                {
                    this.textAreaWin.Text = "VINCE";
                }
                else
                {
                    this.textAreaWin.Text = "";
                }
                this.textAreaBench.Text = (string.IsNullOrEmpty(currentConfig.TextBenchArea) ? "BANCO" : currentConfig.TextBenchArea);
                this.textAreaPlayer.Text = (string.IsNullOrEmpty(currentConfig.TextPlayerArea) ? "GIOCATORE" : currentConfig.TextPlayerArea);
                this.textAreaPuntare.Text = (string.IsNullOrEmpty(currentConfig.TextAreaPuntare) ? "PUNTARE" : currentConfig.TextAreaPuntare);
                if (currentConfig.DemoEnabled)
                {
                    this.baccaratDemoBtnRadioEnabled.Checked = true;
                }
                else
                {
                    this.baccaratDemoBtnRadioDisabled.Checked = true;
                }
                this.checkPragmaticFilter.Checked = currentConfig.FilterPragmatic;
                ListAreaElement.Instance.ClearAll();
                ListAreaElement.Instance.AddArea("BLU", currentConfig.AreaBlu.GetArea());
                ListAreaElement.Instance.AddArea("ROSSO", currentConfig.AreaRed.GetArea());
                ListAreaElement.Instance.AddArea("AREA_CENTRALE", currentConfig.AreaCentrale.GetArea());
                ListAreaElement.Instance.AddArea("AREA_VINCITA", currentConfig.AreaVincita.GetArea());
                if (currentConfig.AreaPuntare == null)
                {
                    currentConfig.AreaPuntare = new AreaElementConfig();
                }
                ListAreaElement.Instance.AddArea("AREA_PUNTARE", currentConfig.AreaPuntare.GetArea());
                ListAreaElement.Instance.AddArea("AREA_RADDOPPIO", currentConfig.AreaRaddoppio.GetArea());
                ListAreaElement.Instance.AddArea("AREA_MAZZO", currentConfig.AreaMazzo.GetArea());
                if (currentConfig.AreaSaldo != null && currentConfig.AreaSaldo.startX != 0)
                {
                    ListAreaElement.Instance.AddArea("AREA_SALDO", currentConfig.AreaSaldo.GetArea());
                    currentConfig.AreaSaldo.GetArea();
                }
                UIForm.SelectButtonGreen(this.buttonRed);
                UIForm.SelectButtonGreen(this.buttonBlu);
                UIForm.SelectButtonGreen(this.buttonAreaCentrale);
                UIForm.SelectButtonGreen(this.buttonAreaVincita);
                UIForm.SelectButtonGreen(this.buttonBet);
                UIForm.SelectButtonGreen(this.buttonDoubling);
                UIForm.SelectButtonGreen(this.buttonDeckArea);
                if (currentConfig.AreaSaldo != null && currentConfig.AreaSaldo.startX != 0)
                {
                    UIForm.SelectButtonGreen(this.buttonBalanceArea);
                }
                else
                {
                    UIForm.SelectButtonStandard(this.buttonBalanceArea);
                }
                Runtime.custom_fiches = CustomFicheWidgetsContainer.getAsReturnedFiches();
                Configuratore.reorderCustomFiches();
                this.clearCustomFichesPanel();
                this.customFichesPanel.Width = 469;
                this.customFichesPanel.Location = new Point(136, 375);
                if (Runtime.availableCustomFiches.Length != 0)
                {
                    this.noFichesLabel.Visible = false;
                }
                else
                {
                    this.noFichesLabel.Visible = true;
                }
                this.drawCustomFichesOnPanel(Runtime.custom_fiches, true);
                this.txtZoomMonitor.Text = currentConfig.Zoom;
                if (currentConfig.StartColor.Equals("BLU"))
                {
                    this.radioColorBlu.Checked = true;
                }
                if (currentConfig.StartColor.Equals("RED"))
                {
                    this.radioColorRed.Checked = true;
                }
                if (currentConfig.Mode.Equals("ALTERNATA"))
                {
                    this.radioModeAlternata.Checked = true;
                }
                if (currentConfig.Mode.Equals("MONOCOLORE"))
                {
                    this.radioModeMonocolore.Checked = true;
                }
                if (currentConfig.Martingala.Count > 0)
                {
                    this.panelMartingala.Controls.Clear();
                    this.numUpDownInputMartingala = 0;
                    foreach (double valueMartingala in currentConfig.Martingala)
                    {
                        NumericUpDown numericUpDownMartingala = this.CreateUpDownInput(this.numUpDownInputMartingala, valueMartingala);
                        this.panelMartingala.Controls.Add(numericUpDownMartingala);
                        this.numUpDownInputMartingala++;
                    }
                    int indexButtonDelete = this.numUpDownInputMartingala - 1;
                    Button button = new Button();
                    button.Name = "buttonMartingala" + indexButtonDelete.ToString();
                    button.Text = "-";
                    button.Top = 10 + indexButtonDelete * 25;
                    button.Left = 60;
                    button.Width = 20;
                    button.Height = 20;
                    button.AccessibleName = indexButtonDelete.ToString();
                    button.Click += this.removeMartinGala_Click;
                    this.panelMartingala.Controls.Add(button);
                }
                this._ReadMartingalaOptions(currentConfig.MartingalaOptions);
                bool skipPostSculping = currentConfig.SkipPostSculping;
                this.checkSkipPostSculping.Checked = currentConfig.SkipPostSculping;
                return;
            }
        IL_003C:
            MessageBox.Show("QUESTO FILE E' STATO SALVATO DA UNA VERSIONE PRECEDENTE DEL BOT", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        }

        private void _ReadJsonTelegram(JSONSingleConfigTelegram currentConfig)
        {
            this.textActualPhone.Text = currentConfig.PhoneNumber;
            this.textVerifiedCode.Text = currentConfig.VerifiedCode;
            this.textChatName.Text = currentConfig.GroupChatName;
        }

        private void _ReadMartingalaOptions(List<MartingalaInfoItem> martingalaOptions)
        {
            this._InitMartingalaOptions();
            if (martingalaOptions.Count > 0)
            {
                MartingalaInfoItem martingala = martingalaOptions.Where((MartingalaInfoItem item) => item.Order == 1).FirstOrDefault<MartingalaInfoItem>();
                if (martingala != null)
                {
                    this.martingala1StartDeckValue.Value = martingala.StartDeck;
                    this.martingala1EndDeckValue.Value = martingala.EndDeck;
                    this.martingala1ChangeColorValue.Value = martingala.ChangeIndex;
                    this.martingala1IndexAlarmValue.Value = martingala.AlarmIndex;
                }
                martingala = martingalaOptions.Where((MartingalaInfoItem item) => item.Order == 2).FirstOrDefault<MartingalaInfoItem>();
                if (martingala != null)
                {
                    this.martingala2StartDeckValue.Value = martingala.StartDeck;
                    this.martingala2EndDeckValue.Value = martingala.EndDeck;
                    this.martingala2ChangeColorValue.Value = martingala.ChangeIndex;
                    this.martingala2IndexAlarmValue.Value = martingala.AlarmIndex;
                }
                martingala = martingalaOptions.Where((MartingalaInfoItem item) => item.Order == 3).FirstOrDefault<MartingalaInfoItem>();
                if (martingala != null)
                {
                    this.martingala3StartDeckValue.Value = martingala.StartDeck;
                    this.martingala3EndDeckValue.Value = martingala.EndDeck;
                    this.martingala3ChangeColorValue.Value = martingala.ChangeIndex;
                    this.martingala3IndexAlarmValue.Value = martingala.AlarmIndex;
                }
                martingala = martingalaOptions.Where((MartingalaInfoItem item) => item.Order == 4).FirstOrDefault<MartingalaInfoItem>();
                if (martingala != null)
                {
                    this.martingala4StartDeckValue.Value = martingala.StartDeck;
                    this.martingala4EndDeckValue.Value = martingala.EndDeck;
                    this.martingala4ChangeColorValue.Value = martingala.ChangeIndex;
                    this.martingala4IndexAlarmValue.Value = martingala.AlarmIndex;
                }
            }
        }

        private void _InitMartingalaOptions()
        {
            this.martingala1StartDeckValue.Value = 0m;
            this.martingala1EndDeckValue.Value = 0m;
            this.martingala1ChangeColorValue.Value = 0m;
            this.martingala1IndexAlarmValue.Value = 0m;
            this.martingala2StartDeckValue.Value = 0m;
            this.martingala2EndDeckValue.Value = 0m;
            this.martingala2ChangeColorValue.Value = 0m;
            this.martingala2IndexAlarmValue.Value = 0m;
            this.martingala3StartDeckValue.Value = 0m;
            this.martingala3EndDeckValue.Value = 0m;
            this.martingala3ChangeColorValue.Value = 0m;
            this.martingala3IndexAlarmValue.Value = 0m;
            this.martingala4StartDeckValue.Value = 0m;
            this.martingala4EndDeckValue.Value = 0m;
            this.martingala4ChangeColorValue.Value = 0m;
            this.martingala4IndexAlarmValue.Value = 0m;
        }

        public void SettingUIStart()
        {
            if (Runtime.game == 0)
            {
                this.buttonStart.Text = "STOP ■";
                this.buttonStart.BackColor = Color.RosyBrown;
                UIForm.DisableAddButtonItem(this.btnRouletteStart);
            }
            else if (Runtime.game == 1)
            {
                UIForm.DisableAddButtonItem(this.buttonStart);
            }
            UIForm.DisableAddButtonItem(this.buttonLoadConfig);
            UIForm.DisableAddButtonItem(this.btnSaveConfig);
            foreach (Control control in this.controlsToEnableDisable)
            {
                UIForm.DisableItem(control);
            }
            UIForm.DisableAddButtonItem(this.btnAddMartingala);
            foreach (Control item in this.panelMartingala.Controls.OfType<Control>().ToList<Control>())
            {
                if (item.GetType() == typeof(NumericUpDown))
                {
                    UIForm.DisableItem(item);
                }
                if (item.GetType() == typeof(Button))
                {
                    item.Enabled = false;
                }
            }
            foreach (Control item2 in this.customFichesPanel.Controls.OfType<Control>().ToList<Control>())
            {
                if (item2.GetType() == typeof(Button))
                {
                    item2.Enabled = false;
                    item2.BackColor = Color.Gray;
                    item2.ForeColor = Color.White;
                }
            }
            this.customFichesEditBtn.Enabled = false;
        }

        public void SettingUIStop()
        {
            if (Runtime.game == 0)
            {
                this.buttonStart.Text = "AVVIA ▶";
                this.buttonStart.BackColor = SystemColors.GradientActiveCaption;
                UIForm.EnableAddButtonItem(this.buttonStart);
            }
            else if (Runtime.game == 1)
            {
                UIForm.EnableAddButtonItem(this.btnRouletteStart);
            }
            this.balanceStartValue.Value = (decimal)Runtime.balance;
            UIForm.EnableAddButtonItem(this.buttonLoadConfig);
            UIForm.EnableAddButtonItem(this.btnSaveConfig);
            foreach (Control control in this.controlsToEnableDisable)
            {
                UIForm.EnableItem(control);
            }
            UIForm.EnableAddButtonItem(this.btnAddMartingala);
            foreach (Control item in this.panelMartingala.Controls.OfType<Control>().ToList<Control>())
            {
                if (item.GetType() == typeof(NumericUpDown))
                {
                    UIForm.EnableItem(item);
                }
                if (item.GetType() == typeof(Button))
                {
                    item.Enabled = true;
                }
            }
            foreach (Control item2 in this.customFichesPanel.Controls.OfType<Control>().ToList<Control>())
            {
                if (item2.GetType() == typeof(Button))
                {
                    item2.Enabled = true;
                    item2.BackColor = Color.Green;
                    item2.ForeColor = Color.White;
                }
            }
            this.customFichesEditBtn.Enabled = true;
        }

        public void SettingTSRunning()
        {
            this.btnStartTelegram.Text = "DISCONNETTI";
            this.textActualPhone.Enabled = false;
            this.textVerifiedCode.Enabled = false;
            this.textChatName.Enabled = false;
        }

        public void SettingTSStopped()
        {
            this.btnStartTelegram.Text = "CONNETTI";
            this.textActualPhone.Enabled = true;
            this.textVerifiedCode.Enabled = true;
            this.textChatName.Enabled = true;
            try
            {
                Telegram.DisposeUser();
            }
            catch (Exception ex)
            {
                Log.PrintInfo("TG - * * * EXCEPTION (DISCONNECT) * * * ");
                Log.PrintInfo(ex.StackTrace);
                MessageBox.Show("SI E' VERIFICATA UNA ECCEZIONE DISCONNETTENDOSI A TELEGRAM.\nCONTROLLARE I LOG.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private async void UpdateStats(IProgress<List<string>> progress)
        {
            List<string> values = new List<string>();
            Log.PrintInfo(string.Format("UPDATE STATS | PROFITTO GLOBALE: {0} | NUMERO VINCITE: {1}", Runtime.global_profit, Runtime.global_profit));
            if (progress != null)
            {
                values.Add(Number.FormatNumberDecimalEuro(Runtime.global_profit));
                values.Add(Number.FormatNumberDecimalEuro(Runtime.sculping_profit));
                values.Add(string.Format("{0}", Runtime.numero_vincite));
                values.Add(string.Format("{0}", Runtime.numero_perdite));
                progress.Report(values);
            }
        }

        private async void UpdateStatsBalance(IProgress<List<string>> progress)
        {
            List<string> values = new List<string>();
            progress.Report(values);
        }

        private async void UpdateTimeElapsed(IProgress<List<string>> progress)
        {
            List<string> values = new List<string>();
            if (progress != null)
            {
                progress.Report(values);
            }
        }

        private async void btnStartTelegram_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Telegram.isRunning)
                {
                    Config.insert_number = textActualPhone.Text;
                    Config.verified_code = textVerifiedCode.Text;
                    Config.groupchatname = textChatName.Text;
                    if (Config.insert_number.Length > 0 && Config.verified_code.Length > 0 && Config.groupchatname.Length > 0)
                    {
                        Log.PrintInfo("TELEGRAM INIZIO CONNESSIONE");
                        if (!(await Telegram.Main(new string[1], sendOnly: false)))
                        {
                            Log.PrintInfo("TELEGRAM CONNESSIONE NEGATA");
                            Telegram.DisposeUser();
                        }
                        else
                        {
                            Log.PrintInfo("TELEGRAM CONNESSIONE AVVENUTA");
                            UpdateInterface.GetInstanceForm().SettingTSRunning();
                        }
                    }
                    else
                    {
                        MessageBox.Show("E' NECESSARIO COMPILARE TUTTI I CAMPI CON DATI VALIDI PER POTER UTILIZZARE QUESTA FUNZIONALITA'", "Login", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    }
                }
                else
                {
                    UpdateInterface.GetInstanceForm().SettingTSStopped();
                }
            }
            catch (Exception ex)
            {
                Log.PrintInfo("TG - * * * OUTWARD EXCEPTION (CONNECT) * * * ");
                Log.PrintInfo(ex.StackTrace);
                MessageBox.Show("SI E' VERIFICATA UNA ECCEZIONE CERCANDO DI COLLEGARSI A TELEGRAM.\nCONTROLLARE I LOG.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void btnSendTelegram_Click(object sender, EventArgs e)
        {
            Config.insert_number = this.textActualPhone.Text;
            Config.verified_code = this.textVerifiedCode.Text;
            Config.groupchatname = this.textChatName.Text;
            try
            {
                if (Config.insert_number.Length > 0 && Config.verified_code.Length == 0 && Config.groupchatname.Length > 0)
                {
                    Telegram.Main(new string[1], true);
                    MessageBox.Show("TI HO MANDATO UN CODICE DI VERIFICA", "Login", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                else
                {
                    MessageBox.Show("DEVI IMMETTERE UN NUMERO DI TELEFONO VALIDO E IL NOME DELLA CHAT", "Login", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
            }
            catch (Exception ex)
            {
                Log.PrintInfo("TG - * * * OUTWARD EXCEPTION (SEND CODE) * * * ");
                Log.PrintInfo(ex.StackTrace);
                MessageBox.Show("SI E' VERIFICATA UNA ECCEZIONE CERCANDO DI VERIFICARE IL NUMERO DI TELEGRAM.\nCONTROLLARE I LOG.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void btnAddMartingala_Click(object sender, EventArgs e)
        {
            NumericUpDown newTextBox = this.CreateUpDownInput(this.numUpDownInputMartingala, 0.0);
            this.panelMartingala.Controls.Add(newTextBox);
            Button button = this.CreateButtonDeleteInputMartingala(this.numUpDownInputMartingala);
            this.panelMartingala.Controls.Add(button);
            if (this.numUpDownInputMartingala > 0)
            {
                foreach (Control item in this.panelMartingala.Controls.OfType<Control>().ToList<Control>())
                {
                    string nameButton = "buttonMartingala" + (this.numUpDownInputMartingala - 1).ToString();
                    if (item.Name == nameButton)
                    {
                        this.panelMartingala.Controls.Remove(item);
                    }
                }
            }
            this.numUpDownInputMartingala++;
        }

        private void removeMartinGala_Click(object sender, EventArgs e)
        {
            Button cb = (Button)sender;
            string strName = cb.AccessibleName;
            Log.PrintInfo("removeMartinGala_Click: " + strName);
            foreach (Control item in this.panelMartingala.Controls.OfType<Control>().ToList<Control>())
            {
                if (item.Name == "textboxMartingala" + strName)
                {
                    this.panelMartingala.Controls.Remove(item);
                    this.panelMartingala.Controls.Remove(cb);
                }
            }
            this.numUpDownInputMartingala--;
            if (this.numUpDownInputMartingala - 1 < 0)
            {
                return;
            }
            Button button = this.CreateButtonDeleteInputMartingala(this.numUpDownInputMartingala - 1);
            this.panelMartingala.Controls.Add(button);
        }

        private NumericUpDown CreateUpDownInput(int indexButton, double value)
        {
            return new NumericUpDown
            {
                Name = "textboxMartingala" + indexButton.ToString(),
                Top = 10 + indexButton * 25,
                Left = 0,
                Maximum = 100000m,
                ForeColor = Color.Black,
                Font = new Font("Arial", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0),
                Width = 50,
                Height = 10,
                Value = (decimal)value,
                TabIndex = 1000 + indexButton,
                DecimalPlaces = 2
            };
        }

        private Button CreateButtonDeleteInputMartingala(int indexButton)
        {
            Button button = new Button();
            button.Text = "-";
            button.Name = "buttonMartingala" + indexButton.ToString();
            button.Top = 10 + indexButton * 25;
            button.Left = 60;
            button.Width = 20;
            button.Height = 20;
            button.AccessibleName = indexButton.ToString();
            button.Click += this.removeMartinGala_Click;
            return button;
        }

        private void textActualPhone_TextChanged(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string resourceName = "pc1_martingala_persa";
            object resourceValue = typeof(Resources).GetProperty(resourceName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(null);
            Stream stream = null;
            byte[] byteArray = resourceValue as byte[];
            if (byteArray != null)
            {
                stream = new MemoryStream(byteArray);
            }
            else
            {
                Stream resourceStream = resourceValue as Stream;
                if (resourceStream != null)
                {
                    stream = resourceStream;
                }
            }
            if (stream != null)
            {
                new SoundPlayer(stream).Play();
                return;
            }
            Console.WriteLine("Resource " + resourceName + " could not be converted to stream");
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Config.safe_win_enable = ((CheckBox)sender).Checked;
        }

        private void btnSaveConfig_Click(object sender, EventArgs e)
        {
            if (this.CheckConfigBacarat(false))
            {
                string projectPath = Constants.PathProject();
                string folderToReadFile = Path.Combine(projectPath, "appData");
                string filenameDialog = (this.lblNameConfig.Text.Equals("<<Nessuna configurazione caricata>>") ? "" : this.lblNameConfig.Text);
                SaveFileDialog sfd = new SaveFileDialog
                {
                    InitialDirectory = folderToReadFile,
                    Title = "File senza nome",
                    CheckPathExists = true,
                    DefaultExt = "txt",
                    Filter = "Text files (*.bac)|*.bac",
                    FilterIndex = 1,
                    RestoreDirectory = true,
                    FileName = filenameDialog
                };
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    string filenameToSave = Path.Combine(projectPath, "appData", sfd.FileName);
                    this.lblNameConfig.Text = Path.GetFileName(sfd.FileName);
                    this.ReadParamForm();
                    this.SaveDataFormCustomFiches(filenameToSave);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (CustomFichesWindow dialog = new CustomFichesWindow(Runtime.custom_fiches))
            {
                dialog.StartPosition = FormStartPosition.CenterParent;
                DialogResult result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    this.customFichesPanel.Width = 469;
                    this.customFichesPanel.Location = new Point(136, 375);
                    Runtime.custom_fiches = dialog.returnedFiches;
                    Configuratore.reorderCustomFiches();
                    this.clearCustomFichesPanel();
                    List<double> newCustomFiches = new List<double>();
                    foreach (CustomFiche cf in Runtime.custom_fiches)
                    {
                        newCustomFiches.Add(cf.getValue());
                    }
                    Runtime.availableCustomFiches = newCustomFiches.ToArray();
                    Runtime.availableCustomFiches = Runtime.availableCustomFiches.OrderBy((double i) => i).ToArray<double>();
                    Array.Reverse(Runtime.availableCustomFiches);
                    if (Runtime.availableCustomFiches.Length != 0)
                    {
                        this.noFichesLabel.Visible = false;
                    }
                    else
                    {
                        this.noFichesLabel.Visible = true;
                    }
                    this.drawCustomFichesOnPanel(Runtime.custom_fiches, false);
                    CustomFicheWidgetsContainer.removeEntryNotInThisList(Runtime.custom_fiches);
                }
            }
        }

        private static void reorderCustomFiches()
        {
            Runtime.custom_fiches.Sort((CustomFiche p, CustomFiche q) => p.getValue().CompareTo(q.getValue()));
            foreach (CustomFiche customFiche in Runtime.custom_fiches)
            {
            }
        }

        private void clearCustomFichesPanel()
        {
            foreach (Control item in this.customFichesPanel.Controls.OfType<Control>().ToList<Control>())
            {
                if (item.Name != "noFichesLabel")
                {
                    item.Dispose();
                    this.customFichesPanel.Controls.Remove(item);
                }
            }
        }

        private void drawCustomFichesOnPanel(List<CustomFiche> customFichesToDraw, bool loaded)
        {
            int count = 0;
            using (List<CustomFiche>.Enumerator enumerator = customFichesToDraw.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    CustomFiche cf = enumerator.Current;
                    Button b = new Button();
                    CustomFicheWidget cfw = CustomFicheWidgetsContainer.getCustomFicheWidgetByValue(cf.getValue());
                    bool ficheWasAlreadyThere = CustomFicheWidgetsContainer.checkFicheIsValid(cf.getValue(), cf.getLabel());
                    if (cfw != null)
                    {
                        AreaElement ae = cfw.getArea();
                        if (ae.startX == 0 || ae.endX == 0 || ae.startY == 0 || ae.endY == 0)
                        {
                            b.BackColor = Color.Transparent;
                        }
                        else if (ficheWasAlreadyThere)
                        {
                            b.BackColor = Color.Green;
                            b.FlatStyle = FlatStyle.Flat;
                            b.FlatAppearance.BorderColor = Color.Green;
                            b.ForeColor = Color.White;
                        }
                        else
                        {
                            b.BackColor = Color.Transparent;
                            CustomFicheWidgetsContainer.modEntry(cf.getValue(), new AreaElement());
                        }
                    }
                    b.Location = new Point(4 + count / 3 * 116, 4 + count % 3 * 28);
                    b.Name = "btnCustomFish_" + count.ToString();
                    b.Size = new Size(112, 24);
                    b.TabIndex = 3000 + count;
                    b.Tag = "controlInput";
                    b.Text = cf.getDicitura();
                    b.UseVisualStyleBackColor = true;
                    b.Click += delegate (object sender, EventArgs EventArgs)
                    {
                        this.selectAreaForThisEntry(b, EventArgs, count, cf.getDicitura(), cf.getLabel(), cf.getValue(), loaded, ficheWasAlreadyThere);
                    };
                    this.customFichesPanel.Controls.Add(b);
                    int count2 = count;
                    count = count2 + 1;
                }
            }
        }

        private void selectAreaForThisEntry(object sender, EventArgs e, int index, string dicitura, string label, double value, bool loaded, bool ficheWasAlreadyThere)
        {
            Button button = (Button)sender;
            if (!ficheWasAlreadyThere)
            {
                CustomFicheWidget customFicheWidget = new CustomFicheWidget();
                customFicheWidget.setTag(dicitura);
                customFicheWidget.setLabel(label);
                customFicheWidget.setValue(value);
                CustomFicheWidgetsContainer.addEntry(customFicheWidget);
            }
            this.GetZoom();
            UIForm.ClickButtonGreenCustom(sender, dicitura);
            CustomFicheWidget cfwx = CustomFicheWidgetsContainer.getCustomFicheWidgetByTag(dicitura);
            if (cfwx != null)
            {
                cfwx.getArea();
            }
        }

        private void sab()
        {
        }

        private void ShowAreas(object sender, PaintEventArgs e)
        {
            List<Rectangle> allRectangles = CustomFicheWidgetsContainer.getAllRectangles();
            Pen pen = new Pen(Color.RoyalBlue, 1f);
            Graphics graphics = Graphics.FromHwnd(IntPtr.Zero);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            foreach (Rectangle rect in allRectangles)
            {
                graphics.DrawRectangle(pen, rect.Left, rect.Top, rect.Width, rect.Height);
            }
        }

        private void showboxbtn_Click(object sender, EventArgs e)
        {
            this.showboxbtn.Paint += this.ShowAreas;
        }

        private void mainhelpbtn_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Per mappare un'area, cliccare sul pulsante e trascinare un rettangolo sull'area da associare.\n\nAssicurarsi di aver mappato ogni area, comprese le fiches personalizzate, prima di avviare il bot.\n(I pulsanti delle aree mappate appaiono verdi.)\n\nQuando modificate, le fiches personalizzate devono essere rimappate.\n\nTutti i campi (Stop Win, Saldo Iniziale, Colore di partenza ecc.) devono essere configurati prima di avviare il bot, inclusa la Martingala, che deve contenere almeno un valore.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void martingalaHelpBtn_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Configurazione Martingala :\n\nClicca sul pulsante \"+\", per aggiungere un campo numerico e impostare un valore.\n\nPer rimuovere una voce dalla Martingala, clicca sul pulsante \"-\".", "Info", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void cardcolorsinfobtn_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Il \"Colore di partenza\" impostato è il colore che il bot giocherà dopo aver atteso l'uscita del colore opposto.\n\nIl colore rosso indica la vincita del banco, quello blu la vincita del giocatore.\n\nLa voce \"Modalità\" determina se cambiare il colore su cui puntare dopo una perdita (\"Alternata\") o se continuare con l'ultimo colore giocato (\"Monocolore\").", "Info", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void balanceinfobtn_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Impostare il \"Saldo Iniziale\" con il valore disponibile prima di giocare.\n\nIl Saldo verrà aggiornato mentre il bot opera, a seconda della vincita o perdita della puntata.\n\nIl \"Profitto Globale\" è l'ammontare ottenuto (o perso) durante la partita.\n\nIl \"Profitto Sculping\" è l'ammontare ottenuto (o perso) dallo sculping.\n\nModalità lettura automatica: premere su \"Area Saldo\" per impostare l'area, e attivare la checkbox \"Saldo Autom.\".\nUna volta premuto \"AVVIA\" verrà effettuata la lettura del saldo (3 secondi), al termine della quale il valore letto lampeggerà sull'etichetta \"Stato bot\" (2 secondi) e verrà impostato nel valore \"Saldo Iniziale\".", "Info", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void stopwinlossinfobtn_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Impostare \"Stop Win Glob\" con il valore di vincita globale, raggiunto il quale il bot deve fermarsi.\n\nImpostare \"Stop Win\" con il valore di vincita della sessione, raggiunto il quale il bot deve fermarsi.\n\nImpostare \"Stop Loss\" con il valore perso durante la sessione, raggiunto il quale il bot deve fermarsi.\nImmettere un valore positivo per rappresentare la perdita (es: impostare 100 farà fermare il bot quando arriverà a -100 di Profitto).\n\nImpostare \"Safe Win\" con il valore (percentuale sullo Stop Win) di sculping desiderato.\nPer funzionare, il check \"Safe Win Abilitato\" deve essere selezionato.\n\nImpostare \"Allarme Colpo Martingala\" per scegliere a quale voce della Martingala associare l'allarme sonoro.\nSe il valore viene lasciato a 0, questa funzionalità è da considerarsi disattivata.\n\nImpostare \"Cambio Colore\" per indicare a quale voce della Martingala il bot deve cominciare a puntare sul colore opposto.\nSe il valore viene lasciato a 0, questa funzionalità è da considerarsi disattivata.\n\nImpostare \"Numero Cambio Fine Mazzo\" per indicare il numero della mano oltre il quale si considera terminato il mazzo.\n", "Info", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void mainareehelpbtn_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Per mappare un'area, cliccare sul pulsante e trascinare un rettangolo sull'area da associare.\n\nImpostare le aree \"Rosso\" e \"Blu\" sul piatto dove puntare per il banco e il player, rispettivamente.\n\nImpostare \"Area Riposo\" su una zona dell'interfaccia non interattiva, ma pur sempre nella schermata.\n\nImpostare \"Area Vincita\" sulla striscia che indica lo status della manche (\"Attendi la prossima partita\", \"Giocatore Vince\" ecc.).\n\nImpostare \"Area Mazzo\" sul numero del mazzo corrente (inclusivo del \"#\" che lo precede).\n\nImpostare \"Area Raddoppio\" sul pulsante dedicato per il raddoppio (se presente).\n\nImpostare, se necessario, \"Zoom Monitor\" immettendo la percentuale di zoom necessaria rispetto allo standard (1920x1080).", "Info", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        public void start_withPreScan()
        {
            AreaElement area = ListAreaElement.Instance.GetAreaByKey("AREA_SALDO");
            if (area == null || area.startX == 0)
            {
                MessageBox.Show("Per poter leggere il saldo devi specificarne l'area!\n\nAlternativamente, disabilitare il checkbox \"Saldo Autom.\" e inserire manualmente il Saldo Iniziale prima di avviare il bot.", "AREA MANCANTE", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
            if (MessageBox.Show("Controllare che il numero del Deck sia inferiore a 50.\nL'avvio oltre tale numero potrebbe portare a comportamenti inaspettati\n\nAvviare il bot?", "ATTENZIONE", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
            {
                base.Opacity = 0.85;
                using (WaitScanBalanceWindow fw = new WaitScanBalanceWindow(new Action(this.scanSaldo)))
                {
                    fw.ShowDialog(this);
                }
                base.Opacity = 1.0;
                /*
                try
                {
                    this.balanceStartValue.Value = Convert.ToDecimal(OCReads.balance);
                    this.labelStatus.Text = "start saldo : " + OCReads.balance;
                    for (int i = 0; i < 8; i++)
                    {
                        Thread.Sleep(250);
                        this.labelStatus.ForeColor = Color.Red;
                        base.Update();
                        Thread.Sleep(250);
                        this.labelStatus.ForeColor = SystemColors.ControlText;
                        base.Update();
                    }
                }
                catch (Exception)
                {
                    this.labelStatus.Text = "start saldo : NON RILEVATO!";
                    for (int j = 0; j < 8; j++)
                    {
                        Thread.Sleep(250);
                        this.labelStatusRoulette.ForeColor = Color.Red;
                        base.Update();
                        Thread.Sleep(250);
                        this.labelStatusRoulette.ForeColor = SystemColors.ControlText;
                        base.Update();
                    }
                }
                */
                this.start_all(true);
            }
        }

        private void scanSaldo()
        {
            AreaElement area = ListAreaElement.Instance.GetAreaByKey((Runtime.game == 0) ? "AREA_SALDO" : "AREA_SALDO_ROULETTE");
            OCRScan ocrScan = new OCRScan();
            int width = area.endX - area.startX;
            int height = area.endY - area.startY;
            Rectangle monitorArea = new Rectangle(area.startX, area.startY, width, height);
            Bitmap currentImage = Gamebot.Models.Monitor.Instance.CaptureScreen(monitorArea);
            int saldo_letto = -1;
            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(600);
                OCRResponse ocrResponse = ocrScan.GetTextFromBitmapAreaSaldo(currentImage, false, false, "balance");
                OCReads.balance = "-1";
                if (ocrResponse.GetResponse().SuccessScan)
                {
                    bool res = int.TryParse(ocrResponse.GetResponse().Message.Trim().Replace("#", ""), out saldo_letto);
                    try
                    {
                        OCReads.balance = ocrResponse.GetResponse().Message.Replace(".", "").Trim();
                        if (OCReads.balance.Equals(Runtime.readSaldo))
                        {
                            Runtime.ocrBalanceCorrect++;
                        }
                        else
                        {
                            Runtime.ocrBalanceIncorrect++;
                        }
                        List<string> values = new List<string>();
                        UpdateInterface.GetInstanceForm().progressBalance.Report(values);
                    }
                    catch (Exception)
                    {
                        OCReads.balance = (res ? Convert.ToString(saldo_letto) : "-1");
                    }
                }
            }
        }

        private void checkBox1_CheckedChanged_1(object sender, EventArgs e)
        {
            Config.send_end_sculping_message = ((CheckBox)sender).Checked;
        }

        private void textAreaInfoBtn_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Il campo \"Testo TIE\" contiene il valore del testo associato al TIE nell'\"AREA VINCITA\".\n\nIl campo \"Testo VINCE\" contiene il valore del testo associato a VINCE nell'\"AREA VINCITA\".\n\nIl campo \"Testo BANCO\" contiene il valore del testo associato a BANCO nell'\"AREA VINCITA\".\n\nIl campo \"Testo GIOCATORE\" contiene il valore del testo associato a GIOCATORE nell'\"AREA VINCITA\".\n", "Info", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private async void button2_Click_1(object sender, EventArgs e)
        {
            DocumentSnapshot snap = await FirestoreHelpers.Database.Collection("settings").Document("configuration").GetSnapshotAsync(default(CancellationToken));
            if (snap.Exists)
            {
                snap.ConvertTo<FirebaseStructureDBSettings>();
            }
        }

        private void typeGamenInfobtn_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Il \"Tipologia GIOCO\" indica come viene gestito il click della giocata (click sulla fiche).\n\n\"VERA\" indica che viene effettuato il click sulla fiche.\n\n\"DEMO\" indica che non viene effettuato il click sulla fiche.\nI valori dei saldi e della Mani Vinte / Perse sono aggiornati lo stesso, ma non viene effettuata nessuna puntata", "Info", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void testBtnWindowOnTop_Click(object sender, EventArgs e)
        {
            new TopAlmostWindow().Show();
        }

        private void buttonBet_Click(object sender, EventArgs e)
        {
            this.GetZoom();
            UIForm.ClickButtonGreen(sender, "AREA_PUNTARE");
        }

        public IProgress<List<string>> progressUI;

        public IProgress<List<string>> progressUIRoulette;

        public IProgress<List<string>> progressBalance;

        public IProgress<List<string>> progressTimeElapsed;

        private int numUpDownInputMartingala;

        private List<Control> controlsToEnableDisable = new List<Control>();

        private List<Control> controlsRouletteToEnableDisable = new List<Control>();

        private List<Control> controlsRouletteToEnableDisableHand1 = new List<Control>();

        private List<Control> controlsRouletteToEnableDisableHand2 = new List<Control>();

        private List<Control> controlsRouletteToEnableDisableHand3 = new List<Control>();

        private readonly IRequestApi _requestApiRepository;

    }
}
