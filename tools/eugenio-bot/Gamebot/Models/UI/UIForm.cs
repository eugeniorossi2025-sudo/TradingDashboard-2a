using Gamebot.Helpers;
using Gamebot.Models.Communication;
using Gamebot.Models.Objects;
using Gamebot.Models.Roulette;
using Gamebot.Models.Roulette.Funcs;
using Gamebot.Models.Roulette.Logic;
using Gamebot.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Media;
using System.Reflection;
using System.Windows.Forms;

namespace Gamebot.Models.UI
{
    internal class UIForm
    {
        public static void ClickButtonGreen(object sender, string keyButton)
        {
            UIForm.TakeScreen(keyButton);
            if (ListAreaElement.Instance.CheckKey(keyButton))
            {
                Button button = (Button)sender;
                button.BackColor = Color.Green;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderColor = Color.Green;
                button.ForeColor = Color.White;
            }
        }

        public static void SelectButtonGreen(object sender)
        {
            Button button = (Button)sender;
            button.BackColor = Color.Green;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = Color.Green;
            button.ForeColor = Color.White;
        }

        public static void SelectButtonStandard(object sender)
        {
            Button button = (Button)sender;
            button.BackColor = Color.Transparent;
            button.FlatStyle = FlatStyle.Standard;
            button.FlatAppearance.BorderColor = Color.Black;
            button.ForeColor = SystemColors.ControlText;
        }

        public static void SelectButtonFichesRoulette(object sender)
        {
            Button button = (Button)sender;
            button.BackColor = Color.Teal;
            button.FlatStyle = FlatStyle.Flat;
            button.ForeColor = Color.White;
        }

        public static void DeselectButtonFichesRoulette(object sender, BtnFiches btnFiches)
        {
            Button button = (Button)sender;
            button.BackColor = btnFiches.BackCurrentColor;
            button.FlatAppearance.BorderColor = btnFiches.BorderCurrentColor;
            button.ForeColor = btnFiches.FrontCurrentColor;
        }

        public static void SelectButtonDisable(object sender)
        {
            Button button = (Button)sender;
            button.BackColor = Color.Gray;
            button.FlatAppearance.BorderColor = Color.Gray;
            button.ForeColor = Color.White;
        }

        public static void SelectNumericUpDownEnable(object sender)
        {
            NumericUpDown numericUpDown = (NumericUpDown)sender;
            numericUpDown.BackColor = Color.White;
            numericUpDown.ForeColor = Color.Black;
        }

        public static void SelectNumericUpDownDisable(object sender)
        {
            NumericUpDown numericUpDown = (NumericUpDown)sender;
            numericUpDown.BackColor = Color.LightGray;
            numericUpDown.ForeColor = Color.DimGray;
        }

        public static void ClickButtonRoulette(object sender, string keyButton)
        {
            UIForm.TakeScreen(keyButton);
            if (ListAreaElement.Instance.CheckKey(keyButton))
            {
                Button button = (Button)sender;
                button.BackColor = Color.Green;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderColor = Color.Green;
                button.ForeColor = Color.White;
            }
        }

        public static void SelectButtonRoulette(object sender)
        {
            Button button = (Button)sender;
            button.BackColor = Color.Green;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = Color.Green;
            button.ForeColor = Color.White;
        }

        public static void ClickButtonGreenCustom(object sender, string tag)
        {
            UIForm.TakeScreenCustom(tag);
            if (CustomFicheWidgetsContainer.containsTag(tag))
            {
                Button button = (Button)sender;
                button.BackColor = Color.Green;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderColor = Color.Green;
                button.ForeColor = Color.White;
            }
        }

        public static List<Control> FindControlsByTag(Control container, string tagValue)
        {
            List<Control> matchingControls = new List<Control>();
            foreach (object obj in container.Controls)
            {
                Control control = (Control)obj;
                if (control.Tag != null && control.Tag.ToString() == tagValue)
                {
                    matchingControls.Add(control);
                }
                if (control.HasChildren)
                {
                    matchingControls.AddRange(UIForm.FindControlsByTag(control, tagValue));
                }
            }
            return matchingControls;
        }

        public static void EnableItem(Control control)
        {
            if (control.GetType() == typeof(Button) && Runtime.game == 0)
            {
                UIForm.SelectButtonGreen(control);
            }
            if (control.GetType() == typeof(NumericUpDown))
            {
                UIForm.SelectNumericUpDownEnable(control);
            }
            control.Enabled = true;
        }

