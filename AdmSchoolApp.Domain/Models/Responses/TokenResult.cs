namespace AdmSchoolApp.Domain.Models.Responses;

public sealed record TokenResult(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    string TokenType = "Bearer"
);
