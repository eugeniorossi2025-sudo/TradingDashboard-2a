using System.Data;
using System.Data.SqlClient;
using System.Text.Json;
using Decisore.Engine;
using Decisore.Models;
using Microsoft.Extensions.Configuration;

namespace Decisore.Repository
{
    public class ProfitResponse
    {
        public decimal Margine { get; set; }
        public decimal SaldoIniziale { get; set; }
    }

    public class DatabaseRepository
    {
        private readonly string _connString;

        public DatabaseRepository(IConfiguration config)
        {
            _connString = config.GetConnectionString("DefaultConnection");
        }

        // VALIDAZIONE UTENTE (UpS_Users_Api)
        public int ValidateUser(string username, string password)
        {
            var conn = new SqlConnection(_connString);
            conn.Open();

            using var cmd = new SqlCommand("UpS_Users_Api", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            SqlCommandBuilder.DeriveParameters(cmd);

            cmd.Parameters["@Username"].Value = username;
            cmd.Parameters["@Password"].Value = password;

            var dt = new DataTable();
            using var da = new SqlDataAdapter(cmd);
            da.Fill(dt);

            conn.Close();

            if (dt.Rows.Count == 0)
                return -1; // utente NON valido

            return Convert.ToInt32(dt.Rows[0]["ID"]);
        }

        public decimal GetMargine(string computer)
        {
            using var conn = new SqlConnection(_connString);
            conn.Open();

            using var cmd = new SqlCommand(
                "SELECT MARGINE FROM dbo.Pc_CurrentStatus WHERE COMPUTER = @C", conn);

            cmd.Parameters.AddWithValue("@C", computer);

            return cmd.ExecuteScalar() is decimal d ? d : 0m;
        }

        public ProfitResponse GetProfitData(string computer)
        {
            using var conn = new SqlConnection(_connString);
            conn.Open();

            using var cmd = new SqlCommand(@"
        SELECT MARGINE, SALDO_INIZIALE
        FROM dbo.Pc_CurrentStatus
        WHERE COMPUTER = @C", conn);

            cmd.Parameters.AddWithValue("@C", computer);

            using var r = cmd.ExecuteReader();

            if (!r.Read())
                return new ProfitResponse { Margine = 0, SaldoIniziale = 0 };

            return new ProfitResponse
            {
                Margine = r["MARGINE"] as decimal? ?? 0m,
                SaldoIniziale = r["SALDO_INIZIALE"] as decimal? ?? 0m
            };
        }


        public DataTable GetAllPcStatus()
        {
            using var conn = new SqlConnection(_connString);
            conn.Open();

            using var cmd = new SqlCommand(@"
                SELECT
                    COMPUTER,
                    ACCOUNT,
                    TAVOLO,
                    STATO,
                    COLORE,
                    ALLARME,

                    SALDO_ISTANTANEO,
                    MARGINE,
                    MEDIA_ORA,

                    COLPO_MARTINGALA,
                    ORE,

                    MAZZO,
                    PBT,

                    LAST_UPDATE,
                    LAST_ADVICE
                FROM dbo.Pc_CurrentStatus
                ORDER BY LAST_UPDATE DESC
            ", conn);

            cmd.CommandType = CommandType.Text;

            var dt = new DataTable();
            using var da = new SqlDataAdapter(cmd);
            da.Fill(dt);

            return dt;
        }

        // SALVATAGGIO PARAMETRI (upI_Values)
        public string OldSaveRequestValue(long key, string field, string value, int userId, bool skipSave)
        {
            var conn = new SqlConnection(_connString);
            conn.Open();
            using var cmd = new SqlCommand("upI_Values", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            SqlCommandBuilder.DeriveParameters(cmd);

            cmd.Parameters["@Key"].Value = key;
            cmd.Parameters["@Description"].Value = field;
            cmd.Parameters["@Value"].Value = value;
            cmd.Parameters["@Id_User"].Value = userId;
            cmd.Parameters["@SkipSave"].Value = skipSave ? 1 : 0;

            try
            {
                cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"ERRORE DB: {ex.Message}");
                conn.Close();
            }

            conn.Close();

            if (field.Equals("COMPUTER", StringComparison.OrdinalIgnoreCase))
            {
                return cmd.Parameters["@ID"].Value.ToString();
            }

            return null;
        }

        public void SaveRequestValue(
            long key,
            string field,
            string value,
            int userId,
            bool skipSave
        )
        {
            _ = Task.Run(async () =>
            {
                var conn = new SqlConnection(_connString);
                try
                {
                    conn.Open();
                    using var cmd = new SqlCommand("upI_Values", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    SqlCommandBuilder.DeriveParameters(cmd);

                    cmd.Parameters["@Key"].Value = key;
                    cmd.Parameters["@Description"].Value = field;
                    cmd.Parameters["@Value"].Value = value;
                    cmd.Parameters["@Id_User"].Value = userId;
                    cmd.Parameters["@SkipSave"].Value = skipSave ? 1 : 0;

                    // Fire & forget reale
                    cmd.ExecuteNonQuery();
                }
                catch
                {
                }
                finally
                {
                    conn.Close();
                }
            });
        }

        public int SaveConfigurationFile(string pc, string config)
        {
            using var conn = new SqlConnection(_connString);

            try
            {
                conn.Open();

                string sql = @"
        INSERT INTO dbo.ApiConfigurations (pc, config)
        VALUES (@pc, @config);

        SELECT SCOPE_IDENTITY();";

                using var cmd = new SqlCommand(sql, conn);

                cmd.Parameters.Add("@pc", SqlDbType.NVarChar, 10)
                    .Value = (object)pc ?? DBNull.Value;

                cmd.Parameters.Add("@config", SqlDbType.NVarChar, 4000)
                    .Value = config;

                var newId = cmd.ExecuteScalar();
                return Convert.ToInt32(newId);
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"ERRORE DB SaveConfigurationFile: {ex.Message}");
                return -1;
            }
        }

        public Dictionary<string, string> GetConfigurations()
        {
            var result = new Dictionary<string, string>();

            using var conn = new SqlConnection(_connString);
            conn.Open();

            using var cmd = new SqlCommand(
                "SELECT K, Value FROM dbo.Configurations ORDER BY pos",
                conn
            );

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var key = reader["K"].ToString();
                var value = reader["Value"] == DBNull.Value
                    ? null
                    : reader["Value"].ToString();

                result[key] = value;
            }

            return result;
        }

        public void UpdatePcStatusSimple(
            long key,
            string computer,
            string account,
            string tavolo,
            double saldoIniziale,
            double saldoIstantaneo,
            double margine,
            double valoreGiocato,
            int colpoMartingala,
            string stato,
            string mazzo,
            string mazzoCalcolato,
            decimal ore,
            string chosenColor)
        {
            _ = Task.Run(async () =>
            {
                var conn = new SqlConnection(_connString);

                try
                {
                    conn.Open();
                    using var cmd = new SqlCommand("Upsert_Pc_CurrentStatus_Simple", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    cmd.Parameters.AddWithValue("@COMPUTER", computer);
                    cmd.Parameters.AddWithValue("@KEY", key);
                    cmd.Parameters.AddWithValue("@PBT", " ");
                    cmd.Parameters.AddWithValue("@ACCOUNT", (object)account ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@TAVOLO", (object)tavolo ?? DBNull.Value);

                    cmd.Parameters.Add("@SALDO_INIZIALE", SqlDbType.Decimal).Value = saldoIniziale;
                    cmd.Parameters.Add("@SALDO_ISTANTANEO", SqlDbType.Decimal).Value = saldoIstantaneo;
                    cmd.Parameters.Add("@MARGINE", SqlDbType.Decimal).Value = margine;
                    cmd.Parameters.Add("@VALORE_GIOCATO", SqlDbType.Decimal).Value = valoreGiocato;

                    cmd.Parameters.AddWithValue("@COLPO_MARTINGALA", colpoMartingala);
                    cmd.Parameters.AddWithValue("@STATO", (object)stato ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@MAZZO", (object)mazzo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@MAZZO_CALCOLATO", (object)mazzoCalcolato ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@CHOSEN_COLOR", (object)chosenColor ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@ORE", ore);

                    cmd.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"ERRORE DB: {ex.Message}");
                }
                finally
                {
                    conn.Close();
                }
            });
        }

        public void UpdatePcStatusDeck(
            long key,
            string computer,
            string account,
            string tavolo,
            string mazzo,
            string mazzoCalcolato)
        {
            _ = Task.Run(async () =>
            {
                var conn = new SqlConnection(_connString);

                try
                {
                    conn.Open();
                    using var cmd = new SqlCommand("Upsert_Pc_CurrentStatus_Deck", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    cmd.Parameters.AddWithValue("@COMPUTER", computer);
                    cmd.Parameters.AddWithValue("@KEY", key);
                    cmd.Parameters.AddWithValue("@ACCOUNT", (object)account ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@TAVOLO", (object)tavolo ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@MAZZO", (object)mazzo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@MAZZO_CALCOLATO", (object)mazzoCalcolato ?? DBNull.Value);

                    cmd.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"ERRORE DB: {ex.Message}");
                }
                finally
                {
                    conn.Close();
                }
            });
        }

        public void UpdatePcStatus(long key,
            string computer,
            string account,
            string tavolo,
            double saldoIniziale,
            double saldoIstantaneo,
            double margine,
            double valoreGiocato,
            int colpoMartingala,
            string stato,
            string mazzo,
            int mazzoCalcolato,
            string pbt,
            decimal ore,
            string lastAdvice,
            string lastInfo,
            string missionSnapshot,
            string valutazioneRisultato,
            int lastAction,
            string chosenColor)
        {
            _ = Task.Run(async () =>
            {
                var conn = new SqlConnection(_connString);

                try
                {
                    conn.Open();
                    using var cmd = new SqlCommand("Upsert_Pc_CurrentStatus", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    cmd.Parameters.AddWithValue("@COMPUTER", computer);
                    cmd.Parameters.AddWithValue("@KEY", key);

                    cmd.Parameters.AddWithValue("@ACCOUNT", (object)account ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@TAVOLO", (object)tavolo ?? DBNull.Value);

                    cmd.Parameters.Add("@SALDO_INIZIALE", SqlDbType.Decimal).Value = saldoIniziale;
                    cmd.Parameters.Add("@SALDO_ISTANTANEO", SqlDbType.Decimal).Value = saldoIstantaneo;
                    cmd.Parameters.Add("@MARGINE", SqlDbType.Decimal).Value = margine;
                    cmd.Parameters.Add("@VALORE_GIOCATO", SqlDbType.Decimal).Value = valoreGiocato;

                    cmd.Parameters.AddWithValue("@COLPO_MARTINGALA", colpoMartingala);

                    cmd.Parameters.AddWithValue("@STATO", (object)stato ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@MAZZO", (object)mazzo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@MAZZO_CALCOLATO", (object)mazzo ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@PBT", (object)pbt ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CHOSEN_COLOR", (object)chosenColor ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@ORE", ore);

                    cmd.Parameters.AddWithValue("@LAST_ADVICE", lastAdvice);
                    cmd.Parameters.AddWithValue("@LAST_INFO", lastInfo);
                    cmd.Parameters.AddWithValue("@MISSION_SNAPSHOT", missionSnapshot);
                    cmd.Parameters.AddWithValue("@VALUTAZIONE_RISULTATO", valutazioneRisultato);
                    cmd.Parameters.AddWithValue("@LAST_ACTION", lastAction);

                    cmd.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"ERRORE DB: {ex.Message}");
                }
                finally
                {
                    conn.Close();
                }
            });
        }

        public void UpdateMargin(string telemetry, double elapsedMinutesMax)
        {
            _ = Task.Run(() =>
            {
                using var conn = new SqlConnection(_connString);

                try
                {
                    conn.Open();

                    // 1️⃣ AggiornaStatistiche
                    using (var cmd = new SqlCommand("AggiornaStatistiche", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@TELEMETRY", telemetry);
                        cmd.Parameters.AddWithValue("@ELAPSED", elapsedMinutesMax);
                        cmd.ExecuteNonQuery();
                    }


                    // 2️⃣ InsertMargine
                    using (var cmd = new SqlCommand("InsertMargine", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"ERRORE DB: {ex.Message}");
                }
            });
        }


        public void ClearPcStatus()
        {
            using var conn = new SqlConnection(_connString);
            conn.Open();

            using var tran = conn.BeginTransaction();

            try
            {
                using (var closeStatsCmd = new SqlCommand(@"
                    UPDATE dbo.Statistiche
                    SET DATA_FINE = SYSUTCDATETIME()
                    WHERE DATA_FINE IS NULL;
                ", conn, tran))
                {
                    closeStatsCmd.ExecuteNonQuery();
                }

                var telemetryJson = JsonSerializer.Serialize(new Telemetry());

                var isStatisticheIdIdentity = false;
                using (var identityCmd = new SqlCommand(@"
                    SELECT COLUMNPROPERTY(OBJECT_ID('dbo.Statistiche'), 'ID', 'IsIdentity');
                ", conn, tran))
                {
                    isStatisticheIdIdentity = Convert.ToInt32(identityCmd.ExecuteScalar() ?? 0) == 1;
                }

                var insertSql = isStatisticheIdIdentity
                    ? @"
                        INSERT INTO dbo.Statistiche
                        (
                            TELEMETRY,
                            DATA_INIZIO,
                            MARGINE_TOT,
                            MARGINE_MIN,
                            MARGINE_MAX,
                            CREATED_AT,
                            ELAPSED
                        )
                        VALUES
                        (
                            @Telemetry,
                            SYSUTCDATETIME(),
                            @MargineTot,
                            @MargineMin,
                            @MargineMax,
                            SYSUTCDATETIME(),
                            @Elapsed
                        );"
                    : @"
                        INSERT INTO dbo.Statistiche
                        (
                            ID,
                            TELEMETRY,
                            DATA_INIZIO,
                            MARGINE_TOT,
                            MARGINE_MIN,
                            MARGINE_MAX,
                            CREATED_AT,
                            ELAPSED
                        )
                        VALUES
                        (
                            (SELECT ISNULL(MAX(ID), 0) + 1 FROM dbo.Statistiche WITH (UPDLOCK, HOLDLOCK)),
                            @Telemetry,
                            SYSUTCDATETIME(),
                            @MargineTot,
                            @MargineMin,
                            @MargineMax,
                            SYSUTCDATETIME(),
                            @Elapsed
                        );";

                using (var insertCmd = new SqlCommand(insertSql, conn, tran))
                {
                    insertCmd.Parameters.AddWithValue("@Telemetry", telemetryJson);
                    insertCmd.Parameters.AddWithValue("@MargineTot", 0);
                    insertCmd.Parameters.AddWithValue("@MargineMin", 0);
                    insertCmd.Parameters.AddWithValue("@MargineMax", 0);
                    insertCmd.Parameters.AddWithValue("@Elapsed", 0);

                    insertCmd.ExecuteNonQuery();
                }


                using (var deleteCmd = new SqlCommand(@"
                    DELETE FROM dbo.Pc_CurrentStatus_PBT_History;
                    DELETE FROM dbo.Pc_CurrentStatus;
                    DELETE FROM dbo.Margini;
                ", conn, tran))
                {
                    deleteCmd.ExecuteNonQuery();
                }

                tran.Commit();
            }
            catch (SqlException ex)
            {
                tran.Rollback();
                Console.WriteLine($"ERRORE DB: {ex.Message}");
                throw;
            }
        }

        public void SaveApiLog(string description, string category, int action)
        {
            _ = Task.Run(() =>
            {
                using var conn = new SqlConnection(_connString);
                try
                {
                    conn.Open();
                    using var cmd = new SqlCommand(@"
                INSERT INTO ApiLogs
                (Description, Category, Action, CreatedAt)
                VALUES (@Description, @Category, @Action, SYSUTCDATETIME())
            ", conn);

                    cmd.Parameters.AddWithValue("@Description", description);
                    cmd.Parameters.AddWithValue("@Category", category);
                    cmd.Parameters.AddWithValue("@Action", action);

                    cmd.ExecuteNonQuery();
                }
                catch
                {
                }
            });
        }
    }
}