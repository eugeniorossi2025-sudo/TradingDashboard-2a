using System.Text.Json;

namespace Decisore.Engine
{

    public sealed class ProactiveEngine
    {
        readonly Dictionary<string, ActiveBet> _activeBets = new();
        readonly Dictionary<string, double> _lastBotMargin = new();

        // Security Filter — per-computer state
        private readonly Dictionary<string, DateTime>              _lastDecideAt            = new();
        private readonly Dictionary<string, Queue<double>>         _handDeltasWindow        = new();
        private readonly Dictionary<string, int>                   _lastMazzoByComputer     = new();
        private readonly Dictionary<string, int>                   _gapFilteredCountByComputer = new();
        private readonly Dictionary<string, (char Outcome, int Count)> _streakByComputer    = new();
        private readonly Dictionary<string, List<DateTime>>        _playerStreakTimestampsByComputer = new();
        private readonly Dictionary<string, int>                   _playerStreakCountByComputer = new();
        private readonly Dictionary<string, bool>                  _prevSecFilterActive     = new();
        private readonly Dictionary<string, SecurityFilterBotTelemetry> _securityFilterByBot = new();

        double _lastGlobalMargin = 0;

        int _globalAuthL6Counter = -1;
        int _globalL5Loss = 0;
        int _globalPBHandsPlayed = 0;

        double _lastPauseReferenceMargin = 0;
        bool _pauseReferenceInitialized = false;
        private bool _pauseWasLoseBucket = false;           
        private double _pauseTriggerMargin = 0;    

        DateTime _pauseScalpingUntil = DateTime.MinValue;
        private DateTime _lastPBPauseEndedAt = DateTime.MinValue;
        
        public bool emergencyStop = false;
        
        // === PB INDICATORS ===
        private int _pbIndex = 0;
        private readonly Queue<double> _pbMarginsWindow = new();
        private double _ewma = 0;
        private double _inc = 0;
        private int _pauseScalpingEndPB = -1;
        private int currentCooldownPB = 0;
        
        #region CONFIG

        public double STOP_WIN = 2000;
        public double STOP_LOSS = -3000;
        public double STOP_TIME = 720;

        public int INITIAL_L6_AUTH = 0;
        public int L6_AUTH_INCREMENT = 1;
        public int L6_AUTH_LOSS = 8;
        public int L6_AUTH_PB_RESET_COUNTER = 600;
        public int SPOT_RESET_THRESHOLD_L5 = 2;
        public bool SPOT_L6_PER_BOT_ENABLED = true;
        /// <summary>Opzione A: logica SPOT/L6 globale disattivata — solo SPOT per-bot è operativo.</summary>
        public const bool LEGACY_GLOBAL_SPOT_L6_ENABLED = false;

        public (int from, int to)[] HOT_ZONES =
        {
            (0, 15),
            (55, 80)
        };

        public double PAUSE_SCALPING_WIN_BUCKET = 20;
        public double PAUSE_SCALPING_LOSE_BUCKET = -20;
        public int PAUSE_SCALPING_SECONDS = 60;
        public int[] PAUSE_SCALPING_MARTINGALA_RANGE_EXCLUDE = [5, 8];
        
        public int WINDOW_PB = 25;
        public double ALPHA = 0.026;
        public double STOP_EWMA = -0.15;
        public int PAUSE_PB = 20;
        public int COOLDOWN_PAUSE_PB = 20;

        // Security Filter config
        public bool   SECURITY_FILTER_ENABLED            = true;
        public int    SECURITY_FILTER_MAX_SHOE_HAND      = 20;
        public int    SECURITY_FILTER_MIN_STREAK         = 5;
        public double SECURITY_FILTER_MAX_AVG_SECONDS    = 25.85;
        public double SECURITY_FILTER_VERY_FAST_SECONDS  = 23.1;
        public int    SECURITY_FILTER_DELTA_WINDOW       = 8;
        public int    SECURITY_FILTER_MIN_SCORE          = 3;
        public bool   PLAYER_RACE_5_FILTER_ENABLED = false;
        public bool   PLAYER_RACE_5_AC3_ENABLED = false;
        public bool   PLAYER_RACE_8_FILTER_ENABLED = false;
        public bool   PLAYER_RACE_8_AC3_ENABLED = false;
        private const int PLAYER_RACE_5_MIN_STREAK = 5;
        private const int PLAYER_RACE_8_MIN_STREAK = 8;
        public double SECURITY_FILTER_PLAYER_P1P5_THRESHOLD_SECONDS = 107;

        // Avg hand pace — rolling window of valid scalping deltas only
        private const double AVG_HAND_MAX_DELTA_SECONDS = 60.0;
        private const int    AVG_HAND_VALID_WINDOW       = 10;
        
        private double _pbBaseMargin = 0;
        private int _pbBaseIndex = 0;
        
        #endregion
        
        #region TELEMETRY

        private int totalPBHandsPlayed = 0;
        private int totalAuthL6Authorized = 0;
        private int totalL5Played = 0;
        private int totalL5Won = 0;
        private int totalL5Lost = 0;
        private int totalL8Played = 0;
        private int totalL8Won = 0;
        private int totalL8Lost = 0;
        
        private bool prevPauseScalpingSoglieStatus = false;
        private bool prevPauseScalpingEWMAStatus = false;
        
        private int totalPauseScalpingSoglieActivated = 0;
        private int totalPauseScalpingEWMAActivated = 0;

        private int totalSecurityFilterActivated = 0;
        private int totalSecurityFilterPreventedL6 = 0;
        private int totalPlayerPaceAC3Activated = 0;
        private readonly Dictionary<string, bool> _prevPlayerPaceAC3Active = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> _spotL5LossByComputer = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> _spotL5PlayedByComputer = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> _spotL6GrantedByComputer = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> _spotPbHandsByComputer = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> _spotCycleIdByComputer = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> _lastMartingalaByComputer = new(StringComparer.OrdinalIgnoreCase);

        private int spotID = 0;
        
       #endregion 

        #region PUBLIC API

