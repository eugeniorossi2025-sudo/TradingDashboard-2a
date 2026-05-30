using System.Globalization;
using System.Net;
using System.Text;
using WebApi.Controllers;

namespace WebApi.Services;

public static class MissionReportHtmlBuilder
{
    public static string Build(MissionRangeReportResponse report)
    {
        var culture = CultureInfo.GetCultureInfo("it-IT");
        var title = report.IsDemoMode ? "Mission Report DEMO" : "Mission Report Production";
        var statement = report.IsDemoMode ? "Demo Statement" : "Official Production Statement";
        var generated = TimeZoneInfo.ConvertTimeFromUtc(
            report.GeneratedAt.Kind == DateTimeKind.Utc ? report.GeneratedAt : report.GeneratedAt.ToUniversalTime(),
            ResolveRomeTimeZone()).ToString("HH:mm", culture);
        var period = $"{report.From:dd MMMM yyyy} - {report.To:dd MMMM yyyy}";
        var subtitle = $"Reporting period: {period} • Strategy mode: {Html(report.RuntimeMode)} • Generated at {generated} (Europe/Rome)";
        var periodResult = report.Totals.PeriodResultEuro;
        var periodTone = Tone(periodResult);
        var periodLabel = periodResult > 0 ? "Periodo positivo" : periodResult < 0 ? "Periodo negativo" : "Periodo in pari";

        var sb = new StringBuilder();
        sb.AppendLine("<!doctype html><html lang=\"it\"><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width,initial-scale=1\">");
        sb.AppendLine($"<title>{Html(title)}</title>");
        sb.AppendLine("""
<style>
:root{color-scheme:light;--ink:#101828;--muted:#667085;--line:#d0d5dd;--soft:#f8fafc;--ok:#057a55;--bad:#b42318;--accent:#111827}
*{box-sizing:border-box}html{background:#eef2f7}body{margin:0;background:#eef2f7;color:var(--ink);font-family:Inter,Segoe UI,Arial,sans-serif;-webkit-print-color-adjust:exact;print-color-adjust:exact}
.container{width:210mm;margin:0 auto;padding:10mm 0}.paper{width:190mm;min-height:277mm;margin:0 auto;background:#fff;border:1px solid var(--line);padding:12mm}
.topRule{height:6px;background:#111827;margin:-12mm -12mm 10mm}.header{display:flex;justify-content:space-between;gap:20px;align-items:flex-start}
.brandKicker{font-size:12px;text-transform:uppercase;letter-spacing:.16em;color:var(--muted);font-weight:800}.h1{font-size:28px;font-weight:850;margin-top:4px}.sub{margin-top:8px;color:var(--muted);font-size:13px}
.seal{border:1px solid var(--line);border-radius:999px;padding:10px 14px;font-size:12px;font-weight:800;white-space:nowrap}.actions{margin-top:16px}.btn{border:1px solid #111827;background:#111827;color:#fff;border-radius:999px;padding:10px 15px;font-weight:800;cursor:pointer}
.hero{margin:20px 0;padding:18px;border:1px solid var(--line);background:var(--soft)}.heroTitle{font-size:13px;text-transform:uppercase;letter-spacing:.12em;color:var(--muted);font-weight:800}.heroValue{font-size:34px;font-weight:900;margin-top:8px}.heroSub{margin-top:6px;color:var(--muted);font-size:13px}
.summaryGrid{display:grid;grid-template-columns:repeat(4,1fr);gap:9px}.card{border:1px solid var(--line);padding:10px;min-height:72px}.k{font-size:10px;color:var(--muted);text-transform:uppercase;letter-spacing:.1em;font-weight:800}.v{font-size:18px;font-weight:850;margin-top:8px}.v.small{font-size:14px}.focus{background:#f9fafb}
.pos{color:var(--ok)}.neg{color:var(--bad)}.neutral{color:var(--ink)}.methodNote,.sectionSub,.footerMeta{color:var(--muted);font-size:11px;line-height:1.35}.section{margin-top:20px}h2{font-size:16px;margin:0 0 7px}
.chartWrap{border:1px solid var(--line);padding:10px}.chart{width:100%;height:155px}.axis{stroke:#d0d5dd}.curve{fill:none;stroke:#111827;stroke-width:3;stroke-linecap:round;stroke-linejoin:round}
.table{width:100%;border-collapse:collapse;border:1px solid var(--line);font-size:11px}.table th,.table td{padding:7px;border-bottom:1px solid var(--line);text-align:left;vertical-align:top}.table th{font-size:9px;text-transform:uppercase;letter-spacing:.1em;color:var(--muted);background:#fff}.ledgerAmount{text-align:right}.mono{font-variant-numeric:tabular-nums}
.footer{margin-top:24px;text-align:center;border-top:1px solid var(--line);padding-top:14px}.footerMark{font-size:18px}.footerNumbers{font-size:16px;letter-spacing:.25em}.footerBrand{font-weight:850;margin-top:6px}
@media(max-width:760px){.container{padding:0}.paper{border:0;padding:22px}.topRule{margin:-22px -22px 22px}.header{display:block}.seal{display:inline-flex;margin-top:14px}.summaryGrid{grid-template-columns:repeat(2,1fr)}}
@page{size:A4 portrait;margin:10mm}
@media print{html,body{width:210mm;background:#fff}body{font-size:11px}.container{width:auto;margin:0;padding:0}.paper{width:auto;min-height:auto;margin:0;border:0;padding:0}.topRule{margin:0 0 8mm;height:4px}.noprint{display:none!important}.header,.hero,.summaryGrid,.card,.chartWrap,.footer{break-inside:avoid;page-break-inside:avoid}.section{break-inside:auto;page-break-inside:auto}h2{break-after:avoid;page-break-after:avoid}table{break-inside:auto;page-break-inside:auto}tr{break-inside:avoid;page-break-inside:avoid}thead{display:table-header-group}tfoot{display:table-footer-group}.summaryGrid{grid-template-columns:repeat(4,1fr);gap:6px}.card{min-height:58px;padding:7px}.v{font-size:15px;margin-top:5px}.hero{margin:8mm 0 5mm;padding:8px}.section{margin-top:8mm}.chart{height:120px}.footer{margin-top:8mm}a{color:inherit;text-decoration:none}}
</style>
""");
        sb.AppendLine("</head><body><div class=\"container\"><div class=\"paper\"><div class=\"topRule\"></div>");
        sb.AppendLine("<div class=\"header\"><div><div class=\"brandKicker\">Eugenio Trading</div>");
        sb.AppendLine($"<div class=\"h1\">{Html(title)}</div><div class=\"sub\">{subtitle}</div></div>");
        sb.AppendLine($"<div><div class=\"seal\">{Html(statement)}</div><div class=\"actions noprint\"><button class=\"btn\" onclick=\"window.print()\">Print / Export PDF</button></div></div></div>");

        sb.AppendLine("<div class=\"hero\">");
        sb.AppendLine("<div class=\"heroTitle\">Risultato periodo</div>");
        sb.AppendLine($"<div class=\"heroValue {periodTone}\">{Html(FormatEuro(periodResult))}</div>");
        sb.AppendLine($"<div class=\"heroSub\">{Html(periodLabel)} • {Html(FormatPercent(report.Totals.PeriodReturnPct))} sul capitale base • {report.Totals.WorkingDays.ToString(CultureInfo.InvariantCulture)} giorni operativi</div>");
        sb.AppendLine("</div>");

        var q = report.QualityMetrics;
        sb.AppendLine("<div class=\"summaryGrid\">");
        AddCard(sb, "Invested Capital", "€ •••••••", "neutral");
        AddCard(sb, "Period Return", FormatPercent(report.Totals.PeriodReturnPct), Tone(report.Totals.PeriodReturnPct));
        AddCard(sb, "Annualised Return", FormatAnnualisedReturn(report.Totals.AnnualisedReturnPct), FormatAnnualisedTone(report.Totals.AnnualisedReturnPct));
        AddCard(sb, "Average Daily P&L", FormatEuro(report.Totals.AverageDailyPnl), Tone(report.Totals.AverageDailyPnl));
        AddCard(sb, "Average Daily Return", FormatPercent(report.Totals.AverageDailyReturnPct), Tone(report.Totals.AverageDailyReturnPct));
        AddCard(sb, "Working Days", report.Totals.WorkingDays.ToString(CultureInfo.InvariantCulture), "neutral");
        AddCard(sb, "Reporting Days", report.Totals.ReportingDays.ToString(CultureInfo.InvariantCulture), "neutral");
        AddCard(sb, "Sessions", report.Totals.SessionCount.ToString(CultureInfo.InvariantCulture), "neutral");
        AddCard(sb, "Real Hands", report.Totals.RealHandsCount.ToString(CultureInfo.InvariantCulture), "neutral");
        AddCard(sb, "Tables", report.Totals.ActiveTables.ToString(CultureInfo.InvariantCulture), "neutral");
        sb.AppendLine("</div>");
        sb.AppendLine("<p class=\"methodNote\">Il Risultato periodo è la somma dei P&amp;L missione nel periodo selezionato (sample clippati su Europe/Rome). Il capitale investito è mascherato per privacy. Il rendimento annualizzato è mostrato solo con almeno 7 giorni operativi osservati; altrimenti N/D.</p>");

        sb.AppendLine("<div class=\"section\"><h2>Risk / Quality Metrics</h2><div class=\"sectionSub\">Calculated from real daily observations in the selected reporting period.</div><div class=\"summaryGrid\">");
        AddCard(sb, "Best Day", FormatEuro(q.BestDay), Tone(q.BestDay));
        AddCard(sb, "Worst Day", FormatEuro(q.WorstDay), Tone(q.WorstDay));
        AddCard(sb, "Positive Days", q.PositiveDays.ToString(CultureInfo.InvariantCulture), "neutral");
        AddCard(sb, "Win Rate", FormatPercent(q.WinRatePct), Tone(q.WinRatePct));
        AddCard(sb, "Max Drawdown", FormatPercent(q.MaxDrawdownPct), q.MaxDrawdownPct > 0 ? "neg" : "neutral");
        AddCard(sb, "Daily Volatility", FormatPercent(q.DailyVolatilityPct), "neutral");
        AddCard(sb, "Sharpe Ratio", q.SharpeRatio.ToString("0.00", CultureInfo.InvariantCulture), "neutral");
        sb.AppendLine("</div></div>");

        if (report.DailyRows.Count > 0)
        {
            sb.AppendLine("<div class=\"section\"><h2>Curva risultato periodo</h2><div class=\"chartWrap\">");
            sb.AppendLine(BuildChart(report.DailyRows.Select(r => r.CumulativePnl).ToList()));
            sb.AppendLine("</div></div>");

            sb.AppendLine("<div class=\"section\"><h2>Daily Performance</h2><table class=\"table\"><thead><tr><th>Date</th><th>P&amp;L giorno</th><th>Daily Return</th></tr></thead><tbody>");
            foreach (var row in report.DailyRows)
            {
                sb.AppendLine($"<tr><td>{row.Date.ToString("dd MMMM yyyy", culture)}</td><td class=\"ledgerAmount {Tone(row.NetPnl)}\">{FormatEuro(row.NetPnl)}</td><td class=\"mono {Tone(row.DailyReturnPct)}\">{FormatPercent(row.DailyReturnPct)}</td></tr>");
            }
            sb.AppendLine($"<tr><td><b>Risultato periodo</b></td><td class=\"ledgerAmount {periodTone}\"><b>{FormatEuro(periodResult)}</b></td><td class=\"mono {periodTone}\"><b>{FormatPercent(report.Totals.PeriodReturnPct)}</b></td></tr>");
            sb.AppendLine("</tbody></table></div>");
        }
        else
        {
            sb.AppendLine("<div class=\"section\"><h2>Daily Performance</h2><div class=\"sectionSub\">Nessun dato contabile missione per il periodo selezionato.</div></div>");
        }

        if (report.Sessions.Count > 0)
        {
            sb.AppendLine("<div class=\"section\"><h2>Mission Sessions</h2><div class=\"sectionSub\">Missioni con Start (Europe/Rome) nel periodo selezionato e almeno un sample clippato nella finestra. Margine PBT a chiusura = livello assoluto del margine al termine della finestra; non è il profitto del periodo.</div><table class=\"table\"><thead><tr><th>Session</th><th>Start (Europe/Rome)</th><th>End (Europe/Rome)</th><th>Runtime</th><th>P&amp;L periodo</th><th>Margine PBT a chiusura</th><th>Real Hands</th><th>Tables</th></tr></thead><tbody>");
            foreach (var session in report.Sessions)
            {
                sb.AppendLine($"<tr><td>#{session.SessionId}</td><td>{FormatRomeDateTime(session.StartTime, culture)}</td><td>{(session.EndTime.HasValue ? FormatRomeDateTime(session.EndTime.Value, culture) : "-")}</td><td>{Html(session.RuntimeMode)}</td><td class=\"ledgerAmount {Tone(session.TotalMarginEuro)}\">{FormatEuro(session.TotalMarginEuro)}</td><td class=\"ledgerAmount neutral\">{FormatEuro(session.FinalMarginEuro)}</td><td>{session.RealHandsCount.ToString(CultureInfo.InvariantCulture)}</td><td>{session.ActiveTables.ToString(CultureInfo.InvariantCulture)}</td></tr>");
            }
            sb.AppendLine("</tbody></table></div>");
        }

        sb.AppendLine("<div class=\"footer\"><div class=\"footerMark\">▲</div><div class=\"footerNumbers\">١ ٣ ٧ ١٥</div><div class=\"footerBrand\">EuGenio Lab — Ingegneria del Trading.</div><div class=\"footerMeta\">Generated automatically by EuGenio Trading Dashboard</div><div class=\"footerMeta\">Annualised return is calculated from observed period performance and does not represent guaranteed future performance.</div></div>");
        sb.AppendLine("</div></div></body></html>");
        return sb.ToString();
    }

