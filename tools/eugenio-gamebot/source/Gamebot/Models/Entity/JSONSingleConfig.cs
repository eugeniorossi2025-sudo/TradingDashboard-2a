using System.Text.Json.Serialization;

namespace Gamebot.Models.Entity
{
    public class JSONSingleConfig
    {
        // (get) Token: 0x060001E0 RID: 480 RVA: 0x0001E971 File Offset: 0x0001CB71
        // (set) Token: 0x060001E1 RID: 481 RVA: 0x0001E979 File Offset: 0x0001CB79
        [JsonPropertyName("configBacarat")]
        public JSONSingleConfigBacarat ConfigBacarat { get; set; }

        // (get) Token: 0x060001E2 RID: 482 RVA: 0x0001E982 File Offset: 0x0001CB82
        // (set) Token: 0x060001E3 RID: 483 RVA: 0x0001E98A File Offset: 0x0001CB8A
        [JsonPropertyName("configTelegram")]
        public JSONSingleConfigTelegram ConfigTelegram { get; set; }
    }
}
