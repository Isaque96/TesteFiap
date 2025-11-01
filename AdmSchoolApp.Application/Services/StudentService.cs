using System.Text;
using AdmSchoolApp.Application.Utils;
using AdmSchoolApp.Application.Validators;
using AdmSchoolApp.Domain.Entities;
using AdmSchoolApp.Domain.Interfaces;
using AdmSchoolApp.Domain.Specifications;
using FluentValidation;
using FluentValidation.Results;

namespace AdmSchoolApp.Application.Services;

public class StudentService(IRepository<Student> repository, IValidator<Student>? validator = null)
    : BaseService<Student>(repository, validator)
{
    public override async Task<(bool IsValid, ValidationResult ValidationResult, Student? Entity)> AddAsync(Student entity)
    {
        var validationResult = await ValidateAsync(entity);
        
        if (!validationResult.IsValid)
            return (false, validationResult, null);

        // Hash da senha se fornecida
        if (entity.PasswordHash is { Length: > 0 })
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
    
    public async Task<bool> CpfExistsAsync(string cpf, Guid? excludeId = null)
    {
        var spec = new StudentByCpfSpecification(cpf);
        var student = await FirstOrDefaultAsync(spec);
        
        if (student == null)
            return false;
            
        return excludeId == null || student.Id != excludeId;
    }

    public async Task<bool> EmailExistsAsync(string email, Guid? excludeId = null)
    {
        var spec = new StudentByEmailSpecification(email);
        var student = await FirstOrDefaultAsync(spec);
        
        if (student == null)
            return false;
            
        return excludeId == null || student.Id != excludeId;
    }

    public async Task<IEnumerable<Student>> SearchByNameAsync(string name)
    {
        var spec = new StudentByNameSpecification(name);
        return await FindAsync(spec);
    }

    public async Task<Student?> GetByCpfAsync(string cpf)
    {
        var spec = new StudentByCpfSpecification(cpf);
        return await FirstOrDefaultAsync(spec);
    }

    public async Task<Student?> GetByEmailAsync(string email)
    {
        var spec = new StudentByEmailSpecification(email);
        return await FirstOrDefaultAsync(spec);
    }
    
    public async Task<Student?> GetWithEnrollmentsAsync(Guid studentId)
    {
        var spec = new StudentsWithEnrollmentsSpecification();
        var students = await FindAsync(spec);
        return students.FirstOrDefault(s => s.Id == studentId);
    } 
    
    public async Task<(IEnumerable<Student> Items, int TotalCount)> GetPaginatedAsync(int pageNumber = 1, int pageSize = 10)
    {
        var spec = new StudentsPaginatedSpecification(pageNumber, pageSize);
        var items = await FindAsync(spec);

        var countSpec = new EmptyStudentSpecification();
        var totalCount = await CountAsync(countSpec);
        
        return (items, totalCount);
    }
    
    public async Task<Student?> AuthenticateAsync(string email, string password)
    {
        var student = await GetByEmailAsync(email);
        
        if (student?.PasswordHash == null)
            return null;

        return !PasswordHasher.VerifyPassword(password, student.PasswordHash) ? null : student;
    }
}