        public Telemetry getTelemetry()
        {
            bool pbPauseActive = _pauseScalpingEndPB > _pbIndex  && !(currentCooldownPB < COOLDOWN_PAUSE_PB);
            bool pbPauseDelayed = _pauseScalpingEndPB > _pbIndex;
            bool timePauseActive = DateTime.UtcNow < _pauseScalpingUntil;
            
            Telemetry telemetry = new Telemetry();

            telemetry.TotalPBHandsPlayed = totalPBHandsPlayed;
            telemetry.TotalAuthL6Authorized = totalAuthL6Authorized;
            telemetry.TotalL5Played = totalL5Played;
            telemetry.TotalL5Won = totalL5Won;
            telemetry.TotalL5Lost = totalL5Lost;
            telemetry.TotalL8Played = totalL8Played;
            telemetry.TotalL8Won = totalL8Won;
            telemetry.TotalL8Lost = totalL8Lost;
            telemetry.BotMargins = _lastBotMargin;
            
            /* Inizio nuovi campi */
            telemetry.SpotID = LEGACY_GLOBAL_SPOT_L6_ENABLED ? spotID : 0;
            telemetry.SpotPBHandsPlayed = LEGACY_GLOBAL_SPOT_L6_ENABLED ? _globalPBHandsPlayed : 0;
            telemetry.SpotAuthL6Counter = _globalAuthL6Counter;
            telemetry.SpotL5Loss = LEGACY_GLOBAL_SPOT_L6_ENABLED ? _globalL5Loss : 0;
            telemetry.SpotL6ThresholdL5 = IsSpotL6ThresholdConfigured() ? SPOT_RESET_THRESHOLD_L5 : 0;
            telemetry.SpotCyclePbHandsLimit = SpotCyclePbHandsLimit;
            telemetry.SpotPerBotOnlyEnabled = SPOT_L6_PER_BOT_ENABLED && !LEGACY_GLOBAL_SPOT_L6_ENABLED;
            telemetry.SpotL6PerBotEnabled = SPOT_L6_PER_BOT_ENABLED;
            telemetry.SpotLegacyGlobalEnabled = LEGACY_GLOBAL_SPOT_L6_ENABLED;
            
            telemetry.GlobalPauseScalping = pbPauseActive || timePauseActive;

            if (pbPauseActive)
            {
                telemetry.GlobalPauseScalpingDetails = "Trigger: EWMA";
                telemetry.GlobalPauseScalpingDuration = $"{_pauseScalpingEndPB - _pbIndex} giocate";
            } else if (timePauseActive)
            {
                telemetry.GlobalPauseScalpingDetails = "Trigger: Soglia Margine";
                telemetry.GlobalPauseScalpingDuration = $"{(int)Math.Round((_pauseScalpingUntil - DateTime.UtcNow).TotalSeconds)} secondi";
            } else if (pbPauseDelayed)
            {
                telemetry.GlobalPauseScalpingDetails = $"Pausa EWMA ritardata da cooldown (numero di mani cooldown rimanenti: {COOLDOWN_PAUSE_PB - currentCooldownPB})";
            }
            
            telemetry.INC = _inc;
            telemetry.EWMA = _ewma;
            
            telemetry.TotalPauseScalpingSoglieActivated = totalPauseScalpingSoglieActivated;
            telemetry.TotalPauseScalpingEWMAActivated = totalPauseScalpingEWMAActivated;

            telemetry.SecurityFilterEnabled          = SECURITY_FILTER_ENABLED;
            telemetry.SecurityFilterMinScore         = SECURITY_FILTER_MIN_SCORE;
            telemetry.SecurityFilterMinStreak        = SECURITY_FILTER_MIN_STREAK;
            telemetry.SecurityFilterMaxShoeHand      = SECURITY_FILTER_MAX_SHOE_HAND;
            telemetry.SecurityFilterMaxAvgSeconds    = SECURITY_FILTER_MAX_AVG_SECONDS;
            telemetry.SecurityFilterVeryFastSeconds  = SECURITY_FILTER_VERY_FAST_SECONDS;
            telemetry.SecurityFilterDeltaWindow      = SECURITY_FILTER_DELTA_WINDOW;
            telemetry.SecurityFilterPlayerP1P5ThresholdSeconds = SECURITY_FILTER_PLAYER_P1P5_THRESHOLD_SECONDS;
            telemetry.PlayerRace5FilterEnabled       = PLAYER_RACE_5_FILTER_ENABLED;
            telemetry.PlayerRace5Ac3Enabled          = PLAYER_RACE_5_AC3_ENABLED;
            telemetry.PlayerRace8FilterEnabled       = PLAYER_RACE_8_FILTER_ENABLED;
            telemetry.PlayerRace8Ac3Enabled            = PLAYER_RACE_8_AC3_ENABLED;
            telemetry.PlayerRace5Enabled             = PLAYER_RACE_5_FILTER_ENABLED;
            telemetry.PlayerRace8Enabled             = PLAYER_RACE_8_FILTER_ENABLED;
            telemetry.PlayerPaceFilterEnabled        = PLAYER_RACE_8_AC3_ENABLED;
            telemetry.TotalSecurityFilterActivated   = totalSecurityFilterActivated;
            telemetry.TotalSecurityFilterPreventedL6 = totalSecurityFilterPreventedL6;
            telemetry.TotalPlayerPaceAC3Activated      = totalPlayerPaceAC3Activated;
            telemetry.ActivePlayerPaceRiskBots       = _securityFilterByBot.Values.Count(x => x.PlayerRace5Alert || x.PlayerRace8Alert);
            telemetry.LastAvgHandSeconds =
                _handDeltasWindow.Values.Where(q => q.Count > 0)
                    .Select(ComputeTrimmedAverage)
                    .Where(x => x > 0)
                    .DefaultIfEmpty(0)
                    .Average();
            telemetry.ActiveSecurityFilterBots = _securityFilterByBot.Values.Count(x => x.SecurityFilterActive);
            telemetry.SecurityFilterByBot = _securityFilterByBot.ToDictionary(
                x => x.Key,
                x => new SecurityFilterBotTelemetry
                {
                    Computer = x.Value.Computer,
                    AvgHandSeconds = x.Value.AvgHandSeconds,
                    LastHandDeltaSeconds = x.Value.LastHandDeltaSeconds,
                    LastTwoHandDeltaSeconds = x.Value.LastTwoHandDeltaSeconds,
                    MinHandDeltaSeconds = x.Value.MinHandDeltaSeconds,
                    MaxHandDeltaSeconds = x.Value.MaxHandDeltaSeconds,
                    RapidL5TriggerActive = x.Value.RapidL5TriggerActive,
                    L6PlayedCount = x.Value.L6PlayedCount,
                    LastL6DeltaSeconds = x.Value.LastL6DeltaSeconds,
                    AvgL6DeltaSeconds = x.Value.AvgL6DeltaSeconds,
                    MinL6DeltaSeconds = x.Value.MinL6DeltaSeconds,
                    MaxL6DeltaSeconds = x.Value.MaxL6DeltaSeconds,
                    LastL6DeltaHands = x.Value.LastL6DeltaHands,
                    AvgL6DeltaHands = x.Value.AvgL6DeltaHands,
                    MinL6DeltaHands = x.Value.MinL6DeltaHands,
                    MaxL6DeltaHands = x.Value.MaxL6DeltaHands,
                    L6DeltaSamples = x.Value.L6DeltaSamples,
                    LastL6PlayedAtUtc = x.Value.LastL6PlayedAtUtc,
                    LastL6PlayedPBHands = x.Value.LastL6PlayedPBHands,
                    AuthorizedL8LostCount = x.Value.AuthorizedL8LostCount,
                    LastAuthorizedL8LostDeltaSeconds = x.Value.LastAuthorizedL8LostDeltaSeconds,
                    AvgAuthorizedL8LostDeltaSeconds = x.Value.AvgAuthorizedL8LostDeltaSeconds,
                    MinAuthorizedL8LostDeltaSeconds = x.Value.MinAuthorizedL8LostDeltaSeconds,
                    MaxAuthorizedL8LostDeltaSeconds = x.Value.MaxAuthorizedL8LostDeltaSeconds,
                    LastAuthorizedL8LostDeltaHands = x.Value.LastAuthorizedL8LostDeltaHands,
                    AvgAuthorizedL8LostDeltaHands = x.Value.AvgAuthorizedL8LostDeltaHands,
                    MinAuthorizedL8LostDeltaHands = x.Value.MinAuthorizedL8LostDeltaHands,
                    MaxAuthorizedL8LostDeltaHands = x.Value.MaxAuthorizedL8LostDeltaHands,
                    AuthorizedL8LostDeltaSamples = x.Value.AuthorizedL8LostDeltaSamples,
                    LastAuthorizedL8LostAtUtc = x.Value.LastAuthorizedL8LostAtUtc,
                    LastAuthorizedL8LostPBHands = x.Value.LastAuthorizedL8LostPBHands,
                    LastL6AuthorizationAtUtc = x.Value.LastL6AuthorizationAtUtc,
                    PBHandsPlayed = x.Value.PBHandsPlayed,
                    LastL6AuthorizationPBHandsPlayed = x.Value.LastL6AuthorizationPBHandsPlayed,
                    LastL6AuthorizationScore = x.Value.LastL6AuthorizationScore,
                    LastL6AuthorizationStreak = x.Value.LastL6AuthorizationStreak,
                    LastL6AuthorizationShoeHand = x.Value.LastL6AuthorizationShoeHand,
                    LastL6AuthorizationAvgHandSeconds = x.Value.LastL6AuthorizationAvgHandSeconds,
                    AuthorizedL8LostFromAuthorizationCount = x.Value.AuthorizedL8LostFromAuthorizationCount,
                    LastAuthorizedL8LossFromAuthorizationSeconds = x.Value.LastAuthorizedL8LossFromAuthorizationSeconds,
                    AvgAuthorizedL8LossFromAuthorizationSeconds = x.Value.AvgAuthorizedL8LossFromAuthorizationSeconds,
                    MinAuthorizedL8LossFromAuthorizationSeconds = x.Value.MinAuthorizedL8LossFromAuthorizationSeconds,
                    MaxAuthorizedL8LossFromAuthorizationSeconds = x.Value.MaxAuthorizedL8LossFromAuthorizationSeconds,
                    LastAuthorizedL8LossFromAuthorizationHands = x.Value.LastAuthorizedL8LossFromAuthorizationHands,
                    LastAuthorizedL8LossSecondsPerHand = x.Value.LastAuthorizedL8LossSecondsPerHand,
                    AvgAuthorizedL8LossSecondsPerHand = x.Value.AvgAuthorizedL8LossSecondsPerHand,
                    MinAuthorizedL8LossSecondsPerHand = x.Value.MinAuthorizedL8LossSecondsPerHand,
                    MaxAuthorizedL8LossSecondsPerHand = x.Value.MaxAuthorizedL8LossSecondsPerHand,
                    LastAuthorizedL8LossAuthorizationScore = x.Value.LastAuthorizedL8LossAuthorizationScore,
                    AvgAuthorizedL8LossAuthorizationScore = x.Value.AvgAuthorizedL8LossAuthorizationScore,
                    CurrentStreak = x.Value.CurrentStreak,
                    CurrentStreakOutcome = x.Value.CurrentStreakOutcome,
                    PlayerStreakCount = x.Value.PlayerStreakCount,
                    PlayerStreakP1ToP5TotalSeconds = x.Value.PlayerStreakP1ToP5TotalSeconds,
                    PlayerStreakMeanIntervalSeconds = x.Value.PlayerStreakMeanIntervalSeconds,
                    PlayerStreakIntervalSeconds = x.Value.PlayerStreakIntervalSeconds,
                    SecurityRiskScore = x.Value.SecurityRiskScore,
                    SecurityFilterActive = x.Value.SecurityFilterActive,
                    PlayerRace5Alert = x.Value.PlayerRace5Alert,
                    PlayerRace5Triggered = x.Value.PlayerRace5Alert,
                    PlayerRace5Ac3Triggered = x.Value.PlayerRace5Ac3Triggered,
                    PlayerRace8Alert = x.Value.PlayerRace8Alert,
                    PlayerPaceRiskActive = x.Value.PlayerRace5Alert || x.Value.PlayerRace8Alert,
                    PlayerPaceTriggeredAC3 = x.Value.PlayerRace5Ac3Triggered || x.Value.PlayerRace8Ac3Triggered,
                    PlayerRace8Ac3Triggered = x.Value.PlayerRace8Ac3Triggered,
                    PauseBot = x.Value.PauseBot,
                    PauseScope = x.Value.PauseScope,
                    PauseComputer = x.Value.PauseComputer,
                    Activations = x.Value.Activations,
                    PreventedL6 = x.Value.PreventedL6,
                    LastShoeHand = x.Value.LastShoeHand,
                    Martingala = x.Value.Martingala,
                    HasL6Credit = x.Value.HasL6Credit,
                    LastReason = x.Value.LastReason,
                    LastUpdatedUtc = x.Value.LastUpdatedUtc,
                    HandSamples = x.Value.HandSamples,
                    ValidSamples = x.Value.ValidSamples,
                    GapFilteredCount = x.Value.GapFilteredCount,
                    SpotL5PlayedCount = x.Value.SpotL5PlayedCount,
                    SpotL5LossCount = x.Value.SpotL5LossCount,
                    SpotL6GrantedCount = x.Value.SpotL6GrantedCount,
                    SpotL6Authorized = x.Value.SpotL6Authorized,
                    NextL5LossWillAuthorizeL6 = x.Value.NextL5LossWillAuthorizeL6,
                    SpotCycleId = x.Value.SpotCycleId,
                    SpotPbHandsPlayed = x.Value.SpotPbHandsPlayed
                });
            
            /* Fine nuovi campi */
            
            return telemetry;
        }

