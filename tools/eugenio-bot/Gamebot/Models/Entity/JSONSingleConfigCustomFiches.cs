using System.Text.Json.Serialization;

namespace Gamebot.Models.Entity
{
    public class JSONSingleConfigCustomFiches
    {
        [JsonPropertyName("configBacarat")]
        public JSONSingleConfigBacaratCustomFiches ConfigBacarat { get; set; }

        [JsonPropertyName("configTelegram")]
        public JSONSingleConfigTelegram ConfigTelegram { get; set; }
    }
}
