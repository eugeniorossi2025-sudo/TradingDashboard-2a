using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using EuGenio.ProattivoSempliceRegiaAstronaveAdattivaPro220_1;

public partial class _Index : System.Web.UI.Page
{
    // Cache statica per tenere traccia del mazzo per ogni combinazione (username, computer, tavolo)
    private class DeckState
    {
        public int LastMazzo { get; set; }
        public int HandIndex { get; set; }
        public int CarteTotali { get; set; }
    }

    private static Dictionary<string, DeckState> _deckStates = new Dictionary<string, DeckState>();
    private static object _lock = new object();

    // Engine statico condiviso
    private static ProactiveEngine _engine = new ProactiveEngine();

    protected void Page_Load(object sender, EventArgs e)
    {
        string retValue = "0";
        try
        {
            long KEY = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmssfff"));

            string username = Utility.VerString(Request.QueryString["username"]);
            string password = Utility.VerString(Request.QueryString["password"]);

            if (username != "" && password != "")
            {
                // Log opzionale (commentato)
                // LogRequest(KEY);

                SqlConnection oCon = Database.GetConn();
                SqlCommand oCmd = new SqlCommand();
                oCmd.Connection = oCon;
                oCmd.CommandText = "UpS_Users_Api";
                oCmd.CommandType = CommandType.StoredProcedure;
                SqlCommandBuilder.DeriveParameters(oCmd);
                oCmd.Parameters["@Username"].Value = username;
                oCmd.Parameters["@Password"].Value = password;

                DataTable Dt = new DataTable();
                SqlDataAdapter Da = new SqlDataAdapter();
                Da.SelectCommand = oCmd;
                Da.Fill(Dt);

                if (Dt.Rows.Count > 0)
                {
                    int userId = Utility.VerInt32(Dt.Rows[0]["ID"]);

                    // Estrai parametri chiave
                    string computer = Request.QueryString["COMPUTER"];
                    string tavolo = Request.QueryString["TAVOLO"];
                    string margine = Request.QueryString["MARGINE"];
                    string colpoMartingala = Request.QueryString["COLPO_MARTINGALA"];
                    string pbt = Request.QueryString["PBT"];
                    string mazzo = Request.QueryString["MAZZO"];
                    string tempo = Request.QueryString["TEMPO"];

                    bool hasCompleteData = !string.IsNullOrEmpty(computer) && !string.IsNullOrEmpty(tavolo) &&
                        !string.IsNullOrEmpty(pbt) && !string.IsNullOrEmpty(margine) &&
                        !string.IsNullOrEmpty(colpoMartingala) && !string.IsNullOrEmpty(mazzo);

                    // Salva tutti i parametri nel database come prima
                    foreach (string key in Request.QueryString.AllKeys)
                    {
                        if (key.ToLower() != "username" && key.ToLower() != "password")
                        {
                            oCmd = new SqlCommand();
                            oCmd.Connection = oCon;
                            oCmd.CommandText = "upI_Values";
                            oCmd.CommandType = CommandType.StoredProcedure;
                            SqlCommandBuilder.DeriveParameters(oCmd);
                            oCmd.Parameters["@Key"].Value = KEY;
                            oCmd.Parameters["@Description"].Value = key;
                            oCmd.Parameters["@Value"].Value = Request.QueryString[key];
                            oCmd.Parameters["@Id_User"].Value = userId;
                            if (Request.QueryString.Count == 3)
                                oCmd.Parameters["@SkipSave"].Value = 1;
                            else
                                oCmd.Parameters["@SkipSave"].Value = 0;

                            oCmd.ExecuteNonQuery();

                            if (key == "COMPUTER")
                            {
                                retValue = oCmd.Parameters["@ID"].Value.ToString();
                            }
                        }
                    }

                    // SE ABBIAMO DATI COMPLETI, CHIAMIAMO IL PROACTIVE ENGINE
                    if (hasCompleteData)
                    {
                        try
                        {
                            int tableId = int.Parse(tavolo);
                            decimal margineValue = decimal.Parse(margine.Replace(",", "."), CultureInfo.InvariantCulture);
                            int martingalaLevel = int.Parse(colpoMartingala) + 1;
                            char esito = 'P';
                            if (pbt.ToUpper() == "B") esito = 'B';
                            else if (pbt.ToUpper() == "T") esito = 'T';

                            int carteRimaste = int.Parse(mazzo);

                            // Calcola handIndexMazzo usando la cache
                            string deckKey = username + "_" + computer + "_" + tableId;
                            int handIndexMazzo = 1;

                            lock (_lock)
                            {
                                if (!_deckStates.ContainsKey(deckKey))
                                {
                                    _deckStates[deckKey] = new DeckState
                                    {
                                        LastMazzo = carteRimaste,
                                        HandIndex = 0,
                                        CarteTotali = 416
                                    };
                                }

                                DeckState deckState = _deckStates[deckKey];

                                // Rilevamento nuovo mazzo
                                if (carteRimaste > deckState.LastMazzo && carteRimaste > deckState.CarteTotali - 20)
                                {
                                    deckState.CarteTotali = carteRimaste > 450 ? 520 :
                                                           carteRimaste > 350 ? 416 :
                                                           carteRimaste > 250 ? 312 : 416;
                                    deckState.HandIndex = 0;
                                    deckState.LastMazzo = carteRimaste;
                                    Functions.Log("NUOVO MAZZO", "Computer=" + computer + ", Table=" + tableId + ", Carte=" + carteRimaste + ", CarteTotali=" + deckState.CarteTotali);
                                }

                                int carteDiff = deckState.LastMazzo - carteRimaste;

                                if (carteDiff < 0)
                                {
                                    int carteGiocate = deckState.CarteTotali - carteRimaste;
                                    deckState.HandIndex = Math.Max(0, carteGiocate / 4);
                                    deckState.LastMazzo = carteRimaste;
                                    Functions.Log("RESET MAZZO", "Computer=" + computer + ", Table=" + tableId + ", HandIndex=" + deckState.HandIndex);
                                }

                                if (carteDiff >= 4)
                                {
                                    int maniGiocate = carteDiff / 4;
                                    deckState.HandIndex += maniGiocate;
                                    deckState.LastMazzo = carteRimaste;
                                }

                                handIndexMazzo = deckState.HandIndex > 0 ? deckState.HandIndex : 1;
                            }

                            // Calcola tempo trascorso
                            double elapsedMinutes = 0;
                            if (!string.IsNullOrEmpty(tempo))
                            {
                                string[] parts = tempo.Split(':');
                                if (parts.Length >= 2)
                                {
                                    int hours = 0, minutes = 0;
                                    if (int.TryParse(parts[0], out hours) && int.TryParse(parts[1], out minutes))
                                    {
                                        elapsedMinutes = hours * 60 + minutes;
                                    }
                                }
                            }

                            // Calcola hot zone
                            bool isInHotZone = (handIndexMazzo >= 11 && handIndexMazzo <= 20) ||
                                               (handIndexMazzo >= 41 && handIndexMazzo <= 50) ||
                                               (handIndexMazzo >= 51 && handIndexMazzo <= 60) ||
                                               (handIndexMazzo >= 61 && handIndexMazzo <= 70);

                            Functions.Log("HOT ZONE CHECK", "HandIndex=" + handIndexMazzo + ", IsHotZone=" + isInHotZone + ", Table=" + tableId);

                            // CHIAMATA AL PROACTIVE ENGINE
                            Advice advice = _engine.FeedAndDecide(
                                tableId: tableId,
                                handIndexMazzo: handIndexMazzo,
                                margineK: margineValue,
                                martingalaUi: martingalaLevel,
                                bSignalW10: false,
                                bHotZone: isInHotZone,
                                esito: esito,
                                totalElapsedMinutes: elapsedMinutes,
                                totaltables: 1
                            );

                            // Calcola ActionCode dalla Reason
                            int actionCode = 0;

                            if (advice.Reason.IndexOf("StopPc", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                advice.Reason.IndexOf("Stop PC", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                advice.Reason.IndexOf("fermati", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                advice.Reason.IndexOf("blocco", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                advice.Prediction == "Disabled" ||
                                advice.TableStatus.Contains("Disabled") ||
                                advice.TableStatus.Contains("🔴") ||
                                advice.StopAtL5)
                            {
                                actionCode = 1; // STOP PC
                            }
                            else if (advice.Reason.IndexOf("azzera", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                     advice.Reason.IndexOf("reset", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                     advice.Reason.IndexOf("SafeWin", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                     advice.Reason.IndexOf("martingala", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                actionCode = 2; // AZZERA MARTINGALA
                            }
                            else if (advice.Reason.IndexOf("start", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                     advice.Reason.IndexOf("avvia", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                     advice.Reason.IndexOf("StartPc", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                actionCode = 3; // START PC
                            }

                            retValue = actionCode.ToString();

                            //Functions.Log("ProactiveEngine", $"Computer={computer}, Table={tableId}, Level={advice.LevelIndex}, Prediction={advice.Prediction}, HotZone={advice.HotZone}, HotZoneLabel={advice.HotZoneLabel}, ActionCode={actionCode}");
                        }
                        catch (Exception engineEx)
                        {
                            Functions.Log("ProactiveEngine ERROR", engineEx.Message + " - " + engineEx.StackTrace);
                            retValue = "9"; // Errore engine
                        }
                    }

                    Lit_Response.Text = retValue;
                }
                else
                {
                    Lit_Response.Text = "0"; // Credenziali errate
                }

                oCon.Close();
                oCon.Dispose();
            }
            else
            {
                Response.Redirect("/Login.aspx");
            }
        }
        catch (Exception ex)
        {
            Functions.Log("Page_Load", ex.Message + " - " + ex.StackTrace);
            Lit_Response.Text = "9"; // Errore generico
        }
    }

    // Metodo helper per logging completo della richiesta (opzionale, da decommentare se serve)
    /*
    private void LogRequest(long KEY)
    {
        string msg = "";
        foreach (string key in Request.QueryString.AllKeys)
        {
            msg += key + ": " + Request.QueryString[key] + System.Environment.NewLine;
        }

        System.IO.File.AppendAllText(
            Server.MapPath("~/Log") + @"\log_" +
            CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday) + ".txt",
            System.Environment.NewLine + "Datetime: " + DateTime.Now + System.Environment.NewLine + "Message:" + System.Environment.NewLine + msg + "____________________________________________________________________________________________________" + System.Environment.NewLine
        );
    }
    */
}