        public SecurityFilterBotTelemetry? getSecurityFilterBot(string computer)
        {
            return getTelemetry().SecurityFilterByBot.TryGetValue(computer, out var bot) ? bot : null;
        }
        
        public void UpdateRealTimeBet(string computer, int tableId, double valoreGiocato, int handIndexMazzo)
        {
            _activeBets[computer] = new ActiveBet
            {
                Computer = computer,
                TableId = tableId,
                Valore = valoreGiocato,
                HandIndex = handIndexMazzo
            };
        }

        public Advice FeedAndDecide(
            string computer,
            int tableId,
            int handIndexMazzo,
            double margine,
            char esito,
            char coloreGiocato,
            double valoreGiocato,
            int martingalaCounter,
            string stato,
            double elapsedMinutes)
        {
            if (_globalAuthL6Counter < 0)
            {
                _globalAuthL6Counter = INITIAL_L6_AUTH;
            }
            _activeBets.Remove(computer);

            _lastBotMargin[computer] = margine;

            double globalMargin = _lastBotMargin.Values.Sum();
            double activeBetsSum = _activeBets.Values.Sum(x => x.Valore);

            var advice = new Advice
            {
                TableId = tableId,
                State = stato,
                Martingala = martingalaCounter,
                LocalMargin = margine,
                GlobalMargin = globalMargin,
                Elapsed = elapsedMinutes,
                Reason = "Default"
            };

            #region STOP MISSION

            if ((globalMargin - activeBetsSum) >= STOP_WIN)
                SetStop(advice, "STOP_WIN");

            else if (globalMargin <= STOP_LOSS)
                SetStop(advice, "STOP_LOSS");

            else if (elapsedMinutes >= STOP_TIME)
                SetStop(advice, "STOP_TIME");
            
            else if (emergencyStop)
                SetStop(advice, "EMERGENCY_STOP");

            #endregion

            #region HOT ZONE

            var hotZone = HOT_ZONES.FirstOrDefault(z => handIndexMazzo >= z.from && handIndexMazzo <= z.to);
            bool isHotZone = hotZone != default;

            advice.HotZone = isHotZone;
            advice.HotZoneLabel = isHotZone ? $"🔥 [{hotZone.from} - {hotZone.to}]" : "";

            #endregion

            #region L6 SYSTEM

            bool pbHandPlayedThisCall = esito != 'T' && valoreGiocato > 0 &&
                (stato.ToLower().Equals("sculping") || stato.ToLower().Equals("scalping"));

            if (pbHandPlayedThisCall)
            {
                _globalPBHandsPlayed++;
                totalPBHandsPlayed++;

                UpdatePBIndicators(globalMargin);
            }

            #endregion

            #region SECURITY FILTER
            // Filtro sperimentale di compressione temporale per mitigazione rischio
            // streak ad alta densità nelle prime mani dello shoe. Deve essere calcolato
            // prima del gate L6, così può bloccare senza consumare credito.

            DateTime nowUtc = DateTime.UtcNow;
            double lastHandDeltaSeconds = 0;
            double avgHandSeconds       = 0;
            double[] lastTwoHandDeltaSeconds = Array.Empty<double>();
            int    currentStreak        = 0;
            if (!_securityFilterByBot.TryGetValue(computer, out var botSecurity))
            {
                botSecurity = new SecurityFilterBotTelemetry { Computer = computer };
                _securityFilterByBot[computer] = botSecurity;
            }

            int prevMartingalaForL6 = _lastMartingalaByComputer.GetValueOrDefault(computer);

            ApplySpotL6PerBot(computer, martingalaCounter, botSecurity, l5PlayedThisCall: false, l5LostThisCall: false);

            // — shoe change: reset avg window (mazzo/deck counter decreased) —
            if (_lastMazzoByComputer.TryGetValue(computer, out var lastMazzo) && handIndexMazzo < lastMazzo)
                ResetHandPaceWindow(computer);
            _lastMazzoByComputer[computer] = handIndexMazzo;

            // — anchor reset when bot is not actively scalping (pause/off/dead time) —
            if (!IsScalpingState(stato))
                _lastDecideAt.Remove(computer);

            // — timing: misura solo le mani P/B realmente giocate dal singolo bot in scalping —
            if (pbHandPlayedThisCall)
            {
                if (_lastDecideAt.TryGetValue(computer, out var lastAt))
                {
                    lastHandDeltaSeconds = (nowUtc - lastAt).TotalSeconds;

                    if (lastHandDeltaSeconds > 0 && lastHandDeltaSeconds <= AVG_HAND_MAX_DELTA_SECONDS)
                    {
                        if (!_handDeltasWindow.TryGetValue(computer, out var win))
                            _handDeltasWindow[computer] = win = new Queue<double>();

                        win.Enqueue(lastHandDeltaSeconds);
                        while (win.Count > AVG_HAND_VALID_WINDOW)
                            win.Dequeue();

                        avgHandSeconds = ComputeTrimmedAverage(win);
                    }
                    else if (lastHandDeltaSeconds > AVG_HAND_MAX_DELTA_SECONDS)
                    {
                        _gapFilteredCountByComputer[computer] =
                            _gapFilteredCountByComputer.GetValueOrDefault(computer) + 1;

                        if (_handDeltasWindow.TryGetValue(computer, out var win) && win.Count > 0)
                            avgHandSeconds = ComputeTrimmedAverage(win);
                        else
                            avgHandSeconds = botSecurity.AvgHandSeconds;
                    }
                }
                else
                {
                    avgHandSeconds = botSecurity.AvgHandSeconds;
                }

                _lastDecideAt[computer] = nowUtc;
            }
            else
            {
                lastHandDeltaSeconds = botSecurity.LastHandDeltaSeconds;
                avgHandSeconds = botSecurity.AvgHandSeconds;
            }

            if (_handDeltasWindow.TryGetValue(computer, out var currentDeltaWindow))
            {
                lastTwoHandDeltaSeconds = currentDeltaWindow
                    .Reverse()
                    .Take(2)
                    .Reverse()
                    .ToArray();
            }

            // — streak colore USCITO (esito PBT), tie non resetta —
            if (esito != 'T')
            {
                if (!_streakByComputer.TryGetValue(computer, out var s) || s.Outcome != esito)
                    _streakByComputer[computer] = (esito, 1);
                else
                    _streakByComputer[computer] = (esito, s.Count + 1);
            }
            if (_streakByComputer.TryGetValue(computer, out var currentStreakEntry))
                currentStreak = currentStreakEntry.Count;

            UpdatePlayerStreakPace(computer, esito, nowUtc, botSecurity);

            // — score composito 0–4 —
            int securityScore = 0;
            if (currentStreak  >= SECURITY_FILTER_MIN_STREAK)                               securityScore++;
            if (avgHandSeconds  > 0 && avgHandSeconds < SECURITY_FILTER_MAX_AVG_SECONDS)    securityScore++;
            if (handIndexMazzo <= SECURITY_FILTER_MAX_SHOE_HAND)                             securityScore++;
            if (avgHandSeconds  > 0 && avgHandSeconds < SECURITY_FILTER_VERY_FAST_SECONDS)  securityScore++;

            bool securityFilterActive = SECURITY_FILTER_ENABLED && securityScore >= SECURITY_FILTER_MIN_SCORE;
            bool rapidL5TriggerActive = SECURITY_FILTER_ENABLED &&
                lastTwoHandDeltaSeconds.Length == 2 &&
                lastTwoHandDeltaSeconds.All(x => x > 0 && x < SECURITY_FILTER_VERY_FAST_SECONDS);

            #endregion

            #region L6 SYSTEM

            bool l6AuthorizedThisCall = false;
            bool securityFilterPreventedL6ThisCall = false;

            if (martingalaCounter == 5)
            {
                if (esito != 'T')
                {
                    totalL5Played++;
                    ApplySpotL6PerBot(
                        computer,
                        martingalaCounter,
                        botSecurity,
                        l5PlayedThisCall: true,
                        l5LostThisCall: esito != coloreGiocato);
                }

                if (esito != 'T') {
                    if (esito != coloreGiocato)
                    {
                        totalL5Lost++;

                        if (LEGACY_GLOBAL_SPOT_L6_ENABLED)
                        {
                            _globalL5Loss++;

                            if (_globalL5Loss >= L6_AUTH_LOSS)
                            {
                                _globalL5Loss = 0;
                                _globalAuthL6Counter += L6_AUTH_INCREMENT;
                            }

                            if (_globalAuthL6Counter > 0 && !isHotZone)
                            {
                                if (securityFilterActive || rapidL5TriggerActive)
                                {
                                    advice.StopL6 = true;
                                    securityFilterPreventedL6ThisCall = true;
                                    advice.Reason = rapidL5TriggerActive
                                        ? $"L6 Bloccato (Trigger rapido L5)"
                                        : $"L6 Bloccato (Security Filter)";
                                }
                                else
                                {
                                    advice.StopL6 = false;
                                    _globalAuthL6Counter--;
                                    totalAuthL6Authorized++;
                                    l6AuthorizedThisCall = true;
                                    advice.Reason = $"L6 Autorizzato";
                                }
                            }
                            else
                            {
                                advice.StopL6 = true;
                                advice.Reason = isHotZone
                                    ? $"L6 Bloccato (Hot Zone)"
                                    : $"L6 Bloccato (0 Autorizzazioni L6 residue)";
                            }
                        }
                        else if (securityFilterActive || rapidL5TriggerActive)
                        {
                            advice.StopL6 = true;
                            securityFilterPreventedL6ThisCall = true;
                            advice.Reason = rapidL5TriggerActive
                                ? $"L6 Bloccato (Trigger rapido L5)"
                                : $"L6 Bloccato (Security Filter)";
                        }
                        else if (isHotZone)
                        {
                            advice.StopL6 = true;
                            advice.Reason = "L6 Bloccato (Hot Zone)";
                        }
                        else if (SPOT_L6_PER_BOT_ENABLED && botSecurity.SpotL6Authorized)
                        {
                            advice.StopL6 = false;
                            advice.Reason =
                                $"L6 AUTORIZZATO [{botSecurity.SpotL5LossCount}/{SPOT_RESET_THRESHOLD_L5} L5 persi nel ciclo SPOT]";
                        }
                        else
                        {
                            advice.StopL6 = false;
                            advice.Reason = !SPOT_L6_PER_BOT_ENABLED
                                ? "L5 perso (SPOT L6 per bot spento)"
                                : IsSpotL6ThresholdConfigured()
                                    ? $"L5 perso [{botSecurity.SpotL5LossCount}/{SPOT_RESET_THRESHOLD_L5} verso L6]"
                                    : "L5 perso (soglia L6 per bot non configurata)";
                        }
                    }
                    else
                    {
                        totalL5Won++;
                    }
                }
            } else if (martingalaCounter >= 6)
            {
                if (martingalaCounter == 8)
                {
                    totalL8Played++;

                    if (esito != 'T') {
                        if (esito != coloreGiocato)
                        {
                            totalL8Lost++;
                        }
                        else
                        {
                            totalL8Won++;
                        }
                    }
                }

                if (prevMartingalaForL6 == 5 && SPOT_L6_PER_BOT_ENABLED)
                {
                    ApplySpotL6TransitionGate(
                        advice,
                        botSecurity,
                        securityFilterActive,
                        rapidL5TriggerActive,
                        isHotZone,
                        ref securityFilterPreventedL6ThisCall);
                }
                else
                {
                    advice.Reason = "Autorizzazione [L6 - L8] concessa";
                }
            }

            if (prevMartingalaForL6 == 5 && martingalaCounter >= 6)
            {
                TryConsumeSpotL6Authorization(
                    computer,
                    botSecurity,
                    advice,
                    ref l6AuthorizedThisCall);
            }

            advice.GlobalAuthL6Counter = _globalAuthL6Counter;
            advice.GlobalL5Loss = _globalL5Loss;
            advice.SpotL5PlayedCount = botSecurity.SpotL5PlayedCount;
            advice.SpotL5LossCount = botSecurity.SpotL5LossCount;
            advice.SpotL6GrantedCount = botSecurity.SpotL6GrantedCount;
            advice.SpotL6Authorized = botSecurity.SpotL6Authorized;
            advice.NextL5LossWillAuthorizeL6 = botSecurity.NextL5LossWillAuthorizeL6;
            advice.SpotL6PerBotEnabled = SPOT_L6_PER_BOT_ENABLED;
            advice.SpotCycleId = botSecurity.SpotCycleId;
            advice.SpotPbHandsPlayed = botSecurity.SpotPbHandsPlayed;
            advice.SpotL6ThresholdL5 = IsSpotL6ThresholdConfigured() ? SPOT_RESET_THRESHOLD_L5 : 0;
            advice.GlobalPBHandsPlayed = _globalPBHandsPlayed;

            #endregion

            #region SECURITY FILTER

            // — contatori transizione false→true —
            bool prevActive = _prevSecFilterActive.GetValueOrDefault(computer, false);
            if (pbHandPlayedThisCall)
            {
                botSecurity.PBHandsPlayed++;
                IncrementSpotPbHandForBot(computer, botSecurity);
            }

            if (securityFilterActive && !prevActive)
            {
                totalSecurityFilterActivated++;
                botSecurity.Activations++;
            }
            if (securityFilterPreventedL6ThisCall)
            {
                totalSecurityFilterPreventedL6++;
                botSecurity.PreventedL6++;
            }
            _prevSecFilterActive[computer] = securityFilterActive;

            botSecurity.AvgHandSeconds = avgHandSeconds;
            botSecurity.LastHandDeltaSeconds = lastHandDeltaSeconds;
            botSecurity.LastTwoHandDeltaSeconds = lastTwoHandDeltaSeconds;
            botSecurity.RapidL5TriggerActive = rapidL5TriggerActive;
            if (pbHandPlayedThisCall && lastHandDeltaSeconds > 0)
            {
                botSecurity.MinHandDeltaSeconds = botSecurity.MinHandDeltaSeconds <= 0
                    ? lastHandDeltaSeconds
                    : Math.Min(botSecurity.MinHandDeltaSeconds, lastHandDeltaSeconds);
                botSecurity.MaxHandDeltaSeconds = Math.Max(botSecurity.MaxHandDeltaSeconds, lastHandDeltaSeconds);
            }
            if (martingalaCounter == 6 && pbHandPlayedThisCall)
            {
                botSecurity.L6PlayedCount++;
                if (botSecurity.LastL6PlayedAtUtc != default)
                {
                    double l6DeltaSeconds = (nowUtc - botSecurity.LastL6PlayedAtUtc).TotalSeconds;
                    int l6DeltaHands = Math.Max(0, botSecurity.PBHandsPlayed - botSecurity.LastL6PlayedPBHands);
                    botSecurity.LastL6DeltaSeconds = l6DeltaSeconds;
                    botSecurity.MinL6DeltaSeconds = botSecurity.MinL6DeltaSeconds <= 0
                        ? l6DeltaSeconds
                        : Math.Min(botSecurity.MinL6DeltaSeconds, l6DeltaSeconds);
                    botSecurity.MaxL6DeltaSeconds = Math.Max(botSecurity.MaxL6DeltaSeconds, l6DeltaSeconds);
                    botSecurity.AvgL6DeltaSeconds =
                        ((botSecurity.AvgL6DeltaSeconds * botSecurity.L6DeltaSamples) + l6DeltaSeconds) /
                        (botSecurity.L6DeltaSamples + 1);
                    botSecurity.LastL6DeltaHands = l6DeltaHands;
                    botSecurity.MinL6DeltaHands = botSecurity.MinL6DeltaHands <= 0
                        ? l6DeltaHands
                        : Math.Min(botSecurity.MinL6DeltaHands, l6DeltaHands);
                    botSecurity.MaxL6DeltaHands = Math.Max(botSecurity.MaxL6DeltaHands, l6DeltaHands);
                    botSecurity.AvgL6DeltaHands =
                        ((botSecurity.AvgL6DeltaHands * botSecurity.L6DeltaSamples) + l6DeltaHands) /
                        (botSecurity.L6DeltaSamples + 1);
                    botSecurity.L6DeltaSamples++;
                }
                botSecurity.LastL6PlayedAtUtc = nowUtc;
                botSecurity.LastL6PlayedPBHands = botSecurity.PBHandsPlayed;
            }
            if (martingalaCounter == 8 && pbHandPlayedThisCall && esito != coloreGiocato)
            {
                botSecurity.AuthorizedL8LostCount++;
                if (botSecurity.LastAuthorizedL8LostAtUtc != default)
                {
                    double l8LostDeltaSeconds = (nowUtc - botSecurity.LastAuthorizedL8LostAtUtc).TotalSeconds;
                    int l8LostDeltaHands = Math.Max(0, botSecurity.PBHandsPlayed - botSecurity.LastAuthorizedL8LostPBHands);
                    botSecurity.LastAuthorizedL8LostDeltaSeconds = l8LostDeltaSeconds;
                    botSecurity.MinAuthorizedL8LostDeltaSeconds = botSecurity.MinAuthorizedL8LostDeltaSeconds <= 0
                        ? l8LostDeltaSeconds
                        : Math.Min(botSecurity.MinAuthorizedL8LostDeltaSeconds, l8LostDeltaSeconds);
                    botSecurity.MaxAuthorizedL8LostDeltaSeconds = Math.Max(botSecurity.MaxAuthorizedL8LostDeltaSeconds, l8LostDeltaSeconds);
                    botSecurity.AvgAuthorizedL8LostDeltaSeconds =
                        ((botSecurity.AvgAuthorizedL8LostDeltaSeconds * botSecurity.AuthorizedL8LostDeltaSamples) + l8LostDeltaSeconds) /
                        (botSecurity.AuthorizedL8LostDeltaSamples + 1);
                    botSecurity.LastAuthorizedL8LostDeltaHands = l8LostDeltaHands;
                    botSecurity.MinAuthorizedL8LostDeltaHands = botSecurity.MinAuthorizedL8LostDeltaHands <= 0
                        ? l8LostDeltaHands
                        : Math.Min(botSecurity.MinAuthorizedL8LostDeltaHands, l8LostDeltaHands);
                    botSecurity.MaxAuthorizedL8LostDeltaHands = Math.Max(botSecurity.MaxAuthorizedL8LostDeltaHands, l8LostDeltaHands);
                    botSecurity.AvgAuthorizedL8LostDeltaHands =
                        ((botSecurity.AvgAuthorizedL8LostDeltaHands * botSecurity.AuthorizedL8LostDeltaSamples) + l8LostDeltaHands) /
                        (botSecurity.AuthorizedL8LostDeltaSamples + 1);
                    botSecurity.AuthorizedL8LostDeltaSamples++;
                }
                botSecurity.LastAuthorizedL8LostAtUtc = nowUtc;
                botSecurity.LastAuthorizedL8LostPBHands = botSecurity.PBHandsPlayed;
                if (botSecurity.LastL6AuthorizationAtUtc != default)
                {
                    double authorizationToLossSeconds = (nowUtc - botSecurity.LastL6AuthorizationAtUtc).TotalSeconds;
                    int authorizationToLossHands = Math.Max(0, botSecurity.PBHandsPlayed - botSecurity.LastL6AuthorizationPBHandsPlayed);
                    botSecurity.LastAuthorizedL8LossFromAuthorizationSeconds = authorizationToLossSeconds;
                    botSecurity.MinAuthorizedL8LossFromAuthorizationSeconds = botSecurity.MinAuthorizedL8LossFromAuthorizationSeconds <= 0
                        ? authorizationToLossSeconds
                        : Math.Min(botSecurity.MinAuthorizedL8LossFromAuthorizationSeconds, authorizationToLossSeconds);
                    botSecurity.MaxAuthorizedL8LossFromAuthorizationSeconds = Math.Max(botSecurity.MaxAuthorizedL8LossFromAuthorizationSeconds, authorizationToLossSeconds);
                    botSecurity.AvgAuthorizedL8LossFromAuthorizationSeconds =
                        ((botSecurity.AvgAuthorizedL8LossFromAuthorizationSeconds * botSecurity.AuthorizedL8LostFromAuthorizationCount) + authorizationToLossSeconds) /
                        (botSecurity.AuthorizedL8LostFromAuthorizationCount + 1);

                    botSecurity.LastAuthorizedL8LossFromAuthorizationHands = authorizationToLossHands;
                    if (authorizationToLossHands > 0)
                    {
                        double secondsPerHand = authorizationToLossSeconds / authorizationToLossHands;
                        botSecurity.LastAuthorizedL8LossSecondsPerHand = secondsPerHand;
                        botSecurity.MinAuthorizedL8LossSecondsPerHand = botSecurity.MinAuthorizedL8LossSecondsPerHand <= 0
                            ? secondsPerHand
                            : Math.Min(botSecurity.MinAuthorizedL8LossSecondsPerHand, secondsPerHand);
                        botSecurity.MaxAuthorizedL8LossSecondsPerHand = Math.Max(botSecurity.MaxAuthorizedL8LossSecondsPerHand, secondsPerHand);
                        botSecurity.AvgAuthorizedL8LossSecondsPerHand =
                            ((botSecurity.AvgAuthorizedL8LossSecondsPerHand * botSecurity.AuthorizedL8LostFromAuthorizationCount) + secondsPerHand) /
                            (botSecurity.AuthorizedL8LostFromAuthorizationCount + 1);
                    }

                    botSecurity.LastAuthorizedL8LossAuthorizationScore = botSecurity.LastL6AuthorizationScore;
                    botSecurity.AvgAuthorizedL8LossAuthorizationScore =
                        ((botSecurity.AvgAuthorizedL8LossAuthorizationScore * botSecurity.AuthorizedL8LostFromAuthorizationCount) + botSecurity.LastL6AuthorizationScore) /
                        (botSecurity.AuthorizedL8LostFromAuthorizationCount + 1);
                    botSecurity.AuthorizedL8LostFromAuthorizationCount++;
                }
            }
            if (martingalaCounter == 8 && esito != 'T')
            {
                botSecurity.LastL6AuthorizationAtUtc = default;
            }
            if (l6AuthorizedThisCall)
            {
                botSecurity.LastL6AuthorizationAtUtc = nowUtc;
                botSecurity.LastL6AuthorizationPBHandsPlayed = botSecurity.PBHandsPlayed;
                botSecurity.LastL6AuthorizationScore = securityScore;
                botSecurity.LastL6AuthorizationStreak = currentStreak;
                botSecurity.LastL6AuthorizationShoeHand = handIndexMazzo;
                botSecurity.LastL6AuthorizationAvgHandSeconds = avgHandSeconds;
            }
            bool playerRace5Alert = EvaluatePlayerRace5Alert(botSecurity);
            bool playerRace5Ac3 = EvaluatePlayerRace5Ac3(botSecurity);
            bool playerRace8Alert = EvaluatePlayerRace8Alert(botSecurity);
            bool playerRace8Ac3 = EvaluatePlayerRace8Ac3(botSecurity);
            bool playerRaceAc3 = playerRace5Ac3 || playerRace8Ac3;

            bool prevPlayerPaceAc3 = _prevPlayerPaceAC3Active.GetValueOrDefault(computer, false);
            if (playerRaceAc3 && !prevPlayerPaceAc3)
                totalPlayerPaceAC3Activated++;
            _prevPlayerPaceAC3Active[computer] = playerRaceAc3;

            botSecurity.CurrentStreak = currentStreak;
            botSecurity.SecurityRiskScore = securityScore;
            botSecurity.SecurityFilterActive = securityFilterActive;
            botSecurity.PlayerRace5Alert = playerRace5Alert;
            botSecurity.PlayerRace5Triggered = playerRace5Alert;
            botSecurity.PlayerRace5Ac3Triggered = playerRace5Ac3;
            botSecurity.PlayerRace8Alert = playerRace8Alert;
            botSecurity.PlayerRace8Ac3Triggered = playerRace8Ac3;
            botSecurity.PlayerPaceTriggeredAC3 = playerRaceAc3;
            botSecurity.PlayerPaceRiskActive = playerRace5Alert || playerRace8Alert;
            botSecurity.PauseBot = securityFilterActive || playerRaceAc3;
            botSecurity.PauseScope = securityFilterActive || playerRaceAc3 ? "BOT" : "NONE";
            botSecurity.PauseComputer = securityFilterActive || playerRaceAc3 ? computer : "";
            botSecurity.LastShoeHand = handIndexMazzo;
            botSecurity.Martingala = martingalaCounter;
            botSecurity.HasL6Credit = LEGACY_GLOBAL_SPOT_L6_ENABLED && _globalAuthL6Counter > 0;
            if (botSecurity.SpotL6Authorized)
            {
                botSecurity.LastReason =
                    $"L6 AUTORIZZATO [{botSecurity.SpotL5LossCount}/{Math.Max(1, SPOT_RESET_THRESHOLD_L5)} L5 persi nel ciclo SPOT]";
            }
            else if (playerRace8Ac3)
            {
                botSecurity.LastReason =
                    $"PLAYER RACE 8 AC3 [{botSecurity.PlayerStreakCount} PLAYER consecutivi ≥ {PLAYER_RACE_8_MIN_STREAK}]";
            }
            else if (playerRace8Alert)
            {
                botSecurity.LastReason =
                    $"PLAYER RACE 8 [{botSecurity.PlayerStreakCount} PLAYER consecutivi ≥ {PLAYER_RACE_8_MIN_STREAK}]";
            }
            else if (playerRace5Ac3)
            {
                botSecurity.LastReason =
                    $"PLAYER RACE 5 AC3 [{botSecurity.PlayerStreakCount} PLAYER consecutivi ≥ {PLAYER_RACE_5_MIN_STREAK}]";
            }
            else if (playerRace5Alert)
            {
                botSecurity.LastReason =
                    $"PLAYER RACE 5 [{botSecurity.PlayerStreakCount} PLAYER consecutivi ≥ {PLAYER_RACE_5_MIN_STREAK}]";
            }
            else if (securityFilterActive)
            {
                botSecurity.LastReason = $"SECURITY FILTER [score {securityScore}/4]";
            }
            else
            {
                botSecurity.LastReason = !SECURITY_FILTER_ENABLED
                    ? $"disabled [score {securityScore}/4]"
                    : $"score {securityScore}/4";
            }
            botSecurity.LastUpdatedUtc = nowUtc;
            var validSampleCount = _handDeltasWindow.TryGetValue(computer, out var samplesWindow) ? samplesWindow.Count : 0;
            botSecurity.ValidSamples = validSampleCount;
            botSecurity.HandSamples = validSampleCount;
            botSecurity.GapFilteredCount = _gapFilteredCountByComputer.GetValueOrDefault(computer);

            advice.SecurityFilterEnabled    = SECURITY_FILTER_ENABLED;
            advice.SecurityRiskScore       = securityScore;
            advice.SecurityFilterActive    = securityFilterActive || securityFilterPreventedL6ThisCall;
            advice.SecurityFilterPauseBot  = securityFilterActive || securityFilterPreventedL6ThisCall;
            advice.SecurityFilterPauseScope = (securityFilterActive || securityFilterPreventedL6ThisCall) ? "BOT" : "NONE";
            advice.SecurityFilterPauseComputer = (securityFilterActive || securityFilterPreventedL6ThisCall) ? computer : "";
            advice.PlayerRace5Triggered      = playerRace5Alert;
            advice.PlayerRace5Ac3Triggered   = playerRace5Ac3;
            advice.PlayerRace5PauseBot       = playerRace5Ac3;
            advice.PlayerRace8Alert          = playerRace8Alert;
            advice.PlayerRace8Ac3Triggered   = playerRace8Ac3;
            advice.PlayerPaceRiskActive      = playerRace5Alert || playerRace8Alert;
            advice.PlayerPaceTriggeredAC3    = playerRaceAc3;
            advice.PlayerPacePauseBot        = playerRaceAc3;
            advice.PlayerPacePauseScope      = playerRaceAc3 ? "BOT" : "NONE";
            advice.PlayerPacePauseComputer   = playerRaceAc3 ? computer : "";
            advice.AvgHandSeconds          = avgHandSeconds;
            advice.LastHandDeltaSeconds    = lastHandDeltaSeconds;
            advice.MinHandDeltaSeconds     = botSecurity.MinHandDeltaSeconds;
            advice.MaxHandDeltaSeconds     = botSecurity.MaxHandDeltaSeconds;
            advice.CurrentStreak           = currentStreak;

            if (securityFilterActive)
                advice.Reason = $"SECURITY FILTER [score {securityScore}/4]: streak {currentStreak} | avg {avgHandSeconds:0.0}s | hand {handIndexMazzo}";
            else if (playerRaceAc3 || playerRace5Alert || playerRace8Alert)
                advice.Reason = botSecurity.LastReason;

            #endregion

            #region PAUSE SCALPING
            
            bool pbPauseActive = _pauseScalpingEndPB > _pbIndex  && !(currentCooldownPB < COOLDOWN_PAUSE_PB);
            bool timePauseActive = DateTime.UtcNow < _pauseScalpingUntil;
            
            /* contatori pause */
            if (pbPauseActive && !prevPauseScalpingEWMAStatus)
            {
                totalPauseScalpingEWMAActivated++;
            }
            else if (timePauseActive && !prevPauseScalpingSoglieStatus)
            {
                totalPauseScalpingSoglieActivated++;
            }

            prevPauseScalpingEWMAStatus = pbPauseActive;
            prevPauseScalpingSoglieStatus = timePauseActive;
            /* contatori pause */

            if (!_pauseReferenceInitialized)
            {
                _lastPauseReferenceMargin = globalMargin;
                _pauseReferenceInitialized = true;
            }
            else if (pbPauseActive || timePauseActive)
            {
                /*
                if (_pauseWasLoseBucket && globalMargin + PAUSE_SCALPING_LOSE_BUCKET >= _pauseTriggerMargin)
                {
                    _pauseScalpingUntil = DateTime.MinValue;
                    _lastPauseReferenceMargin = globalMargin;
                    _pauseWasLoseBucket = false;
                }
                else
                {
                    advice.GlobalPauseScalping = true;
                    advice.GlobalPauseScalpingDuration =
                        (int)Math.Round((_pauseScalpingUntil - DateTime.UtcNow).TotalSeconds);
                }
                */
                
                advice.GlobalPauseScalping = true;

                if (timePauseActive)
                {
                    advice.GlobalPauseScalpingDuration =
                        (int)Math.Round((_pauseScalpingUntil - DateTime.UtcNow).TotalSeconds);
                    
                    advice.Reason = !_pauseWasLoseBucket
                        ? $"PAUSE_SCALPING SOGLIA: +{PAUSE_SCALPING_WIN_BUCKET:0}€"
                        : $"PAUSE_SCALPING SOGLIA: {PAUSE_SCALPING_LOSE_BUCKET:0}€";
                }
                else
                {
                    advice.GlobalPauseScalpingDuration =
                        _pauseScalpingEndPB - _pbIndex;
                    
                    advice.Reason ="PAUSE_SCALPING EWMA";
                }
            }
            else
            {
                double delta = globalMargin - _lastPauseReferenceMargin;

                bool winBucketHit  = delta >= PAUSE_SCALPING_WIN_BUCKET;
                bool loseBucketHit = delta <= PAUSE_SCALPING_LOSE_BUCKET;

                if (winBucketHit || loseBucketHit)
                {
                    _pauseScalpingUntil = DateTime.UtcNow.AddSeconds(PAUSE_SCALPING_SECONDS);
                    _lastPauseReferenceMargin = globalMargin;

                    advice.GlobalPauseScalping = true;
                    advice.GlobalPauseScalpingDuration = PAUSE_SCALPING_SECONDS;

                    if (winBucketHit)
                    {
                        _pauseWasLoseBucket = false;
                    }
                    else
                    {
                        _pauseWasLoseBucket = true;
                        _pauseTriggerMargin = globalMargin;
                    }

                    advice.Reason = winBucketHit
                        ? $"PAUSE_SCALPING SOGLIA: +{PAUSE_SCALPING_WIN_BUCKET:0}€"
                        : $"PAUSE_SCALPING SOGLIA: {PAUSE_SCALPING_LOSE_BUCKET:0}€";
                }
            }

            #endregion


            advice.ActionCode = GetActionCode(advice, stato, martingalaCounter);
            advice.ToolTipJson = BuildToolTip(globalMargin, elapsedMinutes);

            return advice;
        }

