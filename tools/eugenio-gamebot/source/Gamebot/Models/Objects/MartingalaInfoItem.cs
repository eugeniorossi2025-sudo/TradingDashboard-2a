using System.Text.Json.Serialization;

namespace Gamebot.Models.Objects
{
    public class MartingalaInfoItem
    {
        // (get) Token: 0x060002C0 RID: 704 RVA: 0x0001F8B0 File Offset: 0x0001DAB0
        // (set) Token: 0x060002C1 RID: 705 RVA: 0x0001F8B8 File Offset: 0x0001DAB8
        [JsonPropertyName("startDeck")]
        public int StartDeck { get; set; }

        // (get) Token: 0x060002C2 RID: 706 RVA: 0x0001F8C1 File Offset: 0x0001DAC1
        // (set) Token: 0x060002C3 RID: 707 RVA: 0x0001F8C9 File Offset: 0x0001DAC9
        [JsonPropertyName("endDeck")]
        public int EndDeck { get; set; }

        // (get) Token: 0x060002C4 RID: 708 RVA: 0x0001F8D2 File Offset: 0x0001DAD2
        // (set) Token: 0x060002C5 RID: 709 RVA: 0x0001F8DA File Offset: 0x0001DADA
        [JsonPropertyName("changeIndex")]
        public int ChangeIndex { get; set; }

        // (get) Token: 0x060002C6 RID: 710 RVA: 0x0001F8E3 File Offset: 0x0001DAE3
        // (set) Token: 0x060002C7 RID: 711 RVA: 0x0001F8EB File Offset: 0x0001DAEB
        [JsonPropertyName("alarmIndex")]
        public int AlarmIndex { get; set; }

        // (get) Token: 0x060002C8 RID: 712 RVA: 0x0001F8F4 File Offset: 0x0001DAF4
        // (set) Token: 0x060002C9 RID: 713 RVA: 0x0001F8FC File Offset: 0x0001DAFC
        [JsonPropertyName("order")]
        public int Order { get; set; }
    }
}
