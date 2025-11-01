namespace AdmSchoolApp.Domain.Models.Requests;

public record CreateStudentRequest(
    string Name,
    DateOnly BirthDate,
    string Cpf,
    string Email,
    string Password
);

public record UpdateStudentRequest(
    string Name,
    DateOnly BirthDate,
    string Cpf,
    string Email
);