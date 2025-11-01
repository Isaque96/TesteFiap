namespace AdmSchoolApp.Domain.Models.Responses;

public record StudentResponse(
    Guid Id,
    string Name,
    DateOnly BirthDate,
    string Cpf,
    string Email,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record StudentWithEnrollmentsResponse(
    Guid Id,
    string Name,
    DateOnly BirthDate,
    string Cpf,
    string Email,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<EnrollmentSummary> Enrollments
);

public record EnrollmentSummary(
    Guid Id,
    Guid ClassId,
    string ClassName,
    DateTime EnrolledAt
);