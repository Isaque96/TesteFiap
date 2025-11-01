using AdmSchoolApp.Extensions;
using AdmSchoolApp.Application.Services;
using AdmSchoolApp.Domain.Entities;
using AdmSchoolApp.Domain.Models.Requests;
using AdmSchoolApp.Domain.Models.Responses;
using AdmSchoolApp.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace AdmSchoolApp.Endpoints.V1;

public static class ClassEndpoints
{
    private const string TurmaNaoEncontrada = "Turma não encontrada";
    
    public static IEndpointRouteBuilder MapClassEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/classes")
            .WithTags("Classes");

        group.MapGet("/", GetAllClassesAsync)
            .WithName("GetAllClasses")
            .WithSummary("Lista turmas paginadas, ordenadas e com contagem de alunos")
            .WithDescription("Query params: pageNumber (default 1), pageSize (default 10).")
            .Produces<BasePagination<ClassWithStudentCountResponse>>(StatusCodes.Status200OK, SwaggerExtensions.JsonContentType)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/{id:guid}", GetClassByIdAsync)
            .WithName("GetClassById")
            .WithSummary("Busca turma por ID")
            .Produces<ClassWithStudentCountResponse>(StatusCodes.Status200OK, SwaggerExtensions.JsonContentType)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/{id:guid}/students", GetClassWithStudentsAsync)
            .WithName("GetClassWithStudents")
            .WithSummary("Busca turma com lista de alunos matriculados")
            .Produces<ClassWithStudentsResponse>(StatusCodes.Status200OK, SwaggerExtensions.JsonContentType)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPost("/", CreateClassAsync)
            .WithName("CreateClass")
            .WithSummary("Cria nova turma (REQUISITO 3 e 4)")
            .Accepts<CreateClassRequest>(SwaggerExtensions.JsonContentType)
            .Produces<ClassResponse>(StatusCodes.Status201Created, SwaggerExtensions.JsonContentType)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPut("/{id:guid}", UpdateClassAsync)
            .WithName("UpdateClass")
            .WithSummary("Atualiza turma")
            .Accepts<UpdateClassRequest>(SwaggerExtensions.JsonContentType)
            .Produces<ClassResponse>(StatusCodes.Status200OK, SwaggerExtensions.JsonContentType)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapDelete("/{id:guid}", DeleteClassAsync)
            .WithName("DeleteClass")
            .WithSummary("Exclui turma")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        return group;
    }

    private static async Task<IResult> GetAllClassesAsync(
        [FromServices] ClassService service,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10
    )
    {
        var (items, totalCount) = await service.GetPaginatedAsync(pageNumber, pageSize);
        
        // REQUISITO 2: Incluir contagem de alunos
        var responseItems = new List<ClassWithStudentCountResponse>();
        foreach (var item in items)
        {
            var studentCount = await service.CountStudentsAsync(item.Id);
            responseItems.Add(new ClassWithStudentCountResponse(
                item.Id, item.Name, item.Description, studentCount, item.CreatedAt, item.UpdatedAt
            ));
        }
        
        var response = new BasePagination<ClassWithStudentCountResponse>(
            responseItems,
            pageNumber,
            pageSize,
            totalCount
        );
        
        return ApiResponseExtensions.Success(response, "Turmas listadas com sucesso");
    }

    private static async Task<IResult> GetClassByIdAsync(
        Guid id,
        [FromServices] ClassService service
    )
    {
        var classEntity = await service.GetByIdAsync(id);
        
        if (classEntity == null)
            return ApiResponseExtensions.NotFound(TurmaNaoEncontrada);

        var studentCount = await service.CountStudentsAsync(id);
        var response = new ClassWithStudentCountResponse(
            classEntity.Id, classEntity.Name, classEntity.Description, 
            studentCount, classEntity.CreatedAt, classEntity.UpdatedAt
        );

        return ApiResponseExtensions.Success(response);
    }

    private static async Task<IResult> GetClassWithStudentsAsync(
        Guid id,
        [FromServices] ClassService service
    )
    {
        var classEntity = await service.GetWithStudentsAsync(id);
        
        if (classEntity == null)
            return ApiResponseExtensions.NotFound(TurmaNaoEncontrada);

        var response = new ClassWithStudentsResponse(
            classEntity.Id,
            classEntity.Name,
            classEntity.Description,
            classEntity.CreatedAt,
            classEntity.UpdatedAt,
            classEntity.Enrollments.Select(e => new StudentSummary(
                e.Student.Id, e.Student.Name, e.Student.Email, e.Student.Cpf
            )).ToList()
        );

        return ApiResponseExtensions.Success(response);
    }

    private static async Task<IResult> CreateClassAsync(
        [FromBody] CreateClassRequest request,
        [FromServices] ClassService service
    )
    {
        if (await service.NameExistsAsync(request.Name)) 
            return ApiResponseExtensions.BadRequest(["Nome da turma já cadastrado"]);

        var classEntity = new Class
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        var (isValid, validationResult, entity) = await service.AddAsync(classEntity);

        if (!isValid)
            return ApiResponseExtensions.BadRequest(validationResult);

        var response = new ClassResponse(
            entity!.Id, entity.Name, entity.Description, entity.CreatedAt, entity.UpdatedAt
        );

        return ApiResponseExtensions.Created(
            response,
            $"/api/v1/classes/{entity.Id}",
            "Turma criada com sucesso"
        );
    }

    private static async Task<IResult> UpdateClassAsync(
        Guid id,
        [FromBody] UpdateClassRequest request,
        [FromServices] ClassService service
    )
    {
        var existingClass = await service.GetByIdAsync(id);
        
        if (existingClass == null)
            return ApiResponseExtensions.NotFound(TurmaNaoEncontrada);

        existingClass.Name = request.Name;
        existingClass.Description = request.Description;
        existingClass.UpdatedAt = DateTime.UtcNow;

        var (isValid, validationResult) = await service.UpdateAsync(existingClass);

        if (!isValid)
            return ApiResponseExtensions.BadRequest(validationResult);

        var response = new ClassResponse(
            existingClass.Id, existingClass.Name, existingClass.Description,
            existingClass.CreatedAt, existingClass.UpdatedAt
        );

        return ApiResponseExtensions.Success(response, "Turma atualizada com sucesso");
    }

    private static async Task<IResult> DeleteClassAsync(
        Guid id,
        [FromServices] ClassService service
    )
    {
        var classEntity = await service.GetByIdAsync(id);
        
        if (classEntity == null)
            return ApiResponseExtensions.NotFound(TurmaNaoEncontrada);

        await service.DeleteAsync(id);
        
        return ApiResponseExtensions.NoContent("Turma excluída com sucesso");
    }
}