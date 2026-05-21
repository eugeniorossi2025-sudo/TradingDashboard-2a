using Gamebot.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Gamebot.Models.MouseMove;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Tesseract;
using ImageFormat = System.Drawing.Imaging.ImageFormat;
using Point = OpenCvSharp.Point;
using Rect = OpenCvSharp.Rect;
using Size = System.Drawing.Size;

namespace Gamebot.Models.UI
{
    internal class OCRScan
    {
        private static readonly PixToBitmapConverter pixConverter = new PixToBitmapConverter();

        private static string CurrentPath = "C:\\TEMP\\IMAGES_BOT\\";
        
        private DateTime _lastSaveUtc = DateTime.MinValue;

        private static readonly DigitRecognizer4 deckTemplateRecognizer =
            new DigitRecognizer4(Path.Combine(Constants.PathProject(), "deck_templates/" + Config.directory_numeri_mazzo));

        public static Bitmap CropCenter(Bitmap original, int width, int height)
        {
            if (original == null)
            {
                throw new ArgumentNullException("original");
            }
            int x = (original.Width - width) / 2;
            int y = (original.Height - height) / 2;
            if (x < 0 || y < 0 || width > original.Width || height > original.Height)
            {
                throw new ArgumentException("Il crop è fuori dai limiti dell'immagine originale.");
            }
            Rectangle cropRect = new Rectangle(x, y, width, height);
            return original.Clone(cropRect, original.PixelFormat);
        }

        public OCRResponse GetTextFromBitmapRoulette(Bitmap imgsource)
        {
            OCRResponse ocrResponse = OCRResponse.Instance.GetResponse();
            bool successScan = false;
            string ocrText = string.Empty;
            imgsource = AdjustContrastRoulette(imgsource, 500);
            try
            {
                using (TesseractEngine engine = new TesseractEngine(GetCompletePathTessdataDir(), "ita", EngineMode.Default))
                {
                    using Pix img = PixConverter.ToPix(imgsource);
                    using Page page = engine.Process(img);
                    ocrText = page.GetText();
                }
                successScan = !string.IsNullOrEmpty(ocrText);
            }
            catch (Exception ex)
            {
                Log.PrintErrorLog("(R) OCRScan", ex.Message);
                successScan = false;
                ocrText = string.Empty;
            }
            ocrResponse.SetResponse(successScan, ocrText);
            return ocrResponse;
        }

        public OCRResponse GetTextFromBitmapAreaSaldo(Bitmap imgsource, bool useGrayScale, bool saveImage = false, string prefix = "")
        {
            OCRResponse ocrResponse = OCRResponse.Instance.GetResponse();
            bool successScan = false;
            string ocrText = string.Empty;
            if (saveImage)
            {
                imgsource.Save("C:\\Users\\utente\\Desktop\\IMG_BOT\\file_" + prefix + "_" + DateTime.Now.ToString("HH_mm_ss") + "_prefilter.png");
            }
            imgsource = AdjustContrastAreaSaldo(imgsource);
            imgsource = AdjustContrastAreaSaldoNoSymbol(imgsource);
            if (saveImage)
            {
                imgsource.Save("C:\\Users\\utente\\Desktop\\IMG_BOT\\file_" + prefix + "_" + DateTime.Now.ToString("HH_mm_ss") + "_postfilter_2.png");
            }
            try
            {
                string completePathTessdataDir = GetCompletePathTessdataDir();
                string configFile = completePathTessdataDir + "/tessedit_char_whitelist.config";
                using (TesseractEngine engine = new TesseractEngine(completePathTessdataDir, "ita", EngineMode.Default))
                {
                    engine.SetVariable("tessedit_char_whitelist", "1234567890.,");
                    engine.SetVariable("tessedit_config_file", configFile);
                    using Pix img = PixConverter.ToPix(imgsource);
                    using Page page = engine.Process(img);
                    ocrText = page.GetText();
                    Console.WriteLine($"Area saldo: {ocrText}");
                }
                successScan = !string.IsNullOrEmpty(ocrText);
            }
            catch (Exception ex)
            {
                Log.PrintErrorLog("OCRScan", ex.Message);
                successScan = false;
                ocrText = string.Empty;
            }
            ocrResponse.SetResponse(successScan, ocrText);
            return ocrResponse;
        }

