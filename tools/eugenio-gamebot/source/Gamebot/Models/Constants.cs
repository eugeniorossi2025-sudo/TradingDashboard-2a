using System;
using System.Collections.Generic;

namespace Gamebot.Models
{
    internal class Constants
    {
        public enum EnumEnvironment
        {
            DEVELOPMENT,
            PRODUCTION
        }

        public enum EnumColorBaccarat
        {
            BLU_PLAY,
            RED_BANK,
            TIE,
            NAY
        }

        public enum EnumStateBot
        {
            IDLE,
            FIRST_PLAY,
            SCULPING,
            PAUSE_SCALPING,
            SAFE_WIN,
            END_DECK,
            WAITING_NEW_DECK,
            GLOBAL_STOP_WIN,
            GLOBAL_STOP_LOSS,
            NEW_DECK
        }

        public enum EnumAlert
        {
            END_SCULPING,
            MARTINGALA_PERSA,
            MARTINGALA_PERSA_FINE_MAZZO,
            GLOBAL_STOP_WIN,
            GLOBAL_STOP_LOSS,
            WAITING_NEW_DECK,
            INDEX_ALARM,
            START_SCULPING,
            WAITING_TO_START_SCALPING,
            ROULETTE_FINE_MANI_GIOCATE,
            ROULETTE_MANO_GIOCATA_PERSA,
            START_GAME,
            STOP_GAME,
            NEW_DECK
        }

        public const int LIMIT_MIN_NEW_DECK = 0;

        public const int LIMIT_MAX_NEW_DECK = 30;

        public const int LIMIT_MIN_NEW_SCULPING_DECK = 1;

        public const int LIMIT_MAX_NEW_SCULPING_DECK = 3;

        public const int LIMIT_RANDOM_DECK = 3;

        public const int SleepClickMouseMillisecond = 100;

        public const int StepMovementMouseFast = 10;

        public const int StepMovementMouseSlow = 20;

        public const int SleepMovementMouseMillisecond = 5;

        public const int DeltaMovementMouseXY = 30;

        public const int SleepOCRScanWinArea = 150;

        public const int SleepOCRScanBetArea = 150;

        public const int SleepOCRScanDeckArea = 250;

        public const int SleepOCRScanBalanceArea = 1000;

        public const int MOVE_SLEEP = 200;

        public const int MOVE_SLEEP_RANGE = 100;

        public const int REST_SLEEP = 14000;

        public const int REST_SLEEP_RANGE = 2500;

        public const string BtnBlu = "BLU";

        public const string BtnBlk = "NERO";

        public const string BtnRed = "ROSSO";

        public const string BtnCentralArea = "AREA_CENTRALE";

        public const string BtnWinArea = "AREA_VINCITA";

        public const string BtnDoublingArea = "AREA_RADDOPPIO";

        public const string BtnDeckArea = "AREA_MAZZO";

        public const string BtnBalanceArea = "AREA_SALDO";

        public const string BtnBetArea = "AREA_PUNTARE";

        public const string BtnFiche1 = "FICHE_1";

        public const string BtnFiche5 = "FICHE_5";

        public const string BtnFiche25 = "FICHE_25";

        public const string BtnFiche100 = "FICHE_100";

        public const string BtnFiche250 = "FICHE_250";

        public const string BtnFiche500 = "FICHE_500";

        public const string LABEL_BLU = "BLU";

        public const string LABEL_RED = "RED";

        public const string LABEL_MODE_ALTERNATA = "ALTERNATA";

        public const string LABEL_MODE_MONOCOLORE = "MONOCOLORE";

        public const int FICHE_VALUE_1 = 1;

        public const int FICHE_VALUE_5 = 5;

        public const int FICHE_VALUE_25 = 25;

        public const int FICHE_VALUE_100 = 100;

        public const int FICHE_VALUE_250 = 250;

        public const int FICHE_VALUE_500 = 500;

        public const int FICHE_RADDOPPIA = 222;

        public const int FICHE_RADDOPPIA_CUSTOM = -1;

        public const string TERM_ROSSO = "ROSSO";

        public const string TERM_NERO = "NERO";

        public const string TERM_VERDE = "VERDE";

        public static readonly int[] availableFiches = new List<int> { 500, 250, 100, 25, 5, 1 }.ToArray();

        public const string DIR_SAVE_DATA = "appData";

        public const string FILE_DATA_NAME = "config";

        public const string FILE_DATA_EXTENSION = ".bac";

        public const string DIR_SAVE_DATA_LOG = "log";

        public const string FILE_LOG_NAME = "LOG";

        public const string FILE_LOG_EXTENSION = ".txt";

        public const double LOG_ELAPSE_TIME = 7.0;

        public const string TASK_NAME_STATE_MACHINE = "STATE_MACHINE";

        public const string TASK_NAME_TIME_ELAPSED = "TIME_ELAPSED";

        public const string CURRENCY_SYMBOL = "€";

        public const string DEFAULT_FILENAME_CONFIG = "<<Nessuna configurazione caricata>>";

        public static readonly string[] PC_NAME_LIST = new List<string> { "-- SELEZIONA PC --", "PC #1", "PC #2", "PC #3", "PC #4", "PC #5", "PC #6", "PC #7", "PC #8" }.ToArray();

        public static string PathProject()
        {
            return Environment.CurrentDirectory;
        }
    }
}
