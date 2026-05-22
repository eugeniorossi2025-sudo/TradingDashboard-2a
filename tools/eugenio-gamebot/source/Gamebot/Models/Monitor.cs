using Gamebot.Helpers;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Gamebot.Models
{
    internal class Monitor
    {
        private Monitor()
        {
        }

        // (get) Token: 0x060000BE RID: 190 RVA: 0x00017564 File Offset: 0x00015764
        public static Monitor Instance
        {
            get
            {
                if (Monitor.instance == null)
                {
                    Monitor.instance = new Monitor();
                }
                return Monitor.instance;
            }
        }

        public Bitmap CaptureScreen(Rectangle area)
        {
            Bitmap bitmap = new Bitmap(area.Width, area.Height, PixelFormat.Format24bppRgb);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(area.Left, area.Top, 0, 0, area.Size);
            }
            return bitmap;
        }

        public Bitmap IncreaseResolution(Bitmap inputBitmap, int scaleFactor)
        {
            int newWidth = inputBitmap.Width * scaleFactor;
            int newHeight = inputBitmap.Height * scaleFactor;
            Bitmap outputBitmap = new Bitmap(newWidth, newHeight);
            try
            {
                using (Graphics g = Graphics.FromImage(outputBitmap))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(inputBitmap, new Rectangle(0, 0, newWidth, newHeight), new Rectangle(0, 0, inputBitmap.Width, inputBitmap.Height), GraphicsUnit.Pixel);
                }
            }
            catch (Exception ex)
            {
                Log.PrintErrorLog("OCRScan", ex.Message, "");
            }
            return outputBitmap;
        }

        private static Monitor instance;
    }
}
