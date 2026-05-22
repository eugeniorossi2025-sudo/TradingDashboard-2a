using Gamebot.Models.Objects;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Gamebot.Models.Entity
{
    public class JSONSingleConfigBacaratCustomFiches
    {
        [JsonPropertyName("directoryNumeriMazzo")]
        public string DirectoryNumeriMazzo { get; set; }
        
        [JsonPropertyName("globalStopWin")]
        public decimal GlobalStopWin { get; set; }

        [JsonPropertyName("stopWin")]
        public decimal StopWin { get; set; }

        [JsonPropertyName("stopLoss")]
        public decimal StopLoss { get; set; }

        [JsonPropertyName("safeWin")]
        public decimal SafeWin { get; set; }

        [JsonPropertyName("alarm")]
        public decimal Alarm { get; set; }

        public decimal ChangeColor { get; set; }

        [JsonPropertyName("red")]
        public AreaElementConfig AreaRed { get; set; }

        [JsonPropertyName("blu")]
        public AreaElementConfig AreaBlu { get; set; }

        [JsonPropertyName("areaCentrale")]
        public AreaElementConfig AreaCentrale { get; set; }

        [JsonPropertyName("areaVincita")]
        public AreaElementConfig AreaVincita { get; set; }

        [JsonPropertyName("areaPuntare")]
        public AreaElementConfig AreaPuntare { get; set; }

        [JsonPropertyName("areaRaddoppio")]
        public AreaElementConfig AreaRaddoppio { get; set; }

        [JsonPropertyName("areaMazzo")]
        public AreaElementConfig AreaMazzo { get; set; }

        [JsonPropertyName("areaSaldo")]
        public AreaElementConfig AreaSaldo { get; set; }

        [JsonPropertyName("customFiches")]
        public List<CustomFicheData> CustomFiches { get; set; }

        [JsonPropertyName("startColor")]
        public string StartColor { get; set; }

        [JsonPropertyName("mode")]
        public string Mode { get; set; }

        [JsonPropertyName("martingala")]
        public List<double> Martingala { get; set; }

        [JsonPropertyName("zoom")]
        public string Zoom { get; set; }

        [JsonPropertyName("safeWinEnabled")]
        public bool SafeWinEnabled { get; set; }

        [JsonPropertyName("endSculpingMessageEnabled")]
        public bool EndSculpingMessageEnabled { get; set; }

        [JsonPropertyName("numberEndDeck")]
        public decimal NumberEndDeck { get; set; }

        [JsonPropertyName("textTieArea")]
        public string TextTieArea { get; set; } = string.Empty;

        [JsonPropertyName("textWinArea")]
        public string TextWinArea { get; set; } = string.Empty;

        [JsonPropertyName("textBenchArea")]
        public string TextBenchArea { get; set; } = string.Empty;

        [JsonPropertyName("textPlayerArea")]
        public string TextPlayerArea { get; set; } = string.Empty;

        [JsonPropertyName("textAreaPuntare")]
        public string TextAreaPuntare { get; set; } = string.Empty;

        [JsonPropertyName("demoEnabled")]
        public bool DemoEnabled { get; set; }

        [JsonPropertyName("filterPragmatic")]
        public bool FilterPragmatic { get; set; }

        [JsonPropertyName("martingalaOptions")]
        public List<MartingalaInfoItem> MartingalaOptions { get; set; } = new List<MartingalaInfoItem>();

        [JsonPropertyName("skipPostSculping")]
        public bool SkipPostSculping { get; set; }

        [JsonPropertyName("indexNamePc")]
        public int IndexNamePc { get; set; }
    }
}
