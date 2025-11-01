using AdmSchoolApp.Domain.Models.Requests;
using AdmSchoolApp.Domain.Models.Responses;

namespace AdmSchoolApp.Web.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequests request);
    Task<LoginResponse?> RefreshTokenAsync();
    Task LogoutAsync();
    Task<string?> GetTokenAsync();
}