namespace IPrint.Models.Encuentra.Requests;

public class EncuentraTokenValidationResponse
{
    public EncuentraUser User { get; set; }
    public EncuentraSucursal Sucursal { get; set; }
    public string Token { get; set; }
}