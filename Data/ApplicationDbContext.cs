using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using StudyTracker.Models;

namespace StudyTracker.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Course> Courses => Set<Course>();

    public DbSet<StudyTask> StudyTasks => Set<StudyTask>();

    public DbSet<StudentCourse> StudentCourses => Set<StudentCourse>();

    public DbSet<StudentTaskStatus> StudentTaskStatuses => Set<StudentTaskStatus>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigureIdentityTables(builder);
        ConfigureDomainTables(builder);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private static void ConfigureIdentityTables(ModelBuilder builder)
    {
        builder.Entity<User>(entity =>
        {
            entity.ToTable("users", table =>
            {
                table.HasCheckConstraint("ck_users_role", "role IN ('Student', 'Administrator')");
            });

            entity.Property(user => user.Id).HasColumnName("id");
            entity.Property(user => user.UserName).HasColumnName("user_name");
            entity.Property(user => user.NormalizedUserName).HasColumnName("normalized_user_name");
            entity.Property(user => user.Email).HasColumnName("email");
            entity.Property(user => user.NormalizedEmail).HasColumnName("normalized_email");
            entity.Property(user => user.EmailConfirmed).HasColumnName("email_confirmed");
            entity.Property(user => user.PasswordHash).HasColumnName("password_hash");
            entity.Property(user => user.SecurityStamp).HasColumnName("security_stamp");
            entity.Property(user => user.ConcurrencyStamp).HasColumnName("concurrency_stamp");
            entity.Property(user => user.PhoneNumber).HasColumnName("phone_number");
            entity.Property(user => user.PhoneNumberConfirmed).HasColumnName("phone_number_confirmed");
            entity.Property(user => user.TwoFactorEnabled).HasColumnName("two_factor_enabled");
            entity.Property(user => user.LockoutEnd).HasColumnName("lockout_end");
            entity.Property(user => user.LockoutEnabled).HasColumnName("lockout_enabled");
            entity.Property(user => user.AccessFailedCount).HasColumnName("access_failed_count");
            entity.Property(user => user.FullName).HasColumnName("full_name").HasMaxLength(120).IsRequired();
            entity.Property(user => user.Role)
                .HasColumnName("role")
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();
            entity.Property(user => user.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();
            entity.Property(user => user.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.HasIndex(user => user.NormalizedEmail).HasDatabaseName("email_index");
            entity.HasIndex(user => user.NormalizedUserName).IsUnique().HasDatabaseName("user_name_index");
        });

        builder.Entity<IdentityRole>(entity =>
        {
            entity.ToTable("identity_roles");

            entity.Property(role => role.Id).HasColumnName("id");
            entity.Property(role => role.Name).HasColumnName("name");
            entity.Property(role => role.NormalizedName).HasColumnName("normalized_name");
            entity.Property(role => role.ConcurrencyStamp).HasColumnName("concurrency_stamp");

            entity.HasIndex(role => role.NormalizedName).IsUnique().HasDatabaseName("role_name_index");
        });

        builder.Entity<IdentityRoleClaim<string>>(entity =>
        {
            entity.ToTable("identity_role_claims");

            entity.Property(claim => claim.Id).HasColumnName("id");
            entity.Property(claim => claim.RoleId).HasColumnName("role_id");
            entity.Property(claim => claim.ClaimType).HasColumnName("claim_type");
            entity.Property(claim => claim.ClaimValue).HasColumnName("claim_value");
        });

        builder.Entity<IdentityUserClaim<string>>(entity =>
        {
            entity.ToTable("identity_user_claims");

            entity.Property(claim => claim.Id).HasColumnName("id");
            entity.Property(claim => claim.UserId).HasColumnName("user_id");
            entity.Property(claim => claim.ClaimType).HasColumnName("claim_type");
            entity.Property(claim => claim.ClaimValue).HasColumnName("claim_value");
        });

        builder.Entity<IdentityUserLogin<string>>(entity =>
        {
            entity.ToTable("identity_user_logins");

            entity.Property(login => login.LoginProvider).HasColumnName("login_provider");
            entity.Property(login => login.ProviderKey).HasColumnName("provider_key");
            entity.Property(login => login.ProviderDisplayName).HasColumnName("provider_display_name");
            entity.Property(login => login.UserId).HasColumnName("user_id");
        });

        builder.Entity<IdentityUserRole<string>>(entity =>
        {
            entity.ToTable("identity_user_roles");

            entity.Property(userRole => userRole.UserId).HasColumnName("user_id");
            entity.Property(userRole => userRole.RoleId).HasColumnName("role_id");
        });

        builder.Entity<IdentityUserToken<string>>(entity =>
        {
            entity.ToTable("identity_user_tokens");

            entity.Property(token => token.UserId).HasColumnName("user_id");
            entity.Property(token => token.LoginProvider).HasColumnName("login_provider");
            entity.Property(token => token.Name).HasColumnName("name");
            entity.Property(token => token.Value).HasColumnName("value");
        });
    }

    private static void ConfigureDomainTables(ModelBuilder builder)
    {
        builder.Entity<Course>(entity =>
        {
            entity.ToTable("courses");

            entity.Property(course => course.Id).HasColumnName("id");
            entity.Property(course => course.Name).HasMaxLength(120).IsRequired();
            entity.Property(course => course.Name).HasColumnName("name");
            entity.Property(course => course.Description)
                .HasColumnName("description")
                .HasMaxLength(1000)
                .IsRequired();
            entity.Property(course => course.ProfessorName)
                .HasColumnName("professor_name")
                .HasMaxLength(120)
                .IsRequired();
            entity.Property(course => course.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();
            entity.Property(course => course.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();
        });

        builder.Entity<StudyTask>(entity =>
        {
            entity.ToTable("study_tasks", table =>
            {
                table.HasCheckConstraint("ck_study_tasks_status", "status IN ('NotStarted', 'InProgress', 'Completed')");
            });

            entity.Property(task => task.Id).HasColumnName("id");
            entity.Property(task => task.CourseId).HasColumnName("course_id");
            entity.Property(task => task.Title)
                .HasColumnName("title")
                .HasMaxLength(160)
                .IsRequired();
            entity.Property(task => task.Description)
                .HasColumnName("description")
                .HasMaxLength(2000)
                .IsRequired();
            entity.Property(task => task.Deadline)
                .HasColumnName("deadline")
                .HasColumnType("date")
                .IsRequired();
            entity.Property(task => task.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();
            entity.Property(task => task.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();
            entity.Property(task => task.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.HasIndex(task => task.CourseId);
            entity.HasIndex(task => task.Deadline);
            entity.HasIndex(task => task.Status);

            entity
                .HasOne(task => task.Course)
                .WithMany(course => course.Tasks)
                .HasForeignKey(task => task.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<StudentCourse>(entity =>
        {
            entity.ToTable("student_courses");

            entity.Property(link => link.Id).HasColumnName("id");
            entity.Property(link => link.StudentId).HasColumnName("student_id");
            entity.Property(link => link.CourseId).HasColumnName("course_id");
            entity.Property(link => link.AssignedAt)
                .HasColumnName("assigned_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.HasIndex(link => new { link.StudentId, link.CourseId }).IsUnique();
            entity.HasIndex(link => link.CourseId);

            entity
                .HasOne(link => link.Student)
                .WithMany(student => student.StudentCourses)
                .HasForeignKey(link => link.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne(link => link.Course)
                .WithMany(course => course.StudentCourses)
                .HasForeignKey(link => link.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<StudentTaskStatus>(entity =>
        {
            entity.ToTable("student_task_statuses", table =>
            {
                table.HasCheckConstraint("ck_student_task_statuses_status", "status IN ('NotStarted', 'InProgress', 'Completed')");
            });

            entity.Property(status => status.Id).HasColumnName("id");
            entity.Property(status => status.StudentId).HasColumnName("student_id");
            entity.Property(status => status.StudyTaskId).HasColumnName("task_id");
            entity.HasIndex(status => new { status.StudentId, status.StudyTaskId }).IsUnique();
            entity.HasIndex(status => status.Status);
            entity.HasIndex(status => status.StudyTaskId);
            entity.Property(status => status.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();
            entity.Property(status => status.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity
                .HasOne(status => status.Student)
                .WithMany(student => student.TaskStatuses)
                .HasForeignKey(status => status.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne(status => status.StudyTask)
                .WithMany(task => task.StudentStatuses)
                .HasForeignKey(status => status.StudyTaskId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void UpdateTimestamps()
    {
        var utcNow = DateTime.UtcNow;

        UpdateCreatedUpdated(ChangeTracker.Entries<Course>(), utcNow);
        UpdateCreatedUpdated(ChangeTracker.Entries<StudyTask>(), utcNow);
        UpdateCreatedUpdated(ChangeTracker.Entries<User>(), utcNow);

        foreach (var entry in ChangeTracker.Entries<StudentCourse>())
        {
            if (entry.State == EntityState.Added && entry.Entity.AssignedAt == default)
            {
                entry.Entity.AssignedAt = utcNow;
            }
        }

        foreach (var entry in ChangeTracker.Entries<StudentTaskStatus>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.UpdatedAt = utcNow;
            }
        }
    }

    private static void UpdateCreatedUpdated<TEntity>(
        IEnumerable<EntityEntry<TEntity>> entries,
        DateTime utcNow)
        where TEntity : class
    {
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                SetProperty(entry, nameof(Course.CreatedAt), utcNow);
                SetProperty(entry, nameof(Course.UpdatedAt), utcNow);
            }
            else if (entry.State == EntityState.Modified)
            {
                SetProperty(entry, nameof(Course.UpdatedAt), utcNow);
                entry.Property(nameof(Course.CreatedAt)).IsModified = false;
            }
        }
    }

    private static void SetProperty<TEntity>(EntityEntry<TEntity> entry, string propertyName, DateTime value)
        where TEntity : class
    {
        entry.Property(propertyName).CurrentValue = value;
    }
}
