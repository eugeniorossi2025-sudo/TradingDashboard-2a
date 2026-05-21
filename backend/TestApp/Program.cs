// SafeGuardAuditor - Console app per audit dei JSON del motore Proattivo
// .NET 8 single-file (top-level statements). Copia questo file come Program.cs in un progetto Console .NET 8.
// Istruzioni rapide:
// 1) dotnet new console -n SafeGuardAuditor -f net8.0
// 2) Sostituisci Program.cs con questo file
// 3) Aggiungi il pacchetto Microsoft.Data.SqlClient (opzionale: via csproj o CLI)
//    dotnet add package Microsoft.Data.SqlClient --version 5.2.0
// 4) Imposta la connection string qui sotto o passa --cs "..."
// 5) (Facoltativo) Aggiungi la colonna Notes: ALTER TABLE [dbo].[SafeGuardJson] ADD [Notes] NVARCHAR(MAX) NULL;
// 6) Esegui: dotnet run -- --update-notes

using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Text.Json;

// ------------------------------------------------------------
// Config
// ------------------------------------------------------------
var argsList = args.ToList();
var connectionString = GetArg("--cs") ??
                       "Server=VMI1096128\\SQLEXPRESS;Database=Eugenio-svil;Trusted_Connection=True;TrustServerCertificate=True;";
var updateNotes = true;

await EnsureNotesColumnAsync(connectionString);

var rows = await LoadRowsAsync(connectionString);
if (rows.Count == 0)
{
    Console.WriteLine("Nessun record trovato in dbo.SafeGuardJson.");
    return;
}

var culture = CultureInfo.InvariantCulture;
var allSeenEver = new HashSet<string>();

SafeGuardSnapshot? prev = null;
var prevId = 0;

