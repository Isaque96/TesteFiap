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
    public async Task<BasePagination<StudentResponse>> GetStudentsAsync(int pageNumber = 1, int pageSize = 10)
    {
        await SetAuthorizationHeaderAsync();
        return await httpClient.GetFromJsonAsync<BasePagination<StudentResponse>>(
            $"/api/v1/students?pageNumber={pageNumber}&pageSize={pageSize}") 
            ?? new BasePagination<StudentResponse>();
    }

    public async Task<StudentResponse?> GetStudentByIdAsync(Guid id)
    {
        await SetAuthorizationHeaderAsync();
        return await httpClient.GetFromJsonAsync<StudentResponse>($"/api/v1/students/{id}");
    }

    public async Task<StudentWithEnrollmentsResponse?> GetStudentWithEnrollmentsAsync(Guid id)
    {
        await SetAuthorizationHeaderAsync();
        return await httpClient.GetFromJsonAsync<StudentWithEnrollmentsResponse>(
            $"/api/v1/students/{id}/enrollments");
    }

    public async Task<List<StudentResponse>> SearchStudentsByNameAsync(string name)
    {
        await SetAuthorizationHeaderAsync();
        return await httpClient.GetFromJsonAsync<List<StudentResponse>>(
            $"/api/v1/students/search?name={Uri.EscapeDataString(name)}") ?? [];
    }

    public async Task<StudentResponse?> GetStudentByCpfAsync(string cpf)
    {
        await SetAuthorizationHeaderAsync();
        return await httpClient.GetFromJsonAsync<StudentResponse>($"/api/v1/students/cpf/{cpf}");
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
    public async Task<BasePagination<ClassWithStudentCountResponse>> GetClassesAsync(int pageNumber = 1, int pageSize = 10)
    {
        await SetAuthorizationHeaderAsync();
        return await httpClient.GetFromJsonAsync<BasePagination<ClassWithStudentCountResponse>>(
            $"/api/v1/classes?pageNumber={pageNumber}&pageSize={pageSize}") 
            ?? new BasePagination<ClassWithStudentCountResponse>();
    }

    public async Task<ClassWithStudentCountResponse?> GetClassByIdAsync(Guid id)
    {
        await SetAuthorizationHeaderAsync();
        return await httpClient.GetFromJsonAsync<ClassWithStudentCountResponse>($"/api/v1/classes/{id}");
    }

    public async Task<ClassWithStudentsResponse?> GetClassWithStudentsAsync(Guid id)
    {
        await SetAuthorizationHeaderAsync();
        return await httpClient.GetFromJsonAsync<ClassWithStudentsResponse>(
            $"/api/v1/classes/{id}/students");
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
    public async Task<List<EnrollmentResponse>> GetEnrollmentsByStudentAsync(Guid studentId)
    {
        await SetAuthorizationHeaderAsync();
        return await httpClient.GetFromJsonAsync<List<EnrollmentResponse>>(
            $"/api/v1/enrollments/student/{studentId}") ?? [];
    }

    public async Task<EnrollmentResponse?> GetEnrollmentsByClassAsync(Guid classId)
    {
        await SetAuthorizationHeaderAsync();
        return await httpClient.GetFromJsonAsync<EnrollmentResponse>(
            $"/api/v1/enrollments/class/{classId}");
    }

    public async Task<EnrollmentResponse?> GetEnrollmentByIdAsync(Guid id)
    {
        await SetAuthorizationHeaderAsync();
        return await httpClient.GetFromJsonAsync<EnrollmentResponse>($"/api/v1/enrollments/{id}");
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