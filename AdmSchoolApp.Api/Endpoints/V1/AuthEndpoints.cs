using AdmSchoolApp.Application.Services;
using AdmSchoolApp.Domain.Interfaces;
using AdmSchoolApp.Domain.Models.Requests;
using AdmSchoolApp.Domain.Models.Responses;
using AdmSchoolApp.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace AdmSchoolApp.Endpoints.V1;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/auth")
            .WithTags("Authentication");

        group.MapPost("/login", LoginAsync)
            .WithName("Login")
            .WithSummary("Autentica usuário e retorna tokens JWT")
            .WithDescription("Autenticação com JWT usando email e senha")
            .Accepts<LoginRequests>(SwaggerExtensions.JsonContentType)
            .Produces<LoginResponse>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .AllowAnonymous();

        group.MapPost("/refresh", RefreshTokenAsync)
            .WithName("RefreshToken")
            .WithSummary("Renova access token usando refresh token")
            .Accepts<RefreshTokenRequest>(SwaggerExtensions.JsonContentType)
            .Produces<TokenResult>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .AllowAnonymous();

        group.MapPost("/logout", LogoutAsync)
            .WithName("Logout")
            .WithSummary("Revoga refresh token (logout)")
            .Accepts<RefreshTokenRequest>(SwaggerExtensions.JsonContentType)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAuthorization();

        return group;
    }
    
    private static async Task<IResult> LoginAsync(
        [FromBody] LoginRequests request,
        [FromServices] UserService userService,
        [FromServices] ITokenService tokenService,
        CancellationToken ct
    )
    {
        // Validar entrada
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return ApiResponseExtensions.BadRequest(["Email e senha são obrigatórios"]);

        // Autenticar usuário (valida senha com BCrypt)
        var user = await userService.AuthenticateAsync(request.Email, request.Password);

        if (user == null)
            return ApiResponseExtensions.Unauthorized();

        // Gerar tokens
        var tokenResult = await tokenService.GenerateTokenAsync(user, ct);

        // Obter roles do usuário
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList() ?? [];

        var response = new LoginResponse(
            tokenResult.AccessToken,
            tokenResult.RefreshToken,
            tokenResult.ExpiresAt,
            tokenResult.TokenType,
            new UserInfo(user.Id, user.Name, user.Email, roles)
        );

        return ApiResponseExtensions.Success(response, "Login realizado com sucesso");
    }

    private static async Task<IResult> RefreshTokenAsync(
        [FromBody] RefreshTokenRequest request,
        [FromServices] ITokenService tokenService,
        CancellationToken ct
    )
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return ApiResponseExtensions.BadRequest(["Refresh token é obrigatório"]);

        var tokenResult = await tokenService.RefreshTokenAsync(request.RefreshToken, ct);

        return tokenResult == null ?
            ApiResponseExtensions.Unauthorized() :
            ApiResponseExtensions.Success(tokenResult, "Token renovado com sucesso");
    }

    private static async Task<IResult> LogoutAsync(
        [FromBody] RefreshTokenRequest request,
        [FromServices] ITokenService tokenService,
        CancellationToken ct
    )
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return ApiResponseExtensions.BadRequest(["Refresh token é obrigatório"]);

        await tokenService.RevokeRefreshTokenAsync(request.RefreshToken, ct);

        return ApiResponseExtensions.NoContent("Logout realizado com sucesso");
    }
}