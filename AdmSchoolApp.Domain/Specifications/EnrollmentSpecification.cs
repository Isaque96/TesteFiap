using AdmSchoolApp.Domain.Entities;

namespace AdmSchoolApp.Domain.Specifications;

public class EnrollmentByStudentSpecification : BaseSpecification<Enrollment>
{
    public EnrollmentByStudentSpecification(Guid studentId) 
        : base(e => e.StudentId == studentId)
    {
        AddInclude(e => e.Class);
        AddInclude(e => e.Student);
    }
}

public class EnrollmentByClassSpecification : BaseSpecification<Enrollment>
{
    public EnrollmentByClassSpecification(Guid classId) 
        : base(e => e.ClassId == classId)
    {
        AddInclude(e => e.Student);
        AddInclude(e => e.Class);
    }
}

public class EnrollmentByStudentAndClassSpecification(Guid studentId, Guid classId)
    : BaseSpecification<Enrollment>(e => e.StudentId == studentId && e.ClassId == classId);

public class EnrollmentWithDetailsSpecification : BaseSpecification<Enrollment>
{
    public EnrollmentWithDetailsSpecification(Guid enrollmentId) 
        : base(e => e.Id == enrollmentId)
    {
        AddInclude(e => e.Student);
        AddInclude(e => e.Class);
    }
}