namespace AdmSchoolApp.Domain.Models.Responses;

public record ClassResponse(
    Guid Id,
    string Name,
    string Description,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record ClassWithStudentCountResponse(
    Guid Id,
    string Name,
    string Description,
    int StudentCount,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record ClassWithStudentsResponse(
    Guid Id,
    string Name,
    string Description,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<StudentSummary> Students
);

public record StudentSummary(
    Guid Id,
    string Name,
    string Email,
    string Cpf
);