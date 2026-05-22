using System.Drawing;

namespace Gamebot.Models.Roulette
{
    internal class BtnFiches
    {
        // (get) Token: 0x06000155 RID: 341 RVA: 0x0001C0AB File Offset: 0x0001A2AB
        // (set) Token: 0x06000156 RID: 342 RVA: 0x0001C0B3 File Offset: 0x0001A2B3
        public int Value { get; set; }

        // (get) Token: 0x06000157 RID: 343 RVA: 0x0001C0BC File Offset: 0x0001A2BC
        // (set) Token: 0x06000158 RID: 344 RVA: 0x0001C0C4 File Offset: 0x0001A2C4
        public Color BackCurrentColor { get; set; }

        // (get) Token: 0x06000159 RID: 345 RVA: 0x0001C0CD File Offset: 0x0001A2CD
        // (set) Token: 0x0600015A RID: 346 RVA: 0x0001C0D5 File Offset: 0x0001A2D5
        public Color FrontCurrentColor { get; set; }

        // (get) Token: 0x0600015B RID: 347 RVA: 0x0001C0DE File Offset: 0x0001A2DE
        // (set) Token: 0x0600015C RID: 348 RVA: 0x0001C0E6 File Offset: 0x0001A2E6
        public Color BorderCurrentColor { get; set; }

        // (get) Token: 0x0600015D RID: 349 RVA: 0x0001C0EF File Offset: 0x0001A2EF
        // (set) Token: 0x0600015E RID: 350 RVA: 0x0001C0F7 File Offset: 0x0001A2F7
        public bool Removed { get; set; }

        public BtnFiches(int value, Color backCurrentColor, Color frontCurrentColor, Color borderCurrentColor)
        {
            this.Value = value;
            this.BackCurrentColor = backCurrentColor;
            this.FrontCurrentColor = frontCurrentColor;
            this.BorderCurrentColor = borderCurrentColor;
        }
    }
}
