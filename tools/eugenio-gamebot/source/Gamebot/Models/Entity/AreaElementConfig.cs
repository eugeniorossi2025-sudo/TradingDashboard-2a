using Gamebot.Models.UI;
using System.Text.Json.Serialization;

namespace Gamebot.Models.Entity
{
    public class AreaElementConfig
    {
        // (get) Token: 0x06000298 RID: 664 RVA: 0x0001EFE3 File Offset: 0x0001D1E3
        // (set) Token: 0x06000299 RID: 665 RVA: 0x0001EFEB File Offset: 0x0001D1EB
        [JsonPropertyName("startX")]
        public int startX { get; set; }

        // (get) Token: 0x0600029A RID: 666 RVA: 0x0001EFF4 File Offset: 0x0001D1F4
        // (set) Token: 0x0600029B RID: 667 RVA: 0x0001EFFC File Offset: 0x0001D1FC
        [JsonPropertyName("endX")]
        public int endX { get; set; }

        // (get) Token: 0x0600029C RID: 668 RVA: 0x0001F005 File Offset: 0x0001D205
        // (set) Token: 0x0600029D RID: 669 RVA: 0x0001F00D File Offset: 0x0001D20D
        [JsonPropertyName("startY")]
        public int startY { get; set; }

        // (get) Token: 0x0600029E RID: 670 RVA: 0x0001F016 File Offset: 0x0001D216
        // (set) Token: 0x0600029F RID: 671 RVA: 0x0001F01E File Offset: 0x0001D21E
        [JsonPropertyName("endY")]
        public int endY { get; set; }

        public AreaElement GetArea()
        {
            return new AreaElement
            {
                startX = this.startX,
                endX = this.endX,
                startY = this.startY,
                endY = this.endY
            };
        }
    }
}
