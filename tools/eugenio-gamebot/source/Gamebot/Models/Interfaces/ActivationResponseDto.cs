using System.Text.Json.Serialization;

namespace Gamebot.Models.Interfaces
{
    public class ActivationResponseDto
    {
        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; }
    }
}
