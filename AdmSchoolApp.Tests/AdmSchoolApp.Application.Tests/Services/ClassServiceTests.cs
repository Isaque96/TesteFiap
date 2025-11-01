using AdmSchoolApp.Application.Services;
using AdmSchoolApp.Domain.Entities;
using AdmSchoolApp.Domain.Interfaces;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;

namespace AdmSchoolApp.AdmSchoolApp.Application.Tests.Services;

public class ClassServiceTests
{
    private readonly IRepository<Class> _classRepository;
    private readonly IRepository<Enrollment> _enrollmentRepository;
    private readonly IValidator<Class> _validator;
    private readonly ClassService _sut;

    public ClassServiceTests()
    {
        _classRepository = Substitute.For<IRepository<Class>>();
        _enrollmentRepository = Substitute.For<IRepository<Enrollment>>();
        _validator = Substitute.For<IValidator<Class>>();
        _sut = new ClassService(_classRepository, _enrollmentRepository, _validator);
    }

    #region GetByNameAsync Tests

    [Fact]
    public async Task GetByNameAsync_Should_Return_Class_When_Found()
    {
        // Arrange
        var classEntity = new Class
        {
            Id = Guid.NewGuid(),
            Name = "Matemática Avançada",
            Description = "Curso de matemática",
            CreatedAt = DateTime.Today,
            UpdatedAt = DateTime.Today.AddMonths(3),
        };

        _classRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<Class>>(), Arg.Any<CancellationToken>())
            .Returns(classEntity);

