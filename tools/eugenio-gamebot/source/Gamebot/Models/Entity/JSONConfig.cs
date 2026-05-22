using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Gamebot.Models.Entity
{
    internal class JSONConfig
    {
        // (get) Token: 0x060001D6 RID: 470 RVA: 0x0001E91D File Offset: 0x0001CB1D
        // (set) Token: 0x060001D7 RID: 471 RVA: 0x0001E925 File Offset: 0x0001CB25
        [JsonPropertyName("user")]
        public string User { get; set; }

        // (get) Token: 0x060001D8 RID: 472 RVA: 0x0001E92E File Offset: 0x0001CB2E
        // (set) Token: 0x060001D9 RID: 473 RVA: 0x0001E936 File Offset: 0x0001CB36
        [JsonPropertyName("configs")]
        public List<JSONSingleConfig> Configs { get; set; }
    }
}
