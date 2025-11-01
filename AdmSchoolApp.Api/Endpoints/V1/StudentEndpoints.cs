using AdmSchoolApp.Extensions;
using AdmSchoolApp.Domain.Entities;
using AdmSchoolApp.Domain.Enums;
using AdmSchoolApp.Domain.Models.Requests;
using AdmSchoolApp.Application.Services;
using AdmSchoolApp.Domain.Specifications;
using Microsoft.AspNetCore.Mvc;

namespace AdmSchoolApp.Endpoints.V1;

public static class StudentEndpoints
{
    private const string AlunoNaoEncontrado = "Aluno não encontrado";
    
    public static IEndpointRouteBuilder MapStudentEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/students")
            .WithTags("Students")
            .RequireAuthorization();

        group.MapGet("/", GetAllStudentsAsync)
            .WithName("GetAllStudents")
            .WithSummary("Lista todos os alunos paginados");

        group.MapGet("/{id:guid}", GetStudentByIdAsync)
            .WithName("GetStudentById")
            .WithSummary("Busca aluno por ID");

        group.MapGet("/search", SearchStudentsByNameAsync)
            .WithName("SearchStudentsByName")
            .WithSummary("Busca alunos por nome");

        group.MapGet("/cpf/{cpf}", GetStudentByCpfAsync)
            .WithName("GetStudentByCpf")
            .WithSummary("Busca aluno por CPF");

        group.MapPost("/", CreateStudentAsync)
            .WithName("CreateStudent")
            .WithSummary("Cria novo aluno");

        group.MapPut("/{id:guid}", UpdateStudentAsync)
            .WithName("UpdateStudent")
            .WithSummary("Atualiza aluno");

        group.MapDelete("/{id:guid}", DeleteStudentAsync)
            .WithName("DeleteStudent")
            .WithSummary("Exclui aluno");

        return group;
    }

    private static async Task<IResult> GetAllStudentsAsync(
        [FromServices] StudentService service,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10
    )
    {
        var spec = new StudentsPaginatedSpecification(pageNumber, pageSize);
        var students = await service.FindAsync(spec);
        
        return ApiResponseExtensions.Success(students, "Alunos listados com sucesso");
    }

    private static async Task<IResult> GetStudentByIdAsync(
        Guid id,
        [FromServices] StudentService service
    )
    {
        var student = await service.GetByIdAsync(id);
        
        return student == null ?
            ApiResponseExtensions.NotFound(AlunoNaoEncontrado) :
            ApiResponseExtensions.Success(student);
    }

    private static async Task<IResult> SearchStudentsByNameAsync(
        [FromQuery] string name,
        [FromServices] StudentService service
    )
    {
        if (string.IsNullOrWhiteSpace(name))
            return ApiResponseExtensions.BadRequest(["Nome é obrigatório para busca"]);

        var students = await service.SearchByNameAsync(name);
        return ApiResponseExtensions.Success(students, "Busca realizada com sucesso");
    }

    private static async Task<IResult> GetStudentByCpfAsync(
        string cpf,
        [FromServices] StudentService service
    )
    {
        var student = await service.GetByCpfAsync(cpf);
        
        return student == null ?
            ApiResponseExtensions.NotFound(AlunoNaoEncontrado) :
            ApiResponseExtensions.Success(student);
    }

    private static async Task<IResult> CreateStudentAsync(
        [FromBody] CreateStudentRequest request,
        [FromServices] StudentService service
    )
    {
        // REQUISITO 6: Verificar unicidade de CPF e Email
        if (await service.CpfExistsAsync(request.Cpf))
        {
            return ApiResponseExtensions.BadRequest(
                ["CPF já cadastrado"],
                InternalCodes.CpfAlreadyExists
            );
        }

        if (await service.EmailExistsAsync(request.Email))
        {
            return ApiResponseExtensions.BadRequest(
                ["Email já cadastrado"],
                InternalCodes.EmailAlreadyExists
            );
        }

        var student = new Student
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            BirthDate = request.BirthDate,
            Cpf = request.Cpf,
            Email = request.Email,
            PasswordHash = System.Text.Encoding.UTF8.GetBytes(request.Password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var (isValid, validationResult, entity) = await service.AddAsync(student);

        if (!isValid)
            return ApiResponseExtensions.BadRequest(validationResult);

        return ApiResponseExtensions.Created(
            entity,
            $"/api/v1/students/{entity!.Id}",
            "Aluno criado com sucesso"
        );
    }

    private static async Task<IResult> UpdateStudentAsync(
        Guid id,
        [FromBody] CreateStudentRequest request,
        [FromServices] StudentService service
    )
    {
        var existingStudent = await service.GetByIdAsync(id);
        
        if (existingStudent == null)
            return ApiResponseExtensions.NotFound(AlunoNaoEncontrado);

        // REQUISITO 6: Verificar unicidade excluindo o próprio registro
        if (await service.CpfExistsAsync(request.Cpf, id))
            return ApiResponseExtensions.BadRequest(
                ["CPF já cadastrado"],
                InternalCodes.CpfAlreadyExists
            );

        if (await service.EmailExistsAsync(request.Email, id))
            return ApiResponseExtensions.BadRequest(
                ["Email já cadastrado"],
                InternalCodes.EmailAlreadyExists
            );

        existingStudent.Name = request.Name;
        existingStudent.BirthDate = request.BirthDate;
        existingStudent.Cpf = request.Cpf;
        existingStudent.Email = request.Email;
        existingStudent.UpdatedAt = DateTime.UtcNow;

        var (isValid, validationResult) = await service.UpdateAsync(existingStudent);

        if (!isValid)
            return ApiResponseExtensions.BadRequest(validationResult);

        return ApiResponseExtensions.Success(existingStudent, "Aluno atualizado com sucesso");
    }

    private static async Task<IResult> DeleteStudentAsync(
        Guid id,
        [FromServices] StudentService service
    )
    {
        var student = await service.GetByIdAsync(id);
        
        if (student == null)
            return ApiResponseExtensions.NotFound(AlunoNaoEncontrado);

        await service.DeleteAsync(id);
        
        return ApiResponseExtensions.NoContent("Aluno excluído com sucesso");
    }
}