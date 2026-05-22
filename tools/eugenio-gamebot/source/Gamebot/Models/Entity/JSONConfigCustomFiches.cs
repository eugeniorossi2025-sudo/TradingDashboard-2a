using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Gamebot.Models.Entity
{
    internal class JSONConfigCustomFiches
    {
        [JsonPropertyName("user")]
        public string User { get; set; }

        [JsonPropertyName("configs")]
        public List<JSONSingleConfigCustomFiches> Configs { get; set; }
    }
}
