using System;

namespace Gamebot.Models
{
    internal class OCReads
    {
        public static Constants.EnumColorBaccarat current_color = Constants.EnumColorBaccarat.BLU_PLAY;

        public static string label_winner = "";

        public static string label_bet = "";

        public static int number_deck = 0;

        public static string balance = "";
        
        public static DateTime label_bet_last_success_update { get; set; }
    }
}
