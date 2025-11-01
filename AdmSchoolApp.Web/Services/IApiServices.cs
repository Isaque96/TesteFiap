using AdmSchoolApp.Domain.Models;
using AdmSchoolApp.Domain.Models.Requests;
using AdmSchoolApp.Domain.Models.Responses;

namespace AdmSchoolApp.Web.Services;

public interface IApiService
{
    // Students
    Task<BaseResponse<BasePagination<StudentResponse>>> GetStudentsAsync(int pageNumber = 1, int pageSize = 10);
    Task<BaseResponse<StudentResponse>> GetStudentByIdAsync(Guid id);
    Task<BaseResponse<StudentWithEnrollmentsResponse>> GetStudentWithEnrollmentsAsync(Guid id);
    Task<BaseResponse<List<StudentResponse>>> SearchStudentsByNameAsync(string name);
    Task<BaseResponse<StudentResponse>> GetStudentByCpfAsync(string cpf);
    Task<bool> CreateStudentAsync(CreateStudentRequest student);
    Task<bool> UpdateStudentAsync(Guid id, UpdateStudentRequest student);
    Task<bool> DeleteStudentAsync(Guid id);
    
    // Classes
    Task<BaseResponse<BasePagination<ClassWithStudentCountResponse>>> GetClassesAsync(int pageNumber = 1, int pageSize = 10);
    Task<BaseResponse<ClassWithStudentCountResponse>> GetClassByIdAsync(Guid id);
    Task<BaseResponse<ClassWithStudentsResponse>> GetClassWithStudentsAsync(Guid id);
    Task<bool> CreateClassAsync(CreateClassRequest classRequest);
    Task<bool> UpdateClassAsync(Guid id, UpdateClassRequest classRequest);
    Task<bool> DeleteClassAsync(Guid id);
    
    // Enrollments
    Task<BaseResponse<List<EnrollmentResponse>>> GetEnrollmentsByStudentAsync(Guid studentId);
    Task<BaseResponse<EnrollmentResponse>> GetEnrollmentsByClassAsync(Guid classId);
    Task<BaseResponse<EnrollmentResponse>> GetEnrollmentByIdAsync(Guid id);
    Task<bool> CreateEnrollmentAsync(CreateEnrollmentRequest enrollment);
    Task<bool> DeleteEnrollmentAsync(Guid id);
}