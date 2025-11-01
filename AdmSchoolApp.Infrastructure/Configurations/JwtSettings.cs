namespace AdmSchoolApp.Infrastructure.Configurations;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string? Issuer { get; init; }
    public string? Audience { get; init; }
    public string? SecretKey { get; init; }
    public int AccessTokenExpirationMinutes { get; init; } = 15;
    public int RefreshTokenExpirationDays { get; init; } = 7;
}