for (var i = 0; i < rows.Count; i++)
{
    var (id, json, dt) = rows[i];
    var snap = Deserialize(json, id);
    if (snap == null)
    {
        var noteBad = $"❌ JSON non valido o non deserializzabile.";
        await MaybeUpdateNote(connectionString, id, noteBad, updateNotes);
        Console.WriteLine($"[{id}] {noteBad}");
        continue;
    }

    // Ordina e normalizza collezioni
    snap.SeenInputs ??= new List<string>();
    snap.Rows ??= new Dictionary<int, RowStateDto>();
    snap.LastAdvice ??= new Dictionary<int, AdviceDto>();

    var note = new StringBuilder();

    if (prev == null)
    {
        note.AppendLine("🔹 Baseline iniziale.");
        note.AppendLine(
            $"SeenInputs={snap.SeenInputs.Count}, Rows={snap.Rows.Count}, GlobalMargin={snap.GlobalMargin}, HeavyCount={snap.HeavyCount}, Cooldown={snap.Cooldown}.");
        foreach (var k in snap.SeenInputs) allSeenEver.Add(k);
        await MaybeUpdateNote(connectionString, id, note.ToString(), updateNotes);
        Console.WriteLine($"[{id}] baseline acquisita ({dt:yyyy-MM-dd HH:mm:ss}).");
        prev = snap;
        prevId = id;
        continue;
    }

    // Delta chiavi viste
    var prevSet = new HashSet<string>(prev.SeenInputs);
    var curSet = new HashSet<string>(snap.SeenInputs);
    var added = curSet.Except(prevSet).ToList();
    var removed = prevSet.Except(curSet).ToList();

    // Duplicati nella timeline globale (evento già visto in passato)
    var duplicates = added.Where(k => allSeenEver.Contains(k)).ToList();
    foreach (var k in added) allSeenEver.Add(k);

    // Controllo integrità: state change senza nuovi input
    var stateChanged = StateChanged(prev, snap);

    if (added.Count == 0)
    {
        if (stateChanged)
        {
            note.AppendLine("⚠️ Stato cambiato senza nuovi SeenInputs.");
            note.AppendLine(DescribeDiff(prev, snap));
        }
        else
        {
            note.AppendLine("ℹ️ Nessun nuovo input. Stato invariato.");
        }

        if (removed.Count > 0)
            note.AppendLine(
                $"⚠️ SeenInputs diminuiti (rimozioni inattese): {string.Join(", ", removed.Take(5))}{(removed.Count > 5 ? ", ..." : "")}.");
    }
    else
    {
        note.AppendLine(
            $"➕ Nuovi input: {added.Count} {(duplicates.Count > 0 ? "(contiene duplicati)" : string.Empty)}");
        if (added.Count <= 5)
            note.AppendLine("  · " + string.Join("\n  · ", added));
        else
            note.AppendLine("  · (lista lunga, omessa)");

        if (duplicates.Count > 0)
        {
            note.AppendLine("❗ Trovati eventi duplicati rispetto a snapshot precedenti:");
            foreach (var k in duplicates.Take(5)) note.AppendLine("   - " + k);
            if (duplicates.Count > 5) note.AppendLine("   - ...");
        }

        // Prova a rigiocare gli added in un ordine deterministico per  stimare lo stato atteso
        var predicted = CloneSnapshot(prev);
        var settings = new ProactiveSettings();

        // Ordine deterministico: per TableId asc, poi HandIndex asc
        var parsed = new List<ParsedKey>();
        foreach (var k in added)
            if (TryParseKey(k, out var pk)) parsed.Add(pk!);
            else note.AppendLine($"❌ Chiave non parsabile: {k}");
        var ordered = parsed.OrderBy(p => p.TableId).ThenBy(p => p.HandIndex).ToList();

        foreach (var ev in ordered) ApplyEvent(predicted, ev, settings);

        // Confronto stato atteso vs corrente
        var diffs = DescribeDiff(predicted, snap);
        if (string.IsNullOrWhiteSpace(diffs))
        {
            note.AppendLine("✅ Stato coerente con l'algoritmo (entro limiti del riordino deterministico).");
        }
        else
        {
            note.AppendLine("❌ Differenze rispetto allo stato atteso (potenziale bug o ordine eventi reale diverso):");
            note.AppendLine(diffs);
        }

        // Se exactly 1 evento, controllo anche LastAdvice del relativo tavolo
        if (ordered.Count == 1)
        {
            var ev = ordered[0];
            // Ricalcolo solo quell'evento su una copia per estrarre il LastAdvice atteso
            var predictForAdvice = CloneSnapshot(prev);
            ApplyEvent(predictForAdvice, ev, settings);

            if (predictForAdvice.LastAdvice.TryGetValue(ev.TableId, out var advExpected) &&
                snap.LastAdvice.TryGetValue(ev.TableId, out var advCur))
            {
                var advDiff = DescribeAdviceDiff(advExpected, advCur);
                if (string.IsNullOrEmpty(advDiff))
                {
                    note.AppendLine("🟢 LastAdvice coerente per il tavolo " + ev.TableId + ".");
                }
                else
                {
                    note.AppendLine("🟠 LastAdvice difforme per il tavolo " + ev.TableId + ":");
                    note.AppendLine("   " + advDiff);
                }
            }
        }

        if (removed.Count > 0)
            note.AppendLine(
                $"⚠️ SeenInputs diminuiti (rimozioni inattese): {string.Join(", ", removed.Take(5))}{(removed.Count > 5 ? ", ..." : "")}.");
    }

    // Scrivi note
    var noteStr = note.ToString().TrimEnd();
    await MaybeUpdateNote(connectionString, id, noteStr, updateNotes);


    Console.WriteLine($"[{id}]\n{noteStr}\n");


    prev = snap;
    prevId = id;
}

Console.WriteLine(updateNotes
    ? "✔︎ Note aggiornate in tabella."
    : "(dry run) Nessun update sul DB. Usa --update-notes per scrivere le note.");

// ------------------------------------------------------------
// Helpers
// ------------------------------------------------------------

static string? GetArg(string name)
{
    var idx = Array.IndexOf(Environment.GetCommandLineArgs(), name);
    if (idx >= 0 && idx + 1 < Environment.GetCommandLineArgs().Length)
        return Environment.GetCommandLineArgs()[idx + 1];
    return null;
}

static async Task EnsureNotesColumnAsync(string cs)
{
    using var conn = new SqlConnection(cs);
    await conn.OpenAsync();
    var exists =
        await new SqlCommand(
                @"SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='SafeGuardJson' AND COLUMN_NAME='Notes'",
                conn)
            .ExecuteScalarAsync();
    if (exists == null)
    {
        Console.WriteLine("Aggiungo colonna Notes...");
        var cmd = new SqlCommand("ALTER TABLE [dbo].[SafeGuardJson] ADD [Notes] NVARCHAR(MAX) NULL;", conn);
        await cmd.ExecuteNonQueryAsync();
    }
}

