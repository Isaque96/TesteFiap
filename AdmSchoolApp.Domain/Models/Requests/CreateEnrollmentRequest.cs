namespace AdmSchoolApp.Domain.Models.Requests;

public record CreateEnrollmentRequest(
    Guid StudentId,
    Guid ClassId
);