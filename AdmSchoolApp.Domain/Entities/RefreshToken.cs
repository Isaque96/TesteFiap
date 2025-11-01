using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AdmSchoolApp.Domain.Entities;

[Table("RefreshToken", Schema = "adm")]
[Index("UserId", Name = "IX_RefreshToken_UserId")]
[Index("Token", Name = "UX_RefreshToken_Token", IsUnique = true)]
public partial class RefreshToken
{
    [Key]
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    [StringLength(512)]
    public string Token { get; set; } = null!;

    [Precision(0)]
    public DateTime ExpiresAt { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    public bool IsRevoked { get; set; }

    [Precision(0)]
    public DateTime? RevokedAt { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("RefreshTokens")]
    public virtual User User { get; set; } = null!;
}
