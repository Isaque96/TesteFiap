namespace AdmSchoolApp.Domain.Models.Requests;

public record UpdateUserRolesRequest(
    List<string> Roles
);