static async Task<List<(int Id, string Json, DateTime? When)>> LoadRowsAsync(string cs)
{
    var list = new List<(int, string, DateTime?)>();
    using var conn = new SqlConnection(cs);
    await conn.OpenAsync();
    using var cmd = new SqlCommand(@"SELECT ID, JSON, DATETIME FROM dbo.SafeGuardJson ORDER BY DATETIME ASC, ID ASC",
        conn);
    using var rdr = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess);
    while (await rdr.ReadAsync())
    {
        var id = Convert.ToInt32(rdr.GetDecimal(0)); // numeric(18,0)
        var json = rdr.IsDBNull(1) ? "" : rdr.GetString(1);
        DateTime? when = rdr.IsDBNull(2) ? null : rdr.GetDateTime(2);
        list.Add((id, json, when));
    }

    return list;
}

static async Task MaybeUpdateNote(string cs, int id, string note, bool update)
{
    if (!update) return;
    using var conn = new SqlConnection(cs);
    await conn.OpenAsync();
    using var cmd = new SqlCommand("UPDATE dbo.SafeGuardJson SET Notes=@n WHERE ID=@id", conn);
    cmd.Parameters.AddWithValue("@n", note);
    cmd.Parameters.AddWithValue("@id", id);
    await cmd.ExecuteNonQueryAsync();
}

static SafeGuardSnapshot? Deserialize(string json, int id)
{
    if (string.IsNullOrWhiteSpace(json)) return null;
    try
    {
        var opt = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };
        return JsonSerializer.Deserialize<SafeGuardSnapshot>(json, opt);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[{id}] Errore JSON: {ex.Message}");
        return null;
    }
}

static bool StateChanged(SafeGuardSnapshot a, SafeGuardSnapshot b)
{
    if (a.GlobalMargin != b.GlobalMargin) return true;
    if (a.HeavyCount != b.HeavyCount) return true;
    if (a.Cooldown != b.Cooldown) return true;
    if (a.Rows.Count != b.Rows.Count) return true;

    foreach (var kv in a.Rows)
    {
        if (!b.Rows.TryGetValue(kv.Key, out var br)) return true;
        var ar = kv.Value;
        if (ar.PrevMazzo != br.PrevMazzo) return true;
        if (ar.PrevLevel != br.PrevLevel) return true;
        if (ar.PrevStake != br.PrevStake) return true;
        if (ar.PrevMargine != br.PrevMargine) return true;
        if (ar.RunP != br.RunP) return true;
        if (!HistEqual(ar.History, br.History)) return true;
    }

    return false;
}

static string DescribeDiff(SafeGuardSnapshot expected, SafeGuardSnapshot actual)
{
    var sb = new StringBuilder();
    if (expected.GlobalMargin != actual.GlobalMargin)
        sb.AppendLine($" · GlobalMargin atteso {expected.GlobalMargin}, attuale {actual.GlobalMargin}");
    if (expected.HeavyCount != actual.HeavyCount)
        sb.AppendLine($" · HeavyCount atteso {expected.HeavyCount}, attuale {actual.HeavyCount}");
    if (expected.Cooldown != actual.Cooldown)
        sb.AppendLine($" · Cooldown atteso {expected.Cooldown}, attuale {actual.Cooldown}");

    // Rows
    foreach (var kv in expected.Rows)
    {
        if (!actual.Rows.TryGetValue(kv.Key, out var br))
        {
            sb.AppendLine($" · Row {kv.Key} mancante nell'attuale");
            continue;
        }

        var ar = kv.Value;
        if (ar.PrevMazzo != br.PrevMazzo)
            sb.AppendLine($" · Row {kv.Key} PrevMazzo atteso {ar.PrevMazzo}, attuale {br.PrevMazzo}");
        if (ar.PrevLevel != br.PrevLevel)
            sb.AppendLine($" · Row {kv.Key} PrevLevel atteso {ar.PrevLevel}, attuale {br.PrevLevel}");
        if (ar.PrevStake != br.PrevStake)
            sb.AppendLine($" · Row {kv.Key} PrevStake atteso {ar.PrevStake}, attuale {br.PrevStake}");
        if (ar.PrevMargine != br.PrevMargine)
            sb.AppendLine($" · Row {kv.Key} PrevMargine atteso {ar.PrevMargine}, attuale {br.PrevMargine}");
        if (ar.RunP != br.RunP)
            sb.AppendLine($" · Row {kv.Key} RunP atteso {ar.RunP}, attuale {br.RunP}");
        if (!HistEqual(ar.History, br.History))
            sb.AppendLine(
                $" · Row {kv.Key} History diversa (attesa: [{string.Join(',', ar.History)}], attuale: [{string.Join(',', br.History)}])");
    }

    // Extra rows in actual non presenti in expected
    foreach (var kv in actual.Rows)
        if (!expected.Rows.ContainsKey(kv.Key))
            sb.AppendLine($" · Row extra presente nell'attuale: {kv.Key}");

    return sb.ToString().Trim();
}

