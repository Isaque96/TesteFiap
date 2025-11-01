using System.Net.Http.Headers;
using AdmSchoolApp.Domain.Models;
using AdmSchoolApp.Domain.Models.Requests;
using AdmSchoolApp.Domain.Models.Responses;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace AdmSchoolApp.Web.Services;

public class AuthService(HttpClient httpClient, ProtectedSessionStorage sessionStorage)
    : IAuthService
{
    private const string TokenKey = "authToken";
    private const string RefreshTokenKey = "refreshToken";
    private const string ExpirationKey = "tokenExpiration";

    public async Task<LoginResponse?> LoginAsync(LoginRequests request)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("/api/v1/auth/login", request);

            if (!response.IsSuccessStatusCode)
                return null;

            var loginResponse = (await response.Content.ReadFromJsonAsync<BaseResponse<LoginResponse>>())?.Data;

            if (loginResponse == null)
                return null;

            await sessionStorage.SetAsync(TokenKey, loginResponse.AccessToken);
            await sessionStorage.SetAsync(RefreshTokenKey, loginResponse.RefreshToken);
            await sessionStorage.SetAsync(ExpirationKey, loginResponse.ExpiresAt);

            var token = loginResponse.AccessToken;
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

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
            var tokenResult = await sessionStorage.GetAsync<string>(TokenKey);
            var refreshTokenResult = await sessionStorage.GetAsync<string>(RefreshTokenKey);

            if (!tokenResult.Success || string.IsNullOrEmpty(tokenResult.Value) ||
                !refreshTokenResult.Success || string.IsNullOrEmpty(refreshTokenResult.Value))
                return null;

            var request = new RefreshTokenRequest(refreshTokenResult.Value);

            var response = await httpClient.PostAsJsonAsync("/api/v1/auth/refresh", request);

            if (!response.IsSuccessStatusCode)
                return null;

            var loginResponse = (await response.Content.ReadFromJsonAsync<BaseResponse<LoginResponse>>())?.Data;

            if (loginResponse == null)
                return null;

            await sessionStorage.SetAsync(TokenKey, loginResponse.AccessToken);
            await sessionStorage.SetAsync(RefreshTokenKey, loginResponse.RefreshToken);
            await sessionStorage.SetAsync(ExpirationKey, loginResponse.ExpiresAt);

            var token = loginResponse.AccessToken;
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            return loginResponse;
        }
        catch
        {
            return null;
        }
    }

    public async Task LogoutAsync()
    {
        await sessionStorage.DeleteAsync(TokenKey);
        await sessionStorage.DeleteAsync(RefreshTokenKey);
        await sessionStorage.DeleteAsync(ExpirationKey);

        httpClient.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<string?> GetTokenAsync()
    {
        var result = await sessionStorage.GetAsync<string>(TokenKey);
        return result.Success ? result.Value : null;
    }
}