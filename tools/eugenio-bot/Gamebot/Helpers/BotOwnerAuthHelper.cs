using System;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Gamebot.Models;
using Gamebot.UI.WindowForm;

namespace Gamebot.Helpers
{
    public enum BotOwnerAuthResult
    {
        Ok,
        Locked,
        Unauthorized,
        Unreachable
    }

    /// <summary>
    /// Single startup check against Dashboard DEV POST /api/bot-owner-auth/check.
    /// No polling; call only before Player.Instance.Start().
    /// </summary>
    public static class BotOwnerAuthHelper
    {
        public const string BlockedLabel = "Stato Bot: FERMATO DAL PADRONE";

        public static async Task<BotOwnerAuthResult> CheckStartupAsync()
        {
            var baseUrl = (ConfigurationManager.AppSettings["BotOwner.Url"] ?? string.Empty).Trim().TrimEnd('/');
            var userId = (ConfigurationManager.AppSettings["BotOwner.UserId"] ?? string.Empty).Trim();
            var password = ConfigurationManager.AppSettings["BotOwner.Password"] ?? string.Empty;

            if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(password))
                return BotOwnerAuthResult.Unreachable;

            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(12) };
                var uri = baseUrl + "/api/bot-owner-auth/check";
                var payload = JsonSerializer.Serialize(new { userId, password });
                using var content = new StringContent(payload, Encoding.UTF8, "application/json");
                using var response = await client.PostAsync(uri, content).ConfigureAwait(false);
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return ParseResponse((int)response.StatusCode, body);
            }
            catch
            {
                return BotOwnerAuthResult.Unreachable;
            }
        }

        public static BotOwnerAuthResult ParseResponse(int statusCode, string body)
        {
            if (!string.IsNullOrWhiteSpace(body))
            {
                try
                {
                    using var doc = JsonDocument.Parse(body);
                    if (doc.RootElement.TryGetProperty("status", out var statusEl))
                    {
                        var status = statusEl.GetString()?.Trim().ToUpperInvariant();
                        if (status == "OK")
                            return BotOwnerAuthResult.Ok;
                        if (status == "LOCKED")
                            return BotOwnerAuthResult.Locked;
                        if (status == "UNAUTHORIZED")
                            return BotOwnerAuthResult.Unauthorized;
                    }
                }
                catch
                {
                    // fall through to status-code mapping
                }
            }

            if (statusCode == 401)
                return BotOwnerAuthResult.Unauthorized;

            return BotOwnerAuthResult.Unreachable;
        }

        public static void ApplyBlockedUi(Configuratore form)
        {
            Runtime.labelTextCurrentState = BlockedLabel;
            if (form != null && !form.IsDisposed)
                form.labelStatus.Text = BlockedLabel;
        }
    }
}
