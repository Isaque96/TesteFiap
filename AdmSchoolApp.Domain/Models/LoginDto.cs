using System.Text.Json.Serialization;

namespace AdmSchoolApp.Domain.Models;

public class LoginDto
{
    [JsonPropertyName("login")]
    public string? Login { get; set; }
    
    [JsonPropertyName("password")]
    public string? Password { get; set; }
}