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

public class StudentServiceTests
{
    private readonly IRepository<Student> _repository;
    private readonly IValidator<Student> _validator;
    private readonly StudentService _sut;

    public StudentServiceTests()
    {
        _repository = Substitute.For<IRepository<Student>>();
        _validator = Substitute.For<IValidator<Student>>();
        _sut = new StudentService(_repository, _validator);
    }

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_Should_Return_Success_When_Valid_Student()
    {
        // Arrange
        var student = new Student
        {
            Id = Guid.NewGuid(),
            Name = "João Silva",
            Email = "joao@test.com",
            Cpf = "11144477735",
            BirthDate = new DateOnly(2000, 1, 1),
            PasswordHash = Encoding.UTF8.GetBytes("Test@123")
        };

        _validator.ValidateAsync(student, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        _repository.AddAsync(Arg.Any<Student>(), Arg.Any<CancellationToken>())
            .Returns(student);

        _repository.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _sut.AddAsync(student);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Entity.Should().NotBeNull();
        result.Entity!.Email.Should().Be("joao@test.com");
        
        await _repository.Received(1).AddAsync(Arg.Any<Student>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddAsync_Should_Hash_Password_When_Provided()
    {
        // Arrange
        var student = new Student
        {
            Id = Guid.NewGuid(),
            Name = "Maria",
            Email = "maria@test.com",
            Cpf = "93541134780",
            BirthDate = new DateOnly(1999, 5, 10),
            PasswordHash = Encoding.UTF8.GetBytes("ValidPass@123")
        };

        _validator.ValidateAsync(student, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        Student? capturedStudent = null;
        _repository.AddAsync(Arg.Do<Student>(s => capturedStudent = s), Arg.Any<CancellationToken>())
            .Returns(student);

        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        // Act
        var result = await _sut.AddAsync(student);

        // Assert
        result.IsValid.Should().BeTrue();
        capturedStudent.Should().NotBeNull();
        
        // Verifica que a senha foi hasheada (não é mais o texto plano)
        var hashedPassword = Encoding.UTF8.GetString(capturedStudent.PasswordHash!);
        hashedPassword.Should().NotBe("ValidPass@123");
        hashedPassword.Should().StartWith("$2"); // BCrypt hash prefix
    }

    [Fact]
    public async Task AddAsync_Should_Fail_When_Validation_Fails()
    {
        // Arrange
        var student = new Student
        {
            Name = "",
            Email = "invalid-email",
            Cpf = "123",
            BirthDate = default
        };

        var validationFailures = new List<ValidationFailure>
        {
            new("Name", "Nome é obrigatório"),
            new("Email", "Email inválido"),
            new("Cpf", "CPF inválido")
        };

        _validator.ValidateAsync(student, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(validationFailures));

        // Act
        var result = await _sut.AddAsync(student);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ValidationResult.Errors.Should().HaveCount(3);
        result.Entity.Should().BeNull();

        await _repository.DidNotReceive().AddAsync(Arg.Any<Student>(), Arg.Any<CancellationToken>());
        await _repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region CpfExistsAsync Tests

    [Fact]
    public async Task CpfExistsAsync_Should_Return_True_When_Cpf_Exists()
    {
        // Arrange
        var existingStudent = new Student
        {
            Id = Guid.NewGuid(),
            Cpf = "11144477735",
            Email = "test@test.com",
            Name = "Test"
        };

        _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<Student>>(), Arg.Any<CancellationToken>())
            .Returns(existingStudent);

        // Act
        var exists = await _sut.CpfExistsAsync("11144477735");

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task CpfExistsAsync_Should_Return_False_When_Cpf_Not_Found()
    {
        // Arrange
        _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<Student>>(), Arg.Any<CancellationToken>())
            .Returns((Student?)null);

        // Act
        var exists = await _sut.CpfExistsAsync("11144477735");

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task CpfExistsAsync_Should_Respect_ExcludeId()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var existingStudent = new Student
        {
            Id = studentId,
            Cpf = "11144477735",
            Email = "test@test.com",
            Name = "Test"
        };

        _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<Student>>(), Arg.Any<CancellationToken>())
            .Returns(existingStudent);

        // Act - mesmo ID, deve retornar false (é o próprio registro)
        var exists = await _sut.CpfExistsAsync("11144477735", excludeId: studentId);

        // Assert
        exists.Should().BeFalse();
    }

    #endregion

    #region EmailExistsAsync Tests

    [Fact]
    public async Task EmailExistsAsync_Should_Return_True_When_Email_Exists()
    {
        // Arrange
        var existingStudent = new Student
        {
            Id = Guid.NewGuid(),
            Email = "existing@test.com",
            Cpf = "11144477735",
            Name = "Test"
        };

        _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<Student>>(), Arg.Any<CancellationToken>())
            .Returns(existingStudent);

        // Act
        var exists = await _sut.EmailExistsAsync("existing@test.com");

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task EmailExistsAsync_Should_Return_False_When_Email_Not_Found()
    {
        // Arrange
        _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<Student>>(), Arg.Any<CancellationToken>())
            .Returns((Student?)null);

        // Act
        var exists = await _sut.EmailExistsAsync("notfound@test.com");

        // Assert
        exists.Should().BeFalse();
    }

    #endregion

    #region SearchByNameAsync Tests

    [Fact]
    public async Task SearchByNameAsync_Should_Return_Matching_Students()
    {
        // Arrange
        var students = new List<Student>
        {
            new() { Id = Guid.NewGuid(), Name = "Maria Silva", Email = "maria@test.com", Cpf = "11144477735" },
            new() { Id = Guid.NewGuid(), Name = "João Santos", Email = "joao@test.com", Cpf = "93541134780" }
        };

        _repository.FindAsync(Arg.Any<ISpecification<Student>>(), Arg.Any<CancellationToken>())
            .Returns(students);

        // Act
        var result = await _sut.SearchByNameAsync("Maria");

        // Assert
        result.Should().HaveCount(2);
        await _repository.Received(1).FindAsync(Arg.Any<ISpecification<Student>>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region GetByCpfAsync Tests

    [Fact]
    public async Task GetByCpfAsync_Should_Return_Student_When_Found()
    {
        // Arrange
        var student = new Student
        {
            Id = Guid.NewGuid(),
            Cpf = "11144477735",
            Email = "test@test.com",
            Name = "Test Student"
        };

        _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<Student>>(), Arg.Any<CancellationToken>())
            .Returns(student);

        // Act
        var result = await _sut.GetByCpfAsync("11144477735");

        // Assert
        result.Should().NotBeNull();
        result.Cpf.Should().Be("11144477735");
    }

    [Fact]
    public async Task GetByCpfAsync_Should_Return_Null_When_Not_Found()
    {
        // Arrange
        _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<Student>>(), Arg.Any<CancellationToken>())
            .Returns((Student?)null);

        // Act
        var result = await _sut.GetByCpfAsync("11144477735");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByEmailAsync Tests

    [Fact]
    public async Task GetByEmailAsync_Should_Return_Student_When_Found()
    {
        // Arrange
        var student = new Student
        {
            Id = Guid.NewGuid(),
            Email = "found@test.com",
            Cpf = "11144477735",
            Name = "Found Student"
        };

        _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<Student>>(), Arg.Any<CancellationToken>())
            .Returns(student);

        // Act
        var result = await _sut.GetByEmailAsync("found@test.com");

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("found@test.com");
    }

    #endregion

    #region GetWithEnrollmentsAsync Tests

    [Fact]
    public async Task GetWithEnrollmentsAsync_Should_Return_Student_With_Enrollments()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var student = new Student
        {
            Id = studentId,
            Name = "Student With Enrollments",
            Email = "student@test.com",
            Cpf = "11144477735"
        };
        
        student.Enrollments.Add(new Enrollment
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            ClassId = Guid.NewGuid()
        });

        _repository.FindAsync(Arg.Any<ISpecification<Student>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Student> { student });

        // Act
        var result = await _sut.GetWithEnrollmentsAsync(studentId);

        // Assert
        result.Should().NotBeNull();
        result.Enrollments.Should().HaveCount(1);
    }

