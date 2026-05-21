using Gamebot.Helpers;
using Gamebot.Models.Roulette.Funcs;
using Gamebot.Models.Roulette.Logic;
using Gamebot.Models.UI;
using System;
using System.Collections.Generic;

namespace Gamebot.Models.Roulette
{
    internal class MainStateRoulette
    {
        public static void UpdateForm()
        {
            List<string> values = new List<string>();
            values.Clear();
            if (UpdateInterface.GetInstanceForm().progressUIRoulette != null)
            {
                values.Add(Number.FormatNumberDecimalEuro(RouletteValues.Runtime.global_profit) ?? "");
                values.Add(string.Format("{0}", RouletteValues.Runtime.numero_vincite));
                values.Add(string.Format("{0}", RouletteValues.Runtime.numero_perdite));
                UpdateInterface.GetInstanceForm().progressUIRoulette.Report(values);
                if (!RouletteValues.Runtime.runningStateMachineBot)
                {
                    try
                    {
                        Action<string> DelegateTeste_ModifyText = new Action<string>(UpdateInterface.GetInstanceForm().THREAD_MOD);
                        UpdateInterface.GetInstanceForm().Invoke(DelegateTeste_ModifyText, new object[] { "**UPDATE**" });
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        public static void StateMachine()
        {
            SubStateRoulette.MainCycle();
        }
    }
}
