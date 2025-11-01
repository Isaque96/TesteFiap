namespace AdmSchoolApp.Domain.Models.Requests;

public record CreateClassRequest(
    string Name,
    string Description
);

public record UpdateClassRequest(
    string Name,
    string Description
);