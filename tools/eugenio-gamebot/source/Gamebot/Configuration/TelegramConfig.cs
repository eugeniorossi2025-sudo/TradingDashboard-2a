using System.Collections.Generic;

namespace Gamebot.Configuration
{
    internal class TelegramConfig
    {
        public static List<string> COMMAND = new List<string> { "#STOP", "#SALDO", "#COMANDI" };

        public static string API_ID = "23376039";

        public static string API_HASH = "d5669b4e71a2e845feda7aaa01f2b0bf";

        public const string BOT_NAME = "EUGENIO";

        public const string DIRECTORY_SESSION = "telegramSession";
    }

}
