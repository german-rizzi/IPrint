using System.Text.Json.Serialization;

namespace IPrint.Models.Encuentra.Requests;

public class EncuentraUser
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    [JsonPropertyName("companyId")]
    public int CompanyId { get; set; }
}