using AdmSchoolApp.Extensions;
using AdmSchoolApp.Domain.Entities;
using AdmSchoolApp.Domain.Enums;
using AdmSchoolApp.Domain.Models.Requests;
using AdmSchoolApp.Application.Services;
using AdmSchoolApp.Domain.Models;
using AdmSchoolApp.Domain.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace AdmSchoolApp.Endpoints.V1;

public static class StudentEndpoints
{
    private const string AlunoNaoEncontrado = "Aluno não encontrado";
    
    public static IEndpointRouteBuilder MapStudentEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/students")
            .WithTags("Students");

        group.MapGet("/", GetAllStudentsAsync)
            .WithName("GetAllStudents")
            .WithSummary("REQUISITO 1: Lista todos os alunos paginados e ordenados alfabeticamente")
            .WithDescription("Query params: pageNumber (default 1), pageSize (default 10).")
            .Produces<BasePagination<StudentResponse>>(StatusCodes.Status200OK, SwaggerExtensions.JsonContentType)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/{id:guid}", GetStudentByIdAsync)
            .WithName("GetStudentById")
            .WithSummary("Busca aluno por ID")
            .Produces<StudentResponse>(StatusCodes.Status200OK, SwaggerExtensions.JsonContentType)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/{id:guid}/enrollments", GetStudentWithEnrollmentsAsync)
            .WithName("GetStudentWithEnrollments")
            .WithSummary("Busca aluno com suas matrículas")
            .Produces<StudentWithEnrollmentsResponse>(StatusCodes.Status200OK, SwaggerExtensions.JsonContentType)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/search", SearchStudentsByNameAsync)
            .WithName("SearchStudentsByName")
            .WithSummary("REQUISITO 9: Busca alunos por nome")
            .WithDescription("Query param obrigatório: name")
            .Produces<List<StudentResponse>>(StatusCodes.Status200OK, SwaggerExtensions.JsonContentType)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/cpf/{cpf}", GetStudentByCpfAsync)
            .WithName("GetStudentByCpf")
            .WithSummary("REQUISITO 9: Busca aluno por CPF")
            .Produces<StudentResponse>(StatusCodes.Status200OK, SwaggerExtensions.JsonContentType)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPost("/", CreateStudentAsync)
            .WithName("CreateStudent")
            .WithSummary("Cria novo aluno com validações (REQUISITOS 3, 4, 6, 7, 8)")
            .Accepts<CreateStudentRequest>(SwaggerExtensions.JsonContentType)
            .Produces<StudentResponse>(StatusCodes.Status201Created, SwaggerExtensions.JsonContentType)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPut("/{id:guid}", UpdateStudentAsync)
            .WithName("UpdateStudent")
            .WithSummary("Atualiza aluno")
            .Accepts<UpdateStudentRequest>(SwaggerExtensions.JsonContentType)
            .Produces<StudentResponse>(StatusCodes.Status200OK, SwaggerExtensions.JsonContentType)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapDelete("/{id:guid}", DeleteStudentAsync)
            .WithName("DeleteStudent")
            .WithSummary("Exclui aluno")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        return group;
    }

    private static async Task<IResult> GetAllStudentsAsync(
        [FromServices] StudentService service,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10
    )
    {
        var (items, totalCount) = await service.GetPaginatedAsync(pageNumber, pageSize);
        
        var response = new BasePagination<StudentResponse>(
            items.Select(s => new StudentResponse(
                s.Id, s.Name, s.BirthDate, s.Cpf, s.Email, s.CreatedAt, s.UpdatedAt
            )).ToList(),
            pageNumber,
            pageSize,
            totalCount
        );
        
        return ApiResponseExtensions.Success(response, "Alunos listados com sucesso");
    }

    private static async Task<IResult> GetStudentByIdAsync(
        Guid id,
        [FromServices] StudentService service
    )
    {
        var student = await service.GetByIdAsync(id);
        
        if (student == null)
            return ApiResponseExtensions.NotFound("Aluno não encontrado");

        var response = new StudentResponse(
            student.Id, student.Name, student.BirthDate, 
            student.Cpf, student.Email, student.CreatedAt, student.UpdatedAt
        );

        return ApiResponseExtensions.Success(response);
    }

    private static async Task<IResult> GetStudentWithEnrollmentsAsync(
        Guid id,
        [FromServices] StudentService service
    )
    {
        var student = await service.GetWithEnrollmentsAsync(id);
        
        if (student == null)
            return ApiResponseExtensions.NotFound(AlunoNaoEncontrado);

        var response = new StudentWithEnrollmentsResponse(
            student.Id,
            student.Name,
            student.BirthDate,
            student.Cpf,
            student.Email,
            student.CreatedAt,
            student.UpdatedAt,
            student.Enrollments.Select(e => new EnrollmentSummary(
                e.Id, e.ClassId, e.Class.Name, e.CreatedAt
            )).ToList()
        );

        return ApiResponseExtensions.Success(response);
    }

    private static async Task<IResult> SearchStudentsByNameAsync(
        [FromQuery] string name,
        [FromServices] StudentService service
    )
    {
        if (string.IsNullOrWhiteSpace(name))
            return ApiResponseExtensions.BadRequest(["Nome é obrigatório para busca"]);

        var students = await service.SearchByNameAsync(name);
        
        var response = students.Select(s => new StudentResponse(
            s.Id, s.Name, s.BirthDate, s.Cpf, s.Email, s.CreatedAt, s.UpdatedAt
        )).ToList();

        return ApiResponseExtensions.Success(response, "Busca realizada com sucesso");
    }

    private static async Task<IResult> GetStudentByCpfAsync(
        string cpf,
        [FromServices] StudentService service
    )
    {
        var student = await service.GetByCpfAsync(cpf);
        
        if (student == null)
            return ApiResponseExtensions.NotFound(AlunoNaoEncontrado);

        var response = new StudentResponse(
            student.Id, student.Name, student.BirthDate, 
            student.Cpf, student.Email, student.CreatedAt, student.UpdatedAt
        );

        return ApiResponseExtensions.Success(response);
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

        var response = new StudentResponse(
            entity!.Id, entity.Name, entity.BirthDate, 
            entity.Cpf, entity.Email, entity.CreatedAt, entity.UpdatedAt
        );

        return ApiResponseExtensions.Created(
            response,
            $"/api/v1/students/{entity.Id}",
            "Aluno criado com sucesso"
        );
    }

    private static async Task<IResult> UpdateStudentAsync(
        Guid id,
        [FromBody] UpdateStudentRequest request,
        [FromServices] StudentService service
    )
    {
        var existingStudent = await service.GetByIdAsync(id);
        
        if (existingStudent == null)
            return ApiResponseExtensions.NotFound(AlunoNaoEncontrado);

        // REQUISITO 6: Verificar unicidade excluindo o próprio registro
        if (await service.CpfExistsAsync(request.Cpf, id))
        {
            return ApiResponseExtensions.BadRequest(
                ["CPF já cadastrado"],
                InternalCodes.CpfAlreadyExists
            );
        }

        if (await service.EmailExistsAsync(request.Email, id))
        {
            return ApiResponseExtensions.BadRequest(
                ["Email já cadastrado"],
                InternalCodes.EmailAlreadyExists
            );
        }

        existingStudent.Name = request.Name;
        existingStudent.BirthDate = request.BirthDate;
        existingStudent.Cpf = request.Cpf;
        existingStudent.Email = request.Email;
        existingStudent.UpdatedAt = DateTime.UtcNow;

        var (isValid, validationResult) = await service.UpdateAsync(existingStudent);

        if (!isValid)
            return ApiResponseExtensions.BadRequest(validationResult);

        var response = new StudentResponse(
            existingStudent.Id, existingStudent.Name, existingStudent.BirthDate,
            existingStudent.Cpf, existingStudent.Email, existingStudent.CreatedAt, existingStudent.UpdatedAt
        );

        return ApiResponseExtensions.Success(response, "Aluno atualizado com sucesso");
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