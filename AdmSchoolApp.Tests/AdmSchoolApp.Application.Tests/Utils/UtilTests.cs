using AdmSchoolApp.Application.Utils;
using FluentAssertions;

namespace AdmSchoolApp.AdmSchoolApp.Application.Tests.Utils;

public class UtilTests
{
    #region GetAllExceptions Tests

    [Fact]
    public void GetAllExceptions_Should_Return_Single_Exception_When_No_Inner()
    {
        // Arrange
        var exception = new Exception("Outer exception");

        // Act
        var exceptions = exception.GetAllExceptions().ToArray();

        // Assert
        exceptions.Should().ContainSingle();
        exceptions.Should().Contain(exception);
    }

    [Fact]
    public void GetAllExceptions_Should_Return_All_Nested_Exceptions()
    {
        // Arrange
        var innermost = new InvalidOperationException("Innermost");
        var middle = new ArgumentException("Middle", innermost);
        var outer = new Exception("Outer", middle);

        // Act
        var exceptions = outer.GetAllExceptions();

        // Assert
        var exceptionList = new List<Exception>(exceptions);
        exceptionList.Should().HaveCount(3);
        exceptionList[0].Should().Be(outer);
        exceptionList[1].Should().Be(middle);
        exceptionList[2].Should().Be(innermost);
    }

    [Fact]
    public void GetAllExceptions_Should_Preserve_Exception_Order()
    {
        // Arrange
        var level3 = new Exception("Level 3");
        var level2 = new Exception("Level 2", level3);
        var level1 = new Exception("Level 1", level2);

        // Act
        var exceptions = level1.GetAllExceptions();

        // Assert
        var exceptionList = new List<Exception>(exceptions);
        exceptionList[0].Message.Should().Be("Level 1");
        exceptionList[1].Message.Should().Be("Level 2");
        exceptionList[2].Message.Should().Be("Level 3");
    }

    [Fact]
    public void GetAllExceptions_Should_Handle_Deep_Nesting()
    {
        // Arrange
        var current = new Exception("Base");
        for (var i = 1; i <= 10; i++)
            current = new Exception($"Level {i}", current);

        // Act
        var exceptions = current.GetAllExceptions();

        // Assert
        exceptions.Should().HaveCount(11); // 10 níveis + base
    }

    #endregion

    #region SerilogTemplate Tests

    [Fact]
    public void SerilogTemplate_Should_Be_Defined()
    {
        // Act
        const string template = Util.SerilogTemplate;

        // Assert
        template.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void SerilogTemplate_Should_Contain_Required_Placeholders()
    {
        // Act
        const string template = Util.SerilogTemplate;

        // Assert
        template.Should().Contain("{Timestamp");
        template.Should().Contain("{Level");
        template.Should().Contain("{SourceContext}");
        template.Should().Contain("{Message");
        template.Should().Contain("{Exception}");
    }

    [Fact]
    public void SerilogTemplate_Should_Contain_Correlation_Fields()
    {
        // Act
        const string template = Util.SerilogTemplate;

        // Assert
        template.Should().Contain("{CorrelationId}");
        template.Should().Contain("{RequestId}");
        template.Should().Contain("{TraceId}");
        template.Should().Contain("{SpanId}");
    }

    [Fact]
    public void SerilogTemplate_Should_Have_Correct_Format()
    {
        // Act
        const string template = Util.SerilogTemplate;

        // Assert
        template.Should().StartWith("[{Timestamp");
        template.Should().EndWith("{NewLine}{Exception}");
    }

    #endregion
}