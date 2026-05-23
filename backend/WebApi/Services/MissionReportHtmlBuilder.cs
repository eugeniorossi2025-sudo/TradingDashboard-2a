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
        var generated = report.GeneratedAt.ToLocalTime().ToString("HH:mm", culture);
        var period = $"{report.From:dd MMMM yyyy} - {report.To:dd MMMM yyyy}";
        var subtitle = $"Reporting period: {period} • Strategy mode: {Html(report.RuntimeMode)} • Generated at {generated}";

        var sb = new StringBuilder();
        sb.AppendLine("<!doctype html><html lang=\"it\"><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width,initial-scale=1\">");
        sb.AppendLine($"<title>{Html(title)}</title>");
        sb.AppendLine("""
<style>
:root{color-scheme:light;--ink:#101828;--muted:#667085;--line:#d0d5dd;--soft:#f8fafc;--ok:#057a55;--bad:#b42318;--accent:#111827}
*{box-sizing:border-box}body{margin:0;background:#eef2f7;color:var(--ink);font-family:Inter,Segoe UI,Arial,sans-serif}
.container{max-width:980px;margin:0 auto;padding:28px}.paper{background:#fff;border:1px solid var(--line);padding:34px}
.topRule{height:6px;background:#111827;margin:-34px -34px 26px}.header{display:flex;justify-content:space-between;gap:20px;align-items:flex-start}
.brandKicker{font-size:12px;text-transform:uppercase;letter-spacing:.16em;color:var(--muted);font-weight:800}.h1{font-size:28px;font-weight:850;margin-top:4px}.sub{margin-top:8px;color:var(--muted);font-size:13px}
.seal{border:1px solid var(--line);border-radius:999px;padding:10px 14px;font-size:12px;font-weight:800;white-space:nowrap}.actions{margin-top:16px}.btn{border:1px solid #111827;background:#111827;color:#fff;border-radius:999px;padding:10px 15px;font-weight:800;cursor:pointer}
.hero{margin:26px 0;padding:18px;border:1px solid var(--line);background:var(--soft)}.heroTitle{font-size:16px;font-weight:850}.heroText{color:var(--muted);line-height:1.45}
.summaryGrid{display:grid;grid-template-columns:repeat(4,1fr);gap:12px}.card{border:1px solid var(--line);padding:14px;min-height:92px}.k{font-size:11px;color:var(--muted);text-transform:uppercase;letter-spacing:.12em;font-weight:800}.v{font-size:22px;font-weight:850;margin-top:10px}.v.small{font-size:14px}.focus{background:#f9fafb}
.pos{color:var(--ok)}.neg{color:var(--bad)}.neutral{color:var(--ink)}.methodNote,.sectionSub,.footerMeta{color:var(--muted);font-size:12px;line-height:1.45}.section{margin-top:28px}h2{font-size:18px;margin:0 0 8px}
.chartWrap{border:1px solid var(--line);padding:12px}.chart{width:100%;height:190px}.axis{stroke:#d0d5dd}.curve{fill:none;stroke:#111827;stroke-width:3;stroke-linecap:round;stroke-linejoin:round}
.table{width:100%;border-collapse:collapse;border:1px solid var(--line)}.table th,.table td{padding:10px;border-bottom:1px solid var(--line);text-align:left}.table th{font-size:11px;text-transform:uppercase;letter-spacing:.12em;color:var(--muted)}.ledgerAmount{text-align:right}.mono{font-variant-numeric:tabular-nums}
.footer{margin-top:34px;text-align:center;border-top:1px solid var(--line);padding-top:18px}.footerMark{font-size:22px}.footerNumbers{font-size:20px;letter-spacing:.28em}.footerBrand{font-weight:850;margin-top:6px}
@media(max-width:760px){.container{padding:0}.paper{border:0;padding:22px}.topRule{margin:-22px -22px 22px}.header{display:block}.seal{display:inline-flex;margin-top:14px}.summaryGrid{grid-template-columns:repeat(2,1fr)}}
@media print{body{background:#fff}.container{padding:0;max-width:none}.paper{border:0}.noprint{display:none!important}.card,.table,.chartWrap{break-inside:avoid}}
</style>
""");
        sb.AppendLine("</head><body><div class=\"container\"><div class=\"paper\"><div class=\"topRule\"></div>");
        sb.AppendLine("<div class=\"header\"><div><div class=\"brandKicker\">Eugenio Trading</div>");
        sb.AppendLine($"<div class=\"h1\">{Html(title)}</div><div class=\"sub\">{subtitle}</div></div>");
        sb.AppendLine($"<div><div class=\"seal\">{Html(statement)}</div><div class=\"actions noprint\"><button class=\"btn\" onclick=\"window.print()\">Print / Export PDF</button></div></div></div>");
        sb.AppendLine($"<div class=\"hero\"><div class=\"heroTitle\">{Html(statement)}</div><p class=\"heroText\">{Html(statement)} for the reporting period {Html(period)}.</p></div>");

        var q = report.QualityMetrics;
        sb.AppendLine("<div class=\"summaryGrid\">");
        AddCard(sb, "Invested Capital", "€ •••••••", "neutral");
        AddCard(sb, "Net P&L", FormatEuro(report.Totals.TotalMarginEuro), Tone(report.Totals.TotalMarginEuro), true);
        AddCard(sb, "Period Return", FormatPercent(report.Totals.PeriodReturnPct), Tone(report.Totals.PeriodReturnPct));
        AddCard(sb, "Annualised Return", FormatPercent(report.Totals.AnnualisedReturnPct), Tone(report.Totals.AnnualisedReturnPct));
        AddCard(sb, "Average Daily P&L", FormatEuro(report.Totals.AverageDailyPnl), Tone(report.Totals.AverageDailyPnl));
        AddCard(sb, "Average Daily Return", FormatPercent(report.Totals.AverageDailyReturnPct), Tone(report.Totals.AverageDailyReturnPct));
        AddCard(sb, "Working Days", report.Totals.WorkingDays.ToString(CultureInfo.InvariantCulture), "neutral");
        AddCard(sb, "Reporting Days", report.Totals.ReportingDays.ToString(CultureInfo.InvariantCulture), "neutral");
        sb.AppendLine("</div>");
        sb.AppendLine("<p class=\"methodNote\">Invested capital is withheld for privacy. Performance ratios are calculated on the configured capital base. Annualised return is calculated from the observed period performance and does not represent a guaranteed future return.</p>");

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
            sb.AppendLine("<div class=\"section\"><h2>Net P&L Curve</h2><div class=\"chartWrap\">");
            sb.AppendLine(BuildChart(report.DailyRows.Select(r => r.CumulativePnl).ToList()));
            sb.AppendLine("</div></div>");

            sb.AppendLine("<div class=\"section\"><h2>Daily Performance</h2><table class=\"table\"><thead><tr><th>Date</th><th>Net P&amp;L</th><th>Daily Return</th></tr></thead><tbody>");
            foreach (var row in report.DailyRows)
            {
                sb.AppendLine($"<tr><td>{row.Date.ToString("dd MMMM yyyy", culture)}</td><td class=\"ledgerAmount {Tone(row.NetPnl)}\">{FormatEuro(row.NetPnl)}</td><td class=\"mono {Tone(row.DailyReturnPct)}\">{FormatPercent(row.DailyReturnPct)}</td></tr>");
            }
            sb.AppendLine($"<tr><td><b>Total Period</b></td><td class=\"ledgerAmount {Tone(report.Totals.TotalMarginEuro)}\"><b>{FormatEuro(report.Totals.TotalMarginEuro)}</b></td><td class=\"mono {Tone(report.Totals.PeriodReturnPct)}\"><b>{FormatPercent(report.Totals.PeriodReturnPct)}</b></td></tr>");
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

        return $"<svg class=\"chart\" viewBox=\"0 0 700 180\" role=\"img\" aria-label=\"Net P and L curve\"><line class=\"axis\" x1=\"10\" y1=\"160\" x2=\"690\" y2=\"160\"/><path class=\"curve\" d=\"{string.Join(" ", points)}\"/></svg>";
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

    private static string Tone(decimal value) => value > 0 ? "pos" : value < 0 ? "neg" : "neutral";

    private static string Html(string value) => WebUtility.HtmlEncode(value);
}