static string DescribeAdviceDiff(AdviceDto a, AdviceDto b)
{
    var sb = new StringBuilder();
    if (a.LevelIndex != b.LevelIndex) sb.Append($"LevelIndex {a.LevelIndex}->{b.LevelIndex}; ");
    if (a.StakeUnits != b.StakeUnits) sb.Append($"Stake {a.StakeUnits}->{b.StakeUnits}; ");
    if (a.StopAtL5 != b.StopAtL5) sb.Append($"StopAtL5 {a.StopAtL5}->{b.StopAtL5}; ");
    if (a.AuthorizedHeavy != b.AuthorizedHeavy)
        sb.Append($"AuthorizedHeavy {a.AuthorizedHeavy}->{b.AuthorizedHeavy}; ");
    if (!string.Equals(a.SignalW10, b.SignalW10, StringComparison.OrdinalIgnoreCase))
        sb.Append($"SignalW10 {a.SignalW10}->{b.SignalW10}; ");
    if (a.HotZone != b.HotZone) sb.Append($"HotZone {a.HotZone}->{b.HotZone}; ");
    // Reason/GlobalMargin sono informativi, li ignoriamo per evitare falsi positivi.
    return sb.ToString().Trim();
}

static bool HistEqual(List<string>? a, List<string>? b)
{
    a ??= new List<string>();
    b ??= new List<string>();
    if (a.Count != b.Count) return false;
    for (var i = 0; i < a.Count; i++)
        if (!string.Equals(a[i], b[i], StringComparison.Ordinal))
            return false;
    return true;
}

static SafeGuardSnapshot CloneSnapshot(SafeGuardSnapshot src)
{
    return new SafeGuardSnapshot
    {
        GlobalMargin = src.GlobalMargin,
        HeavyCount = src.HeavyCount,
        Cooldown = src.Cooldown,
        Rows = src.Rows.ToDictionary(kv => kv.Key, kv => kv.Value.Clone()),
        SeenInputs = new List<string>(src.SeenInputs),
        LastAdvice = src.LastAdvice.ToDictionary(kv => kv.Key, kv => kv.Value.Clone())
    };
}

