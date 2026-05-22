using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Gamebot.Models.Entity
{
    internal class JSONConfig2
    {
        // (get) Token: 0x06000268 RID: 616 RVA: 0x0001EE4D File Offset: 0x0001D04D
        // (set) Token: 0x06000269 RID: 617 RVA: 0x0001EE55 File Offset: 0x0001D055
        [JsonPropertyName("user")]
        public string User { get; set; }

        // (get) Token: 0x0600026A RID: 618 RVA: 0x0001EE5E File Offset: 0x0001D05E
        // (set) Token: 0x0600026B RID: 619 RVA: 0x0001EE66 File Offset: 0x0001D066
        [JsonPropertyName("configs")]
        public List<JSONSingleConfig2> Configs { get; set; }
    }
}
