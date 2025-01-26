using System.Text.Json.Serialization;

namespace IPrint.Models.Encuentra.Requests;

public class EncuentraResponse<T>
{
    [JsonPropertyName("Result")]
    public T Result { get; set; }
    [JsonPropertyName("message")]
    public string Message { get; set; }
    [JsonPropertyName("status")]
    public string Status { get; set; }
}