// Applica un solo evento (key) al modello, emulando il ProactiveEngine.FeedAndDecide
static void ApplyEvent(SafeGuardSnapshot state, ParsedKey ev, ProactiveSettings s)
{
    // Recupera o crea RowState
    if (!state.Rows.TryGetValue(ev.TableId, out var rs))
    {
        rs = new RowStateDto();
        state.Rows[ev.TableId] = rs;
    }

    var levelIdx = OutcomeInferer.ToLevelIndex(ev.MartingalaUi);
    var stakeNow = s.Levels[levelIdx];

    var outcome = OutcomeInferer.InferOutcome(rs, levelIdx, ev.Margine);

    // History (solo se non Tie)
    if (outcome != 'T')
    {
        rs.History ??= new List<string>();
        rs.History.Add(outcome.ToString());
        while (rs.History.Count > s.WindowW10) rs.History.RemoveAt(0);
    }

    // RunP
    if (outcome == 'P') rs.RunP++;
    else if (outcome == 'B') rs.RunP = 0;

    // GlobalMargin: usa PrevStake se c'era un prev
    if (rs.PrevMazzo.HasValue)
    {
        if (outcome == 'B') state.GlobalMargin += rs.PrevStake;
        else if (outcome == 'P') state.GlobalMargin -= rs.PrevStake;
    }

    // Cooldown countdown
    if (state.Cooldown > 0) state.Cooldown--;

    // Regia
    var (hmax, cdn) = GetRegiaParams(state, s);

    // Signal W10 & HotZone
    var sig = GetSignalW10(rs, s);
    var hot = InHotZone(ev.HandIndex, s);

    var adv = new AdviceDto
    {
        TableId = ev.TableId,
        LevelIndex = levelIdx,
        StakeUnits = stakeNow,
        StopAtL5 = false,
        AuthorizedHeavy = false,
        SignalW10 = sig == Signal.Green ? "Green" : "YellowOrRed",
        HotZone = hot,
        GlobalMargin = state.GlobalMargin,
        Reason = "Default L<=4"
    };

    if (levelIdx < 4)
    {
        // nessuna azione speciale
    }
    else if (levelIdx == 4)
    {
        if (hot)
        {
            adv.StopAtL5 = true;
            adv.Reason = "Stop L5: zona calda";
        }
        else if (adv.SignalW10 != "Green")
        {
            adv.StopAtL5 = true;
            adv.Reason = "Stop L5: semaforo W10 giallo/rosso";
        }
        else if (state.HeavyCount >= hmax)
        {
            adv.StopAtL5 = true;
            adv.Reason = "Stop L5: Hmax raggiunto";
        }
        else if (state.Cooldown > 0)
        {
            adv.StopAtL5 = true;
            adv.Reason = "Stop L5: cooldown sala";
        }
        else
        {
            adv.AuthorizedHeavy = true;
            adv.Reason = "Autorizzato L6: W10 green + zona neutra + gate aperto";
            state.HeavyCount++;
            state.Cooldown = cdn;
        }
    }
    else
    {
        if (rs.RunP >= 5)
        {
            adv.StopAtL5 = true;
            adv.Reason = "Stop immediato >L5: run P>=5";
        }
        else
        {
            adv.AuthorizedHeavy = true;
            adv.Reason = "In heavy (L6+): prosegui salvo tutela P>=5";
        }
    }

    // Aggiorna prev*
    rs.PrevMazzo = ev.HandIndex;
    rs.PrevLevel = levelIdx;
    rs.PrevMargine = ev.Margine;
    rs.PrevStake = stakeNow;

    // LastAdvice del tavolo
    state.LastAdvice[ev.TableId] = adv;

    // SeenInputs (opzionale, utile per tracing locale)
    if (!state.SeenInputs.Contains(ev.Raw)) state.SeenInputs.Add(ev.Raw);
}

static (int hmax, int cdn) GetRegiaParams(SafeGuardSnapshot st, ProactiveSettings s)
{
    if (st.GlobalMargin >= s.HighThresh) return (s.HmaxHigh, s.CooldownHigh);
    if (st.GlobalMargin <= s.LowThresh) return (s.HmaxLow, s.CooldownLow);
    return (s.HmaxMid, s.CooldownMid);
}

static bool InHotZone(int handNo, ProactiveSettings s)
{
    foreach (var z in s.HotZones)
        if (handNo >= z.start && handNo <= z.end)
            return true;
    return false;
}

static Signal GetSignalW10(RowStateDto rs, ProactiveSettings s)
{
    int cur = 0, maxp = 0;
    var hist = rs.History ?? new List<string>();
    foreach (var o in hist.AsEnumerable().Reverse())
        if (o == "P")
        {
            cur++;
            maxp = Math.Max(maxp, cur);
        }
        else if (o == "B")
        {
            cur = 0;
        }

    return maxp <= s.MaxRunPAllowed ? Signal.Green : Signal.YellowOrRed;
}

static bool TryParseKey(string key, out ParsedKey? pk)
{
    pk = null;
    try
    {
        // key format: "tableId-handIndex-margine-martingalaUi";
        // ATTENZIONE: margine può essere negativo ⇒ la stringa contiene due '-'.
        var i1 = key.IndexOf('-');
        var i2 = key.IndexOf('-', i1 + 1);
        var ilast = key.LastIndexOf('-');
        if (i1 < 0 || i2 < 0 || ilast <= i2) return false;
        var tStr = key.Substring(0, i1);
        var hStr = key.Substring(i1 + 1, i2 - i1 - 1);
        var mStr = key.Substring(i2 + 1, ilast - i2 - 1);
        var lStr = key.Substring(ilast + 1);

        var culture = CultureInfo.InvariantCulture;
        pk = new ParsedKey
        {
            Raw = key,
            TableId = int.Parse(tStr, culture),
            HandIndex = int.Parse(hStr, culture),
            Margine = decimal.Parse(mStr, culture),
            MartingalaUi = int.Parse(lStr, culture)
        };
        return true;
    }
    catch
    {
        return false;
    }
}