    private static void AddCard(StringBuilder sb, string label, string value, string tone, bool focus = false)
    {
        sb.AppendLine($"<div class=\"card{(focus ? " focus" : "")}\"><div class=\"k\">{Html(label)}</div><div class=\"v {tone}\">{Html(value)}</div></div>");
    }

    private static string BuildChart(IReadOnlyList<decimal> values)
    {
        if (values.Count == 0) return "<div class=\"sectionSub\">No chart data.</div>";
        var min = Math.Min(0m, values.Min());
        var max = Math.Max(1m, values.Max());
        var span = Math.Max(1m, max - min);
        var maxIndex = Math.Max(1, values.Count - 1);
        var points = values.Select((value, index) =>
        {
            var x = 10m + (index / (decimal)maxIndex) * 680m;
            var y = 160m - ((value - min) / span) * 130m;
            return $"{(index == 0 ? "M" : "L")}{x.ToString("0.0", CultureInfo.InvariantCulture)} {y.ToString("0.0", CultureInfo.InvariantCulture)}";
        });

        return $"<svg class=\"chart\" viewBox=\"0 0 700 180\" role=\"img\" aria-label=\"Period result curve\"><line class=\"axis\" x1=\"10\" y1=\"160\" x2=\"690\" y2=\"160\"/><path class=\"curve\" d=\"{string.Join(" ", points)}\"/></svg>";
    }

