using System.Text.Json;
using IPrint.Models.Encuentra.Requests;

namespace IPrint.Services;

public class EncuentraService
{
    private static HttpClient HttpClient;
    
    public EncuentraService()
    {
        HttpClient = new HttpClient();
        HttpClient.BaseAddress = new Uri("https://encuentra.200.com.uy");
    }
    
    public async Task<EncuentraResponse<EncuentraTokenValidationResponse>?> ValidateTokenAsync(string token)
    {
        EncuentraResponse<EncuentraTokenValidationResponse>? result = null;
        using var response = await HttpClient.PostAsync($"/encuentraAPI/Integraciones/datero/validarToken?token={token}", null);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            result = JsonSerializer.Deserialize<EncuentraResponse<EncuentraTokenValidationResponse>>(content);
        }
        
        return result;
    }
}