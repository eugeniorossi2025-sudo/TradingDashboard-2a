using Gamebot.Configuration;
using Gamebot.Helpers;
using Gamebot.Models.MainState;
using Gamebot.Models.Roulette;
using Gamebot.Models.Roulette.Funcs;
using Gamebot.Models.Roulette.Logic;
using Gamebot.Models.UI;
using Gamebot.UI.WindowForm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TL;
using WTelegram;

namespace Gamebot.Models.Communication
{
    internal static class Telegram
    {
        private static Client client;

        private static User myself;

        private static Messages_Chats messagesChats;

        private static Messages_Dialogs dialogs;

        public static bool isRunning = false;

        private static readonly Dictionary<long, User> Users = new Dictionary<long, User>();

        private static readonly Dictionary<long, ChatBase> Chats = new Dictionary<long, ChatBase>();

        public static async Task<bool> Main(string[] _, bool sendOnly)
        {
            client = new Client(Config);
            client.OnUpdates += Client_OnUpdate;
            try
            {
                myself = await client.LoginUserIfNeeded();
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("PHONE_NUMBER_INVALID"))
                {
                    MessageBox.Show("NUMERO DI TELEFONO NON VALIDO.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                else if (ex.Message.Equals("PHONE_CODE_INVALID"))
                {
                    if (!sendOnly)
                    {
                        MessageBox.Show("CODICE DI VERIFICA ERRATO.\nCONTROLLARE TELEGRAM SU TELEFONO E RIPROVARE.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    }
                }
                else if (ex.Message.Equals("FLOOD_WAIT_X"))
                {
                    MessageBox.Show("HAI SUPERATO IL LIMITE DI CONNESSIONI CONCESSE DA TELEGRAM PER QUESTO DISPOSITIVO.\nRIPROVA FRA 24-48 ORE.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                else if (ex.Message.StartsWith("FLOOD_WAIT_"))
                {
                    MessageBox.Show("HAI SUPERATO IL LIMITE DI CONNESSIONI CONCESSE DA TELEGRAM PER QUESTO DISPOSITIVO.\nRIPROVA FRA 1 ORA.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                else
                {
                    Log.PrintInfo("TG - * * * UNEXPECTED EXCEPTION * * * ");
                    Log.PrintInfo("TG - mess : " + ex.Message);
                    foreach (string k in ex.Data.Keys)
                    {
                        Log.PrintInfo("TG - data : " + k + " : " + ex.Data[k]);
                    }
                    Log.PrintInfo("TG - stack : ");
                    Log.PrintInfo(ex.StackTrace);
                    MessageBox.Show("SI E' VERIFICATA UNA ECCEZIONE DI TIPO " + ex.Message + ". SI PREGA DI CONTATTARE L'AMMINISTRATORE.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                UpdateInterface.GetInstanceForm().SettingTSStopped();
                DisposeUser();
                return false;
            }
            try
            {
                messagesChats = await client.Messages_GetAllChats();
                dialogs = await client.Messages_GetAllDialogs();
                Users[myself.id] = myself;
                dialogs.CollectUsersChats(Users, Chats);
                Log.PrintInfo("TG - Dialogs OK");
            }
            catch (Exception ex2)
            {
                Log.PrintInfo("TG - * * * EXCEPTION * * * ");
                Log.PrintInfo(ex2.StackTrace);
            }
            try
            {
                await SendMessage("Inizializzazione chat");
                Log.PrintInfo("TG -Inizializzazione chat");
                isRunning = true;
                return true;
            }
            catch (Exception ex3)
            {
                isRunning = false;
                Log.PrintInfo("TG - * * * EXCEPTION * * * ");
                Log.PrintInfo("TG -\n" + ex3.StackTrace);
                return false;
            }
        }

        private static string Config(string what)
        {
            string text = Path.Combine(Constants.PathProject(), "telegramSession");
            Directory.CreateDirectory(text);
            string telegramFileSession = Path.Combine(text, "WTelegram.session");
            return what switch
            {
                "api_id" => TelegramConfig.API_ID,
                "api_hash" => TelegramConfig.API_HASH,
                "phone_number" => Gamebot.Models.Config.insert_number,
                "verification_code" => Gamebot.Models.Config.verified_code,
                "first_name" => "",
                "last_name" => "",
                "password" => "",
                "session_pathname" => telegramFileSession,
                _ => null,
            };
        }

        public static async Task SendMessage(string msgToSend)
        {
            UpdateData();
            try
            {
                bool found = false;
                foreach (KeyValuePair<long, ChatBase> chat in messagesChats.chats)
                {
                    if (chat.Value.IsActive && chat.Value.Title.Equals(Gamebot.Models.Config.groupchatname))
                    {
                        Gamebot.Models.Config.selected_chat = chat.Key;
                        found = true;
                        break;
                    }
                }
                if (found)
                {
                    ChatBase target = messagesChats.chats[Gamebot.Models.Config.selected_chat];
                    await client.SendMessageAsync(target, msgToSend);
                }
            }
            catch (Exception)
            {
                Log.PrintInfo("RICEVUTO COMANDO TELEGRAM PRIMA DI INIZIALIZZAZIONE TELEGRAM");
            }
        }

        public static void DisposeUser()
        {
            Log.PrintInfo("TELEGRAM - DU");
            client.Auth_LogOut();
            client.Dispose();
            isRunning = false;
        }

        private static void UpdateData()
        {
            try
            {
                Configuratore instanceForm = UpdateInterface.GetInstanceForm();
                Gamebot.Models.Config.insert_number = instanceForm.textActualPhone.Text;
                Gamebot.Models.Config.verified_code = instanceForm.textVerifiedCode.Text;
                Gamebot.Models.Config.groupchatname = instanceForm.textChatName.Text;
            }
            catch (Exception)
            {
            }
        }

        private static async Task Client_OnUpdate(IObject arg)
        {
            if (arg == null || !(arg is UpdatesBase updates))
            {
                return;
            }
            updates.CollectUsersChats(Users, Chats);
            Update[] updateList = updates.UpdateList;
            foreach (Update updateCommand in updateList)
            {
                if (updateCommand.GetType() == typeof(UpdateNewMessage))
                {
                    await ManageMessage(((UpdateNewMessage)updateCommand).message);
                }
            }
        }

        private static Task ManageMessage(MessageBase messageTelegram)
        {
            if (messageTelegram == null || messageTelegram.GetType() != typeof(TL.Message))
            {
                return null;
            }
            TL.Message message = (TL.Message)messageTelegram;
            Log.PrintInfo("RICEVUTO COMANDO TELEGRAM: " + message.message);
            string prefix = ((Runtime.game == 0) ? "BAC" : "ROU");
            string prefixMessage = "EUGENIO - " + prefix + " - COMANDO: " + message.message + "\n\n";
            if (TelegramConfig.COMMAND.Where((string item) => item.Contains(message.message, StringComparison.OrdinalIgnoreCase) && message.message.ToLower() == "#stop").FirstOrDefault() != null)
            {
                if (Runtime.game == 0)
                {
                    if (Runtime.current_state_bot == Constants.EnumStateBot.IDLE)
                    {
                        SendMessage(prefixMessage + "BOT GIA' FERMATO");
                    }
                    else
                    {
                        SendMessage(prefixMessage + "STOP BOT");
                        UpdateInterface.GetInstanceForm().SettingUIStop();
                        Player.Instance.Stop();
                        MainStateBot.UpdateForm();
                    }
                }
                if (Runtime.game == 1)
                {
                    if (!RouletteValues.Runtime.runningStateMachineBot)
                    {
                        SendMessage(prefixMessage + "BOT GIA' FERMATO");
                    }
                    else
                    {
                        SendMessage(prefixMessage + "STOP BOT");
                        UpdateInterface.GetInstanceForm().RouletteSettingUIStop();
                        RoulettePlayer.Instance.Stop();
                        MainStateRoulette.UpdateForm();
                    }
                }
            }
            if (TelegramConfig.COMMAND.Where((string item) => item.Contains(message.message, StringComparison.OrdinalIgnoreCase) && message.message.ToLower() == "#saldo").FirstOrDefault() != null)
            {
                string playingTime = "TEMPO TRASCORSO: " + UIForm.GetTimeElapsed();
                if (Runtime.game == 0)
                {
                    SendMessage($"{prefixMessage}SALDO INIZIALE: {Number.FormatNumberDecimalEuro(Runtime.balanceInit)}\nSALDO ATTUALE: {Number.FormatNumberDecimalEuro(Runtime.balance)}\nGLOBAL PROFIT: {Number.FormatNumberDecimalEuro(Runtime.global_profit)}\nSCULPING PROFIT: {Number.FormatNumberDecimalEuro(Runtime.sculping_profit)}\nN. MAZZO: {Runtime.number_deck}\nINDICE MARTINGALA: {Runtime.martingala_counter}\nSTATO BOT: {Runtime.current_state_bot}\n{playingTime}");
                }
                else if (Runtime.game == 1)
                {
                    SendMessage($"{prefixMessage}SALDO INIZIALE: {Number.FormatNumberDecimalEuro(RouletteValues.Runtime.balanceInit)}\nSALDO ATTUALE: {Number.FormatNumberDecimalEuro(RouletteValues.Runtime.balance)}\nGLOBAL PROFIT: {Number.FormatNumberDecimalEuro(RouletteValues.Runtime.global_profit)}\nMANO GIOCATA: {SubStateRoulette.playedHandLvl}\nNUMERO VINCITE: {RouletteValues.Runtime.numero_vincite}\nNUMERO PERDITE: {RouletteValues.Runtime.numero_perdite}\nSTATO BOT: {SubStateRoulette.GetState(SubStateRoulette.state)}\n{playingTime}");
                }
            }
            if (TelegramConfig.COMMAND.Where((string item) => item.Contains(message.message, StringComparison.OrdinalIgnoreCase) && message.message.ToLower() == "#comandi").FirstOrDefault() != null)
            {
                SendMessage(prefixMessage + "COMANDI ACCETTATI:\n" + string.Join("\n", TelegramConfig.COMMAND));
            }
            return Task.CompletedTask;
        }
    }
}
