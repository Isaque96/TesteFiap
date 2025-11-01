using AdmSchoolApp.Domain.Models.Requests;
using AdmSchoolApp.Domain.Models.Responses;
using Blazored.LocalStorage;

namespace AdmSchoolApp.Web.Services;

public class AuthService(HttpClient httpClient, ILocalStorageService localStorage) : IAuthService
{
    private const string TokenKey = "authToken";
    private const string RefreshTokenKey = "refreshToken";
    private const string ExpirationKey = "tokenExpiration";

    public async Task<LoginResponse?> LoginAsync(LoginRequests request)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("/api/auth/login", request);

            if (!response.IsSuccessStatusCode) return null;
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

            if (loginResponse == null) return null;
            await localStorage.SetItemAsync(TokenKey, loginResponse.AccessToken);
            await localStorage.SetItemAsync(RefreshTokenKey, loginResponse.RefreshToken);
            await localStorage.SetItemAsync(ExpirationKey, loginResponse.ExpiresAt);
                    
            return loginResponse;
        }
        catch
        {
            return null;
        }
    }

    public async Task<LoginResponse?> RefreshTokenAsync()
    {
        try
        {
            var token = await localStorage.GetItemAsync<string>(TokenKey);
            var refreshToken = await localStorage.GetItemAsync<string>(RefreshTokenKey);

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(refreshToken))
                return null;

            var request = new RefreshTokenRequest(refreshToken);

            var response = await httpClient.PostAsJsonAsync("/api/auth/refresh", request);

            if (!response.IsSuccessStatusCode) return null;
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

            if (loginResponse == null) return null;
            await localStorage.SetItemAsync(TokenKey, loginResponse.AccessToken);
            await localStorage.SetItemAsync(RefreshTokenKey, loginResponse.RefreshToken);
            await localStorage.SetItemAsync(ExpirationKey, loginResponse.ExpiresAt);
                    
            return loginResponse;
        }
        catch
        {
            return null;
        }
    }

    public async Task LogoutAsync()
    {
        await localStorage.RemoveItemAsync(TokenKey);
        await localStorage.RemoveItemAsync(RefreshTokenKey);
        await localStorage.RemoveItemAsync(ExpirationKey);
    }

    public async Task<string?> GetTokenAsync()
    {
        return await localStorage.GetItemAsync<string>(TokenKey);
    }
}