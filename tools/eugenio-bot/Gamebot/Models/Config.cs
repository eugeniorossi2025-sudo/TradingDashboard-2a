using Gamebot.Models.Objects;
using System.Collections.Generic;
using System.Drawing;

namespace Gamebot.Models
{
    internal class Config
    {
        public static string directory_numeri_mazzo = "";
        
        public static Constants.EnumColorBaccarat start_color = Constants.EnumColorBaccarat.BLU_PLAY;

        public static float profit_multiplier = 0.95f;

        public static double[] martingala_array = new double[8];

        public static int cambio_colore = 0;

        public static decimal global_stop_win = default(decimal);

        public static decimal global_stop_loss = default(decimal);

        public static decimal sculping_stop_win = default(decimal);

        public static decimal safe_win = default(decimal);

        public static bool modalita_alternata = true;

        public static int index_alarm = 0;

        public static string insert_number = "";

        public static string verified_code = "";

        public static string groupchatname = "";

        public static long selected_chat = 0L;

        public static int zoom = 100;

        public static bool enableClick = true;

        public static bool safe_win_enable = false;

        public static bool send_end_sculping_message = false;

        public static int limitEndDeck = 55;

        public static string textAreaTie = string.Empty;

        public static string textAreaWin = string.Empty;

        public static string textAreaBench = string.Empty;

        public static string textAreaPlayer = string.Empty;

        public static string textAreaPuntare = string.Empty;

        public static bool baccaratDemoEnabled = false;

        public static bool enableFilterPragmatic = false;

        public static Color targetColorTie = Color.FromArgb(255, 1, 121, 35);

        public static Color targetColorBank1 = Color.FromArgb(255, 251, 0, 1);

        public static Color targetColorBank2 = Color.FromArgb(255, 229, 1, 3);

        public static Color targetColorBank3 = Color.FromArgb(255, 226, 1, 2);

        public static Color targetColorPlayer1 = Color.FromArgb(255, 7, 118, 225);

        public static Color targetColorPlayer2 = Color.FromArgb(255, 7, 116, 223);

        public static Color targetColorPlayer3 = Color.FromArgb(255, 0, 114, 221);

        public static List<MartingalaInfoItem> MartingalaOptions = new List<MartingalaInfoItem>();

        public static bool skipPostSculping = false;

        public static int indexNamePc = 0;

        public static bool Debug { get; set; } = true;
    }
}
