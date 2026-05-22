using Gamebot.Models.Roulette.Logic;
using System.Collections.Generic;

namespace Gamebot.Models.Roulette.Funcs
{
    internal class RouletteValues
    {
        internal class Constants
        {
            public const int SleepClickMouseMillisecond = 100;

            public const int StepMovementMouseFast = 10;

            public const int StepMovementMouseSlow = 20;

            public const int SleepMovementMouseMillisecond = 5;

            public const int DeltaMovementMouseXY = 30;

            public const int SleepOCRScanWinArea = 1000;

            public const int SleepOCRScanDeckArea = 1000;

            public const int SleepOCRScanBalanceArea = 600;

            public const int MOVE_SLEEP = 200;

            public const int MOVE_SLEEP_RANGE = 100;

            public const int REST_SLEEP = 14000;

            public const int REST_SLEEP_RANGE = 2500;

            public const string R_Hand1 = "R_Hand1";

            public const string R_Hand2 = "R_Hand2";

            public const string R_Hand3 = "R_Hand3";

            public const string R_Win = "R_Win";

            public const string R_Wait = "R_Wait";

            public const string BtnBalanceAreaRoulette = "AREA_SALDO_ROULETTE";

            public const string TERM_ROSSO = "ROSSO";

            public const string TERM_NERO = "NERO";

            public const string TERM_VERDE = "VERDE";

            public const string DIR_SAVE_DATA = "appData";

            public const string FILE_DATA_NAME = "r_config";

            public const string FILE_DATA_EXTENSION = ".rou";

            public const string DIR_SAVE_DATA_LOG = "log";

            public const string FILE_LOG_NAME = "r_LOG";

            public const string FILE_LOG_EXTENSION = ".txt";

            public const double LOG_ELAPSE_TIME = 7.0;

            public const string TASK_NAME_STATE_MACHINE = "STATE_MACHINE";

            public const string BOT_NAME_TELEGRAM = "EUGENIO";

            public const string CURRENCY_SYMBOL = "€";

            public static List<string> TELEGRAM_COMMAND = new List<string> { "#STOP", "#SALDO", "#COMANDI" };

            public const string DEFAULT_FILENAME_CONFIG = "<<Nessuna configurazione caricata>>";

            public static List<int> nearby_number = new List<int>
            {
                0, 26, 3, 35, 12, 28, 7, 29, 18, 22,
                9, 31, 14, 20, 1, 33, 16, 24, 5, 10,
                23, 8, 30, 11, 36, 13, 27, 6, 34, 17,
                25, 2, 21, 4, 19, 15, 32
            };

            public const int TIME_SLEEP_AFTER_LOSS = 12000;

            public enum EnumStateBot
            {
                IDLE,
                RUNNING,
                FIRST_PLAY,
                SCULPING,
                PAUSE_SCALPING,
                SAFE_WIN,
                END_DECK,
                WAITING_NEW_DECK,
                GLOBAL_STOP_WIN,
                GLOBAL_STOP_LOSS
            }
        }

        internal class Config
        {
            // (get) Token: 0x0600039D RID: 925 RVA: 0x00023A99 File Offset: 0x00021C99
            // (set) Token: 0x0600039E RID: 926 RVA: 0x00023AA0 File Offset: 0x00021CA0
            public static bool Debug { get; set; } = true;

            public static decimal stop_win = 0m;

            public static decimal stop_loss = 0m;

            public static decimal hand_value_1 = 0m;

            public static decimal hand_value_2 = 0m;

            public static decimal hand_value_3 = 0m;

            public static string insert_number = "";

            public static string verified_code = "";

            public static string groupchatname = "";

            public static long selected_chat = 0L;

            public static int zoom = 100;

            public static bool enableClick = true;
        }

        internal class OCReads
        {
            public static Gamebot.Models.Constants.EnumColorBaccarat current_color = Gamebot.Models.Constants.EnumColorBaccarat.BLU_PLAY;

            public static string label_winner = "";

            public static int number_deck = 0;

            public static float balance = 0f;
        }

        internal static class Runtime
        {
            public static void ResetVariables()
            {
                RouletteValues.Runtime.global_profit = 0f;
                RouletteValues.Runtime.balance = 0f;
                RouletteValues.Runtime.numero_vincite = 0;
                RouletteValues.Runtime.numero_perdite = 0;
                SubStateRoulette.playedHandLvl = 1;
                RouletteValues.Runtime.skip_next = false;
                RouletteValues.Runtime.last_number = -1;
            }

            public static float global_profit = 0f;

            public static int numero_vincite = 0;

            public static int numero_perdite = 0;

            public static int number_deck = 0;

            public static RouletteValues.Constants.EnumStateBot current_state_bot = RouletteValues.Constants.EnumStateBot.IDLE;

            public static bool runningOCRScan = false;

            public static bool runningStateMachineBot = false;

            public static Gamebot.Models.Constants.EnumColorBaccarat[] color_pause_scalping_array = new Gamebot.Models.Constants.EnumColorBaccarat[4];

            public static int index_color_pause_scalping_array = 0;

            public static float balance = 0f;

            public static float balanceInit = 0f;

            public static string labelTextCurrentState = "ATTESA";

            public static bool skip_next = false;

            public static int last_number = -1;
        }
    }
}
