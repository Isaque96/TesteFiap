using System.Text;

namespace AdmSchoolApp.Application.Utils;

public static class PasswordHasher
{
    public static byte[] HashPassword(string password)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        return Encoding.UTF8.GetBytes(hash);
    }

    public static bool VerifyPassword(string password, byte[] passwordHash)
    {
        var hashString = Encoding.UTF8.GetString(passwordHash);
        return BCrypt.Net.BCrypt.Verify(password, hashString);
    }
}