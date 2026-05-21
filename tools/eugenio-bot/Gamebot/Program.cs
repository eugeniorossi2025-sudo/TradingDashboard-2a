using Gamebot.Communication.Firebase;
using Gamebot.Helpers;
using Gamebot.Models;
using Gamebot.Models.Roulette;
using Gamebot.Models.UI;
using Gamebot.UI.WindowForm;
using System;
using System.Windows.Forms;

namespace Gamebot
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            FirestoreHelpers.SetEnvironmentVariable();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ApplicationExit += Program.OnApplicationExit;
            Configuratore form = new Configuratore();
            UpdateInterface.Instance.SetRefForm(form);
            Application.Run(form);
        }

        private static void OnApplicationExit(object sender, EventArgs e)
        {
            DateTime date = DateTime.Now;
            string day = date.Date.ToString("dd/MM/yyyy");
            string hour = ((date.Hour < 10) ? ("0" + date.Hour.ToString()) : date.Hour.ToString());
            string minute = ((date.Minute < 10) ? ("0" + date.Minute.ToString()) : date.Minute.ToString());
            string second = ((date.Second < 10) ? ("0" + date.Second.ToString()) : date.Second.ToString());
            string errorMessage = string.Concat(new string[] { day, " ", hour, ":", minute, ":", second, " | MSG: APPLICAZIONE CHIUSA" });
            string currentDate = DateTime.Now.ToString("dd-MM-yyyy");
            string fileName = "LOG" + "_" + currentDate + ".txt";
            ManageFile.SaveLogFile("log", errorMessage + "\n", fileName, false, true);
            if (Runtime.game == 0)
            {
                Player.Instance.Stop();
                return;
            }
            RoulettePlayer.Instance.Stop();
        }
    }
}
