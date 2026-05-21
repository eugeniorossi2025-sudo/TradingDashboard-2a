using Gamebot.Helpers;
using Gamebot.Models.MainState;
using Gamebot.Models.MouseMove;
using Gamebot.UI.WindowForm;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Gamebot.Models.UI
{
    internal class WorkerTask
    {
        private WorkerTask()
        {
        }

        // (get) Token: 0x06000126 RID: 294 RVA: 0x0001A742 File Offset: 0x00018942
        public static WorkerTask Instance
        {
            get
            {
                if (WorkerTask.instance == null)
                {
                    WorkerTask.instance = new WorkerTask();
                }
                return WorkerTask.instance;
            }
        }

        public void StartGameBot()
        {
            object _lockObj = new object();
            this.form = UpdateInterface.GetInstanceForm();
            this.propertiesTask = new List<WorkerTask.PropertyTask>();
            Runtime.current_state_bot = Constants.EnumStateBot.FIRST_PLAY;
            Runtime.runningOCRScan = true;
            Runtime.runningStateMachineBot = true;
            Runtime.number_deck = 0;
            OCReads.number_deck = 0;
            Runtime.start_new_deck = false;
            Runtime.currentNumberDeck = -2;
            Bets.Startup();
            Runtime.ResetVariables();
            this.StartScan("AREA_MAZZO", 250, _lockObj);
            this.StartScan("AREA_VINCITA", 150, _lockObj);
            this.StartScan("AREA_PUNTARE", 150, _lockObj);
            this.StartTimeElapsedBot();
            this.StartStateMachineBot();
        }

        public void KillBot(Configuratore  fm)
        {
            try
            {
                fm.SettingUIStop();
                fm.RouletteSettingUIStop();
            }
            catch (Exception)
            {
            }
        }

        public void StopGameBot()
        {
            UIForm.SendAlert(Constants.EnumAlert.STOP_GAME);
            this.StopScan("AREA_MAZZO");
            this.StopScan("AREA_VINCITA");
            this.StopScan("AREA_PUNTARE");
            this.StopTimeElapsedBot();
            this.StopStateMachineBot();
            try
            {
                if (Bets.m != null)
                {
                    Bets.m.KillRiposo();
                }
            }
            catch (Exception)
            {
                Log.PrintErrorLog("StopGameBot", "Stop MOUSE", "");
            }
            Runtime.runningOCRScan = false;
            Runtime.number_deck = 0;
            Runtime.current_state_bot = Constants.EnumStateBot.IDLE;
            UIForm.SetStatusBot();
            MainStateBot.UpdateForm();
        }

        private void StartScan(string areaScreen, int delay, object _lock)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            this.propertiesTask.Add(new WorkerTask.PropertyTask
            {
                Key = areaScreen,
                CancellationTokenSource = cancellationTokenSource,
                CancellationToken = cancellationToken
            });
            Task.Run(delegate
            {
                this.TakeOCRArea(cancellationToken, _lock, areaScreen, delay);
            }, cancellationToken);
        }

        private void StopScan(string areaScreen)
        {
            WorkerTask.PropertyTask property = this.propertiesTask.Find((WorkerTask.PropertyTask item) => item.Key == areaScreen);
            if (property != null && property.CancellationTokenSource != null)
            {
                Runtime.runningOCRScan = false;
                property.CancellationTokenSource.Cancel();
            }
        }

        private async Task TakeOCRArea(CancellationToken ct, object lockWorker, string areaScreen, int delay)
        {
            AreaElement area = ListAreaElement.Instance.GetAreaByKey(areaScreen);
            if (!(areaScreen == "AREA_SALDO") || area != null)
            {
                OCRScan ocrScan = new OCRScan();
                int width = area.endX - area.startX;
                int height = area.endY - area.startY;
                Rectangle monitorArea = new Rectangle(area.startX, area.startY, width, height);
                int number_deck = -1;
                int saldo_letto = -1;
                bool res = false;
                while (Runtime.runningOCRScan && !ct.IsCancellationRequested)
                {
                    lock (lockWorker)
                    {
                        Bitmap currentImage = Monitor.Instance.CaptureScreen(monitorArea);
                        if (areaScreen.Equals("AREA_VINCITA"))
                        {
                            OCRResponse ocrResponse = (Config.enableFilterPragmatic ? ocrScan.GetTextFromBitmapWinAreaPragmatic(currentImage, true, false, "") : ocrScan.GetTextFromBitmapWinAreaDefault(currentImage, true, false, ""));
                            OCReads.label_winner = string.Empty;
                            if (ocrResponse.GetResponse().SuccessScan)
                            {
                                OCReads.label_winner = ocrResponse.GetResponse().Message.Trim();
                            }
                        }
                        if (areaScreen.Equals("AREA_PUNTARE"))
                        {
                            OCRResponse ocrResponse2 = (Config.enableFilterPragmatic ? ocrScan.GetTextFromBitmapBetAreaPragmatic(currentImage, true, false, "") : ocrScan.GetTextFromBitmapBetAreaDefault(currentImage, true, false, ""));
                            OCReads.label_bet = string.Empty;
                            if (ocrResponse2.GetResponse().SuccessScan)
                            {
                                OCReads.label_bet = ocrResponse2.GetResponse().Message.Trim();
                            }
                        }
                        if (areaScreen.Equals("AREA_MAZZO"))
                        {
                            OCRResponse ocrResponse3 = (Config.enableFilterPragmatic ? ocrScan.GetTextFromBitmapNumberDeckPragmatic(currentImage, false, "") : ocrScan.GetTextFromBitmapNumberDeckDefault(currentImage, false, ""));
                            OCReads.number_deck = -1;
                            if (ocrResponse3.GetResponse().SuccessScan)
                            {
                                res = int.TryParse(ocrResponse3.GetResponse().Message.Replace("#", "").Replace("H", "").Trim(), out number_deck);
                                OCReads.number_deck = (res ? number_deck : (-1));
                                number_deck = -1;
                            }
                        }
                        if (areaScreen.Equals("AREA_SALDO"))
                        {
                            OCRResponse ocrResponse4 = ocrScan.GetTextFromBitmapAreaSaldo(currentImage, false, false, "balance");
                            OCReads.balance = "-1";
                            if (ocrResponse4.GetResponse().SuccessScan)
                            {
                                res = int.TryParse(ocrResponse4.GetResponse().Message.Trim().Replace("#", ""), out saldo_letto);
                                try
                                {
                                    OCReads.balance = ocrResponse4.GetResponse().Message.Replace(".", "").Trim();
                                    if (OCReads.balance.Equals(Runtime.readSaldo))
                                    {
                                        Runtime.ocrBalanceCorrect++;
                                    }
                                    else
                                    {
                                        Runtime.ocrBalanceIncorrect++;
                                    }
                                    List<string> values = new List<string>();
                                    UpdateInterface.GetInstanceForm().progressBalance.Report(values);
                                }
                                catch (Exception)
                                {
                                    OCReads.balance = (res ? Convert.ToString(saldo_letto) : "-1");
                                }
                                number_deck = -1;
                            }
                        }
                        currentImage.Dispose();
                    }
                    await Task.Delay(delay);
                }
                if (ct.IsCancellationRequested)
                {
                    WorkerTask.PropertyTask property = this.propertiesTask.Find((WorkerTask.PropertyTask item) => item.Key == areaScreen);
                    if (property != null)
                    {
                        property.CancellationTokenSource.Cancel();
                        property.CancellationTokenSource.Dispose();
                        this.propertiesTask.Remove(property);
                    }
                }
            }
        }

        private void StartStateMachineBot()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            new object();
            Log.DeleteOldLog();
            this.propertiesTask.Add(new WorkerTask.PropertyTask
            {
                Key = "STATE_MACHINE",
                CancellationTokenSource = cancellationTokenSource,
                CancellationToken = cancellationToken
            });
            Task.Run(delegate
            {
                this.TaskStateMachine(cancellationToken);
            }, cancellationToken);
        }

        private void StopStateMachineBot()
        {
            WorkerTask.PropertyTask property = this.propertiesTask.Find((WorkerTask.PropertyTask item) => item.Key == "STATE_MACHINE");
            if (property != null && property.CancellationTokenSource != null)
            {
                Runtime.runningStateMachineBot = false;
                property.CancellationTokenSource.Cancel();
                Log.PrintInfo("RICHIESTA CANCELLAZIONE TASK MAIN");
            }
        }

        private void TaskStateMachine(CancellationToken ct)
        {
            Log.PrintInfo("*****************START GAME BOT**********************");
            Runtime.lastLaunch = DateTime.Now;
            UIForm.SendAlert(Constants.EnumAlert.START_GAME);
            while (Runtime.runningStateMachineBot)
            {
                if (ct.IsCancellationRequested)
                {
                    Log.PrintInfo("TASK CANCELED: " + ct.ToString());
                    break;
                }
                if (Runtime.checkForNewAction)
                {
                    Runtime.checkForNewAction  = false;
                    
                }
                MainStateBot.StateMachine();
            }
            if (ct.IsCancellationRequested)
            {
                WorkerTask.PropertyTask property = this.propertiesTask.Find((WorkerTask.PropertyTask item) => item.Key == "STATE_MACHINE");
                if (property != null)
                {
                    property.CancellationTokenSource.Cancel();
                    property.CancellationTokenSource.Dispose();
                    this.propertiesTask.Remove(property);
                }
            }
            Runtime.current_state_bot = Constants.EnumStateBot.IDLE;
            Log.PrintInfo("UCCISO TUTTO");
        }

        private void StartTimeElapsedBot()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            new object();
            Runtime.runningTimeElapsed = true;
            Log.DeleteOldLog();
            this.propertiesTask.Add(new WorkerTask.PropertyTask
            {
                Key = "TIME_ELAPSED",
                CancellationTokenSource = cancellationTokenSource,
                CancellationToken = cancellationToken
            });
            Task.Run(delegate
            {
                this.TaskTimeElapsed(cancellationToken);
            }, cancellationToken);
        }

        private void StopTimeElapsedBot()
        {
            WorkerTask.PropertyTask property = this.propertiesTask.Find((WorkerTask.PropertyTask item) => item.Key == "TIME_ELAPSED");
            if (property != null && property.CancellationTokenSource != null)
            {
                Runtime.runningTimeElapsed = false;
                property.CancellationTokenSource.Cancel();
            }
        }

        private async Task TaskTimeElapsed(CancellationToken ct)
        {
            while (Runtime.runningTimeElapsed)
            {
                MainStateBot.UpdateTimeElapsed();
                if (ct.IsCancellationRequested)
                {
                    Log.PrintInfo("TASK CANCELED: " + ct.ToString());
                    break;
                }
                await Task.Delay(1000);
            }
            if (ct.IsCancellationRequested)
            {
                WorkerTask.PropertyTask property = this.propertiesTask.Find((WorkerTask.PropertyTask item) => item.Key == "TIME_ELAPSED");
                if (property != null)
                {
                    property.CancellationTokenSource.Cancel();
                    property.CancellationTokenSource.Dispose();
                    this.propertiesTask.Remove(property);
                }
            }
        }

        private List<WorkerTask.PropertyTask> propertiesTask = new List<WorkerTask.PropertyTask>();

        private Configuratore  form;

        private static WorkerTask instance;

        private class PropertyTask
        {
            // (get) Token: 0x06000357 RID: 855 RVA: 0x0002212C File Offset: 0x0002032C
            // (set) Token: 0x06000358 RID: 856 RVA: 0x00022134 File Offset: 0x00020334
            public string Key { get; set; }

            // (get) Token: 0x06000359 RID: 857 RVA: 0x0002213D File Offset: 0x0002033D
            // (set) Token: 0x0600035A RID: 858 RVA: 0x00022145 File Offset: 0x00020345
            public CancellationTokenSource CancellationTokenSource { get; set; }

            // (get) Token: 0x0600035B RID: 859 RVA: 0x0002214E File Offset: 0x0002034E
            // (set) Token: 0x0600035C RID: 860 RVA: 0x00022156 File Offset: 0x00020356
            public CancellationToken CancellationToken { get; set; }
        }
    }
}