        public static void EnableRouletteItem(Control control)
        {
            if (control.GetType() == typeof(Button) && control.Tag.Equals("controlInputRoulette") && Runtime.game == 1)
            {
                UIForm.SelectButtonGreen(control);
            }
            if (control.GetType() == typeof(NumericUpDown))
            {
                UIForm.SelectNumericUpDownEnable(control);
            }
            control.Enabled = true;
        }

        public static void DisableItem(Control control)
        {
            if (control.GetType() == typeof(Button) && Runtime.game == 0)
            {
                UIForm.SelectButtonDisable(control);
            }
            if (control.GetType() == typeof(NumericUpDown))
            {
                UIForm.SelectNumericUpDownDisable(control);
            }
            control.Enabled = false;
        }

        public static void DisableRouletteItem(Control control)
        {
            if (control.GetType() == typeof(Button) && control.Tag.Equals("controlInputRoulette") && Runtime.game == 1)
            {
                UIForm.SelectButtonDisable(control);
            }
            if (control.GetType() == typeof(NumericUpDown))
            {
                UIForm.SelectNumericUpDownDisable(control);
            }
            control.Enabled = false;
        }

        public static void DisableAddButtonItem(Button button)
        {
            button.Enabled = false;
        }

        public static void EnableAddButtonItem(Button button)
        {
            button.Enabled = true;
        }

        private static void TakeScreen(string keyButton)
        {
            using (TaskScreenshot.TakeScreenshot screen = new TaskScreenshot.TakeScreenshot(keyButton))
            {
                screen.ShowDialog();
            }
        }

        private static void TakeScreenCustom(string keyButton)
        {
            using (TaskScreenshot.TakeScreenshot screen = new TaskScreenshot.TakeScreenshot(keyButton, 0))
            {
                screen.ShowDialog();
            }
        }