    private static string FormatEuro(decimal value)
    {
        var sign = value > 0 ? "+" : value < 0 ? "-" : "";
        return $"{sign}{Math.Abs(value).ToString("0.00", CultureInfo.InvariantCulture)} €";
    }

    private static string FormatPercent(decimal value)
    {
        var sign = value > 0 ? "+" : value < 0 ? "-" : "";
        return $"{sign}{Math.Abs(value).ToString("0.00", CultureInfo.InvariantCulture)}%";
    }

    private static string FormatAnnualisedReturn(decimal? value)
        => value.HasValue ? FormatPercent(value.Value) : "N/D";

    private static string FormatAnnualisedTone(decimal? value)
        => value.HasValue ? Tone(value.Value) : "neutral";

    private static string FormatRomeDateTime(DateTime timestamp, CultureInfo culture)
    {
        var utc = timestamp.Kind switch
        {
            DateTimeKind.Utc => timestamp,
            DateTimeKind.Local => timestamp.ToUniversalTime(),
            _ => DateTime.SpecifyKind(timestamp, DateTimeKind.Utc)
        };
        var rome = TimeZoneInfo.ConvertTimeFromUtc(utc, ResolveRomeTimeZone());
        return rome.ToString("dd MMMM yyyy HH:mm", culture);
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
