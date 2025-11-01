using AdmSchoolApp.Domain.Entities;

namespace AdmSchoolApp.Domain.Specifications;

public class EmptyStudentSpecification : BaseSpecification<Student>
{
    public EmptyStudentSpecification()
    {
        ApplyOrderBy(s => s.Name);
    }
}

public class StudentByCpfSpecification(string cpf) : BaseSpecification<Student>(s => s.Cpf == cpf);

public class StudentByEmailSpecification(string email) : BaseSpecification<Student>(s => s.Email == email);

public class StudentByNameSpecification : BaseSpecification<Student>
{
    public StudentByNameSpecification(string name) 
        : base(s => s.Name.Contains(name))
    {
        ApplyOrderBy(s => s.Name);
    }
}

public class StudentsPaginatedSpecification : BaseSpecification<Student>
{
    public StudentsPaginatedSpecification(int pageNumber = 1, int pageSize = 10)
    {
        ApplyOrderBy(s => s.Name);
        ApplyPaging((pageNumber - 1) * pageSize, pageSize);
    }
}

public class StudentsWithEnrollmentsSpecification : BaseSpecification<Student>
{
    public StudentsWithEnrollmentsSpecification()
    {
        AddInclude(s => s.Enrollments);
        AddInclude("Enrollments.Class");
    }
}