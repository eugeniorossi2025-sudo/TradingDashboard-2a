using Gamebot.Helpers;
using Gamebot.Models.Objects;
using Gamebot.Models.Roulette.Funcs;
using Gamebot.Models.UI;
using System;
using System.Drawing;
using System.Security;
using System.Threading;

namespace Gamebot.Models.MouseMove
{
    internal class Move
    {
        private enum EnumMouse
        {
            AREA_CENTRALE,
            FICHE_1,
            FICHE_5,
            FICHE_25,
            FICHE_100,
            FICHE_250,
            FICHE_500,
            AREA_RADDOPPIO,
            AREA_BLU,
            AREA_RED,
            AREA_RIPOSO
        }

        private enum EnumMouseRoulette
        {
            AREA_RIPOSO,
            AREA_CENTRALE,
            AREA_MANO_1,
            AREA_MANO_2,
            AREA_MANO_3
        }

        private Thread t;

        private bool isRiposando;

        private bool isRiposandoRoulette;

        private static Move instance;

        private Mouse mouse = new Mouse();

        private Random rnd = new Random();

        private int x;

        private int y;
        
        private DateTime lastBannerRemoveTime = DateTime.Now;

        public static Move Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Move();
                }
                return instance;
            }
        }

        private Move()
        {
            Log.PrintInfo("GAME TYPE : " + ((Runtime.game == 0) ? "BACCARAT" : "ROULETTE"));
            t = ((Runtime.game == 0) ? new Thread(Riposa) : new Thread(RiposaRoulette));
        }

        public static void Dealloc()
        {
            instance = null;
        }

        public void MoveCentrale()
        {
            SetCoords(EnumMouse.AREA_CENTRALE);
            MoveAndStop();
        }

        public void MoveFish1()
        {
            SetCoords(EnumMouse.FICHE_1);
            MoveAndStop();
        }

        public void MoveFish5()
        {
            SetCoords(EnumMouse.FICHE_5);
            MoveAndStop();
        }

        public void MoveFish25()
        {
            SetCoords(EnumMouse.FICHE_25);
            MoveAndStop();
        }

        public void MoveFish100()
        {
            SetCoords(EnumMouse.FICHE_100);
            MoveAndStop();
        }

        public void MoveFish250()
        {
            SetCoords(EnumMouse.FICHE_250);
            MoveAndStop();
        }

        public void MoveFish500()
        {
            SetCoords(EnumMouse.FICHE_500);
            MoveAndStop();
        }

        public void MoveFishRaddoppia()
        {
            SetCoords(EnumMouse.AREA_RADDOPPIO);
            MoveAndStop();
        }

        public void RemoveTimeoutBanner()
        {
            if (DateTime.Now - lastBannerRemoveTime > TimeSpan.FromSeconds(120))
            {
                lastBannerRemoveTime = DateTime.Now;
                MoveRiposo();
                mouse.Click();
            }
        }

        public void MoveFishCustom(double value)
        {
            CustomFicheWidget cfw = CustomFicheWidgetsContainer.getCustomFicheWidgetByValue(value);
            if (cfw != null)
            {
                AreaElement ae = cfw.getArea();
                x = rnd.Next(ae.startX, ae.endX);
                y = rnd.Next(ae.startY, ae.endY);
                MoveAndStop();
            }
        }

        public void MoveRed()
        {
            SetCoords(EnumMouse.AREA_RED);
            MoveAndStop();
        }

        public void MoveBlu()
        {
            SetCoords(EnumMouse.AREA_BLU);
            MoveAndStop();
        }

        public void MoveRiposo()
        {
            SetCoords(EnumMouse.AREA_RIPOSO);
            MoveAndStop();
        }

        public void MoveRiposoRoulette()
        {
            SetCoordsRoulette(EnumMouseRoulette.AREA_RIPOSO);
            MoveAndStop();
        }

        public void MoveRouletteHand1()
        {
            SetCoordsRoulette(EnumMouseRoulette.AREA_MANO_1);
            MoveAndStop();
        }

        public void MoveRouletteHand2()
        {
            SetCoordsRoulette(EnumMouseRoulette.AREA_MANO_2);
            MoveAndStop();
        }

        public void MoveRouletteHand3()
        {
            SetCoordsRoulette(EnumMouseRoulette.AREA_MANO_3);
            MoveAndStop();
        }

        private void SetCoords(EnumMouse where)
        {
            switch (where)
            {
                case EnumMouse.FICHE_1:
                    {
                        AreaElement areaFiche505 = ListAreaElement.Instance.GetAreaByKey("FICHE_1");
                        x = rnd.Next(areaFiche505.startX, areaFiche505.endX);
                        y = rnd.Next(areaFiche505.startY, areaFiche505.endY);
                        break;
                    }
                case EnumMouse.FICHE_5:
                    {
                        AreaElement areaFiche504 = ListAreaElement.Instance.GetAreaByKey("FICHE_5");
                        x = rnd.Next(areaFiche504.startX, areaFiche504.endX);
                        y = rnd.Next(areaFiche504.startY, areaFiche504.endY);
                        break;
                    }
                case EnumMouse.FICHE_25:
                    {
                        AreaElement areaFiche503 = ListAreaElement.Instance.GetAreaByKey("FICHE_25");
                        x = rnd.Next(areaFiche503.startX, areaFiche503.endX);
                        y = rnd.Next(areaFiche503.startY, areaFiche503.endY);
                        break;
                    }
                case EnumMouse.FICHE_100:
                    {
                        AreaElement areaFiche502 = ListAreaElement.Instance.GetAreaByKey("FICHE_100");
                        x = rnd.Next(areaFiche502.startX, areaFiche502.endX);
                        y = rnd.Next(areaFiche502.startY, areaFiche502.endY);
                        break;
                    }
                case EnumMouse.FICHE_250:
                    {
                        AreaElement areaFiche501 = ListAreaElement.Instance.GetAreaByKey("FICHE_250");
                        x = rnd.Next(areaFiche501.startX, areaFiche501.endX);
                        y = rnd.Next(areaFiche501.startY, areaFiche501.endY);
                        break;
                    }
                case EnumMouse.FICHE_500:
                    {
                        AreaElement areaFiche500 = ListAreaElement.Instance.GetAreaByKey("FICHE_500");
                        x = rnd.Next(areaFiche500.startX, areaFiche500.endX);
                        y = rnd.Next(areaFiche500.startY, areaFiche500.endY);
                        break;
                    }
                case EnumMouse.AREA_RADDOPPIO:
                    {
                        AreaElement areaRaddoppio = ListAreaElement.Instance.GetAreaByKey("AREA_RADDOPPIO");
                        x = rnd.Next(areaRaddoppio.startX, areaRaddoppio.endX);
                        y = rnd.Next(areaRaddoppio.startY, areaRaddoppio.endY);
                        break;
                    }
                case EnumMouse.AREA_BLU:
                    {
                        AreaElement areaBlu = ListAreaElement.Instance.GetAreaByKey("BLU");
                        x = rnd.Next(areaBlu.startX, areaBlu.endX);
                        y = rnd.Next(areaBlu.startY, areaBlu.endY);
                        break;
                    }
                case EnumMouse.AREA_RED:
                    {
                        AreaElement areaRed = ListAreaElement.Instance.GetAreaByKey("ROSSO");
                        x = rnd.Next(areaRed.startX, areaRed.endX);
                        y = rnd.Next(areaRed.startY, areaRed.endY);
                        break;
                    }
                case EnumMouse.AREA_RIPOSO:
                    {
                        AreaElement areaRiposo = ListAreaElement.Instance.GetAreaByKey("AREA_CENTRALE");
                        x = rnd.Next(areaRiposo.startX, areaRiposo.endX);
                        y = rnd.Next(areaRiposo.startY, areaRiposo.endY);
                        break;
                    }
            }
        }

        private void SetCoordsRoulette(EnumMouseRoulette where)
        {
            switch (where)
            {
                case EnumMouseRoulette.AREA_RIPOSO:
                    {
                        AreaElement areaRiposo = ListAreaElement.Instance.GetAreaByKey("R_Wait");
                        x = rnd.Next(areaRiposo.startX, areaRiposo.endX);
                        y = rnd.Next(areaRiposo.startY, areaRiposo.endY);
                        break;
                    }
                case EnumMouseRoulette.AREA_CENTRALE:
                    {
                        AreaElement areaCentrale = ListAreaElement.Instance.GetAreaByKey("R_Win");
                        x = rnd.Next(areaCentrale.startX, areaCentrale.endX);
                        y = rnd.Next(areaCentrale.startY, areaCentrale.endY);
                        break;
                    }
                case EnumMouseRoulette.AREA_MANO_1:
                    {
                        AreaElement areaMano5 = ListAreaElement.Instance.GetAreaByKey("R_Hand1");
                        x = rnd.Next(areaMano5.startX, areaMano5.endX);
                        y = rnd.Next(areaMano5.startY, areaMano5.endY);
                        break;
                    }
                case EnumMouseRoulette.AREA_MANO_2:
                    {
                        AreaElement areaMano4 = ListAreaElement.Instance.GetAreaByKey("R_Hand2");
                        x = rnd.Next(areaMano4.startX, areaMano4.endX);
                        y = rnd.Next(areaMano4.startY, areaMano4.endY);
                        break;
                    }
                case EnumMouseRoulette.AREA_MANO_3:
                    {
                        AreaElement areaMano3 = ListAreaElement.Instance.GetAreaByKey("R_Hand3");
                        x = rnd.Next(areaMano3.startX, areaMano3.endX);
                        y = rnd.Next(areaMano3.startY, areaMano3.endY);
                        break;
                    }
            }
        }

        private void MoveAndStop()
        {
            Point toGo = new Point(x, y);
            mouse.MoveTo(toGo);
            Thread.Sleep(200 + rnd.Next(100));
        }

        public void Click()
        {
            bool enableClick = true;
            enableClick = Config.enableClick;
            if (Config.baccaratDemoEnabled)
            {
                enableClick = false;
            }
            if (enableClick)
            {
                mouse.Click();
            }
            Thread.Sleep(200 + rnd.Next(100));
        }

        public void ActivateStartRiposa()
        {
            isRiposando = true;
            t.Start();
        }

        public void ActivateRiposa()
        {
            isRiposando = true;
            t.Resume();
        }

        public void DeactivateRiposa()
        {
            isRiposando = false;
            try
            {
                t.Suspend();
            }
            catch (SecurityException ex)
            {
                Log.PrintInfo("ERROR RAFFA1: " + ex.Message);
            }
            catch (ThreadStateException ex2)
            {
                Log.PrintInfo("ERROR RAFFA2: " + ex2.Message);
            }
            catch (Exception ex3)
            {
                Log.PrintInfo("ERROR RAFFA3: " + ex3.Message);
            }
        }

        public void KillRiposo()
        {
            isRiposando = false;
            t.Abort();
            Dealloc();
        }

        public void Riposa(object parameters)
        {
            while (isRiposando)
            {
                MoveRiposo();
                mouse.Click();
                Log.PrintInfo($"STO RIPOSANDO | STATO: {Runtime.current_state_bot}");
                Thread.Sleep(14000 + rnd.Next(2500));
            }
        }

        public void ActivateStartRiposaRoulette()
        {
            isRiposandoRoulette = true;
            t.Start();
        }

        public void ActivateRiposaRoulette()
        {
            isRiposandoRoulette = true;
            t.Resume();
        }

        public void DeactivateRiposaRoulette()
        {
            isRiposandoRoulette = false;
            try
            {
                t.Suspend();
            }
            catch (SecurityException ex)
            {
                Log.PrintInfo("(R) ERROR 1: " + ex.Message);
            }
            catch (ThreadStateException ex2)
            {
                Log.PrintInfo("(R) ERROR 2: " + ex2.Message);
            }
            catch (Exception ex3)
            {
                Log.PrintInfo("(R) ERROR 3: " + ex3.Message);
            }
        }

        public void KillRiposoRoulette()
        {
            isRiposandoRoulette = false;
            t.Abort();
            Dealloc();
        }

        public void RiposaRoulette(object parameters)
        {
            while (isRiposandoRoulette)
            {
                MoveRiposoRoulette();
                mouse.Click();
                Log.PrintInfo($"(R) STO RIPOSANDO | STATO: {RouletteValues.Runtime.current_state_bot}");
                Thread.Sleep(14000 + rnd.Next(2500));
            }
        }
    }
}
