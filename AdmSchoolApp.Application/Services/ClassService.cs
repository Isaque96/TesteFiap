using AdmSchoolApp.Domain.Entities;
using AdmSchoolApp.Domain.Interfaces;
using AdmSchoolApp.Domain.Specifications;
using FluentValidation;

namespace AdmSchoolApp.Application.Services;

public class ClassService(IRepository<Class> classRepository, IRepository<Enrollment> enrollmentRepository, IValidator<Class>? validator = null)
    : BaseService<Class>(classRepository, validator)
{
    public async Task<Class?> GetByNameAsync(string name)
    {
        var spec = new ClassByNameSpecification(name);
        return await FirstOrDefaultAsync(spec);
    }

    public async Task<int> CountStudentsAsync(Guid classId)
    {
        var spec = new EnrollmentByClassSpecification(classId);
        
        return await enrollmentRepository.CountAsync(spec);
    }
    
    public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null)
    {
        var spec = new ClassByNameSpecification(name);
        var classEntity = await FirstOrDefaultAsync(spec);
        
        if (classEntity == null)
            return false;
            
        return excludeId == null || classEntity.Id != excludeId;
    }
    
    public async Task<Class?> GetWithStudentsAsync(Guid classId)
    {
        var spec = new ClassWithEnrollmentsSpecification(classId);
        return await FirstOrDefaultAsync(spec);
    }

    public async Task<(IEnumerable<Class> Items, int TotalCount)> GetPaginatedAsync(int pageNumber = 1, int pageSize = 10)
    {
        var spec = new ClassesPaginatedSpecification(pageNumber, pageSize);
        var items = await FindAsync(spec);

        var countSpec = new EmptyClassSpecification();
        var totalCount = await CountAsync(countSpec);
        
        return (items, totalCount);
    }
}
