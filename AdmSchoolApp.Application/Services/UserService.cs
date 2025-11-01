using AdmSchoolApp.Application.Utils;
using AdmSchoolApp.Domain.Entities;
using AdmSchoolApp.Domain.Interfaces;
using AdmSchoolApp.Domain.Specifications;
using FluentValidation;
using FluentValidation.Results;

namespace AdmSchoolApp.Application.Services;

public class UserService(IRepository<User> repository, IValidator<User>? validator = null)
    : BaseService<User>(repository, validator)
{
    public override async Task<(bool IsValid, ValidationResult ValidationResult, User? Entity)> AddAsync(User entity)
    {
        var validationResult = await ValidateAsync(entity);
        
        if (!validationResult.IsValid)
            return (false, validationResult, null);

        // Hash da senha
        var password = System.Text.Encoding.UTF8.GetString(entity.PasswordHash);
        entity.PasswordHash = PasswordHasher.HashPassword(password);

        var addedEntity = await Repository.AddAsync(entity);
        return (true, validationResult, addedEntity);
    }
    
    public async Task<User?> GetByEmailAsync(string email)
    {
        var spec = new UserByEmailSpecification(email);
        return await FirstOrDefaultAsync(spec);
    }

    public async Task<bool> EmailExistsAsync(string email, Guid? excludeId = null)
    {
        var spec = new UserByEmailSpecification(email);
        var user = await FirstOrDefaultAsync(spec);
        
        if (user == null)
            return false;
            
        return excludeId == null || user.Id != excludeId;
    }
    
    public async Task<User?> AuthenticateAsync(string email, string password)
    {
        var user = await GetByEmailAsync(email);
        
        if (user is not { IsActive: true })
            return null;

        return !PasswordHasher.VerifyPassword(password, user.PasswordHash) ? null : user;
    }
}