        #endregion

        #region HELPERS
        
        void UpdatePBIndicators(double globalMargin)
        {
            _pbIndex++;

            // Se siamo in pausa PB non aggiorniamo indicatori
            bool pbPauseActive = _pauseScalpingEndPB > _pbIndex && !(currentCooldownPB < COOLDOWN_PAUSE_PB);
            bool timePauseActive = DateTime.UtcNow < _pauseScalpingUntil;

            if (pbPauseActive || timePauseActive)
            {
                return;
            }

            if (_pauseScalpingEndPB > _pbIndex)
            {
                currentCooldownPB++;
                if ((DateTime.UtcNow - _lastPBPauseEndedAt).TotalMinutes > 5)
                {
                    currentCooldownPB = COOLDOWN_PAUSE_PB;
                    return;
                }
            }
            else
            {
                if (_pbIndex == _pauseScalpingEndPB)
                {
                    _lastPBPauseEndedAt = DateTime.UtcNow;
                    
                    _pbBaseMargin = globalMargin;
                    _pbBaseIndex = _pbIndex;
                    _ewma = 0;
                    _pbMarginsWindow.Clear();
                    _pauseScalpingEndPB = -1;
                }
                currentCooldownPB = 0;
            }
            
            if (_pbIndex == 1)
            {
                _pbBaseMargin = globalMargin;
                _pbBaseIndex = _pbIndex;
            }

            _pbMarginsWindow.Enqueue(globalMargin);

            if (_pbMarginsWindow.Count <= WINDOW_PB)
                return;

            _pbMarginsWindow.Dequeue();

            int span = Math.Min(WINDOW_PB, _pbIndex - _pbBaseIndex);

            if (span <= 0)
                return;

            _inc = (globalMargin - _pbBaseMargin) / span;

            _ewma = ALPHA * _inc + (1 - ALPHA) * _ewma;

            if (_ewma <= STOP_EWMA)
            {
                _pauseScalpingEndPB = _pbIndex + PAUSE_PB;
            }
        }

