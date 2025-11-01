using System.Text.Json.Serialization;

namespace AdmSchoolApp.Domain.Models.Requests;

public class LoginRequests
{
    [JsonPropertyName("email")]
    public string? Email { get; set; }
    
    [JsonPropertyName("password")]
    public string? Password { get; set; }
}