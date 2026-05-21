using Gamebot.Models;
using System;
using System.Linq;

namespace Gamebot.Helpers
{
    internal static class Check
    {
        public static bool centralLabelContainsPUNTARE()
        {
            /*
            if (DateTime.Now - OCReads.label_bet_last_success_update < TimeSpan.FromSeconds(5))
            {
                return true;
            }
            */
                
            if (Config.enableFilterPragmatic)
                return Enumerable.Range(3, 12)
                    .Any(i => OCReads.label_bet.Contains(i.ToString()));

            if (OCReads.number_deck == 0)
                return OCReads.label_bet.Contains(Config.textAreaPuntare, StringComparison.OrdinalIgnoreCase);
            
            return Enumerable.Range(3, 8)
                .Any(i => OCReads.label_bet.Contains(
                    $"{Config.textAreaPuntare} {i}",
                    StringComparison.OrdinalIgnoreCase));
        }

        public static bool centralLabelContainsVINCE(ref string winner)
        {
            string ocrLabelCentral = OCReads.label_winner;
            winner = string.Empty;
            if (string.IsNullOrEmpty(OCReads.label_winner))
            {
                return false;
            }
            if (ocrLabelCentral.Contains(Config.textAreaBench, StringComparison.OrdinalIgnoreCase) || ocrLabelCentral.Contains("7 lee", StringComparison.OrdinalIgnoreCase) || ocrLabelCentral.Contains("7.) fee", StringComparison.OrdinalIgnoreCase))
            {
                winner = Config.textAreaBench;
            }
            if (ocrLabelCentral.Contains(Config.textAreaPlayer, StringComparison.OrdinalIgnoreCase) || (!ocrLabelCentral.Contains(Config.textAreaBench, StringComparison.OrdinalIgnoreCase) && !ocrLabelCentral.Contains(Config.textAreaTie, StringComparison.OrdinalIgnoreCase)))
            {
                winner = Config.textAreaPlayer;
            }
            if (string.IsNullOrEmpty(Config.textAreaWin))
            {
                return !ocrLabelCentral.Contains(Config.textAreaTie, StringComparison.OrdinalIgnoreCase);
            }
            return ocrLabelCentral.Contains(Config.textAreaWin, StringComparison.OrdinalIgnoreCase);
        }

        public static bool centralLabelContainsBANCO(string winner)
        {
            return winner.Contains(Config.textAreaBench, StringComparison.OrdinalIgnoreCase);
        }

        public static bool centralLabelContainsGIOCATORE(string winner)
        {
            return winner.Contains(Config.textAreaPlayer, StringComparison.OrdinalIgnoreCase);
        }

        public static bool centralLabelContainsTIE()
        {
            string ocrLabelCentral = OCReads.label_winner;
            return ocrLabelCentral.Contains(Config.textAreaTie, StringComparison.OrdinalIgnoreCase) || ocrLabelCentral.Contains("x. iicic]io", StringComparison.OrdinalIgnoreCase);
        }

        public static bool centralLabelContainsROSSO()
        {
            return OCReads.label_winner.Contains("ROSSO", StringComparison.OrdinalIgnoreCase);
        }

        public static bool centralLabelContainsNERO()
        {
            return OCReads.label_winner.Contains("NERO", StringComparison.OrdinalIgnoreCase);
        }

        public static bool centralLabelContainsVERDE()
        {
            return OCReads.label_winner.Contains("VERDE", StringComparison.OrdinalIgnoreCase);
        }
    }
}
