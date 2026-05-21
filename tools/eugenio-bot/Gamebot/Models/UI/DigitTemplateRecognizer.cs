using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;

public class DigitRecognizer

{
    private readonly Dictionary<int, Mat> templates = new();
    private Mat result = new();
    private const double THRESHOLD = 0.95;

    public DigitRecognizer(string templateFolder)
    {
        foreach (var file in Directory.GetFiles(templateFolder))
        {
            var name = Path.GetFileNameWithoutExtension(file);

            if (int.TryParse(name, out int number)) 
            {
                using var bmp = new Bitmap(file);
                using var mat = BitmapConverter.ToMat(bmp);

                // Converti in grayscale
                Cv2.CvtColor(mat, mat, ColorConversionCodes.BGR2GRAY);

                // Binarizza (scritta bianca -> 255, sfondo -> 0)
                Cv2.Threshold(mat, mat, 200, 255, ThresholdTypes.Binary);

                templates[number] = mat.Clone();
            }
        }

        if (templates.Count == 0)
            throw new Exception("Nessun template trovato!");
    }

    public int Recognize(Bitmap bmp)
    {
        using var mat = BitmapConverter.ToMat(bmp);

        // Grayscale
        using var gray = new Mat();
        Cv2.CvtColor(mat, gray, ColorConversionCodes.BGR2GRAY);

        // Binarizza con soglia fissa
        using var bin = new Mat();
        Cv2.Threshold(gray, bin, 200, 255, ThresholdTypes.Binary);

        // Match diretto senza ridimensionamento
        return MatchTemplate(bin);
    }

    private int MatchTemplate(Mat candidate)
    {
        double bestScore = double.MinValue;
        int bestNumber = -1;

        // Risultato sempre 1x1
        using var result = new Mat(1, 1, MatType.CV_32FC1);

        foreach (var kv in templates)
        {
            int number = kv.Key;
            Mat templ = kv.Value;

            // template e candidate hanno la stessa dimensione → sicuro
            Cv2.MatchTemplate(candidate, templ, result, TemplateMatchModes.CCoeffNormed);

            Cv2.MinMaxLoc(result, out _, out double maxVal);

            if (maxVal > bestScore)
            {
                bestScore = maxVal;
                bestNumber = number;
            }

            // Early exit se supera soglia
            if (bestScore >= THRESHOLD)
                return bestNumber;
        }

        return bestScore >= THRESHOLD ? bestNumber : -1;
    }
}
