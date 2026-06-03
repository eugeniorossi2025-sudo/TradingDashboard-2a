using System.Data;
using Microsoft.AspNetCore.Mvc;
using Decisore.Engine;
using Decisore.Models;
using Decisore.Services;
using Decisore.Repository;
using System.Globalization;
using System.Text.Json;

public enum Azione
{
    Nulla = 0,
    StopPc = 1,
    AzzeraMartingala = 2,
    StartPc = 3
}

namespace Decisore.Controllers
{
    [ApiController]
    [Route("api/proactive")]
    public class ProactiveController : ControllerBase
    {
        private readonly ProactiveEngineService _engine;
        private readonly DatabaseRepository _db;
        private readonly LoggingService _log;
        private readonly AppStateService _state;

        public ProactiveController(
            ProactiveEngineService engine,
            DatabaseRepository db,
            LoggingService log,
            AppStateService state)
        {
            _engine = engine;
            _db = db;
            _log = log;
            _state = state;
        }
        
        [HttpGet("emergency-stop")]
        public int Stop()
        {
            int totalTables = 0;
            var allPCStatus = _db.GetAllPcStatus().Rows;

            try
            {
                DateTimeOffset nowUtc = DateTimeOffset.UtcNow;

                int count = allPCStatus
                    .Cast<DataRow>()
                    .Where(r => r["LAST_UPDATE"] != DBNull.Value)
                    .Count(r =>
                    {
                        DateTime lastUpdate = DateTime.SpecifyKind(
                            (DateTime)r["LAST_UPDATE"],
                            DateTimeKind.Utc);

                        return (nowUtc - lastUpdate).TotalSeconds <= 300;
                    });

                totalTables = count;
            }
            catch
            {
                    
            }
            
            if (totalTables > 0) _engine.emergencyStop();
            return 1;
        }
        
        [HttpPost("update-deck")]
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult UpdateDeck([FromForm] RequestParams p)
        {
            try
            {
                // 1️⃣ Validazione utente (come fai in Decide)
                int userId = _db.ValidateUser(p.USERNAME, p.PASSWORD);
                if (userId == -1)
                {
                    return Ok(0); // utente non valido
                }

                // 2️⃣ Genero KEY come nell’altra API
                long KEY = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                
                // 4️⃣ Aggiorno direttamente la tabella (senza engine)
                _db.UpdatePcStatusDeck(
                    key: KEY,
                    computer: p.COMPUTER,
                    account: p.USERNAME,
                    tavolo: p.TAVOLO,
                    mazzo: p.MAZZO,
                    mazzoCalcolato: p.MAZZO
                );

                return Ok(1); // aggiornato correttamente
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Ok(9); // errore
            }
        }
        
        [HttpPost("get-global-profit")]
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult GetGlobalProfit([FromForm] RequestParams p)
        {
            try
            {
                int userId = _db.ValidateUser(p.USERNAME, p.PASSWORD);
                if (userId == -1)
                    return Ok(new ProfitResponse());

                var profitData = _db.GetProfitData(p.COMPUTER);
                Console.WriteLine($"{p.COMPUTER} ha letto dal db saldo iniziale: {profitData.SaldoIniziale:F2}, margine: {profitData.Margine:F2}");
                return Ok(profitData);
            }
            catch
            {
                return Ok(new ProfitResponse());
            }
        }
        
