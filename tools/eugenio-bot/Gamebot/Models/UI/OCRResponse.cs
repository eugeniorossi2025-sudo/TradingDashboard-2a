namespace Gamebot.Models.UI
{
    public class OCRResponse
    {
        // (get) Token: 0x060000F8 RID: 248 RVA: 0x00019A9B File Offset: 0x00017C9B
        // (set) Token: 0x060000F9 RID: 249 RVA: 0x00019AA3 File Offset: 0x00017CA3
        public bool SuccessScan { get; set; }
        
        public double Similarity { get; set; }

        // (get) Token: 0x060000FA RID: 250 RVA: 0x00019AAC File Offset: 0x00017CAC
        // (set) Token: 0x060000FB RID: 251 RVA: 0x00019AB4 File Offset: 0x00017CB4
        public string Message { get; set; }

        private OCRResponse()
        {
        }

        // (get) Token: 0x060000FD RID: 253 RVA: 0x00019AC5 File Offset: 0x00017CC5
        public static OCRResponse Instance
        {
            get
            {
                if (OCRResponse.instance == null)
                {
                    OCRResponse.instance = new OCRResponse();
                }
                return OCRResponse.instance;
            }
        }

        public void SetResponse(bool success, string message)
        {
            this.SuccessScan = success;
            this.Message = message;
        }

        public OCRResponse GetResponse()
        {
            return this;
        }

        private static OCRResponse instance;
    }
}
