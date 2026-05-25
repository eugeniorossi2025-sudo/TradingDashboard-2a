namespace WebApi.Options;

/// <summary>
/// Collaudo mirror settings (PC96 → Pc_CurrentStatus on dashboard DB).
/// Secret from env <c>Collaudo__MirrorSecret</c> or GitHub secret COLLAUDO_MIRROR_SECRET on IIS.
/// </summary>
public class CollaudoOptions
{
    public const string SectionName = "Collaudo";

    /// <summary>Shared secret for mirror and verify endpoints. Never commit production values.</summary>
    public string MirrorSecret { get; set; } = string.Empty;
}