    #endregion

    #region AuthenticateAsync Tests

    [Fact]
    public async Task AuthenticateAsync_Should_Return_Student_When_Credentials_Valid()
    {
        // Arrange
        var password = "Test@123";
        var hashedPassword = PasswordHasher.HashPassword(password);
        
        var student = new Student
        {
            Id = Guid.NewGuid(),
            Email = "auth@test.com",
            Cpf = "11144477735",
            Name = "Auth Test",
            PasswordHash = hashedPassword
        };

        _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<Student>>(), Arg.Any<CancellationToken>())
            .Returns(student);

        // Act
        var result = await _sut.AuthenticateAsync("auth@test.com", password);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("auth@test.com");
    }

    [Fact]
    public async Task AuthenticateAsync_Should_Return_Null_When_Password_Invalid()
    {
        // Arrange
        var correctPassword = "Test@123";
        var hashedPassword = PasswordHasher.HashPassword(correctPassword);
        
        var student = new Student
        {
            Id = Guid.NewGuid(),
            Email = "auth@test.com",
            Cpf = "11144477735",
            Name = "Auth Test",
            PasswordHash = hashedPassword
        };

        _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<Student>>(), Arg.Any<CancellationToken>())
            .Returns(student);

        // Act
        var result = await _sut.AuthenticateAsync("auth@test.com", "WrongPassword");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AuthenticateAsync_Should_Return_Null_When_Student_Not_Found()
    {
        // Arrange
        _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<Student>>(), Arg.Any<CancellationToken>())
            .Returns((Student?)null);

        // Act
        var result = await _sut.AuthenticateAsync("notfound@test.com", "AnyPassword");

        // Assert
        result.Should().BeNull();
    }

    #endregion
}