using System.Text;
using AdmSchoolApp.Application.Utils;
using AdmSchoolApp.Application.Validators;
using AdmSchoolApp.Domain.Entities;
using AdmSchoolApp.Domain.Interfaces;
using AdmSchoolApp.Domain.Specifications;
using FluentValidation;
using FluentValidation.Results;

namespace AdmSchoolApp.Application.Services;

public class UserService(
    IRepository<User> repository,
    IRepository<Role> roleRepository,
    IRepository<UserRole> userRoleRepository,
    IValidator<User>? validator = null
) : BaseService<User>(repository, validator)
{
    public override async Task<(bool IsValid, ValidationResult ValidationResult, User? Entity)> AddAsync(User entity)
    {
        var validationResult = await ValidateAsync(entity);
        
        if (!validationResult.IsValid)
            return (false, validationResult, null);


        // Hash da senha
        if (entity.PasswordHash.Length > 0)
        {
            var password = Encoding.UTF8.GetString(entity.PasswordHash);
            var passwordValidator = new PasswordValidator();
            var passValResult = await passwordValidator.ValidateAsync(password);
            if (!passValResult.IsValid)
                return (false, passValResult, null);
            
            entity.PasswordHash = PasswordHasher.HashPassword(password);
        }

        var addedEntity = await Repository.AddAsync(entity);
        await Repository.SaveChangesAsync();
        return (true, validationResult, addedEntity);
    }

    public async Task<bool> EmailExistsAsync(string email, Guid? excludeId = null)
    {
        var spec = new UserByEmailSpecification(email);
        var user = await FirstOrDefaultAsync(spec);
        
        if (user == null)
            return false;
            
        return excludeId == null || user.Id != excludeId;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var spec = new UserByEmailSpecification(email);
        return await FirstOrDefaultAsync(spec);
    }

    public async Task<User?> GetWithRolesAsync(Guid userId)
    {
        var spec = new UserWithRolesSpecification(userId);
        return await FirstOrDefaultAsync(spec);
    }

    public async Task<(IEnumerable<User> Items, int TotalCount)> GetPaginatedAsync(int pageNumber = 1, int pageSize = 10)
    {
        var spec = new UsersPaginatedSpecification(pageNumber, pageSize);
        var items = await FindAsync(spec);
        
        var countSpec = new EmptyUserSpecification();
        var totalCount = await CountAsync(countSpec);
        
        return (items, totalCount);
    }

    public async Task<User?> AuthenticateAsync(string email, string password)
    {
        var user = await GetByEmailAsync(email);
        
        if (user is not { IsActive: true })
            return null;

        return !PasswordHasher.VerifyPassword(password, user.PasswordHash) ? null : user;
    }

    public async Task<bool> ValidatePasswordAsync(Guid userId, string password)
    {
        var user = await GetByIdAsync(userId);
        
        return user != null && PasswordHasher.VerifyPassword(password, user.PasswordHash);
    }

    public async Task ChangePasswordAsync(Guid userId, string newPassword)
    {
        var user = await GetByIdAsync(userId);
        
        if (user == null)
            throw new InvalidOperationException("Usuário não encontrado");

        user.PasswordHash = PasswordHasher.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;

        Repository.Update(user);
        await Repository.SaveChangesAsync();
    }

    public async Task AssignRolesAsync(Guid userId, List<string> roleNames)
    {
        // Remover roles existentes
        var existingUserRoles = await userRoleRepository.FindAsync(
            new UserRolesByUserSpecification(userId)
        );
        
        foreach (var userRole in existingUserRoles)
        {
            userRoleRepository.Remove(userRole);
            await userRoleRepository.SaveChangesAsync();
        }

        // Adicionar novas roles
        foreach (var roleName in roleNames)
        {
            var role = await roleRepository.FirstOrDefaultAsync(
                new RoleByNameSpecification(roleName)
            );

            if (role == null) continue;
            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = role.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await userRoleRepository.AddAsync(userRole);
        }
        
        await userRoleRepository.SaveChangesAsync();
    }
}