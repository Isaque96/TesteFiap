namespace AdmSchoolApp.Domain.Models.Responses;

public record EnrollmentResponse(
    Guid Id,
    Guid StudentId,
    string StudentName,
    Guid ClassId,
    string ClassName,
    DateTime CreatedAt
);