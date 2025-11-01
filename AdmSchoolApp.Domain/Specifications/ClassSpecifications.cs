using AdmSchoolApp.Domain.Entities;

namespace AdmSchoolApp.Domain.Specifications;

public class EmptyClassSpecification : BaseSpecification<Class>
{
    public EmptyClassSpecification()
    {
        ApplyOrderBy(c => c.Name);
    }
}

public class ClassByNameSpecification(string name) : BaseSpecification<Class>(c => c.Name == name);

public class ClassesPaginatedSpecification : BaseSpecification<Class>
{
    public ClassesPaginatedSpecification(int pageNumber = 1, int pageSize = 10)
    {
        ApplyOrderBy(c => c.Name);
        ApplyPaging((pageNumber - 1) * pageSize, pageSize);
    }
}

public class ClassWithEnrollmentsSpecification : BaseSpecification<Class>
{
    public ClassWithEnrollmentsSpecification(Guid classId) 
        : base(c => c.Id == classId)
    {
        AddInclude(c => c.Enrollments);
        AddInclude("Enrollments.Student");
    }
}

public class ClassesWithEnrollmentCountSpecification : BaseSpecification<Class>
{
    public ClassesWithEnrollmentCountSpecification()
    {
        AddInclude(c => c.Enrollments);
        ApplyOrderBy(c => c.Name);
    }
}