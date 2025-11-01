using System.Text.Json.Serialization;

namespace AdmSchoolApp.Domain.Models.Requests;

public class LoginRequests
{
    [JsonPropertyName("login")]
    public string? Login { get; set; }
    
    [JsonPropertyName("password")]
    public string? Password { get; set; }
}