        void SetStop(Advice advice, string reason)
        {
            advice.StopMission = true;
            advice.Reason = reason;
        }

        static bool IsScalpingState(string stato) =>
            stato.Equals("sculping", StringComparison.OrdinalIgnoreCase) ||
            stato.Equals("scalping", StringComparison.OrdinalIgnoreCase);

        static bool IsPlayerStreakP(SecurityFilterBotTelemetry botSecurity) =>
            string.Equals(botSecurity.CurrentStreakOutcome, "P", StringComparison.OrdinalIgnoreCase);

        bool AtPlayerRace5(SecurityFilterBotTelemetry botSecurity) =>
            IsPlayerStreakP(botSecurity) && botSecurity.PlayerStreakCount >= PLAYER_RACE_5_MIN_STREAK;

        bool AtPlayerRace8(SecurityFilterBotTelemetry botSecurity) =>
            IsPlayerStreakP(botSecurity) && botSecurity.PlayerStreakCount >= PLAYER_RACE_8_MIN_STREAK;

        bool EvaluatePlayerRace5Alert(SecurityFilterBotTelemetry botSecurity) =>
            PLAYER_RACE_5_FILTER_ENABLED && AtPlayerRace5(botSecurity);

        bool EvaluatePlayerRace5Ac3(SecurityFilterBotTelemetry botSecurity) =>
            PLAYER_RACE_5_AC3_ENABLED && AtPlayerRace5(botSecurity);

