using AdmSchoolApp.Domain.Interfaces;
using AdmSchoolApp.Domain.Models;
using AdmSchoolApp.Infrastructure.Contexts;
using AdmSchoolApp.Infrastructure.Specifications;
using Microsoft.EntityFrameworkCore;

namespace AdmSchoolApp.Infrastructure.Repositories;

public class BaseRepository<T>(AdmSchoolDbContext context) : IRepository<T>
    where T : class
{
    protected readonly AdmSchoolDbContext Context = context;
    protected readonly DbSet<T> DbSet = context.Set<T>();

    // Queries
    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await DbSet.FindAsync([id], ct);
    }

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await DbSet.FindAsync([id], ct);
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
    {
        return await DbSet.AsNoTracking().ToListAsync(ct);
    }

    public virtual async Task<IReadOnlyList<T>> FindAsync(ISpecification<T> spec, CancellationToken ct = default)
    {
        return await ApplySpecification(spec).ToListAsync(ct);
    }

    public virtual async Task<T?> FirstOrDefaultAsync(ISpecification<T> spec, CancellationToken ct = default)
    {
        return await ApplySpecification(spec).FirstOrDefaultAsync(ct);
    }

    public virtual async Task<int> CountAsync(ISpecification<T> spec, CancellationToken ct = default)
    {
        return await ApplySpecification(spec).CountAsync(ct);
    }

    public virtual async Task<bool> AnyAsync(ISpecification<T> spec, CancellationToken ct = default)
    {
        return await ApplySpecification(spec).AnyAsync(ct);
    }

    public virtual async Task<BasePagination<T>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        ISpecification<T>? spec = null,
        CancellationToken ct = default
    )
    {
        var query = spec != null ? ApplySpecification(spec) : DbSet.AsNoTracking();

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new BasePagination<T>(items, pageNumber, pageSize, totalCount);
    }

    // Commands
    public virtual async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await DbSet.AddAsync(entity, ct);
        return entity;
    }

    public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
    {
        await DbSet.AddRangeAsync(entities, ct);
    }

    public virtual void Update(T entity)
    {
        DbSet.Update(entity);
    }

    public virtual void UpdateRange(IEnumerable<T> entities)
    {
        DbSet.UpdateRange(entities);
    }

    public virtual void Remove(T entity)
    {
        DbSet.Remove(entity);
    }

    public virtual void RemoveRange(IEnumerable<T> entities)
    {
        DbSet.RemoveRange(entities);
    }

    public virtual async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await Context.SaveChangesAsync(ct);
    }

    // Helper: aplica specification
    private IQueryable<T> ApplySpecification(ISpecification<T> spec)
    {
        return SpecificationEvaluator<T>.GetQuery(DbSet.AsQueryable(), spec);
    }
}