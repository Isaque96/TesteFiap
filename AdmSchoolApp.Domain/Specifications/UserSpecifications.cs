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

public class UsersPaginatedSpecification : BaseSpecification<User>
{
    public UsersPaginatedSpecification(int pageNumber, int pageSize)
    {
        ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        ApplyOrderBy(u => u.Name);
        AddInclude(u => u.UserRoles);
        AddInclude("UserRoles.Role");
    }
}

public class UserRolesByUserSpecification(Guid userId) : BaseSpecification<UserRole>(ur => ur.UserId == userId);

public class RoleByNameSpecification(string name) : BaseSpecification<Role>(r => r.Name == name);

public class EmptyUserSpecification : BaseSpecification<User>
{
    public EmptyUserSpecification()
    {
        ApplyOrderBy(u => u.Name);
    }
}