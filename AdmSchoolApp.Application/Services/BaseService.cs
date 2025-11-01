using AdmSchoolApp.Domain.Interfaces;
using AdmSchoolApp.Domain.Specifications;
using FluentValidation;
using FluentValidation.Results;

namespace AdmSchoolApp.Application.Services;

public abstract class BaseService<TEntity>(IRepository<TEntity> repository, IValidator<TEntity>? validator = null)
    where TEntity : class
{
    protected readonly IRepository<TEntity> Repository = repository;
    protected readonly IValidator<TEntity>? Validator = validator;

    protected virtual async Task<ValidationResult> ValidateAsync(TEntity entity)
    {
        if (Validator == null)
            return new ValidationResult();

        return await Validator.ValidateAsync(entity);
    }

    public virtual async Task<(bool IsValid, ValidationResult ValidationResult, TEntity? Entity)> AddAsync(TEntity entity)
    {
        var validationResult = await ValidateAsync(entity);
        
        if (!validationResult.IsValid)
            return (false, validationResult, null);

        var addedEntity = await Repository.AddAsync(entity);
        await Repository.SaveChangesAsync();
        return (true, validationResult, addedEntity);
    }

    public virtual async Task<(bool IsValid, ValidationResult ValidationResult)> UpdateAsync(TEntity entity)
    {
        var validationResult = await ValidateAsync(entity);
        
        if (!validationResult.IsValid)
            return (false, validationResult);

        Repository.Update(entity);
        await Repository.SaveChangesAsync();
        return (true, validationResult);
    }

    public virtual async Task<TEntity?> GetByIdAsync(Guid id)
    {
        return await Repository.GetByIdAsync(id);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await Repository.GetAllAsync();
    }

    public virtual async Task<IEnumerable<TEntity>> FindAsync(BaseSpecification<TEntity> specification)
    {
        return await Repository.FindAsync(specification);
    }

    public virtual async Task<TEntity?> FirstOrDefaultAsync(BaseSpecification<TEntity> specification)
    {
        return await Repository.FirstOrDefaultAsync(specification);
    }

    public virtual async Task<int> CountAsync(BaseSpecification<TEntity> specification)
    {
        return await Repository.CountAsync(specification);
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        var entity = await Repository.GetByIdAsync(id);
        if (entity != null)
        {
            Repository.Remove(entity);
            await Repository.SaveChangesAsync();
        }
    }

    public virtual async Task<bool> ExistsAsync(BaseSpecification<TEntity> specification)
    {
        return await Repository.CountAsync(specification) > 0;
    }
}