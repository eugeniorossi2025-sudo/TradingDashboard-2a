using System.Text.Json.Serialization;

namespace Gamebot.Models.Interfaces
{
    public class ExternalResponse<T> where T : class
    {
        [JsonPropertyName("data")]
        public T Data { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; } = true;

        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; set; }

        [JsonIgnore]
        public int StatusCode { get; set; }

        public virtual ExternalResponse<T> Ok(T data)
        {
            return new ExternalResponse<T>
            {
                Data = data,
                Success = true,
                ErrorMessage = null,
                StatusCode = 200
            };
        }

        public virtual ExternalResponse<T> BadRequest(string errorMessage)
        {
            return new ExternalResponse<T>
            {
                Data = default(T),
                Success = false,
                ErrorMessage = errorMessage,
                StatusCode = 400
            };
        }
    }
}
