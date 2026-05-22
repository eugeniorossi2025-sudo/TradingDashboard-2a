using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

public class ReadNumber {
    public int number;
    public double similarity;

    public ReadNumber(int bestNumber, double d)
    {
        this.number = bestNumber;
        this.similarity = d;
    }
}

public class DigitRecognizer2
{
    private readonly Dictionary<int, Mat> templates = new();

    // riuso buffer -> meno garbage
    private readonly Mat gray = new();
    private readonly Mat bin = new();

    public DigitRecognizer2(string templateFolder)
    {
        foreach (var file in Directory.GetFiles(templateFolder))
        {
            var name = Path.GetFileNameWithoutExtension(file);

            if (!int.TryParse(name, out int number))
                continue;

            using var bmp = new Bitmap(file);
            using var mat = BitmapConverter.ToMat(bmp);

            // grayscale
            Cv2.CvtColor(mat, mat, ColorConversionCodes.BGR2GRAY);

            // binarizzazione automatica → robusto a sfondi scuri di colore diverso
            Cv2.Threshold(mat, mat, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);

            // assicura 8 bit 1 canale
            mat.ConvertTo(mat, MatType.CV_8UC1);

            templates[number] = mat.Clone();
        }

        if (templates.Count == 0)
            throw new Exception("Nessun template valido trovato!");
    }

    public ReadNumber Recognize(Bitmap bmp)
    {
        using var mat = BitmapConverter.ToMat(bmp);

        // grayscale
        Cv2.CvtColor(mat, gray, ColorConversionCodes.BGR2GRAY);

        // binarizzazione adattiva → robusto a diversi colori di sfondo
        Cv2.Threshold(gray, bin, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);

        return MatchTemplate(bin);
    }

    private ReadNumber MatchTemplate(Mat candidate)
    {
        int bestError = int.MaxValue;
        int bestNumber = -1;

        int totalPixels = candidate.Rows * candidate.Cols;

        foreach (var kv in templates)
        {
            int number = kv.Key;
            Mat templ = kv.Value;

            using var diff = new Mat();

            // XOR → pixel diversi = 255
            Cv2.BitwiseXor(candidate, templ, diff);

            // conta i pixel diversi
            int error = Cv2.CountNonZero(diff);

            if (error < bestError)
            {
                bestError = error;
                bestNumber = number;

                // corrispondenza perfetta → inutile continuare
                if (error == 0)
                    break;
            }
        }

        double similarity = 1.0 - (bestError / (double)totalPixels);

        return new ReadNumber(bestNumber, similarity);
    }
}
