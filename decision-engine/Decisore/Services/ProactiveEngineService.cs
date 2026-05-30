using System.Globalization;
using System.Text.Json;
using Decisore.Models;
using Decisore.Engine;

namespace Decisore.Services

{
    public class ProactiveEngineService
    {
        private readonly object _sync = new object();
        private ProactiveEngine _engine;

        public void startOrUpdateMission(Dictionary<string, string> cfg)
        {
            lock (_sync) 
            {
                if (_engine is null)
                {
                    _engine = new ProactiveEngine();
                }

                try
                {
                    _engine.STOP_WIN = cfg.TryGetValue("STOP_WIN", out var stopWin)
                        ? double.Parse(stopWin, CultureInfo.InvariantCulture)
                        : _engine.STOP_WIN;
                    
                    _engine.STOP_LOSS = cfg.TryGetValue("STOP_LOSS", out var stopLoss)
                        ? double.Parse(stopLoss, CultureInfo.InvariantCulture)
                        : _engine.STOP_LOSS;
                    
                    _engine.STOP_TIME = cfg.TryGetValue("STOP_TIME", out var stopTime)
                        ? int.Parse(stopTime, CultureInfo.InvariantCulture)
                        : _engine.STOP_TIME;
                    
                    _engine.INITIAL_L6_AUTH = cfg.TryGetValue("INITIAL_L6_AUTH", out var initialL6Auth)
                        ? int.Parse(initialL6Auth, CultureInfo.InvariantCulture)
                        : _engine.INITIAL_L6_AUTH;
                    
                    _engine.L6_AUTH_INCREMENT = cfg.TryGetValue("L6_AUTH_INCREMENT", out var l6AuthIncrement)
                        ? int.Parse(l6AuthIncrement, CultureInfo.InvariantCulture)
                        : _engine.L6_AUTH_INCREMENT;
                    
                    _engine.L6_AUTH_LOSS = cfg.TryGetValue("L6_AUTH_LOSS", out var l6AuthLoss)
                        ? int.Parse(l6AuthLoss, CultureInfo.InvariantCulture)
                        : _engine.L6_AUTH_LOSS;
                    
                    _engine.L6_AUTH_PB_RESET_COUNTER = cfg.TryGetValue("L6_AUTH_PB_RESET_COUNTER", out var l6AuthPBResetCounter)
                        ? int.Parse(l6AuthPBResetCounter, CultureInfo.InvariantCulture)
                        : _engine.L6_AUTH_PB_RESET_COUNTER;
                    
                    _engine.PAUSE_SCALPING_WIN_BUCKET = cfg.TryGetValue("PAUSE_SCALPING_WIN_BUCKET", out var pauseScalpingWinBucket)
                        ? double.Parse(pauseScalpingWinBucket, CultureInfo.InvariantCulture)
                        : _engine.PAUSE_SCALPING_WIN_BUCKET;

                    _engine.PAUSE_SCALPING_LOSE_BUCKET = cfg.TryGetValue("PAUSE_SCALPING_LOSE_BUCKET", out var pauseScalpingLoseBucket)
                        ? double.Parse(pauseScalpingLoseBucket, CultureInfo.InvariantCulture)
                        : _engine.PAUSE_SCALPING_LOSE_BUCKET;

                    _engine.PAUSE_SCALPING_SECONDS = cfg.TryGetValue("PAUSE_SCALPING_SECONDS", out var pauseScalpingSeconds)
                        ? int.Parse(pauseScalpingSeconds, CultureInfo.InvariantCulture)
                        : _engine.PAUSE_SCALPING_SECONDS;
                    
                    _engine.PAUSE_SCALPING_MARTINGALA_RANGE_EXCLUDE = cfg.TryGetValue("PAUSE_SCALPING_MARTINGALA_RANGE_EXCLUDE", out var pauseScalpingMartingalaRangeExclude)
                        ? JsonSerializer.Deserialize<int[]>(pauseScalpingMartingalaRangeExclude)
                        : _engine.PAUSE_SCALPING_MARTINGALA_RANGE_EXCLUDE;
                    
                    _engine.WINDOW_PB = cfg.TryGetValue("WINDOW_PB", out var windowPB)
                        ? int.Parse(windowPB, CultureInfo.InvariantCulture)
                        : _engine.WINDOW_PB;
                    
                    _engine.ALPHA = cfg.TryGetValue("ALPHA", out var alpha)
                        ? double.Parse(alpha, CultureInfo.InvariantCulture)
                        : _engine.ALPHA;
                    
                    _engine.STOP_EWMA = cfg.TryGetValue("STOP_EWMA", out var stopEWMA)
                        ? double.Parse(stopEWMA, CultureInfo.InvariantCulture)
                        : _engine.STOP_EWMA;
                    
                    _engine.PAUSE_PB = cfg.TryGetValue("PAUSE_PB", out var pausePB)
                        ? int.Parse(pausePB, CultureInfo.InvariantCulture)
                        : _engine.PAUSE_PB;
                    
                    _engine.COOLDOWN_PAUSE_PB = cfg.TryGetValue("COOLDOWN_PAUSE_PB", out var cooldownPausePB)
                        ? int.Parse(cooldownPausePB, CultureInfo.InvariantCulture)
                        : _engine.COOLDOWN_PAUSE_PB;
                    
                    _engine.HOT_ZONES = cfg.TryGetValue("HOT_ZONES", out var hotzones)
                        ? JsonSerializer.Deserialize<int[][]>(hotzones).Select(x => (from: x[0], to: x[1])).ToArray()
                        : _engine.HOT_ZONES;

                    _engine.SECURITY_FILTER_ENABLED = cfg.TryGetValue("SECURITY_FILTER_ENABLED", out var sfEnabled)
                        ? ParseEnabledFlag(sfEnabled)
                        : _engine.SECURITY_FILTER_ENABLED;

                    _engine.SECURITY_FILTER_MAX_SHOE_HAND = cfg.TryGetValue("SECURITY_FILTER_MAX_SHOE_HAND", out var sfMaxHand)
                        ? int.Parse(sfMaxHand, CultureInfo.InvariantCulture)
                        : _engine.SECURITY_FILTER_MAX_SHOE_HAND;

                    _engine.SECURITY_FILTER_MIN_STREAK = cfg.TryGetValue("SECURITY_FILTER_MIN_STREAK", out var sfMinStreak)
                        ? int.Parse(sfMinStreak, CultureInfo.InvariantCulture)
                        : _engine.SECURITY_FILTER_MIN_STREAK;

                    _engine.SECURITY_FILTER_MAX_AVG_SECONDS = cfg.TryGetValue("SECURITY_FILTER_MAX_AVG_SECONDS", out var sfMaxAvg)
                        ? double.Parse(sfMaxAvg, CultureInfo.InvariantCulture)
                        : _engine.SECURITY_FILTER_MAX_AVG_SECONDS;

                    _engine.SECURITY_FILTER_VERY_FAST_SECONDS = cfg.TryGetValue("SECURITY_FILTER_VERY_FAST_SECONDS", out var sfVeryFast)
                        ? double.Parse(sfVeryFast, CultureInfo.InvariantCulture)
                        : _engine.SECURITY_FILTER_VERY_FAST_SECONDS;

                    _engine.SECURITY_FILTER_DELTA_WINDOW = cfg.TryGetValue("SECURITY_FILTER_DELTA_WINDOW", out var sfWindow)
                        ? int.Parse(sfWindow, CultureInfo.InvariantCulture)
                        : _engine.SECURITY_FILTER_DELTA_WINDOW;

                    _engine.SECURITY_FILTER_MIN_SCORE = cfg.TryGetValue("SECURITY_FILTER_MIN_SCORE", out var sfMinScore)
                        ? int.Parse(sfMinScore, CultureInfo.InvariantCulture)
                        : _engine.SECURITY_FILTER_MIN_SCORE;
                } catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private static bool ParseEnabledFlag(string value)
        {
            return value.Trim().Equals("1", StringComparison.OrdinalIgnoreCase)
                || value.Trim().Equals("true", StringComparison.OrdinalIgnoreCase)
                || value.Trim().Equals("yes", StringComparison.OrdinalIgnoreCase)
                || value.Trim().Equals("on", StringComparison.OrdinalIgnoreCase);
        }

        public void emergencyStop()
        {
            lock (_sync)
            {
                if (_engine != null)
                {
                    _engine.emergencyStop = true;
                }
            }
        }

        public (Advice advice, string infoString, string missionSnapshot, string valutazioneRisultato) Process(
            string computer,
            int tableId,
            int handIndex,
            double margine,
            int martingalaLevel,
            char esito,
            char giocata,
            double valoreGiocato,
            double elapsedMinutes,
            string stato)
        {
            lock (_sync)
            {
                if (_engine is null)
                    throw new InvalidOperationException("Engine non inizializzato. Chiama startOrUpdateMission prima di Process.");

                var advice = _engine.FeedAndDecide(
                    computer: computer,
                    tableId: tableId,
                    handIndexMazzo: handIndex,
                    margine: margine,
                    esito: esito,
                    giocata,
                    valoreGiocato: valoreGiocato,
                    martingalaCounter:martingalaLevel,
                    stato,
                    elapsedMinutes: elapsedMinutes);
                
                var infoString = "{}";
                var missionSnapshot = "{}";
                var valutazioneRisultato = "{}";

                try
                {
                    (infoString, missionSnapshot, valutazioneRisultato) = getAddInfo();
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                return (advice, infoString, missionSnapshot, valutazioneRisultato);
            }
        }

        public (string info, string missionSnapshot, string valutazioneRisultato) getAddInfo()
        {
            return (
                "{}",
                "{}",
                "{}"
            );
        }

        public Telemetry getTelemetry()
        {
            lock (_sync)
            {
                if (_engine != null)
                {
                    return _engine.getTelemetry();
                }
                return new Telemetry();
            }
        }

        public SecurityFilterBotTelemetry? getSecurityFilterBot(string computer)
        {
            lock (_sync)
            {
                return _engine?.getSecurityFilterBot(computer);
            }
        }

        public void updateCurrentBet(string computer, int tableId, double valoreGiocato, int handIndexMazzo)
        {
            lock (_sync)
            {
                if (_engine != null)
                {
                    _engine.UpdateRealTimeBet(computer, tableId, valoreGiocato, handIndexMazzo);
                }
            }
        }

        public void reset()
        {
            lock (_sync)
            {
                _engine = null;
            }
        }
    }
}
