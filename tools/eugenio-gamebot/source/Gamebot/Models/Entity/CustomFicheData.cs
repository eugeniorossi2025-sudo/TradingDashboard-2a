using System.Text.Json.Serialization;

namespace Gamebot.Models.Entity
{
    public class CustomFicheData
    {
        // (get) Token: 0x060002A2 RID: 674 RVA: 0x0001F066 File Offset: 0x0001D266
        // (set) Token: 0x060002A3 RID: 675 RVA: 0x0001F06E File Offset: 0x0001D26E
        [JsonPropertyName("value")]
        public double value { get; set; }

        // (get) Token: 0x060002A4 RID: 676 RVA: 0x0001F077 File Offset: 0x0001D277
        // (set) Token: 0x060002A5 RID: 677 RVA: 0x0001F07F File Offset: 0x0001D27F
        [JsonPropertyName("label")]
        public string label { get; set; } = string.Empty;

        // (get) Token: 0x060002A6 RID: 678 RVA: 0x0001F088 File Offset: 0x0001D288
        // (set) Token: 0x060002A7 RID: 679 RVA: 0x0001F090 File Offset: 0x0001D290
        [JsonPropertyName("tag")]
        public string tag { get; set; } = string.Empty;

        // (get) Token: 0x060002A8 RID: 680 RVA: 0x0001F099 File Offset: 0x0001D299
        // (set) Token: 0x060002A9 RID: 681 RVA: 0x0001F0A1 File Offset: 0x0001D2A1
        [JsonPropertyName("startX")]
        public int startX { get; set; }

        // (get) Token: 0x060002AA RID: 682 RVA: 0x0001F0AA File Offset: 0x0001D2AA
        // (set) Token: 0x060002AB RID: 683 RVA: 0x0001F0B2 File Offset: 0x0001D2B2
        [JsonPropertyName("endX")]
        public int endX { get; set; }

        // (get) Token: 0x060002AC RID: 684 RVA: 0x0001F0BB File Offset: 0x0001D2BB
        // (set) Token: 0x060002AD RID: 685 RVA: 0x0001F0C3 File Offset: 0x0001D2C3
        [JsonPropertyName("startY")]
        public int startY { get; set; }

        // (get) Token: 0x060002AE RID: 686 RVA: 0x0001F0CC File Offset: 0x0001D2CC
        // (set) Token: 0x060002AF RID: 687 RVA: 0x0001F0D4 File Offset: 0x0001D2D4
        [JsonPropertyName("endY")]
        public int endY { get; set; }
    }
}
