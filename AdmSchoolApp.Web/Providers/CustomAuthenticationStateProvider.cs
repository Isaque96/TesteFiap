using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;

namespace AdmSchoolApp.Web.Providers
{
    public class CustomAuthenticationStateProvider(ProtectedSessionStorage sessionStorage) : AuthenticationStateProvider
    {
        private const string TokenKey = "authToken";

        // Inicializa e notifica (pode ser chamado no startup ou após login)
        public async Task InitializeAsync()
        {
            var result = await sessionStorage.GetAsync<string>(TokenKey);
            var token = result.Success ? result.Value : null;

            var user = CreatePrincipalFromToken(token);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        // Usado pelo framework para obter o estado atual
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var result = await sessionStorage.GetAsync<string>(TokenKey);
                var token = result.Success ? result.Value : null;

                var user = CreatePrincipalFromToken(token);
                return new AuthenticationState(user);
            }
            catch (Exception)
            {
                return  new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        // Marca o usuário como autenticado, persiste token e notifica
        public async Task MarkUserAsAuthenticated(string token)
        {
            await sessionStorage.SetAsync(TokenKey, token);
            var user = CreatePrincipalFromToken(token);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        // Marca o usuário como deslogado, remove token e notifica
        public async Task MarkUserAsLoggedOut()
        {
            await sessionStorage.DeleteAsync(TokenKey);
            var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
        }

        // Cria ClaimsPrincipal a partir do token (no exemplo simples apenas um Claim Name).
        // Em app real, parseie o JWT e crie claims reais (roles, exp, sub, etc).
        private static ClaimsPrincipal CreatePrincipalFromToken(string? token)
        {
            if (string.IsNullOrEmpty(token)) return new ClaimsPrincipal(new ClaimsIdentity());
            // Exemplo simples: você pode extrair claims reais do JWT aqui.
            var claims = new[] { new Claim(ClaimTypes.Name, "admin@admin") };
            var identity = new ClaimsIdentity(claims, "apiauth");
            return new ClaimsPrincipal(identity);
        }
    }
}