        public static void SendAlert(Constants.EnumAlert stateAlert)
        {
            string messageTelegram = string.Empty;
            Stream stream = null;
            string prefix = ((Runtime.game == 0) ? "TRAD 1" : " TRAD 2");
            int game = Runtime.game;
            string currentBalanceTotal = ((Runtime.game == 0) ? Number.FormatNumberDecimalEuro(Runtime.balanceInit + Runtime.global_profit) : Number.FormatNumberDecimalEuro(RouletteValues.Runtime.balanceInit + RouletteValues.Runtime.global_profit));
            string globalProfit = Number.FormatNumberDecimalEuro((Runtime.game == 0) ? Runtime.global_profit : ((double)RouletteValues.Runtime.global_profit));
            string playingTime = "TEMPO TRASCORSO: " + UIForm.GetTimeElapsed();
            string.Format("pc{0}", Config.indexNamePc);
            string empty = string.Empty;
            switch (stateAlert)
            {
                case Constants.EnumAlert.END_SCULPING:
                    if (!Config.send_end_sculping_message)
                    {
                        return;
                    }
                    messageTelegram = string.Concat(new string[]
                    {
                    "END SCULPING\nSALDO TOTALE: ",
                    currentBalanceTotal,
                    "\nGLOBAL PROFIT: ",
                    globalProfit,
                    "\nSCULPING PROFIT: ",
                    Number.FormatNumberDecimalEuro(Runtime.sculping_profit),
                    "\n",
                    playingTime
                    });
                    stream = new MemoryStream(Resources.Alert_EndSculping);
                    break;
                case Constants.EnumAlert.MARTINGALA_PERSA:
                    messageTelegram = string.Concat(new string[] { "SCALATA PERSA!!!\nSALDO TOTALE: ", currentBalanceTotal, "\nGLOBAL PROFIT: ", globalProfit, "\n", playingTime });
                    stream = new MemoryStream(Resources.Alert_Loss);
                    break;
                case Constants.EnumAlert.MARTINGALA_PERSA_FINE_MAZZO:
                    messageTelegram = string.Concat(new string[] { "SCALATA PERSA SU FINE SESSIONE\nSALDO TOTALE: ", currentBalanceTotal, "\nGLOBAL PROFIT: ", globalProfit, "\n", playingTime });
                    stream = new MemoryStream(Resources.Alert_Loss);
                    break;
                case Constants.EnumAlert.GLOBAL_STOP_WIN:
                    messageTelegram = string.Concat(new string[] { "GLOBAL STOP WIN!!!\nSALDO TOTALE: ", currentBalanceTotal, "\nGLOBAL PROFIT: ", globalProfit, "\n", playingTime });
                    stream = new MemoryStream(Resources.Alert_SoundWin);
                    break;
                case Constants.EnumAlert.GLOBAL_STOP_LOSS:
                    messageTelegram = string.Concat(new string[] { "GLOBAL STOP LOSS!!!\nSALDO TOTALE: ", currentBalanceTotal, "\nGLOBAL PROFIT: ", globalProfit, "\n", playingTime });
                    stream = new MemoryStream(Resources.Alert_Loss);
                    break;
                case Constants.EnumAlert.WAITING_NEW_DECK:
                    messageTelegram = string.Concat(new string[] { "ATTESA!!!\nSALDO TOTALE: ", currentBalanceTotal, "\nGLOBAL PROFIT: ", globalProfit, "\n", playingTime });
                    stream = new MemoryStream(Resources.Alert_CambioMazzo);
                    break;
                case Constants.EnumAlert.INDEX_ALARM:
                    messageTelegram = string.Format("ALLARME COLPO SCALATA\nINDICE SCALATA: {0}\nSALDO TOTALE: {1}", Runtime.martingala_counter + 1, currentBalanceTotal) + "\n" + playingTime;
                    stream = new MemoryStream(Resources.Alert_IndexAlarm);
                    break;
                case Constants.EnumAlert.START_SCULPING:
                    messageTelegram = string.Concat(new string[] { "START SCULPING\nSALDO TOTALE: ", currentBalanceTotal, "\nGLOBAL PROFIT: ", globalProfit, "\n", playingTime });
                    stream = null;
                    break;
                case Constants.EnumAlert.WAITING_TO_START_SCALPING:
                    messageTelegram = string.Concat(new string[] { "NUOVA SESSIONE; RIPRENDO\nSALDO TOTALE: ", currentBalanceTotal, "\nGLOBAL PROFIT: ", globalProfit, "\n", playingTime });
                    stream = null;
                    break;
                case Constants.EnumAlert.ROULETTE_FINE_MANI_GIOCATE:
                    messageTelegram = string.Concat(new string[] { "SCALATA PERSA!!!\nSALDO TOTALE: ", currentBalanceTotal, "\nGLOBAL PROFIT: ", globalProfit, "\n", playingTime });
                    stream = new MemoryStream(Resources.Alert_Loss);
                    break;
                case Constants.EnumAlert.ROULETTE_MANO_GIOCATA_PERSA:
                    messageTelegram = string.Format("BET PERSA: {0}\nSALDO TOTALE: {1}\nGLOBAL PROFIT: {2}", SubStateRoulette.playedHandLvl, currentBalanceTotal, globalProfit) + "\n" + playingTime;
                    stream = null;
                    break;
                case Constants.EnumAlert.START_GAME:
                    messageTelegram = "INIZIO TRADING\n" + playingTime;
                    stream = null;
                    break;
                case Constants.EnumAlert.STOP_GAME:
                    messageTelegram = "STOP TRADING\n" + playingTime;
                    stream = null;
                    break;
                case Constants.EnumAlert.NEW_DECK:
                    messageTelegram = string.Concat(new string[] { "NUOVA SESSIONE; RIPRENDO\nSALDO TOTALE: ", currentBalanceTotal, "\nGLOBAL PROFIT: ", globalProfit, "\n", playingTime });
                    stream = null;
                    break;
            }
            if (stream != null)
            {
                new SoundPlayer(stream).Play();
            }
            if (!string.IsNullOrEmpty(messageTelegram) && Telegram.isRunning)
            {
                Telegram.SendMessage("EUGENIO - " + prefix + " - " + messageTelegram);
            }
        }

