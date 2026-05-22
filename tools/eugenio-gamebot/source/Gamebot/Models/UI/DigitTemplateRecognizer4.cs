using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Size = OpenCvSharp.Size;

public class DigitRecognizer4
{
    private readonly Dictionary<int, Mat> templates = new();

    // Parametri CLAHE (puoi modificarli se vuoi)
    private const double CLAHE_CLIP = 3.0;
    private static readonly Size CLAHE_TILE = new Size(8, 8);

    public DigitRecognizer4(string templateFolder)
    {
        var clahe = Cv2.CreateCLAHE(CLAHE_CLIP, CLAHE_TILE);

        foreach (var file in Directory.GetFiles(templateFolder))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            if (!int.TryParse(name, out int number))
                continue;

            using var bmp = new Bitmap(file);
            using var mat = BitmapConverter.ToMat(bmp);

            // 1) Grayscale
            Cv2.CvtColor(mat, mat, ColorConversionCodes.BGR2GRAY);

            // 2) Aumento contrasto (IMPORTANTE: anche sui template!)
            using var matEq = new Mat();
            clahe.Apply(mat, matEq);

            templates[number] = matEq.Clone();
        }
    }

    public ReadNumber Recognize(Bitmap bmp)
    {
        if (templates.Count == 0)
        {
            return new ReadNumber(-1, 0);
        }

        using var mat = BitmapConverter.ToMat(bmp);
        Cv2.CvtColor(mat, mat, ColorConversionCodes.BGR2GRAY);

        // Applichiamo lo stesso contrasto usato per i template
        var clahe = Cv2.CreateCLAHE(CLAHE_CLIP, CLAHE_TILE);
        using var matEq = new Mat();
        clahe.Apply(mat, matEq);

        return MatchHybrid(matEq);
    }

    private ReadNumber MatchHybrid(Mat candidate)
    {
        double bestScore = double.NegativeInfinity;
        int bestNumber = -1;

        using var candBlur = new Mat();
        using var candDil = new Mat();
        using var candEro = new Mat();

        Cv2.GaussianBlur(candidate, candBlur, new Size(3, 3), 0);
        Cv2.Dilate(candidate, candDil,
            Cv2.GetStructuringElement(MorphShapes.Rect, new Size(2, 2)));
        Cv2.Erode(candidate, candEro,
            Cv2.GetStructuringElement(MorphShapes.Rect, new Size(2, 2)));

        foreach (var kv in templates)
        {
            var number = kv.Key;
            var templ = kv.Value;

            double score =
                Math.Max(
                    Math.Max(
                        NCC(candidate, templ),
                        NCC(candBlur, templ)
                    ),
                    Math.Max(
                        NCC(candDil, templ),
                        NCC(candEro, templ)
                    )
                );

            if (score > bestScore)
            {
                bestScore = score;
                bestNumber = number;
            }
        }

        return new ReadNumber(bestNumber, bestScore);
    }

    private double NCC(Mat img, Mat templ)
    {
        using var result = new Mat();
        Cv2.MatchTemplate(img, templ, result, TemplateMatchModes.CCoeffNormed);
        Cv2.MinMaxLoc(result, out _, out double maxVal);
        return maxVal;
    }
}
