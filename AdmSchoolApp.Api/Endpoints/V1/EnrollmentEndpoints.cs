using AdmSchoolApp.Extensions;
using AdmSchoolApp.Application.Services;
using AdmSchoolApp.Domain.Entities;
using AdmSchoolApp.Domain.Enums;
using AdmSchoolApp.Domain.Models.Requests;
using AdmSchoolApp.Domain.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace AdmSchoolApp.Endpoints.V1;

public static class EnrollmentEndpoints
{
    public static IEndpointRouteBuilder MapEnrollmentEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/enrollments")
            .WithTags("Enrollments");

        group.MapGet("/student/{studentId:guid}", GetEnrollmentsByStudentAsync)
            .WithName("GetEnrollmentsByStudent")
            .WithSummary("Lista matrículas de um aluno");

        group.MapGet("/class/{classId:guid}", GetEnrollmentsByClassAsync)
            .WithName("GetEnrollmentsByClass")
            .WithSummary("Lista alunos matriculados em uma turma");

        group.MapGet("/{id:guid}", GetEnrollmentByIdAsync)
            .WithName("GetEnrollmentById")
            .WithSummary("Busca matrícula por ID");

        group.MapPost("/", CreateEnrollmentAsync)
            .WithName("CreateEnrollment")
            .WithSummary("Matricula aluno em turma (valida duplicidade)");

        group.MapDelete("/{id:guid}", DeleteEnrollmentAsync)
            .WithName("DeleteEnrollment")
            .WithSummary("Cancela matrícula");

        return group;
    }

    private static async Task<IResult> GetEnrollmentsByStudentAsync(
        Guid studentId,
        [FromServices] EnrollmentService service,
        [FromServices] StudentService studentService
    )
    {
        var student = await studentService.GetByIdAsync(studentId);
        if (student == null)
            return ApiResponseExtensions.NotFound("Aluno não encontrado");

        var enrollments = await service.GetByStudentAsync(studentId);
        
        var response = enrollments.Select(e => new EnrollmentResponse(
            e.Id, e.StudentId, e.Student.Name, e.ClassId, e.Class.Name, e.CreatedAt
        )).ToList();

        return ApiResponseExtensions.Success(response, "Matrículas listadas com sucesso");
    }

    private static async Task<IResult> GetEnrollmentsByClassAsync(
        Guid classId,
        [FromServices] EnrollmentService service,
        [FromServices] ClassService classService
    )
    {
        var classEntity = await classService.GetByIdAsync(classId);
        if (classEntity == null)
            return ApiResponseExtensions.NotFound("Turma não encontrada");

        var enrollments = await service.GetByClassAsync(classId);
        
        var response = enrollments.Select(e => new EnrollmentResponse(
            e.Id, e.StudentId, e.Student.Name, e.ClassId, e.Class.Name, e.CreatedAt
        )).ToList();

        return ApiResponseExtensions.Success(response, "Alunos matriculados listados com sucesso");
    }

    private static async Task<IResult> GetEnrollmentByIdAsync(
        Guid id,
        [FromServices] EnrollmentService service
    )
    {
        var enrollment = await service.GetWithDetailsAsync(id);
        
        if (enrollment == null)
            return ApiResponseExtensions.NotFound("Matrícula não encontrada");

        var response = new EnrollmentResponse(
            enrollment.Id, enrollment.StudentId, enrollment.Student.Name,
            enrollment.ClassId, enrollment.Class.Name, enrollment.CreatedAt
        );

        return ApiResponseExtensions.Success(response);
    }

    private static async Task<IResult> CreateEnrollmentAsync(
        [FromBody] CreateEnrollmentRequest request,
        [FromServices] EnrollmentService service,
        [FromServices] StudentService studentService,
        [FromServices] ClassService classService
    )
    {
        // Validar se aluno existe
        var student = await studentService.GetByIdAsync(request.StudentId);
        if (student == null)
            return ApiResponseExtensions.NotFound("Aluno não encontrado");

        // Validar se turma existe
        var classEntity = await classService.GetByIdAsync(request.ClassId);
        if (classEntity == null)
            return ApiResponseExtensions.NotFound("Turma não encontrada");

        // REQUISITO 5: Verificar se aluno já está matriculado
        if (await service.IsStudentEnrolledInClassAsync(request.StudentId, request.ClassId))
            return ApiResponseExtensions.BadRequest(
                ["Aluno já está matriculado nesta turma"],
                InternalCodes.StudentAlreadyEnrolled
            );

        var enrollment = new Enrollment
        {
            Id = Guid.NewGuid(),
            StudentId = request.StudentId,
            ClassId = request.ClassId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var (isValid, validationResult, entity) = await service.AddAsync(enrollment);

        if (!isValid)
            return ApiResponseExtensions.BadRequest(validationResult);

        // Recarregar com detalhes
        var enrollmentWithDetails = await service.GetWithDetailsAsync(entity!.Id);

        var response = new EnrollmentResponse(
            enrollmentWithDetails!.Id,
            enrollmentWithDetails.StudentId,
            enrollmentWithDetails.Student.Name,
            enrollmentWithDetails.ClassId,
            enrollmentWithDetails.Class.Name,
            enrollmentWithDetails.CreatedAt
        );

        return ApiResponseExtensions.Created(
            response,
            $"/api/v1/enrollments/{entity.Id}",
            "Matrícula realizada com sucesso"
        );
    }

    private static async Task<IResult> DeleteEnrollmentAsync(
        Guid id,
        [FromServices] EnrollmentService service
    )
    {
        var enrollment = await service.GetByIdAsync(id);
        
        if (enrollment == null)
            return ApiResponseExtensions.NotFound("Matrícula não encontrada");

        await service.DeleteAsync(id);
        
        return ApiResponseExtensions.NoContent("Matrícula cancelada com sucesso");
    }
}