        public static void SetStatusBot()
        {
            switch (Runtime.current_state_bot)
            {
                case Constants.EnumStateBot.IDLE:
                    Runtime.labelTextCurrentState = "Stato Bot: " + "In Attesa";
                    return;
                case Constants.EnumStateBot.FIRST_PLAY:
                    Runtime.labelTextCurrentState = "Stato Bot: " + "Start Play";
                    return;
                case Constants.EnumStateBot.SCULPING:
                    Runtime.labelTextCurrentState = "Stato Bot: " + "Sculping";
                    return;
                case Constants.EnumStateBot.PAUSE_SCALPING:
                    Runtime.labelTextCurrentState = "Stato Bot: " + "Pause Sculping";
                    return;
                case Constants.EnumStateBot.SAFE_WIN:
                    Runtime.labelTextCurrentState = "Stato Bot: " + "Safe Win";
                    return;
                case Constants.EnumStateBot.END_DECK:
                    break;
                case Constants.EnumStateBot.WAITING_NEW_DECK:
                    Runtime.labelTextCurrentState = "Stato Bot: " + "Attesa Nuovo Mazzo";
                    return;
                case Constants.EnumStateBot.GLOBAL_STOP_WIN:
                    Runtime.labelTextCurrentState = "Stato Bot: " + "Global Stop Win";
                    return;
                case Constants.EnumStateBot.GLOBAL_STOP_LOSS:
                    Runtime.labelTextCurrentState = "Stato Bot: " + "Global Stop Loss";
                    return;
                case Constants.EnumStateBot.NEW_DECK:
                    Runtime.labelTextCurrentState = "Stato Bot: " + "Inizio Nuovo Mazzo";
                    break;
                default:
                    return;
            }
        }

        public static void SetRouletteStatusBot()
        {
            switch (RouletteValues.Runtime.current_state_bot)
            {
                case RouletteValues.Constants.EnumStateBot.IDLE:
                    RouletteValues.Runtime.labelTextCurrentState = "Stato Bot: " + "In Attesa";
                    return;
                case RouletteValues.Constants.EnumStateBot.RUNNING:
                case RouletteValues.Constants.EnumStateBot.END_DECK:
                    break;
                case RouletteValues.Constants.EnumStateBot.FIRST_PLAY:
                    RouletteValues.Runtime.labelTextCurrentState = "Stato Bot: " + "Start Play";
                    return;
                case RouletteValues.Constants.EnumStateBot.SCULPING:
                    RouletteValues.Runtime.labelTextCurrentState = "Stato Bot: " + "Sculping";
                    return;
                case RouletteValues.Constants.EnumStateBot.PAUSE_SCALPING:
                    RouletteValues.Runtime.labelTextCurrentState = "Stato Bot: " + "Pause Sculping";
                    return;
                case RouletteValues.Constants.EnumStateBot.SAFE_WIN:
                    RouletteValues.Runtime.labelTextCurrentState = "Stato Bot: " + "Safe Win";
                    return;
                case RouletteValues.Constants.EnumStateBot.WAITING_NEW_DECK:
                    RouletteValues.Runtime.labelTextCurrentState = "Stato Bot: " + "Attesa Nuovo Mazzo";
                    return;
                case RouletteValues.Constants.EnumStateBot.GLOBAL_STOP_WIN:
                    RouletteValues.Runtime.labelTextCurrentState = "Stato Bot: " + "Global Stop Win";
                    return;
                case RouletteValues.Constants.EnumStateBot.GLOBAL_STOP_LOSS:
                    RouletteValues.Runtime.labelTextCurrentState = "Stato Bot: " + "Global Stop Loss";
                    break;
                default:
                    return;
            }
        }

        public static string PrintTotalBalance()
        {
            return "Saldo: " + Number.FormatNumberDecimalEuro(Runtime.balance);
        }

        public static string PrintReadBalance()
        {
            return OCReads.balance;
        }

        public static string PrintTotalBalanceRoulette()
        {
            return "Saldo: " + Number.FormatNumberDecimalEuro(RouletteValues.Runtime.balance);
        }

        public static string ReplaceDotIntoCommaValueText(string text)
        {
            return text.Replace(".", ",");
        }

        public static string ReplaceDotAndCommaValueText(string text)
        {
            return text.Replace(".", "").Replace(",", "");
        }

        public static string GetTimeElapsed()
        {
            TimeSpan dt = DateTime.Now.Subtract(Runtime.lastLaunch);
            int totalHours = dt.Days * 24 + dt.Hours;
            return string.Format("{0:00}:{1:00}:{2:00}", totalHours, dt.Minutes, dt.Seconds);
        }

        public static int GetTotalSecondsElapsed()
        {
            return (int)DateTime.Now.Subtract(Runtime.lastLaunch).TotalSeconds;
        }

        private static Stream GetAudioStrem(string resourceName)
        {
            object resourceValue = typeof(Resources).GetProperty(resourceName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(null);
            byte[] byteArray = resourceValue as byte[];
            if (byteArray != null)
            {
                return new MemoryStream(byteArray);
            }
            Stream resourceStream = resourceValue as Stream;
            if (resourceStream != null)
            {
                return resourceStream;
            }
            return null;
        }
    }
}
