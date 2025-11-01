namespace AdmSchoolApp.Domain.Models.Responses;

public record UserResponse(
    Guid Id,
    string Name,
    string Email,
    bool IsActive,
    List<string> Roles,
    DateTime CreatedAt,
    DateTime UpdatedAt
);