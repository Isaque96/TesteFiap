using System.Net.Http.Headers;
using AdmSchoolApp.Domain.Models;
using AdmSchoolApp.Domain.Models.Requests;
using AdmSchoolApp.Domain.Models.Responses;

namespace AdmSchoolApp.Web.Services;

public class ApiService(HttpClient httpClient, IAuthService authService) : IApiService
{
    private async Task SetAuthorizationHeaderAsync()
    {
        var token = await authService.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
        }
    }

    // Students
    public async Task<BaseResponse<BasePagination<StudentResponse>>> GetStudentsAsync(int pageNumber = 1, int pageSize = 10)
    {
        await SetAuthorizationHeaderAsync();
        return await httpClient.GetFromJsonAsync<BaseResponse<BasePagination<StudentResponse>>>(
            $"/api/v1/students?pageNumber={pageNumber}&pageSize={pageSize}") 
            ?? new BaseResponse<BasePagination<StudentResponse>>();
    }

    public async Task<BaseResponse<StudentResponse>> GetStudentByIdAsync(Guid id)
    {
        await SetAuthorizationHeaderAsync();
        return await httpClient.GetFromJsonAsync<BaseResponse<StudentResponse>>($"/api/v1/students/{id}") ??
               new BaseResponse<StudentResponse>();
    }

    public async Task<BaseResponse<StudentWithEnrollmentsResponse>> GetStudentWithEnrollmentsAsync(Guid id)
    {
        await SetAuthorizationHeaderAsync();
        return await httpClient.GetFromJsonAsync<BaseResponse<StudentWithEnrollmentsResponse>>(
            $"/api/v1/students/{id}/enrollments") ??
               new BaseResponse<StudentWithEnrollmentsResponse>();
    }

    public async Task<BaseResponse<List<StudentResponse>>> SearchStudentsByNameAsync(string name)
    {
        await SetAuthorizationHeaderAsync();
        return await httpClient.GetFromJsonAsync<BaseResponse<List<StudentResponse>>>(
            $"/api/v1/students/search?name={Uri.EscapeDataString(name)}") ?? new BaseResponse<List<StudentResponse>>();
    }

    public async Task<BaseResponse<StudentResponse>> GetStudentByCpfAsync(string cpf)
    {
        await SetAuthorizationHeaderAsync();
        return await httpClient.GetFromJsonAsync<BaseResponse<StudentResponse>>($"/api/v1/students/cpf/{cpf}") ??
               new BaseResponse<StudentResponse>();
    }

    public async Task<bool> CreateStudentAsync(CreateStudentRequest student)
    {
        await SetAuthorizationHeaderAsync();
        var response = await httpClient.PostAsJsonAsync("/api/v1/students", student);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateStudentAsync(Guid id, UpdateStudentRequest student)
    {
        await SetAuthorizationHeaderAsync();
        var response = await httpClient.PutAsJsonAsync($"/api/v1/students/{id}", student);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteStudentAsync(Guid id)
    {
        await SetAuthorizationHeaderAsync();
        var response = await httpClient.DeleteAsync($"/api/v1/students/{id}");
        return response.IsSuccessStatusCode;
    }

    // Classes
    public async Task<BaseResponse<BasePagination<ClassWithStudentCountResponse>>> GetClassesAsync(int pageNumber = 1, int pageSize = 10)
    {
        await SetAuthorizationHeaderAsync();
        return await httpClient.GetFromJsonAsync<BaseResponse<BasePagination<ClassWithStudentCountResponse>>>(
            $"/api/v1/classes?pageNumber={pageNumber}&pageSize={pageSize}") 
            ?? new BaseResponse<BasePagination<ClassWithStudentCountResponse>>();
    }

    public async Task<BaseResponse<ClassWithStudentCountResponse>> GetClassByIdAsync(Guid id)
    {
        await SetAuthorizationHeaderAsync();
        return await httpClient.GetFromJsonAsync<BaseResponse<ClassWithStudentCountResponse>>($"/api/v1/classes/{id}") ??
               new BaseResponse<ClassWithStudentCountResponse>();
    }

    public async Task<BaseResponse<ClassWithStudentsResponse>> GetClassWithStudentsAsync(Guid id)
    {
        await SetAuthorizationHeaderAsync();
        return await httpClient.GetFromJsonAsync<BaseResponse<ClassWithStudentsResponse>>(
            $"/api/v1/classes/{id}/students") ?? new BaseResponse<ClassWithStudentsResponse>();
    }

    public async Task<bool> CreateClassAsync(CreateClassRequest classRequest)
    {
        await SetAuthorizationHeaderAsync();
        var response = await httpClient.PostAsJsonAsync("/api/v1/classes", classRequest);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateClassAsync(Guid id, UpdateClassRequest classRequest)
    {
        await SetAuthorizationHeaderAsync();
        var response = await httpClient.PutAsJsonAsync($"/api/v1/classes/{id}", classRequest);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteClassAsync(Guid id)
    {
        await SetAuthorizationHeaderAsync();
        var response = await httpClient.DeleteAsync($"/api/v1/classes/{id}");
        return response.IsSuccessStatusCode;
    }

    // Enrollments
    public async Task<BaseResponse<List<EnrollmentResponse>>> GetEnrollmentsByStudentAsync(Guid studentId)
    {
        await SetAuthorizationHeaderAsync();
        return await httpClient.GetFromJsonAsync<BaseResponse<List<EnrollmentResponse>>>(
            $"/api/v1/enrollments/student/{studentId}") ?? new BaseResponse<List<EnrollmentResponse>>();
    }

    public async Task<BaseResponse<EnrollmentResponse>> GetEnrollmentsByClassAsync(Guid classId)
    {
        await SetAuthorizationHeaderAsync();
        return await httpClient.GetFromJsonAsync<BaseResponse<EnrollmentResponse>>(
            $"/api/v1/enrollments/class/{classId}") ?? new BaseResponse<EnrollmentResponse>();
    }

    public async Task<BaseResponse<EnrollmentResponse>> GetEnrollmentByIdAsync(Guid id)
    {
        await SetAuthorizationHeaderAsync();
        return await httpClient.GetFromJsonAsync<BaseResponse<EnrollmentResponse>>($"/api/v1/enrollments/{id}") ??
               new BaseResponse<EnrollmentResponse>();
    }

    public async Task<bool> CreateEnrollmentAsync(CreateEnrollmentRequest enrollment)
    {
        await SetAuthorizationHeaderAsync();
        var response = await httpClient.PostAsJsonAsync("/api/v1/enrollments", enrollment);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteEnrollmentAsync(Guid id)
    {
        await SetAuthorizationHeaderAsync();
        var response = await httpClient.DeleteAsync($"/api/v1/enrollments/{id}");
        return response.IsSuccessStatusCode;
    }
}