        public OCRResponse GetTextFromBitmapWinAreaDefault(Bitmap imgsource, bool useGrayScale, bool saveImage = false, string prefix = "")
        {
            OCRResponse ocrResponse = OCRResponse.Instance.GetResponse();
            bool successScan = false;
            string ocrText = string.Empty;
            if (!useGrayScale)
            {
                imgsource = AdjustContrast(imgsource, 30f);
                imgsource = Sharpen(imgsource);
                imgsource = Sharpen(imgsource);
            }
            else
            {
                imgsource = AdjustContrastBacarat(imgsource, 60f, 150);
            }
            if (saveImage)
            {
                imgsource.Save(CurrentPath + "file_win_area_" + DateTime.Now.ToString("HH_mm_ss") + ".png");
            }
            try
            {
                using (TesseractEngine engine = new TesseractEngine(GetCompletePathTessdataDir(), "ita", EngineMode.Default))
                {
                    using Pix img = PixConverter.ToPix(imgsource);
                    using Page page = engine.Process(img);
                    ocrText = page.GetText();
                    Console.WriteLine($"Area win: {ocrText}");
                }
                successScan = !string.IsNullOrEmpty(ocrText);
            }
            catch (Exception ex)
            {
                Log.PrintErrorLog("OCRScan", ex.Message);
                successScan = false;
                ocrText = string.Empty;
            }
            ocrResponse.SetResponse(successScan, ocrText);
            return ocrResponse;
        }

        public OCRResponse GetTextFromBitmapWinAreaPragmatic(Bitmap imgsource, bool useGrayScale, bool saveImage = false, string prefix = "")
        {
            OCRResponse ocrResponse = OCRResponse.Instance.GetResponse();
            bool successScan = false;
            string ocrText = string.Empty;
            int countTie = 0;
            int countBank = 0;
            int countPlayer = 0;
            for (int y = 0; y < imgsource.Height; y++)
            {
                for (int x = 0; x < imgsource.Width; x++)
                {
                    Color pixelColor = imgsource.GetPixel(x, y);
                    if (pixelColor.ToArgb() == Config.targetColorTie.ToArgb())
                    {
                        successScan = true;
                        countTie++;
                    }
                    if (pixelColor.ToArgb() == Config.targetColorBank1.ToArgb() || pixelColor.ToArgb() == Config.targetColorBank2.ToArgb() || pixelColor.ToArgb() == Config.targetColorBank3.ToArgb())
                    {
                        successScan = true;
                        countBank++;
                    }
                    if (pixelColor.ToArgb() == Config.targetColorPlayer1.ToArgb() || pixelColor.ToArgb() == Config.targetColorPlayer2.ToArgb() || pixelColor.ToArgb() == Config.targetColorPlayer3.ToArgb())
                    {
                        successScan = true;
                        countPlayer++;
                    }
                }
            }
            if (successScan)
            {
                KeyValuePair<string, int> finalResul = new Dictionary<string, int>
            {
                { "Tie", countTie },
                { "Bank", countBank },
                { "Player", countPlayer }
            }.OrderByDescending((KeyValuePair<string, int> pair) => pair.Value).First();
                if (finalResul.Key == "Bank")
                {
                    ocrText = Config.textAreaBench;
                }
                else if (finalResul.Key == "Player")
                {
                    ocrText = Config.textAreaPlayer;
                }
                else if (finalResul.Key == "Tie")
                {
                    ocrText = Config.textAreaTie;
                }
                Log.PrintInfo("OCR AREA VINCITA PRAGMATIC: " + ocrText);
            }
            ocrResponse.SetResponse(successScan, ocrText);
            Task.Delay(100).GetAwaiter().GetResult();
            return ocrResponse;
        }

        public OCRResponse GetTextFromBitmapBetAreaDefault(Bitmap imgsource, bool useGrayScale, bool saveImage = false, string prefix = "")
        {
            OCRResponse ocrResponse = OCRResponse.Instance.GetResponse();
            bool successScan = false;
            string ocrText = string.Empty;
            if (!useGrayScale)
            {
                imgsource = AdjustContrast(imgsource, 30f);
                imgsource = Sharpen(imgsource);
            }
            else
            {
                imgsource = AdjustContrastBacarat(imgsource, 60f, 150);
            }
            if (saveImage)
            {
                imgsource.Save(CurrentPath + "file_win_area_" + DateTime.Now.ToString("HH_mm_ss") + ".png");
            }
            try
            {
                using (TesseractEngine engine = new TesseractEngine(GetCompletePathTessdataDir(), "ita", EngineMode.Default))
                {
                    using Pix img = PixConverter.ToPix(imgsource);
                    using Page page = engine.Process(img);
                    ocrText = page.GetText();
                    Console.WriteLine($"Area bet: {ocrText}");
                }
                successScan = !string.IsNullOrEmpty(ocrText);
            }
            catch (Exception ex)
            {
                Log.PrintErrorLog("OCRScan", ex.Message);
                successScan = false;
                ocrText = string.Empty;
            }
            ocrResponse.SetResponse(successScan, ocrText);
            return ocrResponse;
        }