        bool EvaluatePlayerRace8Alert(SecurityFilterBotTelemetry botSecurity) =>
            PLAYER_RACE_8_FILTER_ENABLED && AtPlayerRace8(botSecurity);

        bool EvaluatePlayerRace8Ac3(SecurityFilterBotTelemetry botSecurity) =>
            PLAYER_RACE_8_AC3_ENABLED && AtPlayerRace8(botSecurity);

        bool IsSpotL6ThresholdConfigured() =>
            SPOT_RESET_THRESHOLD_L5 >= 1;

        int SpotCyclePbHandsLimit =>
            L6_AUTH_PB_RESET_COUNTER >= 1 ? L6_AUTH_PB_RESET_COUNTER : 0;

        void EnsureSpotCycleIdInitialized(string computer)
        {
            if (!_spotCycleIdByComputer.ContainsKey(computer))
                _spotCycleIdByComputer[computer] = 1;
        }

        void ClearSpotL6FieldsForBot(SecurityFilterBotTelemetry bot)
        {
            bot.SpotL5PlayedCount = 0;
            bot.SpotL5LossCount = 0;
            bot.SpotL6GrantedCount = 0;
            bot.SpotL6Authorized = false;
            bot.NextL5LossWillAuthorizeL6 = false;
            bot.HasL6Credit = false;
        }

