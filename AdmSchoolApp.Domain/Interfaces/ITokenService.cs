using AdmSchoolApp.Domain.Entities;
using AdmSchoolApp.Domain.Models.Responses;

namespace AdmSchoolApp.Domain.Interfaces;

public interface ITokenService
{
    /// <summary>
    /// Gera access token e refresh token para o usuário com suas roles.
    /// </summary>
    Task<TokenResult> GenerateTokenAsync(User user, CancellationToken ct = default);

    /// <summary>
    /// Valida e renova tokens usando o refresh token.
    /// </summary>
    Task<TokenResult?> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);

    /// <summary>
    /// Valida um access token e retorna o userId se válido.
    /// </summary>
    Guid? ValidateToken(string token);

    /// <summary>
    /// Revoga um refresh token (logout).
    /// </summary>
    Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
}