namespace AdmSchoolApp.Domain.Models.Requests;

public record CreateUserRequest(
    string Name,
    string Email,
    string Password
);

public record UpdateUserRequest(
    string Name,
    string Email,
    bool IsActive
);