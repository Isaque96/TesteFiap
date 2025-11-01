using AdmSchoolApp.Domain.Entities;
using AdmSchoolApp.Domain.Interfaces;
using AdmSchoolApp.Domain.Specifications;
using FluentValidation;

namespace AdmSchoolApp.Application.Services;

public class EnrollmentService(IRepository<Enrollment> repository, IValidator<Enrollment>? validator = null)
    : BaseService<Enrollment>(repository, validator)
{
    public async Task<bool> IsStudentEnrolledInClassAsync(Guid studentId, Guid classId)
    {
        var spec = new EnrollmentByStudentAndClassSpecification(studentId, classId);
        return await ExistsAsync(spec);
    }

    public async Task<IEnumerable<Enrollment>> GetByStudentAsync(Guid studentId)
    {
        var spec = new EnrollmentByStudentSpecification(studentId);
        return await FindAsync(spec);
    }

    public async Task<IEnumerable<Enrollment>> GetByClassAsync(Guid classId)
    {
        var spec = new EnrollmentByClassSpecification(classId);
        return await FindAsync(spec);
    }
    
    public async Task<Enrollment?> GetWithDetailsAsync(Guid enrollmentId)
    {
        var spec = new EnrollmentWithDetailsSpecification(enrollmentId);
        return await FirstOrDefaultAsync(spec);
    }
}