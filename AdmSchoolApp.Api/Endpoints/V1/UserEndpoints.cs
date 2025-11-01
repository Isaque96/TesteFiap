using AdmSchoolApp.Extensions;
using AdmSchoolApp.Domain.Entities;
using AdmSchoolApp.Domain.Enums;
using AdmSchoolApp.Domain.Models.Requests;
using AdmSchoolApp.Domain.Models.Responses;
using AdmSchoolApp.Application.Services;
using AdmSchoolApp.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace AdmSchoolApp.Endpoints.V1;

public static class UserEndpoints
{
    private const string Admin = "Admin";
    private const string UsuarioNaoEncontrado = "Usuário não encontrado";
    
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/users")
            .WithTags("Users");

        group.MapGet("/", GetAllUsersAsync)
            .WithName("GetAllUsers")
            .WithSummary("Lista todos os usuários paginados")
            .RequireAuthorization(policy => policy.RequireRole(Admin));

        group.MapGet("/{id:guid}", GetUserByIdAsync)
            .WithName("GetUserById")
            .WithSummary("Busca usuário por ID");

        group.MapGet("/email/{email}", GetUserByEmailAsync)
            .WithName("GetUserByEmail")
            .WithSummary("Busca usuário por email")
            .RequireAuthorization(policy => policy.RequireRole(Admin));

        group.MapPost("/", CreateUserAsync)
            .WithName("CreateUser")
            .WithSummary("Cria novo usuário com roles")
            .RequireAuthorization(policy => policy.RequireRole(Admin));

        group.MapPut("/{id:guid}", UpdateUserAsync)
            .WithName("UpdateUser")
            .WithSummary("Atualiza usuário")
            .RequireAuthorization(policy => policy.RequireRole(Admin));

        group.MapPut("/{id:guid}/password", ChangePasswordAsync)
            .WithName("ChangePassword")
            .WithSummary("Altera senha do usuário");

        group.MapPut("/{id:guid}/roles", UpdateUserRolesAsync)
            .WithName("UpdateUserRoles")
            .WithSummary("Atualiza roles do usuário")
            .RequireAuthorization(policy => policy.RequireRole(Admin));

        group.MapDelete("/{id:guid}", DeleteUserAsync)
            .WithName("DeleteUser")
            .WithSummary("Exclui usuário")
            .RequireAuthorization(policy => policy.RequireRole(Admin));

