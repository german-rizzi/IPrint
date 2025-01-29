using System.Text.Json.Serialization;

namespace IPrint.Models.Encuentra;

public class EncuentraUser
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    [JsonPropertyName("companyID")]
    public int CompanyId { get; set; }
}