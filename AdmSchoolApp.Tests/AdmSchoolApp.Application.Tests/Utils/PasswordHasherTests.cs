using System.Text;
using AdmSchoolApp.Application.Utils;
using FluentAssertions;

namespace AdmSchoolApp.AdmSchoolApp.Application.Tests.Utils;

public class PasswordHasherTests
{
    [Fact]
    public void HashPassword_Should_Return_BCrypt_Hash()
    {
        // Arrange
        const string password = "Test@123";

        // Act
        var hash = PasswordHasher.HashPassword(password);

        // Assert
        hash.Should().NotBeNull();
        hash.Length.Should().BeGreaterThan(0);
        
        var hashString = Encoding.UTF8.GetString(hash);
        hashString.Should().StartWith("$2"); // BCrypt prefix
        hashString.Should().NotBe(password); // Não deve ser texto plano
    }

    [Fact]
    public void HashPassword_Should_Generate_Different_Hashes_For_Same_Password()
    {
        // Arrange
        const string password = "Test@123";

        // Act
        var hash1 = PasswordHasher.HashPassword(password);
        var hash2 = PasswordHasher.HashPassword(password);

        // Assert
        hash1.Should().NotBeEquivalentTo(hash2); // BCrypt usa salt aleatório
    }

    [Fact]
    public void VerifyPassword_Should_Return_True_For_Correct_Password()
    {
        // Arrange
        const string password = "MySecurePass@123";
        var hash = PasswordHasher.HashPassword(password);

        // Act
        var result = PasswordHasher.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_Should_Return_False_For_Incorrect_Password()
    {
        // Arrange
        const string correctPassword = "MySecurePass@123";
        const string wrongPassword = "WrongPassword";
        var hash = PasswordHasher.HashPassword(correctPassword);

        // Act
        var result = PasswordHasher.VerifyPassword(wrongPassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("Simple@123")]
    [InlineData("Complex!Pass#2024")]
    [InlineData("Abc@123456789")]
    public void HashPassword_And_Verify_Should_Work_For_Various_Passwords(string password)
    {
        // Arrange & Act
        var hash = PasswordHasher.HashPassword(password);
        var isValid = PasswordHasher.VerifyPassword(password, hash);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_Should_Handle_Empty_Password()
    {
        // Arrange
        const string password = "";
        var hash = PasswordHasher.HashPassword(password);

        // Act
        var result = PasswordHasher.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HashPassword_Should_Be_Deterministic_In_Verification()
    {
        // Arrange
        const string password = "Test@123";
        var hash = PasswordHasher.HashPassword(password);

        // Act - Verificar múltiplas vezes
        var result1 = PasswordHasher.VerifyPassword(password, hash);
        var result2 = PasswordHasher.VerifyPassword(password, hash);
        var result3 = PasswordHasher.VerifyPassword(password, hash);

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeTrue();
        result3.Should().BeTrue();
    }
}