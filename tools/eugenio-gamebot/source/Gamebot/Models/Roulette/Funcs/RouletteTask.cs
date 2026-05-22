using Gamebot.Helpers;
using Gamebot.Models.Roulette.Logic;
using Gamebot.Models.Roulette.MouseMove;
using Gamebot.Models.UI;
using Gamebot.UI.WindowForm;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Gamebot.Models.Roulette.Funcs
{
    internal class RouletteTask
    {
        private RouletteTask()
        {
        }

        // (get) Token: 0x0600017D RID: 381 RVA: 0x0001CD0E File Offset: 0x0001AF0E
        public static RouletteTask Instance
        {
            get
            {
                if (RouletteTask.instance == null)
                {
                    RouletteTask.instance = new RouletteTask();
                }
                return RouletteTask.instance;
            }
        }

        public void StartGameBot()
        {
            Log.PrintInfo("(R) START GAME BOT");
            this.form = UpdateInterface.GetInstanceForm();
            this.propertiesTask = new List<RouletteTask.PropertyTask>();
            RouletteValues.Runtime.current_state_bot = RouletteValues.Constants.EnumStateBot.FIRST_PLAY;
            RouletteValues.Runtime.runningOCRScan = true;
            RouletteValues.Runtime.runningStateMachineBot = true;
            RouletteBets.Startup();
            RouletteValues.Runtime.ResetVariables();
            this.StartScan("R_Win", 1000);
            this.StartStateMachineBot();
        }

        public void StopGameBot()
        {
            UIForm.SendAlert(Constants.EnumAlert.STOP_GAME);
            this.StopScan("R_Win");
            this.StopStateMachineBot();
            try
            {
                if (RouletteBets.m != null)
                {
                    RouletteBets.m.KillRiposoRoulette();
                }
            }
            catch (Exception e)
            {
                Log.PrintInfo("(R) StopGameBot: " + e.Message);
            }
            SubStateRoulette.state = 0;
            RouletteValues.Runtime.runningOCRScan = false;
            UIForm.SetRouletteStatusBot();
            MainStateRoulette.UpdateForm();
        }

        private void StartScan(string areaScreen, int delay)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            object lockResource = new object();
            this.propertiesTask.Add(new RouletteTask.PropertyTask
            {
                Key = areaScreen,
                CancellationTokenSource = cancellationTokenSource,
                CancellationToken = cancellationToken
            });
            Task.Run(delegate
            {
                this.TakeOCRArea(cancellationToken, lockResource, areaScreen, delay);
            }, cancellationToken);
        }

        private void StopScan(string areaScreen)
        {
            RouletteTask.PropertyTask property = this.propertiesTask.Find((RouletteTask.PropertyTask item) => item.Key == areaScreen);
            if (property != null && property.CancellationTokenSource != null)
            {
                RouletteValues.Runtime.runningOCRScan = false;
                property.CancellationTokenSource.Cancel();
            }
        }

        private async Task TakeOCRArea(CancellationToken ct, object lockWorker, string areaScreen, int delay)
        {
            try
            {
                AreaElement area = ListAreaElement.Instance.GetAreaByKey(areaScreen);
                OCRScan ocrScan = new OCRScan();
                int width = area.endX - area.startX;
                int height = area.endY - area.startY;
                Rectangle monitorArea = new Rectangle(area.startX, area.startY, width, height);
                while (RouletteValues.Runtime.runningOCRScan && !ct.IsCancellationRequested)
                {
                    lock (lockWorker)
                    {
                        Bitmap currentImage = Monitor.Instance.CaptureScreen(monitorArea);
                        if (areaScreen.Equals("R_Win"))
                        {
                            OCRResponse ocrResponse = ocrScan.GetTextFromBitmapRoulette(currentImage);
                            RouletteValues.OCReads.label_winner = string.Empty;
                            if (ocrResponse.GetResponse().SuccessScan)
                            {
                                RouletteValues.OCReads.label_winner = ocrResponse.GetResponse().Message.Trim();
                            }
                        }
                        currentImage.Dispose();
                    }
                    await Task.Delay(delay);
                }
                if (ct.IsCancellationRequested)
                {
                    Log.PrintInfo("KILL OCR");
                    RouletteTask.PropertyTask property = this.propertiesTask.Find((RouletteTask.PropertyTask item) => item.Key == areaScreen);
                    if (property != null)
                    {
                        Log.PrintInfo("(R) TASK CANCELED: " + ct.ToString());
                        property.CancellationTokenSource.Cancel();
                        property.CancellationTokenSource.Dispose();
                        this.propertiesTask.Remove(property);
                        return;
                    }
                }
                ocrScan = null;
                monitorArea = default(Rectangle);
            }
            catch (Exception ex)
            {
                Log.PrintInfo("(R) - EXCEPTION IN OCR :");
                Log.PrintInfo(ex.Message);
            }
        }

        private void StartStateMachineBot()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            new object();
            Log.DeleteOldLog();
            this.propertiesTask.Add(new RouletteTask.PropertyTask
            {
                Key = "STATE_MACHINE",
                CancellationTokenSource = cancellationTokenSource,
                CancellationToken = cancellationToken
            });
            RouletteValues.Runtime.balance = RouletteValues.Runtime.balanceInit + RouletteValues.Runtime.global_profit;
            MainStateRoulette.UpdateForm();
            Task.Run(delegate
            {
                this.TaskStateMachine(cancellationToken);
            }, cancellationToken);
        }

        private void StopStateMachineBot()
        {
            RouletteTask.PropertyTask property = this.propertiesTask.Find((RouletteTask.PropertyTask item) => item.Key == "STATE_MACHINE");
            if (property != null && property.CancellationTokenSource != null)
            {
                RouletteValues.Runtime.runningStateMachineBot = false;
                property.CancellationTokenSource.Cancel();
                Log.PrintInfo("(R) RICHIESTA CANCELLAZIONE TASK MAIN");
            }
        }

        private void TaskStateMachine(CancellationToken ct)
        {
            Log.PrintInfo("***************** START ROULETTE GAME BOT **********************");
            Runtime.lastLaunch = DateTime.Now;
            UIForm.SendAlert(Constants.EnumAlert.START_GAME);
            while (RouletteValues.Runtime.runningStateMachineBot)
            {
                if (ct.IsCancellationRequested)
                {
                    Log.PrintInfo("(R) TASK CANCELED: " + ct.ToString());
                    break;
                }
                MainStateRoulette.StateMachine();
            }
            if (ct.IsCancellationRequested)
            {
                RouletteTask.PropertyTask property = this.propertiesTask.Find((RouletteTask.PropertyTask item) => item.Key == "STATE_MACHINE");
                if (property != null)
                {
                    property.CancellationTokenSource.Cancel();
                    property.CancellationTokenSource.Dispose();
                    this.propertiesTask.Remove(property);
                }
            }
            RouletteValues.Runtime.current_state_bot = RouletteValues.Constants.EnumStateBot.IDLE;
            Log.PrintInfo("(R) UCCISO TUTTO");
        }

        private List<RouletteTask.PropertyTask> propertiesTask = new List<RouletteTask.PropertyTask>();

        private Configuratore form;

        private static RouletteTask instance;

        private class PropertyTask
        {
            // (get) Token: 0x06000386 RID: 902 RVA: 0x0002354A File Offset: 0x0002174A
            // (set) Token: 0x06000387 RID: 903 RVA: 0x00023552 File Offset: 0x00021752
            public string Key { get; set; }

            // (get) Token: 0x06000388 RID: 904 RVA: 0x0002355B File Offset: 0x0002175B
            // (set) Token: 0x06000389 RID: 905 RVA: 0x00023563 File Offset: 0x00021763
            public CancellationTokenSource CancellationTokenSource { get; set; }

            // (get) Token: 0x0600038A RID: 906 RVA: 0x0002356C File Offset: 0x0002176C
            // (set) Token: 0x0600038B RID: 907 RVA: 0x00023574 File Offset: 0x00021774
            public CancellationToken CancellationToken { get; set; }
        }
    }
}
