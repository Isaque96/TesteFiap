using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AdmSchoolApp.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;

namespace AdmSchoolApp.Web.Providers;

public class CustomAuthenticationStateProvider(IAuthService authService) : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await authService.GetTokenAsync();

        if (string.IsNullOrEmpty(token))
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        if (jwtToken.ValidTo < DateTime.UtcNow)
        {
            // Token expirado, tentar refresh
            var refreshResult = await authService.RefreshTokenAsync();
            
            if (refreshResult == null)
            {
                await authService.LogoutAsync();
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            token = refreshResult.AccessToken;
            jwtToken = handler.ReadJwtToken(token);
        }

        var claims = jwtToken.Claims.ToList();
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        return new AuthenticationState(user);
    }

    public void NotifyAuthenticationStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}