namespace AdmSchoolApp.Domain.Models.Responses;

public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    string TokenType,
    UserInfo User
);

public sealed record UserInfo(
    Guid Id,
    string Name,
    string Email,
    IEnumerable<string> Roles
);

public sealed record RefreshTokenRequest(string RefreshToken);