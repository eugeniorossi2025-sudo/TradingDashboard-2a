using Gamebot.Models;
using System;
using System.IO;

namespace Gamebot.Helpers
{
    internal class Log
    {
        public static void PrintInfoLog(string tag, string msg, string method = "")
        {
            bool debug = Config.Debug;
        }

        public static void PrintErrorLog(string tag, string msg, string method = "")
        {
            bool debug = Config.Debug;
        }

        public static void PrintInfo(string msg)
        {
            DateTime date = DateTime.Now;
            string day = date.Date.ToString("dd/MM/yyyy");
            string hour = ((date.Hour < 10) ? ("0" + date.Hour.ToString()) : date.Hour.ToString());
            string minute = ((date.Minute < 10) ? ("0" + date.Minute.ToString()) : date.Minute.ToString());
            string second = ((date.Second < 10) ? ("0" + date.Second.ToString()) : date.Second.ToString());
            string errorMessage = string.Concat(new string[] { day, " ", hour, ":", minute, ":", second, " | MSG: ", msg });
            string currentDate = DateTime.Now.ToString("dd-MM-yyyy");
            string fileName = "LOG" + "_" + currentDate + ".txt";
            ManageFile.SaveLogFile("log", errorMessage + "\n", fileName, false, true);
        }

        public static void DeleteOldLog()
        {
            DateTime currentDateDeleteFile = DateTime.Now;
            foreach (string file in Directory.GetFiles(Path.Combine(Constants.PathProject(), "log"), "*", SearchOption.TopDirectoryOnly))
            {
                DateTime fileDate = Directory.GetCreationTime(file).AddDays(7.0);
                if (currentDateDeleteFile > fileDate)
                {
                    Log.PrintInfo("DELETE FILE " + file);
                    File.Delete(file);
                }
            }
        }
    }
}
