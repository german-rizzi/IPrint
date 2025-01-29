namespace IPrint.Models.Encuentra.Responses;

public class EncuentraTokenValidationResponse
{
    public EncuentraUser User { get; set; }
    public EncuentraSucursal Sucursal { get; set; }
    public string Token { get; set; }
}