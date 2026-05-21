using System;
using System.Globalization;

namespace Gamebot.Models.Objects
{
    public class CustomFiche
    {
        // (get) Token: 0x060002CB RID: 715 RVA: 0x0001F90D File Offset: 0x0001DB0D
        // (set) Token: 0x060002CC RID: 716 RVA: 0x0001F915 File Offset: 0x0001DB15
        private double value { get; set; }

        // (get) Token: 0x060002CD RID: 717 RVA: 0x0001F91E File Offset: 0x0001DB1E
        // (set) Token: 0x060002CE RID: 718 RVA: 0x0001F926 File Offset: 0x0001DB26
        private string label { get; set; } = string.Empty;

        public string getDicitura()
        {
            if (this.label.Length > 0)
            {
                return this.label;
            }
            //return ((this.value > 1) ? "Fiches " : "Fiche ") + Convert.ToString(this.value);
            return "Fiche " + this.value.ToString("0.##", CultureInfo.InvariantCulture);
        }

        public void setValue(double newValue)
        {
            this.value = newValue;
        }

        public double getValue()
        {
            return this.value;
        }

        public void setLabel(string newLabel)
        {
            this.label = newLabel;
        }

        public string getLabel()
        {
            return this.label;
        }
    }
}
