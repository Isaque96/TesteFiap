using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AdmSchoolApp.Infrastructure;

[Table("Student", Schema = "adm")]
[Index("Cpf", Name = "UX_Student_Cpf", IsUnique = true)]
[Index("Email", Name = "UX_Student_Email", IsUnique = true)]
public partial class Student
{
    [Key]
    public Guid Id { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    public DateOnly BirthDate { get; set; }

    [StringLength(11)]
    [Unicode(false)]
    public string Cpf { get; set; } = null!;

    [StringLength(256)]
    public string Email { get; set; } = null!;

    [MaxLength(512)]
    public byte[]? PasswordHash { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [Precision(0)]
    public DateTime UpdatedAt { get; set; }

    [InverseProperty("Student")]
    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
