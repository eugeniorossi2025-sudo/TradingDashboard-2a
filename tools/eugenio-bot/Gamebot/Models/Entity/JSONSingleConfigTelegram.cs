using System.Text.Json.Serialization;

namespace Gamebot.Models.Entity
{
    public class JSONSingleConfigTelegram
    {
        // (get) Token: 0x06000291 RID: 657 RVA: 0x0001EFA8 File Offset: 0x0001D1A8
        // (set) Token: 0x06000292 RID: 658 RVA: 0x0001EFB0 File Offset: 0x0001D1B0
        [JsonPropertyName("phoneNumber")]
        public string PhoneNumber { get; set; }

        // (get) Token: 0x06000293 RID: 659 RVA: 0x0001EFB9 File Offset: 0x0001D1B9
        // (set) Token: 0x06000294 RID: 660 RVA: 0x0001EFC1 File Offset: 0x0001D1C1
        [JsonPropertyName("verifiedCode")]
        public string VerifiedCode { get; set; }

        // (get) Token: 0x06000295 RID: 661 RVA: 0x0001EFCA File Offset: 0x0001D1CA
        // (set) Token: 0x06000296 RID: 662 RVA: 0x0001EFD2 File Offset: 0x0001D1D2
        [JsonPropertyName("groupChatName")]
        public string GroupChatName { get; set; }
    }
}