        public OCRResponse GetTextFromBitmapBetAreaPragmatic(Bitmap imgsource, bool useGrayScale, bool saveImage = false, string prefix = "")
        {
            int width = imgsource.Width;
            int height = Convert.ToInt16((double)imgsource.Height * 0.8);
            Bitmap imgtarget = new Bitmap(width, height);
            OCRResponse ocrResponse = OCRResponse.Instance.GetResponse();
            bool successScan = false;
            string ocrText = string.Empty;
            imgsource = AdjustContrastBacarat(imgsource, 60f, 200);
            imgsource = Sharpen(imgsource);
            imgsource = CropCenter(imgsource, width / 3, height);
            using (Graphics g = Graphics.FromImage(imgtarget))
            {
                for (int i = 0; i < 3; i++)
                {
                    g.DrawImage(imgsource, i * width / 3, 0);
                }
            }
            if (saveImage)
            {
                imgtarget.Save(CurrentPath + "file_bet_area_" + DateTime.Now.ToString("HH_mm_ss") + ".png");
            }
            try
            {
                using (TesseractEngine engine = new TesseractEngine(GetCompletePathTessdataDir(), "ita", EngineMode.Default))
                {
                    using Pix img = PixConverter.ToPix(imgtarget);
                    using Page page = engine.Process(img);
                    ocrText = page.GetText();
                }
                successScan = !string.IsNullOrEmpty(ocrText);
            }
            catch (Exception ex)
            {
                Log.PrintErrorLog("OCRScan", ex.Message);
                successScan = false;
                ocrText = string.Empty;
            }
            ocrResponse.SetResponse(successScan, ocrText);
            return ocrResponse;
        }
        
