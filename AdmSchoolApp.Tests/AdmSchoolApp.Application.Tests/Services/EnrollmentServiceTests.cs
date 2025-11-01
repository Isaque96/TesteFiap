using AdmSchoolApp.Application.Services;
using AdmSchoolApp.Domain.Entities;
using AdmSchoolApp.Domain.Interfaces;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;

namespace AdmSchoolApp.AdmSchoolApp.Application.Tests.Services;

public class EnrollmentServiceTests
{
    private readonly IRepository<Enrollment> _repository;
    private readonly IValidator<Enrollment> _validator;
    private readonly EnrollmentService _sut;

    public EnrollmentServiceTests()
    {
        _repository = Substitute.For<IRepository<Enrollment>>();
        _validator = Substitute.For<IValidator<Enrollment>>();
        _sut = new EnrollmentService(_repository, _validator);
    }

    #region IsStudentEnrolledInClassAsync Tests

    [Fact]
    public async Task IsStudentEnrolledInClassAsync_Should_Return_True_When_Enrolled()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var classId = Guid.NewGuid();

        _repository.CountAsync(Arg.Any<ISpecification<Enrollment>>(), Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _sut.IsStudentEnrolledInClassAsync(studentId, classId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsStudentEnrolledInClassAsync_Should_Return_False_When_Not_Enrolled()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var classId = Guid.NewGuid();

        _repository.CountAsync(Arg.Any<ISpecification<Enrollment>>(), Arg.Any<CancellationToken>())
            .Returns(0);

        // Act
        var result = await _sut.IsStudentEnrolledInClassAsync(studentId, classId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetByStudentAsync Tests

    [Fact]
    public async Task GetByStudentAsync_Should_Return_All_Student_Enrollments()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var enrollments = new List<Enrollment>
        {
            new()
            {
                Id = Guid.NewGuid(),
                StudentId = studentId,
                ClassId = Guid.NewGuid(),
                CreatedAt = DateTime.Today
            },
            new()
            {
                Id = Guid.NewGuid(),
                StudentId = studentId,
                ClassId = Guid.NewGuid(),
                CreatedAt = DateTime.Today
            }
        };

        _repository.FindAsync(Arg.Any<ISpecification<Enrollment>>(), Arg.Any<CancellationToken>())
            .Returns(enrollments);

        // Act
        var result = (await _sut.GetByStudentAsync(studentId)).ToArray();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(e => e.StudentId == studentId);
    }

    [Fact]
    public async Task GetByStudentAsync_Should_Return_Empty_When_No_Enrollments()
    {
        // Arrange
        var studentId = Guid.NewGuid();

        _repository.FindAsync(Arg.Any<ISpecification<Enrollment>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Enrollment>());

        // Act
        var result = await _sut.GetByStudentAsync(studentId);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetByClassAsync Tests

    [Fact]
    public async Task GetByClassAsync_Should_Return_All_Class_Enrollments()
    {
        // Arrange
        var classId = Guid.NewGuid();
        var enrollments = new List<Enrollment>
        {
            new()
            {
                Id = Guid.NewGuid(),
                StudentId = Guid.NewGuid(),
                ClassId = classId,
                CreatedAt = DateTime.Today
            },
            new()
            {
                Id = Guid.NewGuid(),
                StudentId = Guid.NewGuid(),
                ClassId = classId,
                CreatedAt = DateTime.Today
            },
            new()
            {
                Id = Guid.NewGuid(),
                StudentId = Guid.NewGuid(),
                ClassId = classId,
                CreatedAt = DateTime.Today
            }
        };

        _repository.FindAsync(Arg.Any<ISpecification<Enrollment>>(), Arg.Any<CancellationToken>())
            .Returns(enrollments);

        // Act
        var result = (await _sut.GetByClassAsync(classId)).ToArray();

        // Assert
        result.Should().HaveCount(3);
        result.Should().OnlyContain(e => e.ClassId == classId);
    }

    #endregion

    #region GetWithDetailsAsync Tests

    [Fact]
    public async Task GetWithDetailsAsync_Should_Return_Enrollment_With_Details()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var classId = Guid.NewGuid();

        var enrollment = new Enrollment
        {
            Id = enrollmentId,
            StudentId = studentId,
            ClassId = classId,
            CreatedAt = DateTime.Today,
            Student = new Student
            {
                Id = studentId,
                Name = "João Silva",
                Email = "joao@test.com",
                Cpf = "11144477735"
            },
            Class = new Class
            {
                Id = classId,
                Name = "Matemática",
                Description = "Desc",
                CreatedAt = DateTime.Today,
                UpdatedAt = DateTime.Today.AddMonths(3)
            }
        };

        _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<Enrollment>>(), Arg.Any<CancellationToken>())
            .Returns(enrollment);

        // Act
        var result = await _sut.GetWithDetailsAsync(enrollmentId);

        // Assert
        result.Should().NotBeNull();
        result.Student.Should().NotBeNull();
        result.Class.Should().NotBeNull();
        result.Student.Name.Should().Be("João Silva");
        result.Class.Name.Should().Be("Matemática");
    }

    [Fact]
    public async Task GetWithDetailsAsync_Should_Return_Null_When_Not_Found()
    {
        // Arrange
        _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<Enrollment>>(), Arg.Any<CancellationToken>())
            .Returns((Enrollment?)null);

        // Act
        var result = await _sut.GetWithDetailsAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_Should_Create_Enrollment_When_Valid()
    {
        // Arrange
        var enrollment = new Enrollment
        {
            Id = Guid.NewGuid(),
            StudentId = Guid.NewGuid(),
            ClassId = Guid.NewGuid(),
            CreatedAt = DateTime.Today
        };

        _validator.ValidateAsync(enrollment, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        _repository.AddAsync(Arg.Any<Enrollment>(), Arg.Any<CancellationToken>())
            .Returns(enrollment);

        _repository.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _sut.AddAsync(enrollment);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Entity.Should().NotBeNull();

        await _repository.Received(1).AddAsync(Arg.Any<Enrollment>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddAsync_Should_Fail_When_Validation_Fails()
    {
        // Arrange
        var enrollment = new Enrollment
        {
            StudentId = Guid.Empty,
            ClassId = Guid.Empty,
            CreatedAt = default
        };

        var validationFailures = new List<ValidationFailure>
        {
            new("StudentId", "StudentId é obrigatório"),
            new("ClassId", "ClassId é obrigatório")
        };

        _validator.ValidateAsync(enrollment, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(validationFailures));

        // Act
        var result = await _sut.AddAsync(enrollment);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ValidationResult.Errors.Should().HaveCount(2);
        result.Entity.Should().BeNull();

        await _repository.DidNotReceive().AddAsync(Arg.Any<Enrollment>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_Should_Remove_Enrollment_When_Exists()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var enrollment = new Enrollment
        {
            Id = enrollmentId,
            StudentId = Guid.NewGuid(),
            ClassId = Guid.NewGuid(),
            CreatedAt = DateTime.Today
        };

        _repository.GetByIdAsync(enrollmentId, Arg.Any<CancellationToken>())
            .Returns(enrollment);

        _repository.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        await _sut.DeleteAsync(enrollmentId);

        // Assert
        _repository.Received(1).Remove(Arg.Is<Enrollment>(e => e.Id == enrollmentId));
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_Should_Not_Throw_When_Enrollment_Not_Found()
    {
        // Arrange
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Enrollment?)null);

        // Act
        var act = async () => await _sut.DeleteAsync(Guid.NewGuid());

        // Assert
        await act.Should().NotThrowAsync();
        _repository.DidNotReceive().Remove(Arg.Any<Enrollment>());
    }

    #endregion
}