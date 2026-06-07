using System.Globalization;
using System.Net;
using System.Text;
using WebApi.Controllers;

namespace WebApi.Services;

public static class MissionReportHtmlBuilder
{
    public const string TemplateMarker = "mission-report-html:v2026-06-07-period-result-hero";

    public static string Build(MissionRangeReportResponse report)
    {
        var culture = CultureInfo.GetCultureInfo("it-IT");
        var title = report.IsDemoMode ? "Report missione DEMO" : "Report missione Production";
        var statement = report.IsDemoMode ? "Demo Statement" : "Official Production Statement";
        var generated = TimeZoneInfo.ConvertTimeFromUtc(
            report.GeneratedAt.Kind == DateTimeKind.Utc ? report.GeneratedAt : report.GeneratedAt.ToUniversalTime(),
            ResolveRomeTimeZone()).ToString("HH:mm", culture);
        var period = $"{report.From:dd MMMM yyyy} - {report.To:dd MMMM yyyy}";
        var subtitle = $"Periodo: {period} • Modalità: {Html(report.RuntimeMode)} • Generato alle {generated} (Europe/Rome)";

        var singleSession = report.Sessions.Count == 1 ? report.Sessions[0] : null;
        var heroMargin = report.Totals.PeriodResultEuro;
        var heroLabel = singleSession != null
            ? $"RISULTATO PERIODO • #{singleSession.SessionId}"
            : "RISULTATO PERIODO";
        var heroTone = Tone(heroMargin);

        var sb = new StringBuilder();
        sb.AppendLine("<!doctype html><html lang=\"it\"><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width,initial-scale=1\">");
        sb.AppendLine($"<!-- {TemplateMarker} -->");
        sb.AppendLine($"<title>{Html(title)}</title>");
        sb.AppendLine("""
<style>
:root{color-scheme:light;--ink:#101828;--muted:#667085;--line:#d0d5dd;--soft:#f8fafc;--ok:#057a55;--bad:#b42318;--accent:#111827}
*{box-sizing:border-box}html{background:#eef2f7}body{margin:0;background:#eef2f7;color:var(--ink);font-family:Inter,Segoe UI,Arial,sans-serif;-webkit-print-color-adjust:exact;print-color-adjust:exact}
.container{width:210mm;margin:0 auto;padding:10mm 0}.paper{width:190mm;min-height:277mm;margin:0 auto;background:#fff;border:1px solid var(--line);padding:12mm}
.topRule{height:6px;background:#111827;margin:-12mm -12mm 10mm}.header{display:flex;justify-content:space-between;gap:20px;align-items:flex-start}
.brandKicker{font-size:12px;text-transform:uppercase;letter-spacing:.16em;color:var(--muted);font-weight:800}.h1{font-size:28px;font-weight:850;margin-top:4px}.sub{margin-top:8px;color:var(--muted);font-size:13px}
.seal{border:1px solid var(--line);border-radius:999px;padding:10px 14px;font-size:12px;font-weight:800;white-space:nowrap}.actions{margin-top:16px}.btn{border:1px solid #111827;background:#111827;color:#fff;border-radius:999px;padding:10px 15px;font-weight:800;cursor:pointer}
.hero{margin:20px 0;padding:22px 20px;border:2px solid var(--line);background:var(--soft);text-align:center}
.heroTitle{font-size:13px;text-transform:uppercase;letter-spacing:.14em;color:var(--muted);font-weight:800}.heroValue{font-size:42px;font-weight:900;margin-top:10px;letter-spacing:-.02em}.heroSub{margin-top:8px;color:var(--muted);font-size:13px}
.section{margin-top:22px}h2{font-size:16px;margin:0 0 7px}.sectionSub{color:var(--muted);font-size:11px;line-height:1.35;margin:-2px 0 10px}
.table{width:100%;border-collapse:collapse;border:1px solid var(--line);font-size:12px}.table th,.table td{padding:9px 8px;border-bottom:1px solid var(--line);text-align:left;vertical-align:top}.table th{font-size:9px;text-transform:uppercase;letter-spacing:.1em;color:var(--muted);background:#fff}.ledgerAmount{text-align:right;font-weight:800}.mono{font-variant-numeric:tabular-nums}
.pos{color:var(--ok)}.neg{color:var(--bad)}.neutral{color:var(--ink)}
.debug-tech{margin-top:18px;border:1px dashed var(--line);padding:10px 12px;font-size:10px;color:var(--muted)}.debug-tech summary{cursor:pointer;font-weight:700}
.footer{margin-top:24px;text-align:center;border-top:1px solid var(--line);padding-top:14px;color:var(--muted);font-size:11px}
@media(max-width:760px){.container{padding:0}.paper{border:0;padding:22px}.topRule{margin:-22px -22px 22px}.header{display:block}.seal{display:inline-flex;margin-top:14px}}
@page{size:A4 portrait;margin:10mm}
@media print{html,body{width:210mm;background:#fff}.container{width:auto;margin:0;padding:0}.paper{width:auto;min-height:auto;margin:0;border:0;padding:0}.topRule{margin:0 0 8mm;height:4px}.noprint{display:none!important}.hero,.section,.footer{break-inside:avoid}thead{display:table-header-group}tr{break-inside:avoid}a{color:inherit;text-decoration:none}}
</style>
""");
        sb.AppendLine("</head><body><div class=\"container\"><div class=\"paper\"><div class=\"topRule\"></div>");
        sb.AppendLine("<div class=\"header\"><div><div class=\"brandKicker\">Eugenio Trading</div>");
        sb.AppendLine($"<div class=\"h1\">{Html(title)}</div><div class=\"sub\">{subtitle}</div></div>");
        sb.AppendLine($"<div><div class=\"seal\">{Html(statement)}</div><div class=\"actions noprint\"><button class=\"btn\" onclick=\"window.print()\">Stampa / PDF</button></div></div></div>");

        sb.AppendLine("<div class=\"hero\">");
        sb.AppendLine($"<div class=\"heroTitle\">{Html(heroLabel)}</div>");
        sb.AppendLine($"<div class=\"heroValue {heroTone}\">{Html(FormatEuro(heroMargin))}</div>");
        if (singleSession != null)
        {
            var dur = FormatDuration(singleSession.StartTime, singleSession.EndTime);
            sb.AppendLine($"<div class=\"heroSub\">{FormatRomeDateTime(singleSession.StartTime, culture)} → {(singleSession.EndTime.HasValue ? FormatRomeDateTime(singleSession.EndTime.Value, culture) : "in corso")} • Durata {Html(dur)} • {singleSession.RealHandsCount.ToString(CultureInfo.InvariantCulture)} mani • {singleSession.ActiveTables.ToString(CultureInfo.InvariantCulture)} tavoli</div>");
        }
        else if (report.Sessions.Count > 0)
        {
            sb.AppendLine($"<div class=\"heroSub\">{report.Sessions.Count.ToString(CultureInfo.InvariantCulture)} missioni • P&amp;L periodo (delta sample clippati) • esclude missione aperta</div>");
        }
        sb.AppendLine("</div>");

        if (report.Sessions.Count > 0)
        {
            var sectionTitle = report.Sessions.Count == 1 ? "Dettaglio missione" : "Missioni nel periodo";
            sb.AppendLine($"<div class=\"section\"><h2>{Html(sectionTitle)}</h2>");
            sb.AppendLine("<div class=\"sectionSub\">Margine missione = risultato reale registrato a chiusura missione, non il delta sample nella finestra contabile del periodo.</div>");
            sb.AppendLine("<table class=\"table\"><thead><tr><th>Missione</th><th>Start</th><th>End</th><th>Durata</th><th>Tavoli</th><th>Mani</th><th>P&amp;L periodo</th><th>Margine missione</th></tr></thead><tbody>");
            foreach (var session in report.Sessions)
            {
                var margin = MissionMargin(session);
                var periodPnl = session.PeriodNetPnlEuro;
                sb.AppendLine(
                    $"<tr><td>#{session.SessionId}</td>" +
                    $"<td>{FormatRomeDateTime(session.StartTime, culture)}</td>" +
                    $"<td>{(session.EndTime.HasValue ? FormatRomeDateTime(session.EndTime.Value, culture) : "-")}</td>" +
                    $"<td class=\"mono\">{Html(FormatDuration(session.StartTime, session.EndTime))}</td>" +
                    $"<td class=\"mono\">{session.ActiveTables.ToString(CultureInfo.InvariantCulture)}</td>" +
                    $"<td class=\"mono\">{session.RealHandsCount.ToString(CultureInfo.InvariantCulture)}</td>" +
                    $"<td class=\"ledgerAmount {Tone(periodPnl)}\">{FormatEuro(periodPnl)}</td>" +
                    $"<td class=\"ledgerAmount {Tone(margin)}\">{FormatEuro(margin)}</td></tr>");
            }
            sb.AppendLine("</tbody></table></div>");
        }

        if (HasTechnicalDebug(report))
        {
            sb.AppendLine("<details class=\"debug-tech noprint\"><summary>Debug tecnico contabilità periodo (non stampare)</summary>");
            sb.AppendLine("<p>periodNetPnlEuro = delta primo/ultimo sample nella finestra del report; può differire dal margine missione se la missione attraversa il taglio del periodo.</p>");
            sb.AppendLine("<table class=\"table\"><thead><tr><th>Missione</th><th>missionMarginEuro</th><th>periodNetPnlEuro</th><th>finalMarginEuro</th></tr></thead><tbody>");
            foreach (var session in report.Sessions)
            {
                sb.AppendLine(
                    $"<tr><td>#{session.SessionId}</td>" +
                    $"<td class=\"mono\">{MissionMargin(session).ToString("0.00", CultureInfo.InvariantCulture)}</td>" +
                    $"<td class=\"mono\">{session.PeriodNetPnlEuro.ToString("0.00", CultureInfo.InvariantCulture)}</td>" +
                    $"<td class=\"mono\">{session.FinalMarginEuro.ToString("0.00", CultureInfo.InvariantCulture)}</td></tr>");
            }
            sb.AppendLine("</tbody></table></details>");
        }

        sb.AppendLine("<div class=\"footer\">EuGenio Lab — Ingegneria del Trading.<br/>Generato automaticamente da EuGenio Trading Dashboard</div>");
        sb.AppendLine("</div></div></body></html>");
        return sb.ToString();
    }

