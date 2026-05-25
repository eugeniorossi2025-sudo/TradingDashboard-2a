namespace WebApi.Options;

/// <summary>
/// Decider connectivity for diagnostics only (GET /api/decider/config, GET /api/decider/health).
/// Does not feed dashboard data; Pc_CurrentStatus and missions remain on the WebApi local DB.
/// </summary>
public class DeciderOptions
{
    public const string SectionName = "Decider";

    public bool Enabled { get; set; }

    public string Mode { get; set; } = "Remote";

    public string BaseUrl { get; set; } = "http://51.210.181.37";

    public string ApiBasePath { get; set; } = "/api/proactive";

    public string ApiBaseUrl => BaseUrl.TrimEnd('/');

    public string ProactiveUrl(string relativePath) =>
        $"{ApiBaseUrl}{ApiBasePath.TrimEnd('/')}/{relativePath.TrimStart('/')}";
}
