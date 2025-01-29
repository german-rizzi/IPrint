using System.Text.Json.Serialization;

namespace IPrint.Models.Encuentra.Responses;

public class EncuentraResponse<T>
{
    [JsonPropertyName("result")]
    public T Result { get; set; }
    [JsonPropertyName("message")]
    public string Message { get; set; }
    [JsonPropertyName("status")]
    public string Status { get; set; }
}