        return group;
    }

    private static async Task<IResult> GetAllUsersAsync(
        [FromServices] UserService service,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10
    )
    {
        var (items, totalCount) = await service.GetPaginatedAsync(pageNumber, pageSize);
        
        var responseItems = items.Select(u => new UserResponse(
            u.Id,
            u.Name,
            u.Email,
            u.IsActive,
            u.UserRoles.Select(ur => ur.Role.Name).ToList(),
            u.CreatedAt,
            u.UpdatedAt
        )).ToList();
        
        var response = new BasePagination<UserResponse>(
            responseItems,
            pageNumber,
            pageSize,
            totalCount
        );
        
        return ApiResponseExtensions.Success(response, "Usuários listados com sucesso");
    }

    private static async Task<IResult> GetUserByIdAsync(
        Guid id,
        [FromServices] UserService service
    )
    {
        var user = await service.GetWithRolesAsync(id);
        
        if (user == null)
            return ApiResponseExtensions.NotFound(UsuarioNaoEncontrado);

        var response = new UserResponse(
            user.Id,
            user.Name,
            user.Email,
            user.IsActive,
            user.UserRoles.Select(ur => ur.Role.Name).ToList(),
            user.CreatedAt,
            user.UpdatedAt
        );

        return ApiResponseExtensions.Success(response);
    }

    private static async Task<IResult> GetUserByEmailAsync(
        string email,
        [FromServices] UserService service
    )
    {
        var user = await service.GetByEmailAsync(email);
        
        if (user == null)
            return ApiResponseExtensions.NotFound(UsuarioNaoEncontrado);

        var response = new UserResponse(
            user.Id,
            user.Name,
            user.Email,
            user.IsActive,
            user.UserRoles.Select(ur => ur.Role.Name).ToList(),
            user.CreatedAt,
            user.UpdatedAt
        );

        return ApiResponseExtensions.Success(response);
    }

    private static async Task<IResult> CreateUserAsync(
        [FromBody] CreateUserRequest request,
        [FromServices] UserService service
    )
    {
        // Verificar se email já existe
        if (await service.EmailExistsAsync(request.Email))
            return ApiResponseExtensions.BadRequest(
                ["Email já cadastrado"],
                InternalCodes.EmailAlreadyExists
            );

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            PasswordHash = System.Text.Encoding.UTF8.GetBytes(request.Password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var (isValid, validationResult, entity) = await service.AddAsync(user);

        if (!isValid)
            return ApiResponseExtensions.BadRequest(validationResult);

        // Atribuir roles padrão
        await service.AssignRolesAsync(entity!.Id, ["User"]);

        // Recarregar com roles
        var userWithRoles = await service.GetWithRolesAsync(entity.Id);

        var response = new UserResponse(
            userWithRoles!.Id,
            userWithRoles.Name,
            userWithRoles.Email,
            userWithRoles.IsActive,
            userWithRoles.UserRoles.Select(ur => ur.Role.Name).ToList(),
            userWithRoles.CreatedAt,
            userWithRoles.UpdatedAt
        );

        return ApiResponseExtensions.Created(
            response,
            $"/api/v1/users/{entity.Id}",
            "Usuário criado com sucesso"
        );
    }

    private static async Task<IResult> UpdateUserAsync(
        Guid id,
        [FromBody] UpdateUserRequest request,
        [FromServices] UserService service
    )
    {
        var existingUser = await service.GetByIdAsync(id);
        
        if (existingUser == null)
            return ApiResponseExtensions.NotFound(UsuarioNaoEncontrado);

        // Verificar unicidade de email excluindo o próprio registro
        if (await service.EmailExistsAsync(request.Email, id))
        {
            return ApiResponseExtensions.BadRequest(
                ["Email já cadastrado"],
                InternalCodes.EmailAlreadyExists
            );
        }

        existingUser.Name = request.Name;
        existingUser.Email = request.Email;
        existingUser.IsActive = request.IsActive;
        existingUser.UpdatedAt = DateTime.UtcNow;

        var (isValid, validationResult) = await service.UpdateAsync(existingUser);

        if (!isValid)
            return ApiResponseExtensions.BadRequest(validationResult);

        var userWithRoles = await service.GetWithRolesAsync(id);

        var response = new UserResponse(
            userWithRoles!.Id,
            userWithRoles.Name,
            userWithRoles.Email,
            userWithRoles.IsActive,
            userWithRoles.UserRoles.Select(ur => ur.Role.Name).ToList(),
            userWithRoles.CreatedAt,
            userWithRoles.UpdatedAt
        );

        return ApiResponseExtensions.Success(response, "Usuário atualizado com sucesso");
    }

    private static async Task<IResult> ChangePasswordAsync(
        Guid id,
        [FromBody] ChangePasswordRequest request,
        [FromServices] UserService service
    )
    {
        var user = await service.GetByIdAsync(id);
        
        if (user == null)
            return ApiResponseExtensions.NotFound(UsuarioNaoEncontrado);

        // Validar senha atual
        if (!await service.ValidatePasswordAsync(id, request.CurrentPassword))
        {
            return ApiResponseExtensions.BadRequest(
                ["Senha atual incorreta"],
                InternalCodes.UnauthorizedRequest
            );
        }

        // Atualizar senha
        await service.ChangePasswordAsync(id, request.NewPassword);

        return ApiResponseExtensions.NoContent("Senha alterada com sucesso");
    }

    private static async Task<IResult> UpdateUserRolesAsync(
        Guid id,
        [FromBody] UpdateUserRolesRequest request,
        [FromServices] UserService service
    )
    {
        var user = await service.GetByIdAsync(id);
        
        if (user == null)
            return ApiResponseExtensions.NotFound(UsuarioNaoEncontrado);

        await service.AssignRolesAsync(id, request.Roles);

        var userWithRoles = await service.GetWithRolesAsync(id);

        var response = new UserResponse(
            userWithRoles!.Id,
            userWithRoles.Name,
            userWithRoles.Email,
            userWithRoles.IsActive,
            userWithRoles.UserRoles.Select(ur => ur.Role.Name).ToList(),
            userWithRoles.CreatedAt,
            userWithRoles.UpdatedAt
        );

        return ApiResponseExtensions.Success(response, "Roles atualizadas com sucesso");
    }

    private static async Task<IResult> DeleteUserAsync(
        Guid id,
        [FromServices] UserService service
    )
    {
        var user = await service.GetByIdAsync(id);
        
        if (user == null)
            return ApiResponseExtensions.NotFound(UsuarioNaoEncontrado);

        await service.DeleteAsync(id);
        
        return ApiResponseExtensions.NoContent("Usuário excluído com sucesso");
    }
}