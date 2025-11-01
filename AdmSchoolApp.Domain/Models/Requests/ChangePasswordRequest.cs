namespace AdmSchoolApp.Domain.Models.Requests;

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
);