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

    // REQUISITO 2: Contar alunos por turma
    public async Task<int> CountStudentsAsync(Guid classId)
    {
        var spec = new EnrollmentByClassSpecification(classId);
        
        return await enrollmentRepository.CountAsync(spec as BaseSpecification<Enrollment>);
            
        return 0;
    }
}
