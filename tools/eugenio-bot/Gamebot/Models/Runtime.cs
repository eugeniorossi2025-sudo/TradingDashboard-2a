using Gamebot.Models.Objects;
using System;
using System.Collections.Generic;
using Gamebot.Helpers;

namespace Gamebot.Models
{
    internal static class Runtime
    {
        public static void ResetVariables()
        {
            Runtime.chosen_color = Constants.EnumColorBaccarat.NAY;
            Runtime.last_color = Constants.EnumColorBaccarat.NAY;
            Runtime.last_result = Constants.EnumColorBaccarat.NAY;
            Runtime.last_result_update = DateTime.Now;
            Runtime.last_win = false;
            Runtime.fine_mazzo_counter = 0;
            Runtime.martingala_counter = 0;
            Runtime.valore_giocata = 0.0;
            
            Runtime.first_giocata = true;
            Runtime.martingala_persa = false;
            Runtime.pause_sculping_counter = 0;
            //Runtime.global_profit = 0.0;
            Runtime.sculping_profit = 0.0;
            Runtime.numero_vincite = 0;
            Runtime.numero_perdite = 0;
            Runtime.fiches_array = new int[8];

            Runtime.number_deck = -1;
            Runtime.bcomando_sf = false;
            
            DashboardApiHelper.LoadGlobalProfit();
        }

        public static void ErasePauseScalpingArray()
        {
            Runtime.color_pause_scalping_array[0] = Constants.EnumColorBaccarat.NAY;
            Runtime.color_pause_scalping_array[1] = Constants.EnumColorBaccarat.NAY;
            Runtime.color_pause_scalping_array[2] = Constants.EnumColorBaccarat.NAY;
            Runtime.color_pause_scalping_array[3] = Constants.EnumColorBaccarat.NAY;
        }

        public static int game = 0;

        public static Constants.EnumColorBaccarat chosen_color = Constants.EnumColorBaccarat.NAY;
        
        public static string old_chosen_color = "";

        public static Constants.EnumColorBaccarat last_color = Constants.EnumColorBaccarat.NAY;

        public static Constants.EnumColorBaccarat last_result = Constants.EnumColorBaccarat.NAY;
        
        public static DateTime last_result_update = DateTime.Now;
        
        public static bool last_win = false;

        public static int fine_mazzo_counter = 0;

        public static int martingala_counter = 0;
        
        public static int old_martingala_counter = 0;

        public static double valore_giocata = 0.0;

        public static double puntata = 0.0;

        public static bool bcomando_sf=false;

        public static bool first_giocata = true;

        public static bool martingala_persa = false;

        public static int pause_sculping_counter = 0;

        public static int waiting_deck_counter = 0;

        public static double global_profit = 0.0;

        public static double sculping_profit = 0.0;

        public static int numero_vincite = 0;

        public static int numero_perdite = 0;

        public static int number_deck = -1;

        public static int[] fiches_array = new int[8];

        public static List<CustomFiche> custom_fiches = new List<CustomFiche>();

        public static double[] availableCustomFiches = new List<double>().ToArray();

        public static DateTime lastLaunch = DateTime.Now;

        public static Constants.EnumStateBot current_state_bot = Constants.EnumStateBot.IDLE;

        public static bool runningOCRScan = false;

        public static bool runningStateMachineBot = false;

        public static bool runningTimeElapsed = false;

        public static Constants.EnumColorBaccarat[] color_pause_scalping_array = new Constants.EnumColorBaccarat[4];

        public static int index_color_pause_scalping_array = 0;

        public static double balance = 0.0;

        public static double balanceInit = 0.0;

        public static string labelTextCurrentState = "ATTESA";

        public static string readSaldo = "";

        public static int ocrBalanceCorrect = 0;

        public static int ocrBalanceIncorrect = 0;

        public static bool start_new_deck = false;

        public static int currentNumberDeck = -2;
    }
}
