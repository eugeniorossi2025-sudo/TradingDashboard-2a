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
        private readonly Dictionary<string, (char Outcome, int Count)> _streakByComputer    = new();
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
        public double SECURITY_FILTER_MAX_AVG_SECONDS    = 23.5;
        public double SECURITY_FILTER_VERY_FAST_SECONDS  = 21.0;
        public int    SECURITY_FILTER_DELTA_WINDOW       = 8;
        public int    SECURITY_FILTER_MIN_SCORE          = 3;
        
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
            telemetry.SpotID = spotID;
            telemetry.SpotPBHandsPlayed = _globalPBHandsPlayed;
            telemetry.SpotAuthL6Counter = _globalAuthL6Counter;
            telemetry.SpotL5Loss = _globalL5Loss;
            
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
            telemetry.TotalSecurityFilterActivated   = totalSecurityFilterActivated;
            telemetry.TotalSecurityFilterPreventedL6 = totalSecurityFilterPreventedL6;
            telemetry.LastAvgHandSeconds =
                _handDeltasWindow.Values.Where(q => q.Count > 0)
                    .Select(q => q.Average())
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
                    SecurityRiskScore = x.Value.SecurityRiskScore,
                    SecurityFilterActive = x.Value.SecurityFilterActive,
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
                    HandSamples = x.Value.HandSamples
                });
            
            /* Fine nuovi campi */
            
            return telemetry;
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

            if (_globalPBHandsPlayed >= L6_AUTH_PB_RESET_COUNTER)
            {
                spotID++;

                _globalPBHandsPlayed = 0;
                _globalAuthL6Counter = INITIAL_L6_AUTH;
                _globalL5Loss = 0;
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

            // — timing: misura solo le mani P/B realmente giocate dal singolo bot —
            if (pbHandPlayedThisCall)
            {
                if (_lastDecideAt.TryGetValue(computer, out var lastAt))
                {
                    lastHandDeltaSeconds = (nowUtc - lastAt).TotalSeconds;

                    if (!_handDeltasWindow.TryGetValue(computer, out var win))
                        _handDeltasWindow[computer] = win = new Queue<double>();

                    win.Enqueue(lastHandDeltaSeconds);
                    if (win.Count > SECURITY_FILTER_DELTA_WINDOW)
                        win.Dequeue();

                    // media trimmata: rimuovi il minimo e il massimo per attenuare spike rete/OCR
                    if (win.Count >= 3)
                    {
                        var sorted  = win.OrderBy(x => x).ToList();
                        var trimmed = sorted.Skip(1).Take(Math.Max(1, sorted.Count - 2));
                        avgHandSeconds = trimmed.Average();
                    }
                    else if (win.Count > 0)
                    {
                        avgHandSeconds = win.Average();
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
                    totalL5Played++;

                if (esito != 'T') {
                    if (esito != coloreGiocato)
                    {
                        _globalL5Loss++;
                        totalL5Lost++;

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

                            if (isHotZone)
                            {
                                advice.Reason = $"L6 Bloccato (Hot Zone)";
                            }
                            else
                            {
                                advice.Reason = $"L6 Bloccato (0 Autorizzazioni L6 residue)";
                            }
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

                advice.Reason = "Autorizzazione [L6 - L8] concessa";
            }

            advice.GlobalAuthL6Counter = _globalAuthL6Counter;
            advice.GlobalL5Loss = _globalL5Loss;
            advice.GlobalPBHandsPlayed = _globalPBHandsPlayed;

            #endregion

            #region SECURITY FILTER

            // — contatori transizione false→true —
            bool prevActive = _prevSecFilterActive.GetValueOrDefault(computer, false);
            if (pbHandPlayedThisCall)
                botSecurity.PBHandsPlayed++;

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
            botSecurity.CurrentStreak = currentStreak;
            botSecurity.SecurityRiskScore = securityScore;
            botSecurity.SecurityFilterActive = securityFilterActive;
            botSecurity.PauseBot = securityFilterActive;
            botSecurity.PauseScope = securityFilterActive ? "BOT" : "NONE";
            botSecurity.PauseComputer = securityFilterActive ? computer : "";
            botSecurity.LastShoeHand = handIndexMazzo;
            botSecurity.Martingala = martingalaCounter;
            botSecurity.HasL6Credit = _globalAuthL6Counter > 0;
            botSecurity.LastReason = securityFilterActive
                ? $"SECURITY FILTER [score {securityScore}/4]"
                : !SECURITY_FILTER_ENABLED
                    ? $"disabled [score {securityScore}/4]"
                : $"score {securityScore}/4";
            botSecurity.LastUpdatedUtc = nowUtc;
            botSecurity.HandSamples = _handDeltasWindow.TryGetValue(computer, out var samplesWindow) ? samplesWindow.Count : 0;

            advice.SecurityFilterEnabled    = SECURITY_FILTER_ENABLED;
            advice.SecurityRiskScore       = securityScore;
            advice.SecurityFilterActive    = securityFilterActive || securityFilterPreventedL6ThisCall;
            advice.SecurityFilterPauseBot  = securityFilterActive || securityFilterPreventedL6ThisCall;
            advice.SecurityFilterPauseScope = (securityFilterActive || securityFilterPreventedL6ThisCall) ? "BOT" : "NONE";
            advice.SecurityFilterPauseComputer = (securityFilterActive || securityFilterPreventedL6ThisCall) ? computer : "";
            advice.AvgHandSeconds          = avgHandSeconds;
            advice.LastHandDeltaSeconds    = lastHandDeltaSeconds;
            advice.MinHandDeltaSeconds     = botSecurity.MinHandDeltaSeconds;
            advice.MaxHandDeltaSeconds     = botSecurity.MaxHandDeltaSeconds;
            advice.CurrentStreak           = currentStreak;

            if (securityFilterActive)
                advice.Reason = $"SECURITY FILTER [score {securityScore}/4]: streak {currentStreak} | avg {avgHandSeconds:0.0}s | hand {handIndexMazzo}";

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

        int GetActionCode(Advice advice, string stato, int martingalaCounter)
        {
            int actionCode = 0;

            if (advice.StopMission)
                actionCode = 1;

            else if (advice.SecurityFilterActive)
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

                ["SISTEMA_L6"] =
                    $"🔐 Autorizzazioni L6 disponibili: {_globalAuthL6Counter} | " +
                    $"Perdite consecutive in L5: {_globalL5Loss} su {L6_AUTH_LOSS}",

                ["MANI_GIOCATE_PB"] =
                    $"🎴 Mani Player/Banker giocate: {_globalPBHandsPlayed} su {L6_AUTH_PB_RESET_COUNTER} " +
                    $"(reset automatico)",

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