        public OCRResponse GetTextFromBitmapNumberDeckDefault(Bitmap imgsource, bool saveImage = false, string prefix = "")
        {
            OCRResponse ocrResponse = OCRResponse.Instance.GetResponse();
            bool successScan = false;
            string ocrText = string.Empty;
            
            try
            {
                ReadNumber resp = deckTemplateRecognizer.Recognize(imgsource);
                int res = resp.number;
                Console.WriteLine($"Numero mazzo: {res}, similarity: {resp.similarity}");
                ocrResponse.SetResponse(true, res.ToString());
                ocrResponse.Similarity = resp.similarity;
                if (resp.similarity <= 0.90)
                {
                    saveImage = true;
                } 
                if (resp.similarity <= 0.40)
                {
                    ocrResponse.SetResponse(false, "-1");
                }
            }
            catch (Exception e)
            {
                ocrResponse.SetResponse(false, "-1");
                ocrResponse.Similarity = 0;
                saveImage = true;
            }
            
            if (saveImage)
            {
                var now = DateTime.UtcNow;
                if (now - _lastSaveUtc >= TimeSpan.FromSeconds(5))
                {
                    _lastSaveUtc = now;
                    try
                    {
                        string imgPath = Path.Combine(Constants.PathProject(),
                            "deck_screenshots/" + Config.directory_numeri_mazzo, "file_number_deck_" + DateTime.Now.ToString("HH_mm_ss") + ".bmp");
                        string directory = Path.GetDirectoryName(imgPath);
                        if (!Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        imgsource.Save(imgPath, ImageFormat.Bmp);
                    }
                    catch (Exception ex)
                    {
                        string a="";
                    }
                }
            }

            if (!ocrResponse.SuccessScan)
            {
                Bets.m.RemoveTimeoutBanner();
            }
            
            imgsource.Dispose();
            return ocrResponse;
            
            imgsource = AdjustContrastBacarat(imgsource, 60f, 150);
            imgsource = new Bitmap(imgsource, new Size(imgsource.Width * 2, imgsource.Height * 2));
            imgsource = Sharpen(imgsource);
            
            try
            {
                using (TesseractEngine engine = new TesseractEngine(GetCompletePathTessdataDir(), "eng", EngineMode.LstmOnly))
                {
                    engine.DefaultPageSegMode = PageSegMode.SingleBlock;
                    engine.SetVariable("tessedit_char_whitelist", "0123456789");
                    engine.SetVariable("classify_bln_numeric_mode", "1");
                    engine.SetVariable("load_system_dawg", "false");
                    engine.SetVariable("load_freq_dawg", "false");
                    
                    using Pix img = PixConverter.ToPix(imgsource);
                    using Page page = engine.Process(img);
                    ocrText = page.GetText();
                    Console.WriteLine($"Numero mazzo: {ocrText}");
                }
                successScan = !string.IsNullOrEmpty(ocrText);
            }
            catch (Exception ex)
            {
                Log.PrintErrorLog("OCRScan", ex.Message);
                successScan = false;
                ocrText = string.Empty;
            }
            ocrResponse.SetResponse(successScan, ocrText);
            return ocrResponse;
        }

        public OCRResponse GetTextFromBitmapNumberDeckPragmatic(Bitmap imgsource, bool saveImage = false, string prefix = "")
        {
            string dateTime = DateTime.Now.ToString("HH_mm_ss");
            string filenamePreElaboration = "file_number_deck_pre_" + dateTime + ".png";
            string filenamePostElaboration = "file_number_deck_post_" + dateTime + ".png";
            if (saveImage)
            {
                imgsource.Save(CurrentPath + filenamePreElaboration);
            }
            OCRResponse ocrResponse = OCRResponse.Instance.GetResponse();
            bool successScan = false;
            string ocrText = string.Empty;
            imgsource = AdjustContrastNumberDeckPragmatic(imgsource, 210);
            imgsource = SharpenPragmatic_V2(imgsource, 1.0, preserveBrightness: true);
            imgsource = new Bitmap(imgsource, new Size(imgsource.Width * 2, imgsource.Height * 2));
            if (saveImage)
            {
                imgsource.Save(CurrentPath + filenamePostElaboration);
            }
            try
            {
                using (TesseractEngine engine = new TesseractEngine(GetCompletePathTessdataDir(), "ita", EngineMode.Default))
                {
                    using Pix img = PixConverter.ToPix(imgsource);
                    using Page page = engine.Process(img);
                    ocrText = page.GetText();
                }
                successScan = !string.IsNullOrEmpty(ocrText);
            }
            catch (Exception ex)
            {
                Log.PrintErrorLog("OCRScan", ex.Message);
                successScan = false;
                ocrText = string.Empty;
            }
            ocrResponse.SetResponse(successScan, ocrText);
            return ocrResponse;
        }

        public static Bitmap SharpenPragmatic_V1(Bitmap input)
        {
            double[,] kernel = new double[3, 3]
            {
            { 0.0, -1.0, 0.0 },
            { -1.0, 5.0, -1.0 },
            { 0.0, -1.0, 0.0 }
            };
            return ConvolutionFilter_V1(input, kernel, 1.0, 0);
        }

        private unsafe static Bitmap ConvolutionFilter_V1(Bitmap input, double[,] kernel, double factor, int bias)
        {
            Bitmap output = new Bitmap(input.Width, input.Height);
            int width = input.Width;
            int height = input.Height;
            Rectangle rect = new Rectangle(0, 0, width, height);
            BitmapData srcData = input.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dstData = output.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            int stride = srcData.Stride;
            int bytesPerPixel = 3;
            byte* srcPtr = (byte*)(void*)srcData.Scan0;
            byte* dstPtr = (byte*)(void*)dstData.Scan0;
            int offset = 3 / 2;
            for (int y = offset; y < height - offset; y++)
            {
                for (int x = offset; x < width - offset; x++)
                {
                    double r = 0.0;
                    double g = 0.0;
                    double b = 0.0;
                    for (int ky = -offset; ky <= offset; ky++)
                    {
                        for (int kx = -offset; kx <= offset; kx++)
                        {
                            int px = x + kx;
                            int py = y + ky;
                            byte* p = srcPtr + py * stride + px * bytesPerPixel;
                            b += (double)(int)(*p) * kernel[ky + offset, kx + offset];
                            g += (double)(int)p[1] * kernel[ky + offset, kx + offset];
                            r += (double)(int)p[2] * kernel[ky + offset, kx + offset];
                        }
                    }
                    int i = y * stride + x * bytesPerPixel;
                    dstPtr[i] = (byte)Math.Min(Math.Max(factor * b + (double)bias, 0.0), 255.0);
                    dstPtr[i + 1] = (byte)Math.Min(Math.Max(factor * g + (double)bias, 0.0), 255.0);
                    dstPtr[i + 2] = (byte)Math.Min(Math.Max(factor * r + (double)bias, 0.0), 255.0);
                }
            }
            input.UnlockBits(srcData);
            output.UnlockBits(dstData);
            return output;
        }

        public static Bitmap SharpenPragmatic_V2(Bitmap input, double strength = 1.0, bool preserveBrightness = false)
        {
            double[,] kernel = new double[3, 3]
            {
            {
                0.0,
                0.0 - strength,
                0.0
            },
            {
                0.0 - strength,
                1.0 + 4.0 * strength,
                0.0 - strength
            },
            {
                0.0,
                0.0 - strength,
                0.0
            }
            };
            if (preserveBrightness)
            {
                double sum = 0.0;
                double[,] array = kernel;
                foreach (double v in array)
                {
                    sum += v;
                }
                if (Math.Abs(sum) > double.Epsilon)
                {
                    for (int y = 0; y < kernel.GetLength(0); y++)
                    {
                        for (int x = 0; x < kernel.GetLength(1); x++)
                        {
                            kernel[y, x] /= sum;
                        }
                    }
                }
            }
            return ConvolutionFilter_V2(input, kernel, 1.0, 0);
        }

        private unsafe static Bitmap ConvolutionFilter_V2(Bitmap input, double[,] kernel, double factor, int bias)
        {
            Bitmap output = new Bitmap(input.Width, input.Height);
            int width = input.Width;
            int height = input.Height;
            Rectangle rect = new Rectangle(0, 0, width, height);
            BitmapData srcData = input.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dstData = output.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            int stride = srcData.Stride;
            int bytesPerPixel = 3;
            byte* srcPtr = (byte*)(void*)srcData.Scan0;
            byte* dstPtr = (byte*)(void*)dstData.Scan0;
            int offset = 3 / 2;
            for (int y = offset; y < height - offset; y++)
            {
                for (int x = offset; x < width - offset; x++)
                {
                    double r = 0.0;
                    double g = 0.0;
                    double b = 0.0;
                    for (int ky = -offset; ky <= offset; ky++)
                    {
                        for (int kx = -offset; kx <= offset; kx++)
                        {
                            int px = x + kx;
                            int py = y + ky;
                            byte* p = srcPtr + py * stride + px * bytesPerPixel;
                            double coeff = kernel[ky + offset, kx + offset];
                            b += (double)(int)(*p) * coeff;
                            g += (double)(int)p[1] * coeff;
                            r += (double)(int)p[2] * coeff;
                        }
                    }
                    int i = y * stride + x * bytesPerPixel;
                    dstPtr[i] = (byte)Math.Min(Math.Max(factor * b + (double)bias, 0.0), 255.0);
                    dstPtr[i + 1] = (byte)Math.Min(Math.Max(factor * g + (double)bias, 0.0), 255.0);
                    dstPtr[i + 2] = (byte)Math.Min(Math.Max(factor * r + (double)bias, 0.0), 255.0);
                }
            }
            input.UnlockBits(srcData);
            output.UnlockBits(dstData);
            return output;
        }

        public unsafe Bitmap AdjustContrastNumberDeckPragmatic(Bitmap input, byte soglia = 210)
        {
            Bitmap output = new Bitmap(input.Width, input.Height, PixelFormat.Format24bppRgb);
            Rectangle rect = new Rectangle(0, 0, input.Width, input.Height);
            BitmapData dataInput = input.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dataOutput = output.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            int strideInput = dataInput.Stride;
            int strideOutput = dataOutput.Stride;
            int bytesPerPixel = 3;
            byte* ptrInput = (byte*)(void*)dataInput.Scan0;
            byte* ptrOutput = (byte*)(void*)dataOutput.Scan0;
            for (int y = 0; y < input.Height; y++)
            {
                byte* rowInput = ptrInput + y * strideInput;
                byte* rowOutput = ptrOutput + y * strideOutput;
                for (int x = 0; x < input.Width; x++)
                {
                    byte b = rowInput[x * bytesPerPixel];
                    byte g = rowInput[x * bytesPerPixel + 1];
                    byte r = rowInput[x * bytesPerPixel + 2];
                    byte colore = (byte)(((int)(0.299 * (double)(int)r + 0.587 * (double)(int)g + 0.114 * (double)(int)b) >= soglia) ? byte.MaxValue : 0);
                    rowOutput[x * bytesPerPixel] = colore;
                    rowOutput[x * bytesPerPixel + 1] = colore;
                    rowOutput[x * bytesPerPixel + 2] = colore;
                }
            }
            input.UnlockBits(dataInput);
            output.UnlockBits(dataOutput);
            return output;
        }

        public OCRResponse GetTextFromBitmapNumberDeckPragmatic_Old(Bitmap imgsource, bool saveImage = false, string prefix = "")
        {
            string dateTime = DateTime.Now.ToString("HH_mm_ss");
            string filenamePreElaboration = "file_number_deck_pre_" + dateTime + ".png";
            string filenamePostElaboration = "file_number_deck_post_" + dateTime + ".png";
            if (saveImage)
            {
                imgsource.Save(CurrentPath + filenamePreElaboration);
            }
            OCRResponse ocrResponse = OCRResponse.Instance.GetResponse();
            bool successScan = false;
            string ocrText = string.Empty;
            imgsource = AdjustContrastBacarat(imgsource, 170f, 150);
            imgsource = Sharpen(imgsource);
            imgsource = new Bitmap(imgsource, new Size(imgsource.Width * 6, imgsource.Height * 6));
            if (saveImage)
            {
                imgsource.Save(CurrentPath + filenamePostElaboration);
            }
            try
            {
                using (TesseractEngine engine = new TesseractEngine(GetCompletePathTessdataDir(), "ita", EngineMode.Default))
                {
                    using Pix img = PixConverter.ToPix(imgsource);
                    using Page page = engine.Process(img);
                    ocrText = page.GetText();
                }
                successScan = !string.IsNullOrEmpty(ocrText);
            }
            catch (Exception ex)
            {
                Log.PrintErrorLog("OCRScan", ex.Message);
                successScan = false;
                ocrText = string.Empty;
            }
            ocrResponse.SetResponse(successScan, ocrText);
            return ocrResponse;
        }

        public OCRResponse GetTextFromImage(string filename)
        {
            Bitmap imgsource = new Bitmap(new MemoryStream(File.ReadAllBytes(filename)));
            OCRResponse ocrResponse = OCRResponse.Instance.GetResponse();
            bool successScan = false;
            string ocrText = string.Empty;
            try
            {
                using (TesseractEngine engine = new TesseractEngine(GetCompletePathTessdataDir(), "ita", EngineMode.Default))
                {
                    using Pix img = PixConverter.ToPix(imgsource);
                    using Page page = engine.Process(img);
                    ocrText = page.GetText();
                }
                successScan = !string.IsNullOrEmpty(ocrText);
            }
            catch (Exception ex)
            {
                Log.PrintErrorLog("OCRScan", ex.Message);
                successScan = false;
                ocrText = string.Empty;
            }
            ocrResponse.SetResponse(successScan, ocrText);
            return ocrResponse;
        }

        private unsafe Bitmap AdjustContrast(Bitmap Image, float Value)
        {
            Value = (100f + Value) / 100f;
            Value *= Value;
            Bitmap NewBitmap = (Bitmap)Image.Clone();
            try
            {
                BitmapData data = NewBitmap.LockBits(new Rectangle(0, 0, NewBitmap.Width, NewBitmap.Height), ImageLockMode.ReadWrite, NewBitmap.PixelFormat);
                int Height = NewBitmap.Height;
                int Width = NewBitmap.Width;
                for (int y = 0; y < Height; y++)
                {
                    byte* row = (byte*)(void*)data.Scan0 + y * data.Stride;
                    int columnOffset = 0;
                    for (int x = 0; x < Width; x++)
                    {
                        byte B = row[columnOffset];
                        byte G = row[columnOffset + 1];
                        float num = (float)(int)row[columnOffset + 2] / 255f;
                        float Green = (float)(int)G / 255f;
                        float Blue = (float)(int)B / 255f;
                        float num2 = ((num - 0.5f) * Value + 0.5f) * 255f;
                        Green = ((Green - 0.5f) * Value + 0.5f) * 255f;
                        Blue = ((Blue - 0.5f) * Value + 0.5f) * 255f;
                        int iR = (int)num2;
                        iR = ((iR > 255) ? 255 : iR);
                        iR = ((iR >= 0) ? iR : 0);
                        int iG = (int)Green;
                        iG = ((iG > 255) ? 255 : iG);
                        iG = ((iG >= 0) ? iG : 0);
                        int iB = (int)Blue;
                        iB = ((iB > 255) ? 255 : iB);
                        iB = ((iB >= 0) ? iB : 0);
                        row[columnOffset] = (byte)iB;
                        row[columnOffset + 1] = (byte)iG;
                        row[columnOffset + 2] = (byte)iR;
                        columnOffset += 4;
                    }
                }
                NewBitmap.UnlockBits(data);
            }
            catch (Exception ex)
            {
                Log.PrintErrorLog("OCRScan", ex.Message);
            }
            return NewBitmap;
        }

        private unsafe Bitmap AdjustContrastRoulette(Bitmap Image, int maxValThresh)
        {
            Bitmap NewBitmap = (Bitmap)Image.Clone();
            try
            {
                BitmapData data = NewBitmap.LockBits(new Rectangle(0, 0, NewBitmap.Width, NewBitmap.Height), ImageLockMode.ReadWrite, NewBitmap.PixelFormat);
                int Height = NewBitmap.Height;
                int Width = NewBitmap.Width;
                for (int y = 0; y < Height; y++)
                {
                    byte* row = (byte*)(void*)data.Scan0 + y * data.Stride;
                    int columnOffset = 0;
                    for (int x = 0; x < Width; x++)
                    {
                        byte B = row[columnOffset];
                        byte G = row[columnOffset + 1];
                        byte num = row[columnOffset + 2];
                        byte none = 0;
                        byte full = byte.MaxValue;
                        int num2 = num + G + B;
                        byte newB = ((num2 > maxValThresh) ? none : full);
                        byte newG = ((num2 > maxValThresh) ? none : full);
                        byte newR = ((num2 > maxValThresh) ? none : full);
                        row[columnOffset] = newB;
                        row[columnOffset + 1] = newG;
                        row[columnOffset + 2] = newR;
                        columnOffset += 4;
                    }
                }
                NewBitmap.UnlockBits(data);
            }
            catch (Exception ex)
            {
                Log.PrintErrorLog("OCRScan", ex.Message);
            }
            return NewBitmap;
        }

        private unsafe Bitmap AdjustContrastBacarat(Bitmap Image, float Value, byte thresholdNew = 150)
        {
            Bitmap NewBitmap = (Bitmap)Image.Clone();
            try
            {
                BitmapData data = NewBitmap.LockBits(new Rectangle(0, 0, NewBitmap.Width, NewBitmap.Height), ImageLockMode.ReadWrite, NewBitmap.PixelFormat);
                int Height = NewBitmap.Height;
                int Width = NewBitmap.Width;
                for (int y = 0; y < Height; y++)
                {
                    byte* row = (byte*)(void*)data.Scan0 + y * data.Stride;
                    int columnOffset = 0;
                    for (int x = 0; x < Width; x++)
                    {
                        byte B = row[columnOffset];
                        byte G = row[columnOffset + 1];
                        byte num = row[columnOffset + 2];
                        byte threshold = thresholdNew;
                        byte full = byte.MaxValue;
                        byte none = 0;
                        byte newB = ((B > threshold) ? full : none);
                        byte newG = ((G > threshold) ? full : none);
                        byte newR = ((num > threshold) ? full : none);
                        newB = ((newB <= none || newG <= none || newR <= none) ? (newG = (newR = none)) : (newG = (newR = full)));
                        row[columnOffset] = newB;
                        row[columnOffset + 1] = newG;
                        row[columnOffset + 2] = newR;
                        columnOffset += 4;
                    }
                }
                NewBitmap.UnlockBits(data);
            }
            catch (Exception ex)
            {
                Log.PrintErrorLog("OCRScan", ex.Message);
            }
            return NewBitmap;
        }

        private unsafe Bitmap AdjustContrastAreaSaldo(Bitmap Image)
        {
            Bitmap NewBitmap = (Bitmap)Image.Clone();
            try
            {
                BitmapData data = NewBitmap.LockBits(new Rectangle(0, 0, NewBitmap.Width, NewBitmap.Height), ImageLockMode.ReadWrite, NewBitmap.PixelFormat);
                int Height = NewBitmap.Height;
                int Width = NewBitmap.Width;
                for (int y = 0; y < Height; y++)
                {
                    byte* row = (byte*)(void*)data.Scan0 + y * data.Stride;
                    int columnOffset = 0;
                    for (int x = 0; x < Width; x++)
                    {
                        byte B = row[columnOffset];
                        byte G = row[columnOffset + 1];
                        byte R = row[columnOffset + 2];
                        byte thresholdLow = 100;
                        int thresholdWhite = 500;
                        byte full = byte.MaxValue;
                        byte none = 0;
                        byte newB = none;
                        byte newG = none;
                        byte newR = none;
                        int sumRGB = 0;
                        sumRGB += B;
                        sumRGB += G;
                        sumRGB += R;
                        if (sumRGB > thresholdWhite)
                        {
                            newB = none;
                            newG = none;
                            newR = none;
                        }
                        else
                        {
                            newB = none;
                            newG = none;
                            newR = none;
                            if ((double)((float)(int)G / (float)sumRGB) > 0.4 && G > thresholdLow)
                            {
                                newB = full;
                                newG = full;
                                newR = full;
                            }
                        }
                        row[columnOffset] = newB;
                        row[columnOffset + 1] = newG;
                        row[columnOffset + 2] = newR;
                        columnOffset += 4;
                    }
                }
                NewBitmap.UnlockBits(data);
            }
            catch (Exception ex)
            {
                Log.PrintErrorLog("OCRScan", ex.Message);
            }
            return NewBitmap;
        }

        private unsafe Bitmap AdjustContrastAreaSaldoNoSymbol(Bitmap Image)
        {
            Bitmap NewBitmap = (Bitmap)Image.Clone();
            NewBitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
            try
            {
                BitmapData data = NewBitmap.LockBits(new Rectangle(0, 0, NewBitmap.Width, NewBitmap.Height), ImageLockMode.ReadWrite, NewBitmap.PixelFormat);
                int Height = NewBitmap.Height;
                int Width = NewBitmap.Width;
                byte* row0 = (byte*)(void*)data.Scan0;
                _ = *row0;
                _ = row0[1];
                _ = row0[2];
                byte full = 0;
                byte none = byte.MaxValue;
                int alreadyWhite = 0;
                for (int y = 0; y < Height; y++)
                {
                    bool eraseColumn = false;
                    byte* row1 = (byte*)(void*)data.Scan0 + y * data.Stride;
                    int columnOffset = 0;
                    bool allColumnBlack = true;
                    for (int x = 0; x < Width; x++)
                    {
                        byte B = row1[columnOffset];
                        byte G = row1[columnOffset + 1];
                        byte R = row1[columnOffset + 2];
                        byte newB = none;
                        byte newG = none;
                        byte newR = none;
                        if (0 + B + G + R > 120)
                        {
                            allColumnBlack = false;
                            newB = full;
                            newG = full;
                            newR = full;
                            if (alreadyWhite == 0)
                            {
                                alreadyWhite = 1;
                            }
                            if (alreadyWhite != 2)
                            {
                                eraseColumn = true;
                            }
                        }
                        if (eraseColumn)
                        {
                            newB = none;
                            newG = none;
                            newR = none;
                            eraseColumn = false;
                        }
                        row1[columnOffset] = newB;
                        row1[columnOffset + 1] = newG;
                        row1[columnOffset + 2] = newR;
                        columnOffset += 4;
                    }
                    if (allColumnBlack && alreadyWhite == 1)
                    {
                        alreadyWhite = 2;
                        eraseColumn = false;
                    }
                }
                NewBitmap.UnlockBits(data);
                NewBitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
            }
            catch (Exception ex)
            {
                Log.PrintErrorLog("OCRScan", ex.Message);
            }
            return NewBitmap;
        }

        private static Bitmap Sharpen(Bitmap image)
        {
            Bitmap sharpenImage = (Bitmap)image.Clone();
            try
            {
                int width = image.Width;
                int height = image.Height;
                BitmapData pbits = sharpenImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                int bytes = pbits.Stride * height;
                byte[] rgbValues = new byte[bytes];
                Marshal.Copy(pbits.Scan0, rgbValues, 0, bytes);
                Marshal.Copy(rgbValues, 0, pbits.Scan0, bytes);
                sharpenImage.UnlockBits(pbits);
            }
            catch (Exception ex)
            {
                Log.PrintErrorLog("OCRScan", ex.Message);
            }
            return sharpenImage;
        }

        private string GetCompletePathTessdataDir()
        {
            return Path.Combine(Constants.PathProject(), "tessdata");
        }
    }
}
