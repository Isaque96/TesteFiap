using AdmSchoolApp.Domain.Models;
using AdmSchoolApp.Domain.Models.Requests;
using AdmSchoolApp.Domain.Models.Responses;

namespace AdmSchoolApp.Web.Services;

public interface IApiService
{
    // Students
    Task<BasePagination<StudentResponse>> GetStudentsAsync(int pageNumber = 1, int pageSize = 10);
    Task<StudentResponse?> GetStudentByIdAsync(Guid id);
    Task<StudentWithEnrollmentsResponse?> GetStudentWithEnrollmentsAsync(Guid id);
    Task<List<StudentResponse>> SearchStudentsByNameAsync(string name);
    Task<StudentResponse?> GetStudentByCpfAsync(string cpf);
    Task<bool> CreateStudentAsync(CreateStudentRequest student);
    Task<bool> UpdateStudentAsync(Guid id, UpdateStudentRequest student);
    Task<bool> DeleteStudentAsync(Guid id);
    
    // Classes
    Task<BasePagination<ClassWithStudentCountResponse>> GetClassesAsync(int pageNumber = 1, int pageSize = 10);
    Task<ClassWithStudentCountResponse?> GetClassByIdAsync(Guid id);
    Task<ClassWithStudentsResponse?> GetClassWithStudentsAsync(Guid id);
    Task<bool> CreateClassAsync(CreateClassRequest classRequest);
    Task<bool> UpdateClassAsync(Guid id, UpdateClassRequest classRequest);
    Task<bool> DeleteClassAsync(Guid id);
    
    // Enrollments
    Task<List<EnrollmentResponse>> GetEnrollmentsByStudentAsync(Guid studentId);
    Task<EnrollmentResponse?> GetEnrollmentsByClassAsync(Guid classId);
    Task<EnrollmentResponse?> GetEnrollmentByIdAsync(Guid id);
    Task<bool> CreateEnrollmentAsync(CreateEnrollmentRequest enrollment);
    Task<bool> DeleteEnrollmentAsync(Guid id);
}