        [HttpPost("update-params")]
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult UpdateParams([FromForm] RequestParams p)
        {
            try
            {
                // 1️⃣ Validazione utente (come fai in Decide)
                int userId = _db.ValidateUser(p.USERNAME, p.PASSWORD);
                if (userId == -1)
                {
                    return Ok(0); // utente non valido
                }

                // 2️⃣ Genero KEY come nell’altra API
                long KEY = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                
                _state.LoadConfigurations();
                var cfg = _state.Configurations;
                
                double baseUnit = cfg.TryGetValue("BASE_UNIT", out var bU)
                    ? double.Parse(bU, CultureInfo.InvariantCulture)
                    : 1;

                // 3️⃣ Parsing valori numerici MINIMI per aggiornare la tabella
                double saldoIniziale = 0;
                double saldoIstantaneo = 0;
                double margine = 0;
                double valoreGiocato = 0;
                decimal ore = 0;

                if (!string.IsNullOrEmpty(p.SALDO_INIZIALE))
                    saldoIniziale =
                        (double)decimal.Parse(p.SALDO_INIZIALE.Replace(",", "."), CultureInfo.InvariantCulture);
                else
                    saldoIniziale = 1000;

                if (!string.IsNullOrEmpty(p.SALDO_ISTANTANEO))
                    saldoIstantaneo = (double)decimal.Parse(p.SALDO_ISTANTANEO.Replace(",", "."), CultureInfo.InvariantCulture);
                else
                    saldoIstantaneo = saldoIniziale; // fallback

                if (!string.IsNullOrEmpty(p.MARGINE))
                {
                    margine = (double)decimal.Parse(p.MARGINE.Replace(",", "."), CultureInfo.InvariantCulture); 
                    saldoIstantaneo = saldoIniziale + margine;
                }

                if (!string.IsNullOrEmpty(p.VALORE_GIOCATO))
                    valoreGiocato = (double)decimal.Parse(p.VALORE_GIOCATO, CultureInfo.InvariantCulture) * baseUnit;

                if (!string.IsNullOrEmpty(p.TEMPO))
                {
                    var parts = p.TEMPO.Split(':');
                    if (parts.Length >= 2 &&
                        int.TryParse(parts[0], out int h) &&
                        int.TryParse(parts[1], out int m))
                    {
                        ore = Convert.ToDecimal((h * 60 + m) / 60.0);
                    }
                }
                
                Console.WriteLine($"Chiamata PC: {p.COMPUTER} Saldo iniziale: {saldoIniziale:F2} Margine: {margine:F2} ");
                
                _engine.updateCurrentBet(p.COMPUTER, int.Parse(p.TAVOLO), valoreGiocato, int.Parse(p.MAZZO));
                
                String giocata = " ";
                if (!String.IsNullOrEmpty(p.CHOSEN_COLOR))
                {
                    giocata = p.CHOSEN_COLOR;
                }
                
                _db.UpdatePcStatusSimple(
                    key: KEY,
                    computer: p.COMPUTER,
                    account: p.USERNAME,
                    tavolo: p.TAVOLO,
                    saldoIniziale: saldoIniziale,
                    saldoIstantaneo: saldoIstantaneo,
                    margine: margine,
                    valoreGiocato: valoreGiocato,
                    colpoMartingala: (string.IsNullOrEmpty(p.COLPO_MARTINGALA) ? 0 : int.Parse(p.COLPO_MARTINGALA)) + 1,
                    stato: p.STATO,
                    mazzo: p.MAZZO,
                    mazzoCalcolato: p.MAZZO,
                    ore: ore,
                    chosenColor: giocata
                );

                return Ok(1);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Ok(9); // errore
            }
        }
        
        [HttpPost("bot-app-config")]
        public IActionResult LoadBotAppConfig([FromBody] BotAppConfigDTO body)
        {
            try
            {
                if (body == null || string.IsNullOrEmpty(body.content))
                {
                    return BadRequest("Contenuto file mancante");
                }

                _db.SaveConfigurationFile(body.pc, body.content);

                return Ok(1);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errore POST: " + ex.Message);
                return StatusCode(500, "Errore server");
            }
        }

        /* ---------------- TELEMETRY DETAIL ---------------- */

        [HttpGet("security-filter/{computer}")]
        public IActionResult GetSecurityFilterBot(string computer)
        {
            if (string.IsNullOrWhiteSpace(computer))
                return NotFound();

            var bot = _engine.getSecurityFilterBot(computer.Trim());
            if (bot == null)
                return NotFound();

            return Ok(bot);
        }

        /* ---------------- RESET ---------------- */

        [HttpGet("reset")]
        public int Reset()
        {
            _db.ClearPcStatus();
            _engine.reset();
            _state.ResetElapsed();
            _state.LoadConfigurations();
            return 1;
        }

        /* ---------------- DECIDE ---------------- */

