using AdmSchoolApp.Domain.Entities;

namespace AdmSchoolApp.Domain.Specifications;

public class UserByEmailSpecification : BaseSpecification<User>
{
    public UserByEmailSpecification(string email) 
        : base(u => u.Email == email)
    {
        AddInclude(u => u.UserRoles);
        AddInclude("UserRoles.Role");
    }
}

public class UserWithRolesSpecification : BaseSpecification<User>
{
    public UserWithRolesSpecification(Guid userId) 
        : base(u => u.Id == userId)
    {
        AddInclude(u => u.UserRoles);
        AddInclude("UserRoles.Role");
    }
}

public class ActiveUsersSpecification : BaseSpecification<User>
{
    public ActiveUsersSpecification() 
        : base(u => u.IsActive)
    {
        ApplyOrderBy(u => u.Name);
    }
}