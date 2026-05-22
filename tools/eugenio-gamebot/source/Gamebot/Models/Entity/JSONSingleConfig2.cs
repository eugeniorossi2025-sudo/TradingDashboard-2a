using System.Text.Json.Serialization;

namespace Gamebot.Models.Entity
{
    public class JSONSingleConfig2
    {
        // (get) Token: 0x0600026D RID: 621 RVA: 0x0001EE77 File Offset: 0x0001D077
        // (set) Token: 0x0600026E RID: 622 RVA: 0x0001EE7F File Offset: 0x0001D07F
        [JsonPropertyName("configRoulette")]
        public JSONSingleConfigRoulette ConfigRoulette { get; set; }

        // (get) Token: 0x0600026F RID: 623 RVA: 0x0001EE88 File Offset: 0x0001D088
        // (set) Token: 0x06000270 RID: 624 RVA: 0x0001EE90 File Offset: 0x0001D090
        [JsonPropertyName("configTelegram")]
        public JSONSingleConfigTelegram ConfigTelegram { get; set; }
    }
}