        [HttpGet("decide")]
        public IActionResult Decide([FromQuery] RequestParams p)
        {
            try
            {
                long KEY = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmssfff"));

                if (_db.ValidateUser(p.USERNAME, p.PASSWORD) == -1)
                    return Ok(0);

                var cfg = _state.Configurations;

                double baseUnit = cfg.TryGetValue("BASE_UNIT", out var bd)
                    ? double.Parse(bd, CultureInfo.InvariantCulture)
                    : 1;
                
                double valoreGiocato =
                    double.Parse(p.VALORE_GIOCATO.Replace(",", "."), CultureInfo.InvariantCulture) * baseUnit;
                string valoreGiocatoFormatted = valoreGiocato.ToString("C", new CultureInfo("it-IT"));

                double margine =
                    double.Parse(p.MARGINE.Replace(",", "."), CultureInfo.InvariantCulture) * baseUnit;
                string margineFormated = margine.ToString("C", new CultureInfo("it-IT"));
                
                _log.Log(
                    $"Ricevuta chiamata [PBT = {p.PBT}, COLORE GIOCATO = {p.CHOSEN_COLOR}, VALORE GIOCATO = {valoreGiocatoFormatted}, MAZZO = {p.MAZZO}, STATO = {p.STATO}, Margine: {margineFormated}]");

                
                bool hasCompleteData =
                    !string.IsNullOrEmpty(p.COMPUTER) &&
                    !string.IsNullOrEmpty(p.TAVOLO) &&
                    !string.IsNullOrEmpty(p.MARGINE) &&
                    !string.IsNullOrEmpty(p.COLPO_MARTINGALA) &&
                    !string.IsNullOrEmpty(p.MAZZO) &&
                    !string.IsNullOrEmpty(p.PBT) &&
                    !string.IsNullOrEmpty(p.SALDO_INIZIALE);
                
                if (!hasCompleteData)
                {
                    Console.WriteLine($"--- INIZIO {p.COMPUTER} ---");
                    Console.WriteLine(string.Join("\n", _log._logs));
                    Console.WriteLine($"--- FINE {p.COMPUTER} ---");
                    return Ok(0);
                }

                int martingala = int.Parse(p.COLPO_MARTINGALA) + 1;
                int tavolo = int.Parse(p.TAVOLO);
                int mazzo = int.Parse(p.MAZZO);
                char esito = p.PBT[0];
                char giocata = ' ';
                if (!String.IsNullOrEmpty(p.CHOSEN_COLOR))
                    giocata = p.CHOSEN_COLOR[0];
                
                double elapsedMinutesMax = _state.GetElapsedMinutes();

                /*
                int totalTables = Math.Max(
                    1,
                    _db.GetAllPcStatus().Rows
                        .Cast<DataRow>()
                        .Count(r =>
                            r["LAST_UPDATE"] != DBNull.Value &&
                            (DateTime.UtcNow -
                             DateTime.SpecifyKind((DateTime)r["LAST_UPDATE"], DateTimeKind.Utc))
                            .TotalSeconds <= 300)
                );
                */

                _engine.startOrUpdateMission(cfg);
                
                _log.Category = p.COMPUTER;
                _log.Log($"Avvio feedAndDevice(tavolo: {tavolo}, mazzo: {mazzo}, martingala: {martingala})");

                var (advice, info, snapshot, valutazione) =
                    _engine.Process(
                        p.COMPUTER,
                        tavolo,
                        mazzo,
                        margine,
                        martingala,
                        esito,
                        giocata,
                        valoreGiocato,
                        elapsedMinutesMax,
                        p.STATO);
                
                
                _log.Log($"\n");
                _log.Log($"--- INIZIO ADVICE ---");
                _log.Log($"Martingala: {advice.Martingala}");
                _log.Log($"HotZone: {advice.HotZone}");
                _log.Log($"StopL6: {advice.StopL6}");
                
                /*
                _log.Log($"GlobalMargin: {advice.GlobalMargin}");
                _log.Log($"GlobalAuthL6Counter: {advice.GlobalAuthL6Counter}");
                _log.Log($"GlobalL5Loss: {advice.GlobalL5Loss}");
                _log.Log($"GlobalPBHandsPlayed: {advice.GlobalPBHandsPlayed}");
                _log.Log($"GlobalPauseScalping: {advice.GlobalPauseScalping}");
                _log.Log($"GlobalPauseScalpingDuration: {advice.GlobalPauseScalpingDuration}");
                */
                
                _log.Log($"Reason: {advice.Reason}");
                _log.Log($"--- FINE ADVICE ---");
                _log.Log($"\n");

                _log.Log($"--- SECURITY_FILTER_EVAL ---");
                _log.Log($"computer={p.COMPUTER} hand={mazzo} streak={advice.CurrentStreak} avg={advice.AvgHandSeconds:0.00}s delta={advice.LastHandDeltaSeconds:0.00}s score={advice.SecurityRiskScore}/4 active={advice.SecurityFilterActive}");
                var sfBot = _engine.getSecurityFilterBot(p.COMPUTER);
                var playerP1P5Threshold = _engine.getTelemetry().SecurityFilterPlayerP1P5ThresholdSeconds;
                if (sfBot != null)
                {
                    var intervals = sfBot.PlayerStreakIntervalSeconds ?? Array.Empty<double>();
                    var i1 = intervals.Length > 0 ? intervals[0] : 0;
                    var i2 = intervals.Length > 1 ? intervals[1] : 0;
                    var i3 = intervals.Length > 2 ? intervals[2] : 0;
                    var i4 = intervals.Length > 3 ? intervals[3] : 0;
                    _log.Log(
                        $"PLAYER_PACE_DEBUG player_streak={sfBot.PlayerStreakCount} outcome={sfBot.CurrentStreakOutcome} " +
                        $"p1_p5={sfBot.PlayerStreakP1ToP5TotalSeconds:0.0}s mean={sfBot.PlayerStreakMeanIntervalSeconds:0.0}s " +
                        $"d12={i1:0.0}s d23={i2:0.0}s d34={i3:0.0}s d45={i4:0.0}s threshold={playerP1P5Threshold:0.0}s");
                }
                else
                {
                    _log.Log($"PLAYER_PACE_DEBUG player_streak=- outcome=- p1_p5=- mean=- d12=- d23=- d34=- d45=- threshold={playerP1P5Threshold:0.0}s");
                }
                _log.Log($"--- FINE SECURITY_FILTER_EVAL ---");
                _log.Log($"\n");

                var telemetry = _engine.getTelemetry();
                    
                _log.Log($"--- INIZIO TELEMETRY ---");
                _log.Log($"SpotID: {telemetry.SpotID}");
                _log.Log($"SpotPBHandsPlayed: {telemetry.SpotPBHandsPlayed}");
                _log.Log($"SpotAuthL6Counter: {telemetry.SpotAuthL6Counter}");
                _log.Log($"SpotL5Loss: {telemetry.SpotL5Loss}");
                _log.Log($"GlobalPauseScalping: {telemetry.GlobalPauseScalping}");
                _log.Log($"GlobalPauseScalpingDetails: {telemetry.GlobalPauseScalpingDetails}");
                _log.Log($"GlobalPauseScalpingDuration: {telemetry.GlobalPauseScalpingDuration}");
                _log.Log($"INC: {telemetry.INC}");
                _log.Log($"EWMA: {telemetry.EWMA}");
                _log.Log($"--- FINE TELEMETRY ---");
                _log.Log($"\n");
                
                _log.Log($"Margine globale: {advice.GlobalMargin.ToString("C", new CultureInfo("it-IT"))}");
                _log.Log($"\n");
                
                int action = advice.ActionCode;

                string description = "Nulla";
                if (action == 1) description = "Stop PC";
                if (action == 2) description = "Azzera Martingala";
                if (action == 3) description = "Pausa Scalping Forzata";
                
                _log.Log($"Azione: {action} ({description})");
                _log.Action = action;

                _ = Task.Run(() =>
                {
                    try
                    {
                        double saldoIniziale = 1000;
                        
                        if (!string.IsNullOrEmpty(p.SALDO_INIZIALE))
                            saldoIniziale = (double)decimal.Parse(p.SALDO_INIZIALE.Replace(",", "."), CultureInfo.InvariantCulture);

                        double saldoIstantaneo = saldoIniziale + margine;
                        
                        double elapsed = 0;

                        if (!string.IsNullOrEmpty(p.TEMPO))
                        {
                            var parts = p.TEMPO.Split(':');
                            if (parts.Length >= 2 &&
                                int.TryParse(parts[0], out int h) &&
                                int.TryParse(parts[1], out int m))
                            {
                                elapsed = h * 60 + m;
                            }
                        }

                        _db.UpdatePcStatus(
                            KEY,
                            p.COMPUTER,
                            p.USERNAME,
                            p.TAVOLO,
                            saldoIniziale,
                            saldoIstantaneo,
                            margine,
                            valoreGiocato,
                            martingala,
                            p.STATO,
                            p.MAZZO,
                            mazzo,
                            p.PBT,
                            Convert.ToDecimal(elapsed / 60),
                            JsonSerializer.Serialize(advice),
                            info,
                            snapshot,
                            valutazione,
                            action,
                            p.CHOSEN_COLOR);
                        
                        var persistence = TelemetryPersistence.From(telemetry);
                        var telemetryJson = JsonSerializer.Serialize(persistence);
                        var securityFilterJson = JsonSerializer.Serialize(persistence.SecurityFilterByBot);
                        var numeroBot = persistence.SecurityFilterByBot?.Count ?? 0;
                        Console.WriteLine(
                            $"TELEMETRY_SIZE telemetryJson.Length={telemetryJson.Length} numeroBot={numeroBot} dimensioneSecurityFilterByBot={securityFilterJson.Length}");
                        _log.Log(
                            $"TELEMETRY_SIZE telemetryJson.Length={telemetryJson.Length} numeroBot={numeroBot} dimensioneSecurityFilterByBot={securityFilterJson.Length}");

                        _db.UpdateMargin(telemetryJson, elapsedMinutesMax);
                    }
                    catch { }
                });

                return Ok(action);
            }
            catch
            {
                return Ok(9);
            }
        }
    }
}