        // Act
        var result = await _sut.GetByNameAsync("Matemática Avançada");

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Matemática Avançada");
    }

    [Fact]
    public async Task GetByNameAsync_Should_Return_Null_When_Not_Found()
    {
        // Arrange
        _classRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<Class>>(), Arg.Any<CancellationToken>())
            .Returns((Class?)null);

        // Act
        var result = await _sut.GetByNameAsync("Turma Inexistente");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CountStudentsAsync Tests

    [Fact]
    public async Task CountStudentsAsync_Should_Return_Enrollment_Count()
    {
        // Arrange
        var classId = Guid.NewGuid();

        _enrollmentRepository.CountAsync(Arg.Any<ISpecification<Enrollment>>(), Arg.Any<CancellationToken>())
            .Returns(15);

        // Act
        var count = await _sut.CountStudentsAsync(classId);

        // Assert
        count.Should().Be(15);
        await _enrollmentRepository.Received(1).CountAsync(
            Arg.Any<ISpecification<Enrollment>>(),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task CountStudentsAsync_Should_Return_Zero_When_No_Enrollments()
    {
        // Arrange
        var classId = Guid.NewGuid();

        _enrollmentRepository.CountAsync(Arg.Any<ISpecification<Enrollment>>(), Arg.Any<CancellationToken>())
            .Returns(0);

        // Act
        var count = await _sut.CountStudentsAsync(classId);

        // Assert
        count.Should().Be(0);
    }

    #endregion

    #region NameExistsAsync Tests

    [Fact]
    public async Task NameExistsAsync_Should_Return_True_When_Name_Exists()
    {
        // Arrange
        var existingClass = new Class
        {
            Id = Guid.NewGuid(),
            Name = "Física Quântica",
            CreatedAt = DateTime.Today,
            UpdatedAt = DateTime.Today.AddMonths(3),
        };

        _classRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<Class>>(), Arg.Any<CancellationToken>())
            .Returns(existingClass);

        // Act
        var exists = await _sut.NameExistsAsync("Física Quântica");

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task NameExistsAsync_Should_Return_False_When_Name_Not_Found()
    {
        // Arrange
        _classRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<Class>>(), Arg.Any<CancellationToken>())
            .Returns((Class?)null);

        // Act
        var exists = await _sut.NameExistsAsync("Turma Inexistente");

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task NameExistsAsync_Should_Respect_ExcludeId()
    {
        // Arrange
        var classId = Guid.NewGuid();
        var classEntity = new Class
        {
            Id = classId,
            Name = "Química Orgânica",
            CreatedAt = DateTime.Today,
            UpdatedAt = DateTime.Today.AddMonths(3),
        };

        _classRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<Class>>(), Arg.Any<CancellationToken>())
            .Returns(classEntity);

        // Act - mesmo ID, deve retornar false
        var exists = await _sut.NameExistsAsync("Química Orgânica", excludeId: classId);

        // Assert
        exists.Should().BeFalse();
    }

    #endregion

    #region GetWithStudentsAsync Tests

    [Fact]
    public async Task GetWithStudentsAsync_Should_Return_Class_With_Enrollments()
    {
        // Arrange
        var classId = Guid.NewGuid();
        var classEntity = new Class
        {
            Id = classId,
            Name = "Biologia",
            CreatedAt = DateTime.Today,
            UpdatedAt = DateTime.Today.AddMonths(3),
        };

        classEntity.Enrollments.Add(new Enrollment
        {
            Id = Guid.NewGuid(),
            ClassId = classId,
            StudentId = Guid.NewGuid(),
            CreatedAt = DateTime.Today
        });

        _classRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<Class>>(), Arg.Any<CancellationToken>())
            .Returns(classEntity);

        // Act
        var result = await _sut.GetWithStudentsAsync(classId);

        // Assert
        result.Should().NotBeNull();
        result.Enrollments.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetWithStudentsAsync_Should_Return_Null_When_Class_Not_Found()
    {
        // Arrange
        _classRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<Class>>(), Arg.Any<CancellationToken>())
            .Returns((Class?)null);

        // Act
        var result = await _sut.GetWithStudentsAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetPaginatedAsync Tests

    [Fact]
    public async Task GetPaginatedAsync_Should_Return_Paginated_Results()
    {
        // Arrange
        var classes = new List<Class>
        {
            new() { Id = Guid.NewGuid(), Name = "Turma 1", CreatedAt = DateTime.Today, UpdatedAt = DateTime.Today.AddMonths(3),  },
            new() { Id = Guid.NewGuid(), Name = "Turma 2", CreatedAt = DateTime.Today, UpdatedAt = DateTime.Today.AddMonths(3),  }
        };

        _classRepository.FindAsync(Arg.Any<ISpecification<Class>>(), Arg.Any<CancellationToken>())
            .Returns(classes);

        _classRepository.CountAsync(Arg.Any<ISpecification<Class>>(), Arg.Any<CancellationToken>())
            .Returns(50);

        // Act
        var result = await _sut.GetPaginatedAsync(pageNumber: 1, pageSize: 10);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(50);
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_Should_Create_Class_When_Valid()
    {
        // Arrange
        var classEntity = new Class
        {
            Id = Guid.NewGuid(),
            Name = "Nova Turma",
            Description = "Descrição",
            CreatedAt = DateTime.Today,
            UpdatedAt = DateTime.Today.AddMonths(3),
        };

        _validator.ValidateAsync(classEntity, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        _classRepository.AddAsync(Arg.Any<Class>(), Arg.Any<CancellationToken>())
            .Returns(classEntity);

        _classRepository.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _sut.AddAsync(classEntity);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Entity.Should().NotBeNull();
        result.Entity!.Name.Should().Be("Nova Turma");

        await _classRepository.Received(1).AddAsync(Arg.Any<Class>(), Arg.Any<CancellationToken>());
        await _classRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddAsync_Should_Fail_When_Validation_Fails()
    {
        // Arrange
        var classEntity = new Class
        {
            Name = "",
            CreatedAt = default,
            UpdatedAt = default,
        };

        var validationFailures = new List<ValidationFailure>
        {
            new("Name", "Nome é obrigatório"),
            new("Capacity", "Capacidade deve ser maior que zero")
        };

        _validator.ValidateAsync(classEntity, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(validationFailures));

        // Act
        var result = await _sut.AddAsync(classEntity);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ValidationResult.Errors.Should().HaveCount(2);
        result.Entity.Should().BeNull();

        await _classRepository.DidNotReceive().AddAsync(Arg.Any<Class>(), Arg.Any<CancellationToken>());
    }

    #endregion
}