// ------------------------------------------------------------
// DTOs e logica motore (mirror del tuo codice)
// ------------------------------------------------------------

public enum Signal
{
    Green,
    YellowOrRed
}

public class ProactiveSettings
{
    public int[] Levels { get; set; } = new[] { 1, 3, 7, 15, 35, 75, 155, 340 };
    public int WindowW10 { get; set; } = 10;
    public int MaxRunPAllowed { get; set; } = 2;
    public (int start, int end)[] HotZones { get; set; } = new (int, int)[] { (1, 10), (11, 20), (41, 50), (51, 60) };
    public int HighThresh { get; set; } = 500;
    public int LowThresh { get; set; } = -500;
    public int HmaxHigh { get; set; } = 2;
    public int HmaxMid { get; set; } = 1;
    public int HmaxLow { get; set; } = 0;
    public int CooldownHigh { get; set; } = 0;
    public int CooldownMid { get; set; } = 1;
    public int CooldownLow { get; set; } = 2;
}

public sealed class SafeGuardSnapshot
{
    public int GlobalMargin { get; set; }
    public int HeavyCount { get; set; }
    public int Cooldown { get; set; }
    public Dictionary<int, RowStateDto> Rows { get; set; } = new();
    public List<string> SeenInputs { get; set; } = new();
    public Dictionary<int, AdviceDto> LastAdvice { get; set; } = new();
}

public sealed class RowStateDto
{
    public int? PrevMazzo { get; set; }
    public int PrevLevel { get; set; }
    public decimal PrevMargine { get; set; }
    public int PrevStake { get; set; }
    public List<string>? History { get; set; } = new();
    public int RunP { get; set; } = 0;

    public RowStateDto Clone() => new()
    {
        PrevMazzo = PrevMazzo,
        PrevLevel = PrevLevel,
        PrevMargine = PrevMargine,
        PrevStake = PrevStake,
        History = History == null ? new List<string>() : new List<string>(History),
        RunP = RunP
    };
}

public sealed class AdviceDto
{
    public int TableId { get; set; }
    public int LevelIndex { get; set; }
    public int StakeUnits { get; set; }
    public bool StopAtL5 { get; set; }
    public bool AuthorizedHeavy { get; set; }
    public string Reason { get; set; } = "";
    public string SignalW10 { get; set; } = "Green";
    public bool HotZone { get; set; } = false;
    public int GlobalMargin { get; set; }

    public AdviceDto Clone() => new()
    {
        TableId = TableId,
        LevelIndex = LevelIndex,
        StakeUnits = StakeUnits,
        StopAtL5 = StopAtL5,
        AuthorizedHeavy = AuthorizedHeavy,
        Reason = Reason,
        SignalW10 = SignalW10,
        HotZone = HotZone,
        GlobalMargin = GlobalMargin
    };
}

public static class OutcomeInferer
{
    private static bool Approx(decimal x, decimal y, decimal tol = 0.6m) => Math.Abs(x - y) <= tol;

    public static int ToLevelIndex(int martingalaUi)
    {
        if (martingalaUi >= 1 && martingalaUi <= 8)
            return martingalaUi - 1;
        var max = 7;
        var min = 0;
        return Math.Max(Math.Min(martingalaUi, max), min);
    }

    public static char InferOutcome(RowStateDto s, int levelIdxNow, decimal margineNow)
    {
        if (s.PrevMazzo is null) return 'T';
        var dM = margineNow - s.PrevMargine;
        var dL = levelIdxNow - s.PrevLevel;

        if (Approx(dM, 0m) && dL == 0) return 'T';
        if ((levelIdxNow == 0 || dL < 0) && Approx(dM, +s.PrevStake)) return 'B';
        if (dL >= 1 && Approx(dM, -s.PrevStake)) return 'P';
        if (dL > 0) return 'P';
        if (dL < 0 || levelIdxNow == 0) return 'B';
        return 'T';
    }
}

public sealed class ParsedKey
{
    public required string Raw { get; set; }
    public required int TableId { get; set; }
    public required int HandIndex { get; set; }
    public required decimal Margine { get; set; }
    public required int MartingalaUi { get; set; }
}