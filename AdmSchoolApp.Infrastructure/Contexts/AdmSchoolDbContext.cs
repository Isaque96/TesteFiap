using Microsoft.EntityFrameworkCore;

namespace AdmSchoolApp.Infrastructure.Contexts;

public partial class AdmSchoolDbContext : DbContext
{
    public AdmSchoolDbContext()
    {
    }

    public AdmSchoolDbContext(DbContextOptions<AdmSchoolDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<Enrollment> Enrollments { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:Default");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Class>(entity =>
        {
            entity.ToTable("Class", "adm", tb => tb.HasTrigger("trg_Class_SetUpdatedAt"));

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.ToTable("Enrollment", "adm", tb => tb.HasTrigger("trg_Enrollment_SetUpdatedAt"));

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Class).WithMany(p => p.Enrollments).HasConstraintName("FK_Enrollment_Class");

            entity.HasOne(d => d.Student).WithMany(p => p.Enrollments).HasConstraintName("FK_Enrollment_Student");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Role", "adm", tb => tb.HasTrigger("trg_Role_SetUpdatedAt"));

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("Student", "adm", tb => tb.HasTrigger("trg_Student_SetUpdatedAt"));

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.Cpf).IsFixedLength();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User", "adm", tb => tb.HasTrigger("trg_User_SetUpdatedAt"));

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRole", "adm", tb => tb.HasTrigger("trg_UserRole_SetUpdatedAt"));

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles).HasConstraintName("FK_UserRole_Role");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles).HasConstraintName("FK_UserRole_User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
