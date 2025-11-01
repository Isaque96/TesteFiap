using System.Text;
using AdmSchoolApp.Application.Services;
using AdmSchoolApp.Application.Utils;
using AdmSchoolApp.Domain.Entities;
using AdmSchoolApp.Domain.Interfaces;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;

namespace AdmSchoolApp.AdmSchoolApp.Application.Tests.Services;

public class UserServiceTests
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Role> _roleRepository;
    private readonly IRepository<UserRole> _userRoleRepository;
    private readonly IValidator<User> _validator;
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _userRepository = Substitute.For<IRepository<User>>();
        _roleRepository = Substitute.For<IRepository<Role>>();
        _userRoleRepository = Substitute.For<IRepository<UserRole>>();
        _validator = Substitute.For<IValidator<User>>();
        _sut = new UserService(_userRepository, _roleRepository, _userRoleRepository, _validator);
    }

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_Should_Hash_Password_And_Create_User()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Admin User",
            Email = "admin@test.com",
            PasswordHash = Encoding.UTF8.GetBytes("Admin@123")
        };

        _validator.ValidateAsync(user, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        User? capturedUser = null;
        _userRepository.AddAsync(Arg.Do<User>(u => capturedUser = u), Arg.Any<CancellationToken>())
            .Returns(user);

        _userRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        // Act
        var result = await _sut.AddAsync(user);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Entity.Should().NotBeNull();
        
        capturedUser.Should().NotBeNull();
        var hashedPassword = Encoding.UTF8.GetString(capturedUser!.PasswordHash);
        hashedPassword.Should().NotBe("Admin@123");
        hashedPassword.Should().StartWith("$2"); // BCrypt prefix
    }

    [Fact]
    public async Task AddAsync_Should_Fail_When_Validation_Fails()
    {
        // Arrange
        var user = new User
        {
            Name = "",
            Email = "invalid",
            PasswordHash = Encoding.UTF8.GetBytes("weak")
        };

        var validationFailures = new List<ValidationFailure>
        {
            new("Name", "Nome é obrigatório"),
            new("Email", "Email inválido")
        };

        _validator.ValidateAsync(user, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(validationFailures));

        // Act
        var result = await _sut.AddAsync(user);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ValidationResult.Errors.Should().HaveCount(2);
        result.Entity.Should().BeNull();

        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region EmailExistsAsync Tests

    [Fact]
    public async Task EmailExistsAsync_Should_Return_True_When_Email_Exists()
    {
        // Arrange
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "existing@test.com",
            Name = "Existing User"
        };

        _userRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<User>>(), Arg.Any<CancellationToken>())
            .Returns(existingUser);

        // Act
        var exists = await _sut.EmailExistsAsync("existing@test.com");

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task EmailExistsAsync_Should_Return_False_When_Email_Not_Found()
    {
        // Arrange
        _userRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<User>>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var exists = await _sut.EmailExistsAsync("notfound@test.com");

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task EmailExistsAsync_Should_Respect_ExcludeId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@test.com",
            Name = "Test"
        };

        _userRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<User>>(), Arg.Any<CancellationToken>())
            .Returns(user);

        // Act - mesmo ID, deve retornar false
        var exists = await _sut.EmailExistsAsync("test@test.com", excludeId: userId);

        // Assert
        exists.Should().BeFalse();
    }

    #endregion

    #region GetByEmailAsync Tests

    [Fact]
    public async Task GetByEmailAsync_Should_Return_User_When_Found()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "found@test.com",
            Name = "Found User"
        };

        _userRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<User>>(), Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        var result = await _sut.GetByEmailAsync("found@test.com");

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("found@test.com");
    }

    [Fact]
    public async Task GetByEmailAsync_Should_Return_Null_When_Not_Found()
    {
        // Arrange
        _userRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<User>>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var result = await _sut.GetByEmailAsync("notfound@test.com");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetWithRolesAsync Tests

    [Fact]
    public async Task GetWithRolesAsync_Should_Return_User_With_Roles()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "admin@test.com",
            Name = "Admin User"
        };

        user.UserRoles.Add(new UserRole
        {
            UserId = userId,
            RoleId = 1,
            Role = new Role { Id = 1, Name = "Admin" }
        });

        _userRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<User>>(), Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        var result = await _sut.GetWithRolesAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.UserRoles.Should().HaveCount(1);
    }

    #endregion

    #region AuthenticateAsync Tests

    [Fact]
    public async Task AuthenticateAsync_Should_Return_User_When_Credentials_Valid()
    {
        // Arrange
        const string password = "Admin@123";
        var hashedPassword = PasswordHasher.HashPassword(password);
    
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@test.com",
            Name = "Admin",
            PasswordHash = hashedPassword,
            IsActive = true
        };

        _userRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<User>>(), Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        var result = await _sut.AuthenticateAsync("admin@test.com", password);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("admin@test.com");
        result.Id.Should().Be(user.Id);
        await _userRepository.Received(1).FirstOrDefaultAsync(Arg.Any<ISpecification<User>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AuthenticateAsync_Should_Return_Null_When_Password_Invalid()
    {
        // Arrange
        var correctPassword = "Admin@123";
        var hashedPassword = PasswordHasher.HashPassword(correctPassword);
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@test.com",
            Name = "Admin",
            PasswordHash = hashedPassword
        };

        _userRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<User>>(), Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        var result = await _sut.AuthenticateAsync("admin@test.com", "WrongPassword");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AuthenticateAsync_Should_Return_Null_When_User_Not_Found()
    {
        // Arrange
        _userRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<User>>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var result = await _sut.AuthenticateAsync("notfound@test.com", "AnyPassword");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region ValidatePasswordAsync Tests

    [Fact]
    public async Task ValidatePasswordAsync_Should_Return_True_When_Password_Matches()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var password = "Test@123";
        var hashedPassword = PasswordHasher.HashPassword(password);
        
        var user = new User
        {
            Id = userId,
            Email = "test@test.com",
            Name = "Test",
            PasswordHash = hashedPassword
        };

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        var result = await _sut.ValidatePasswordAsync(userId, password);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidatePasswordAsync_Should_Return_False_When_Password_Does_Not_Match()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var correctPassword = "Test@123";
        var hashedPassword = PasswordHasher.HashPassword(correctPassword);
        
        var user = new User
        {
            Id = userId,
            Email = "test@test.com",
            Name = "Test",
            PasswordHash = hashedPassword
        };

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        var result = await _sut.ValidatePasswordAsync(userId, "WrongPassword");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ChangePasswordAsync Tests

    [Fact]
    public async Task ChangePasswordAsync_Should_Update_Password_Hash()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var oldPassword = "OldPass@123";
        var newPassword = "NewPass@123";
        
        var user = new User
        {
            Id = userId,
            Email = "test@test.com",
            Name = "Test",
            PasswordHash = PasswordHasher.HashPassword(oldPassword)
        };

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        _userRepository.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        await _sut.ChangePasswordAsync(userId, newPassword);

        // Assert
        _userRepository.Received(1).Update(Arg.Is<User>(u => u.Id == userId));
        await _userRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        
        // Verifica que a nova senha funciona
        var passwordMatches = PasswordHasher.VerifyPassword(newPassword, user.PasswordHash);
        passwordMatches.Should().BeTrue();
    }

    #endregion

    #region AssignRolesAsync Tests

    [Fact]
    public async Task AssignRolesAsync_Should_Assign_Roles_To_User()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var adminRole = new Role { Id = 2, Name = "Admin" };
        var teacherRole = new Role { Id = 1, Name = "Teacher" };

        _userRoleRepository.FindAsync(Arg.Any<ISpecification<UserRole>>(), Arg.Any<CancellationToken>())
            .Returns(new List<UserRole>());

        // Configurar retornos sequenciais - primeira chamada retorna adminRole, segunda retorna teacherRole
        _roleRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<Role>>(), Arg.Any<CancellationToken>())
            .Returns(adminRole, teacherRole);

        _userRoleRepository.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        await _sut.AssignRolesAsync(userId, ["Admin", "Teacher"]);

        // Assert
        await _roleRepository.Received(2).FirstOrDefaultAsync(Arg.Any<ISpecification<Role>>(), Arg.Any<CancellationToken>());
        await _userRoleRepository.Received(2).AddAsync(Arg.Is<UserRole>(ur => 
            ur.UserId == userId && (ur.RoleId == adminRole.Id || ur.RoleId == teacherRole.Id)));
        await _userRoleRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion
}