        void ResetSpotL6CountersForBot(string computer, SecurityFilterBotTelemetry bot)
        {
            _spotL5LossByComputer[computer] = 0;
            _spotL5PlayedByComputer[computer] = 0;
            _spotL6GrantedByComputer[computer] = 0;
            ClearSpotL6FieldsForBot(bot);
            SyncSpotL6BotFields(computer, bot);
        }

        void ResetSpotCycleForBot(string computer, SecurityFilterBotTelemetry bot)
        {
            EnsureSpotCycleIdInitialized(computer);
            _spotCycleIdByComputer[computer]++;
            _spotPbHandsByComputer[computer] = 0;
            _spotL5LossByComputer[computer] = 0;
            _spotL5PlayedByComputer[computer] = 0;
            _spotL6GrantedByComputer[computer] = 0;
            ClearSpotL6FieldsForBot(bot);
            bot.SpotPbHandsPlayed = 0;
            SyncSpotL6BotFields(computer, bot);
        }

        void IncrementSpotPbHandForBot(string computer, SecurityFilterBotTelemetry bot)
        {
            if (!SPOT_L6_PER_BOT_ENABLED)
                return;

            EnsureSpotCycleIdInitialized(computer);
            var count = _spotPbHandsByComputer.GetValueOrDefault(computer) + 1;
            _spotPbHandsByComputer[computer] = count;
            bot.SpotPbHandsPlayed = count;
            SyncSpotL6BotFields(computer, bot);

            var limit = SpotCyclePbHandsLimit;
            // Mostra N/N alla mano N; nuovo ciclo alla mano successiva (N+1).
            if (limit >= 1 && count > limit)
                ResetSpotCycleForBot(computer, bot);
        }

        bool IsSpotL6AuthorizationMatured(string computer)
        {
            if (!SPOT_L6_PER_BOT_ENABLED || !IsSpotL6ThresholdConfigured())
                return false;

            return _spotL5LossByComputer.GetValueOrDefault(computer) >= SPOT_RESET_THRESHOLD_L5;
        }

        void ApplySpotL6TransitionGate(
            Advice advice,
            SecurityFilterBotTelemetry botSecurity,
            bool securityFilterActive,
            bool rapidL5TriggerActive,
            bool isHotZone,
            ref bool securityFilterPreventedL6ThisCall)
        {
            if (!IsSpotL6AuthorizationMatured(botSecurity.Computer))
            {
                advice.StopL6 = true;
                advice.Reason = !SPOT_L6_PER_BOT_ENABLED
                    ? "L6 Bloccato (SPOT L6 per bot spento)"
                    : IsSpotL6ThresholdConfigured()
                        ? $"L6 Bloccato (autorizzazione SPOT non maturata [{botSecurity.SpotL5LossCount}/{SPOT_RESET_THRESHOLD_L5}])"
                        : "L6 Bloccato (soglia L6 per bot non configurata)";
                return;
            }

            if (securityFilterActive || rapidL5TriggerActive)
            {
                advice.StopL6 = true;
                securityFilterPreventedL6ThisCall = true;
                advice.Reason = rapidL5TriggerActive
                    ? "L6 Bloccato (Trigger rapido L5)"
                    : "L6 Bloccato (Security Filter)";
                return;
            }

            if (isHotZone)
            {
                advice.StopL6 = true;
                advice.Reason = "L6 Bloccato (Hot Zone)";
                return;
            }

            advice.StopL6 = false;
            advice.Reason =
                $"L6 AUTORIZZATO [{botSecurity.SpotL5LossCount}/{SPOT_RESET_THRESHOLD_L5} L5 persi nel ciclo SPOT]";
        }

