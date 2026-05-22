using System;
using Gamebot.Helpers;
using System.Threading.Tasks;

namespace Gamebot.Models.MouseMove
{
    internal class Bets
    {
        public static async void Startup()
        {
            Runtime.game = 0;
            Bets.m = Move.Instance;
            Bets.m.MoveRiposo();
            Bets.m.ActivateStartRiposa();
        }

        public static async Task DoTheBet(int[] fiches_array)
        {
            Bets.m.DeactivateRiposa();
            int last_fiche = 0;
            for (int i = 0; i < fiches_array.Length; i++)
            {
                if (fiches_array[i] != last_fiche)
                {
                    last_fiche = fiches_array[i];
                    if (last_fiche <= 25)
                    {
                        if (last_fiche != 1)
                        {
                            if (last_fiche != 5)
                            {
                                if (last_fiche == 25)
                                {
                                    Bets.m.MoveFish25();
                                }
                            }
                            else
                            {
                                Bets.m.MoveFish5();
                            }
                        }
                        else
                        {
                            Bets.m.MoveFish1();
                        }
                    }
                    else if (last_fiche <= 222)
                    {
                        if (last_fiche != 100)
                        {
                            if (last_fiche == 222)
                            {
                                Bets.m.MoveFishRaddoppia();
                            }
                        }
                        else
                        {
                            Bets.m.MoveFish100();
                        }
                    }
                    else if (last_fiche != 250)
                    {
                        if (last_fiche == 500)
                        {
                            Bets.m.MoveFish500();
                        }
                    }
                    else
                    {
                        Bets.m.MoveFish250();
                    }
                    if (last_fiche != 222)
                    {
                        Bets.m.Click();
                        if (Runtime.chosen_color == Constants.EnumColorBaccarat.RED_BANK)
                        {
                            Bets.m.MoveRed();
                        }
                        else
                        {
                            Bets.m.MoveBlu();
                        }
                    }
                }
                Bets.m.Click();
            }
            Bets.m.MoveRiposo();
            Bets.m.ActivateRiposa();
            Log.PrintInfo("BET | FICHES: [" + string.Join<int>(",", fiches_array) + "]");
        }

        public static async Task DoTheCustomBet(double[] fiches_array)
        {
            Bets.m.DeactivateRiposa();
            double last_fiche = 0;
            double totale_puntata = 0.0;

            for (int i = 0; i < fiches_array.Length; i++)
            {
                totale_puntata += fiches_array[i];  
            }
            Runtime.puntata = totale_puntata;
            
            for (int i = 0; i < fiches_array.Length; i++)
            {
                if (fiches_array[i] != last_fiche)
                {
                    last_fiche = fiches_array[i];
                    if (last_fiche == -1)
                    {
                        Bets.m.MoveFishRaddoppia();
                    }
                    else
                    {
                        Bets.m.MoveFishCustom(last_fiche);
                    }
                    if (last_fiche != -1)
                    {
                        Bets.m.Click();
                        if (Runtime.chosen_color == Constants.EnumColorBaccarat.RED_BANK)
                        {
                            Bets.m.MoveRed();
                        }
                        else
                        {
                            Bets.m.MoveBlu();
                        }
                    }
                }
                Bets.m.Click();
            }
            Bets.m.MoveRiposo();
            Bets.m.ActivateRiposa();
            string fiches = string.Join<double>("#", fiches_array).Replace(",", ".").Replace("#", ",");
            Log.PrintInfo("BET | CUSTOM FICHES: [" + fiches + "]");
        }

        public static Move m;
    }
}
