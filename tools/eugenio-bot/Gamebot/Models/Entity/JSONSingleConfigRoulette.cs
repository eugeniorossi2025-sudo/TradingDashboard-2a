using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Gamebot.Models.Entity
{
    public class JSONSingleConfigRoulette
    {
        // (get) Token: 0x06000272 RID: 626 RVA: 0x0001EEA1 File Offset: 0x0001D0A1
        // (set) Token: 0x06000273 RID: 627 RVA: 0x0001EEA9 File Offset: 0x0001D0A9
        [JsonPropertyName("rouletteStopWin")]
        public decimal RouletteStopWin { get; set; }

        // (get) Token: 0x06000274 RID: 628 RVA: 0x0001EEB2 File Offset: 0x0001D0B2
        // (set) Token: 0x06000275 RID: 629 RVA: 0x0001EEBA File Offset: 0x0001D0BA
        [JsonPropertyName("rouletteStopLoss")]
        public decimal RouletteStopLoss { get; set; }

        // (get) Token: 0x06000276 RID: 630 RVA: 0x0001EEC3 File Offset: 0x0001D0C3
        // (set) Token: 0x06000277 RID: 631 RVA: 0x0001EECB File Offset: 0x0001D0CB
        [JsonPropertyName("rouletteHandArea1")]
        public AreaElementConfig RouletteHandArea1 { get; set; }

        // (get) Token: 0x06000278 RID: 632 RVA: 0x0001EED4 File Offset: 0x0001D0D4
        // (set) Token: 0x06000279 RID: 633 RVA: 0x0001EEDC File Offset: 0x0001D0DC
        [JsonPropertyName("rouletteHandArea2")]
        public AreaElementConfig RouletteHandArea2 { get; set; }

        // (get) Token: 0x0600027A RID: 634 RVA: 0x0001EEE5 File Offset: 0x0001D0E5
        // (set) Token: 0x0600027B RID: 635 RVA: 0x0001EEED File Offset: 0x0001D0ED
        [JsonPropertyName("rouletteHandArea3")]
        public AreaElementConfig RouletteHandArea3 { get; set; }

        // (get) Token: 0x0600027C RID: 636 RVA: 0x0001EEF6 File Offset: 0x0001D0F6
        // (set) Token: 0x0600027D RID: 637 RVA: 0x0001EEFE File Offset: 0x0001D0FE
        [JsonPropertyName("rouletteWinArea")]
        public AreaElementConfig RouletteWinArea { get; set; }

        // (get) Token: 0x0600027E RID: 638 RVA: 0x0001EF07 File Offset: 0x0001D107
        // (set) Token: 0x0600027F RID: 639 RVA: 0x0001EF0F File Offset: 0x0001D10F
        [JsonPropertyName("rouletteWaitingArea")]
        public AreaElementConfig RouletteWaitingArea { get; set; }

        // (get) Token: 0x06000280 RID: 640 RVA: 0x0001EF18 File Offset: 0x0001D118
        // (set) Token: 0x06000281 RID: 641 RVA: 0x0001EF20 File Offset: 0x0001D120
        [JsonPropertyName("rouletteAreaSaldo")]
        public AreaElementConfig RouletteAreaSaldo { get; set; }

        // (get) Token: 0x06000282 RID: 642 RVA: 0x0001EF29 File Offset: 0x0001D129
        // (set) Token: 0x06000283 RID: 643 RVA: 0x0001EF31 File Offset: 0x0001D131
        [JsonPropertyName("rouletteValueHand1")]
        public decimal RouletteValueHand1 { get; set; }

        // (get) Token: 0x06000284 RID: 644 RVA: 0x0001EF3A File Offset: 0x0001D13A
        // (set) Token: 0x06000285 RID: 645 RVA: 0x0001EF42 File Offset: 0x0001D142
        [JsonPropertyName("rouletteValueHand2")]
        public decimal RouletteValueHand2 { get; set; }

        // (get) Token: 0x06000286 RID: 646 RVA: 0x0001EF4B File Offset: 0x0001D14B
        // (set) Token: 0x06000287 RID: 647 RVA: 0x0001EF53 File Offset: 0x0001D153
        [JsonPropertyName("rouletteValueHand3")]
        public decimal RouletteValueHand3 { get; set; }

        // (get) Token: 0x06000288 RID: 648 RVA: 0x0001EF5C File Offset: 0x0001D15C
        // (set) Token: 0x06000289 RID: 649 RVA: 0x0001EF64 File Offset: 0x0001D164
        [JsonPropertyName("rouletteHand1Numbers")]
        public List<int> RouletteHand1Numbers { get; set; }

        // (get) Token: 0x0600028A RID: 650 RVA: 0x0001EF6D File Offset: 0x0001D16D
        // (set) Token: 0x0600028B RID: 651 RVA: 0x0001EF75 File Offset: 0x0001D175
        [JsonPropertyName("rouletteHand2Numbers")]
        public List<int> RouletteHand2Numbers { get; set; }

        // (get) Token: 0x0600028C RID: 652 RVA: 0x0001EF7E File Offset: 0x0001D17E
        // (set) Token: 0x0600028D RID: 653 RVA: 0x0001EF86 File Offset: 0x0001D186
        [JsonPropertyName("rouletteHand3Numbers")]
        public List<int> RouletteHand3Numbers { get; set; }

        // (get) Token: 0x0600028E RID: 654 RVA: 0x0001EF8F File Offset: 0x0001D18F
        // (set) Token: 0x0600028F RID: 655 RVA: 0x0001EF97 File Offset: 0x0001D197
        [JsonPropertyName("zoom")]
        public string Zoom { get; set; }
    }
}