        void TryConsumeSpotL6Authorization(
            string computer,
            SecurityFilterBotTelemetry botSecurity,
            Advice advice,
            ref bool l6AuthorizedThisCall)
        {
            if (!SPOT_L6_PER_BOT_ENABLED || advice.StopL6 || !IsSpotL6AuthorizationMatured(computer))
                return;

            _spotL6GrantedByComputer[computer] = _spotL6GrantedByComputer.GetValueOrDefault(computer) + 1;
            _spotL5LossByComputer[computer] = 0;
            l6AuthorizedThisCall = true;
            SyncSpotL6BotFields(computer, botSecurity);
        }

        void SyncSpotL6BotFields(string computer, SecurityFilterBotTelemetry bot)
        {
            EnsureSpotCycleIdInitialized(computer);
            var threshold = IsSpotL6ThresholdConfigured() ? SPOT_RESET_THRESHOLD_L5 : 0;
            bot.SpotCycleId = _spotCycleIdByComputer[computer];
            bot.SpotL5PlayedCount = _spotL5PlayedByComputer.GetValueOrDefault(computer);
            bot.SpotL5LossCount = _spotL5LossByComputer.GetValueOrDefault(computer);
            bot.SpotL6GrantedCount = _spotL6GrantedByComputer.GetValueOrDefault(computer);
            bot.SpotPbHandsPlayed = _spotPbHandsByComputer.GetValueOrDefault(computer);
            var matured = threshold >= 1 && bot.SpotL5LossCount >= threshold;
            bot.SpotL6Authorized = matured;
            bot.NextL5LossWillAuthorizeL6 =
                threshold >= 1 && !matured && bot.SpotL5LossCount == threshold - 1;
        }

        void ApplySpotL6PerBot(
            string computer,
            int martingalaCounter,
            SecurityFilterBotTelemetry botSecurity,
            bool l5PlayedThisCall,
            bool l5LostThisCall)
        {
            if (!SPOT_L6_PER_BOT_ENABLED)
            {
                ClearSpotL6FieldsForBot(botSecurity);
                botSecurity.SpotCycleId = 0;
                botSecurity.SpotPbHandsPlayed = 0;
                return;
            }

            if (l5PlayedThisCall)
                _spotL5PlayedByComputer[computer] = _spotL5PlayedByComputer.GetValueOrDefault(computer) + 1;

            if (l5LostThisCall)
                _spotL5LossByComputer[computer] = _spotL5LossByComputer.GetValueOrDefault(computer) + 1;

            SyncSpotL6BotFields(computer, botSecurity);

            _lastMartingalaByComputer[computer] = martingalaCounter;
            SyncSpotL6BotFields(computer, botSecurity);
        }

        void UpdatePlayerStreakPace(string computer, char esito, DateTime nowUtc, SecurityFilterBotTelemetry botSecurity)
        {
            if (!_playerStreakTimestampsByComputer.TryGetValue(computer, out var timestamps))
                timestamps = new List<DateTime>(capacity: PLAYER_RACE_8_MIN_STREAK);

            if (!_playerStreakCountByComputer.TryGetValue(computer, out var playerCount))
                playerCount = 0;

            if (esito == 'P')
            {
                playerCount++;
                if (timestamps.Count < PLAYER_RACE_8_MIN_STREAK)
                    timestamps.Add(nowUtc);
            }
            else if (esito == 'B')
            {
                playerCount = 0;
                timestamps = new List<DateTime>(capacity: PLAYER_RACE_8_MIN_STREAK);
            }

            _playerStreakCountByComputer[computer] = playerCount;
            _playerStreakTimestampsByComputer[computer] = timestamps;

            char streakOutcome = _streakByComputer.TryGetValue(computer, out var streakEntry) ? streakEntry.Outcome : '\0';
            botSecurity.CurrentStreakOutcome = streakOutcome == '\0' ? "" : streakOutcome.ToString();
            botSecurity.PlayerStreakCount = playerCount;

            int tsCount = timestamps.Count;
            if (tsCount >= 2)
            {
                var intervals = new double[tsCount - 1];
                for (int i = 1; i < tsCount; i++)
                    intervals[i - 1] = (timestamps[i] - timestamps[i - 1]).TotalSeconds;

                botSecurity.PlayerStreakIntervalSeconds = intervals;

                if (tsCount >= 5)
                {
                    double total = (timestamps[4] - timestamps[0]).TotalSeconds;
                    botSecurity.PlayerStreakP1ToP5TotalSeconds = total;
                    botSecurity.PlayerStreakMeanIntervalSeconds = total / 4.0;
                }
                else
                {
                    botSecurity.PlayerStreakP1ToP5TotalSeconds = 0;
                    botSecurity.PlayerStreakMeanIntervalSeconds = 0;
                }
            }
            else
            {
                botSecurity.PlayerStreakP1ToP5TotalSeconds = 0;
                botSecurity.PlayerStreakMeanIntervalSeconds = 0;
                botSecurity.PlayerStreakIntervalSeconds = Array.Empty<double>();
            }
        }

        void ResetHandPaceWindow(string computer)
        {
            _handDeltasWindow.Remove(computer);
            _lastDecideAt.Remove(computer);
        }

        static double ComputeTrimmedAverage(IEnumerable<double> samples)
        {
            var list = samples.ToList();
            if (list.Count == 0)
                return 0;

            if (list.Count >= 3)
            {
                var sorted = list.OrderBy(x => x).ToList();
                return sorted.Skip(1).Take(Math.Max(1, sorted.Count - 2)).Average();
            }

            return list.Average();
        }

        int GetActionCode(Advice advice, string stato, int martingalaCounter)
        {
            int actionCode = 0;

            if (advice.StopMission)
                actionCode = 1;

            else if (advice.SecurityFilterActive)
                actionCode = 3;

            else if (advice.PlayerPaceTriggeredAC3)
                actionCode = 3;

            else if (advice.GlobalPauseScalping &&
                     (stato.ToLower().Equals("sculping") || stato.ToLower().Equals("scalping")) &&
                     (martingalaCounter < PAUSE_SCALPING_MARTINGALA_RANGE_EXCLUDE[0] || martingalaCounter > PAUSE_SCALPING_MARTINGALA_RANGE_EXCLUDE[1]))
                actionCode = 3;

            else if (advice.StopL6)
                actionCode = 2;

            return actionCode;
        }

        string BuildToolTip(double globalMargin, double elapsed)
        {
            double l5WinRate = totalL5Played > 0
                ? (double)totalL5Won / totalL5Played * 100
                : 0;

            double l8WinRate = totalL8Played > 0
                ? (double)totalL8Won / totalL8Played * 100
                : 0;

            double averageBotMargin = _lastBotMargin.Count > 0
                ? _lastBotMargin.Values.Average()
                : 0;

            double activeBetsTotal = _activeBets.Values.Sum(x => x.Valore);
            double realExposure = globalMargin - activeBetsTotal;

            var tooltip = new Dictionary<string, string>
            {
                ["STATO_GENERALE"] =
                    $"📊 Margine globale: {globalMargin:+0;-0}€ | " +
                    $"Tempo trascorso: {elapsed:0} minuti",

                ["ESPOSIZIONE_ATTUALE"] =
                    $"💥 Esposizione reale: {realExposure:+0;-0}€ | " +
                    $"Valore totale puntate attive: {activeBetsTotal:0}€",

                ["SISTEMA_L6"] = LEGACY_GLOBAL_SPOT_L6_ENABLED
                    ? $"🔐 Autorizzazioni L6 disponibili: {_globalAuthL6Counter} | " +
                      $"Perdite consecutive in L5: {_globalL5Loss} su {L6_AUTH_LOSS}"
                    : $"🔺 SPOT per-bot attivo | soglia {SPOT_RESET_THRESHOLD_L5} L5 persi | legacy globale OFF",

                ["MANI_GIOCATE_PB"] = SpotCyclePbHandsLimit >= 1
                    ? $"🎴 Ciclo SPOT per-bot: {SpotCyclePbHandsLimit}/{SpotCyclePbHandsLimit} poi nuovo ciclo alla mano {SpotCyclePbHandsLimit + 1}"
                    : $"🎴 Mani PB totali missione: {_globalPBHandsPlayed}",

                ["STATISTICHE_OPERATIVE"] =
                    $"📈 Livello 5: {totalL5Won} vinte su {totalL5Played} " +
                    $"({l5WinRate:0.0}% di successo) | " +
                    $"Livello 8: {totalL8Won} vinte su {totalL8Played} " +
                    $"({l8WinRate:0.0}% di successo)",

                ["VALUTAZIONE_SISTEMA"] = globalMargin >= 0
                    ? "✅ Il sistema mantiene un margine positivo e controllato"
                    : "⚠️ Il sistema è sotto pressione con margine negativo"
            };

            return JsonSerializer.Serialize(tooltip);
        }


        #endregion

        #region INTERNAL TYPES

        sealed class ActiveBet
        {
            public string Computer;
            public int TableId;
            public double Valore;
            public int HandIndex;
        }

        #endregion
    }
}