    private static decimal MissionMargin(MissionReportSession session)
        => session.MissionMarginEuro;

    private static bool HasTechnicalDebug(MissionRangeReportResponse report)
        => report.Sessions.Any(s => Math.Abs(MissionMargin(s) - s.PeriodNetPnlEuro) > 0.005m);

    private static string FormatDuration(DateTime startUtc, DateTime? endUtc)
    {
        var end = endUtc ?? DateTime.UtcNow;
        var start = startUtc.Kind switch
        {
            DateTimeKind.Utc => startUtc,
            DateTimeKind.Local => startUtc.ToUniversalTime(),
            _ => DateTime.SpecifyKind(startUtc, DateTimeKind.Utc)
        };
        var endNorm = end.Kind switch
        {
            DateTimeKind.Utc => end,
            DateTimeKind.Local => end.ToUniversalTime(),
            _ => DateTime.SpecifyKind(end, DateTimeKind.Utc)
        };
        var d = endNorm - start;
        if (d < TimeSpan.Zero) d = TimeSpan.Zero;
        if (d.TotalHours >= 1)
            return $"{(int)d.TotalHours}h{d.Minutes:00}m";
        if (d.TotalMinutes >= 1)
            return $"{(int)d.TotalMinutes}m";
        return $"{(int)d.TotalSeconds}s";
    }

    private static string FormatEuro(decimal value)
    {
        var sign = value > 0 ? "+" : value < 0 ? "-" : "";
        return $"{sign}{Math.Abs(value).ToString("0.00", CultureInfo.InvariantCulture)} €";
    }

    private static string FormatRomeDateTime(DateTime timestamp, CultureInfo culture)
    {
        var utc = timestamp.Kind switch
        {
            DateTimeKind.Utc => timestamp,
            DateTimeKind.Local => timestamp.ToUniversalTime(),
            _ => DateTime.SpecifyKind(timestamp, DateTimeKind.Utc)
        };
        var rome = TimeZoneInfo.ConvertTimeFromUtc(utc, ResolveRomeTimeZone());
        return rome.ToString("dd/MM/yyyy HH:mm", culture);
    }

    private static string Tone(decimal value) => value > 0 ? "pos" : value < 0 ? "neg" : "neutral";

    private static string Html(string value) => WebUtility.HtmlEncode(value);

    private static TimeZoneInfo ResolveRomeTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Rome");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
        }
    }
}
