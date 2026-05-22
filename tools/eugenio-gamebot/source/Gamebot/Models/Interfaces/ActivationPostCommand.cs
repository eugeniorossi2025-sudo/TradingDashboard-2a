using System.Text.Json.Serialization;

namespace Gamebot.Models.Interfaces
{
    public class ActivationPostCommand
    {
        [JsonPropertyName("appname")]
        public string AppName { get; set; }

        [JsonPropertyName("version")]
        public int Version { get; set; }
    }
}
