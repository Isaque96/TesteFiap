using AdmSchoolApp.Domain.Entities;
using AdmSchoolApp.Domain.Interfaces;
using AdmSchoolApp.Domain.Models.Responses;
using AdmSchoolApp.Infrastructure.Contexts;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdmSchoolApp.Endpoints.V1;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/auth")
            .WithTags("Authentication");

        group.MapPost("/login", LoginAsync)
            .WithName("Login")
            .WithSummary("Autentica usuário e retorna tokens JWT")
            .AllowAnonymous();

        group.MapPost("/refresh", RefreshTokenAsync)
            .WithName("RefreshToken")
            .WithSummary("Renova access token usando refresh token")
            .AllowAnonymous();

        group.MapPost("/logout", LogoutAsync)
            .WithName("Logout")
            .WithSummary("Revoga refresh token (logout)")
            .RequireAuthorization();

        return group;
    }
    
    private static async Task<IResult> LoginAsync(
        [FromBody] LoginRequest request,
        [FromServices] IRepository<User> userRepo,
        [FromServices] ITokenService tokenService,
        [FromServices] AdmSchoolDbContext context,
        CancellationToken ct
    )
    {
        // Buscar usuário com roles
        var user = await context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email, ct);

        if (user is null || !user.IsActive)
            return Results.Unauthorized();

        // Validar senha (você deve usar BCrypt/Argon2 aqui)
        // Exemplo simplificado:
        // if (!BCrypt.Net.BCrypt.Verify(request.Password, Encoding.UTF8.GetString(user.PasswordHash)))
        // {
        //     return Results.Unauthorized();
        // }

        // TODO: Implementar validação de senha real

        // Gerar tokens
        var tokenResult = await tokenService.GenerateTokenAsync(user, ct);

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

        var response = new LoginResponse(
            tokenResult.AccessToken,
            tokenResult.RefreshToken,
            tokenResult.ExpiresAt,
            tokenResult.TokenType,
            new UserInfo(user.Id, user.Name, user.Email, roles)
        );

        return Results.Ok(response);
    }

    private static async Task<IResult> RefreshTokenAsync(
        [FromBody] RefreshTokenRequest request,
        [FromServices] ITokenService tokenService,
        CancellationToken ct)
    {
        var tokenResult = await tokenService.RefreshTokenAsync(request.RefreshToken, ct);

        return tokenResult is null ? Results.Unauthorized() : Results.Ok(tokenResult);
    }

    private static async Task<IResult> LogoutAsync(
        [FromBody] RefreshTokenRequest request,
        [FromServices] ITokenService tokenService,
        CancellationToken ct)
    {
        await tokenService.RevokeRefreshTokenAsync(request.RefreshToken, ct);
        return Results.NoContent();
    }
}