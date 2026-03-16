using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ScholarFlow.Domain.Entities;
using ScholarFlow.Domain.Entities.Base;
using ScholarFlow.Domain.Enums;
using ScholarFlow.Domain.Interfaces;
using System.Linq.Expressions;

namespace ScholarFlow.Infrastructure.Persistence;

/// <summary>
/// Application database context with Identity integration
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets
    public DbSet<StudentProfile> StudentProfiles { get; set; }
    public DbSet<TeacherProfile> TeacherProfiles { get; set; }
    public DbSet<AcademicStream> AcademicStreams { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<StreamSubject> StreamSubjects { get; set; }
    public DbSet<Topic> Topics { get; set; }
    public DbSet<SubTopic> SubTopics { get; set; }
    public DbSet<Paper> Papers { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Option> Options { get; set; }
    public DbSet<Explanation> Explanations { get; set; }
    public DbSet<ExplanationSection> ExplanationSections { get; set; }
    public DbSet<ExamSession> ExamSessions { get; set; }
    public DbSet<UserResponse> UserResponses { get; set; }
    public DbSet<StudentSubjectSelection> StudentSubjectSelections { get; set; }
    public DbSet<StudentTeacherConnection> StudentTeacherConnections { get; set; }
    
    // Enhanced Entities
    public DbSet<QuestionReview> QuestionReviews { get; set; }
    public DbSet<EnhancedExamSettings> EnhancedExamSettings { get; set; }
    public DbSet<StudentPerformanceAnalytics> StudentPerformanceAnalytics { get; set; }
    public DbSet<QuestionBank> QuestionBanks { get; set; }
    public DbSet<QuestionAttemptAnalytics> QuestionAttemptAnalytics { get; set; }
    public DbSet<SecurityEvent> SecurityEvents { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        
        // Suppress the pending model changes warning for development
        optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    public DbSet<BrowserLockdown> BrowserLockdowns { get; set; }
    public DbSet<QuestionImport> QuestionImports { get; set; }
    public DbSet<QuestionVersion> QuestionVersions { get; set; }
    
    // Expose Users from IdentityDbContext
    public new DbSet<ApplicationUser> Users { get; set; }

    // Interface implementation compatibility
    public DbSet<AcademicStream> Streams => AcademicStreams;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        // ===== DECIMAL PRECISION CONFIGURATIONS =====
        #region Decimal Precision Configurations
        
        // EnhancedExamSettings
        modelBuilder.Entity<EnhancedExamSettings>(entity =>
        {
            entity.Property(e => e.NegativeMarking).HasColumnType("decimal(5,2)");
            entity.Property(e => e.PassingScore).HasColumnType("decimal(5,2)");
            entity.Property(e => e.UnattemptedMarks).HasColumnType("decimal(5,2)");
        });

        // Question
        modelBuilder.Entity<Question>(entity =>
        {
            entity.Property(e => e.AverageTimeToAnswer).HasColumnType("decimal(10,2)");
            entity.Property(e => e.CalculatedDifficulty).HasColumnType("decimal(3,2)");
            entity.Property(e => e.Marks).HasColumnType("decimal(5,2)");
            entity.Property(e => e.QualityScore).HasColumnType("decimal(3,2)");
            entity.Property(e => e.SuccessRate).HasColumnType("decimal(5,2)");
        });

        // QuestionAttemptAnalytics
        modelBuilder.Entity<QuestionAttemptAnalytics>(entity =>
        {
            entity.Property(e => e.DifficultyMatch).HasColumnType("decimal(5,2)");
        });

        // QuestionBank  
        modelBuilder.Entity<QuestionBank>(entity =>
        {
            entity.Property(e => e.AverageQualityScore).HasColumnType("decimal(3,2)");
        });

        // StudentPerformanceAnalytics
        modelBuilder.Entity<StudentPerformanceAnalytics>(entity =>
        {
            entity.Property(e => e.AccuracyRate).HasColumnType("decimal(5,2)");
            entity.Property(e => e.AverageScore).HasColumnType("decimal(5,2)");
            entity.Property(e => e.AverageTimePerQuestion).HasColumnType("decimal(10,2)");
            entity.Property(e => e.ConsistencyScore).HasColumnType("decimal(3,2)");
            entity.Property(e => e.LearningPace).HasColumnType("decimal(5,2)");
            entity.Property(e => e.MasteryLevel).HasColumnType("decimal(3,2)");
            entity.Property(e => e.StrengthIndicator).HasColumnType("decimal(3,2)");
            entity.Property(e => e.WeaknessIndicator).HasColumnType("decimal(3,2)");
        });
        
        #endregion

        // Apply all configurations from the Configurations folder
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Note: StreamSubject relationships are now configured via separate configuration classes
        // to use explicit join entity instead of automatic many-to-many

        modelBuilder.Entity<StudentSubjectSelection>(entity =>
        {
            entity.HasOne(s => s.StudentProfile)
                .WithMany(p => p.SelectedSubjects)
                .HasForeignKey(s => s.StudentProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.Subject)
                .WithMany()
                .HasForeignKey(s => s.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(s => new { s.StudentProfileId, s.SubjectId })
                .IsUnique();
        });

        modelBuilder.Entity<StudentTeacherConnection>(entity =>
        {
            entity.Property(c => c.ConnectedAt).HasColumnType("datetime2");
            entity.Property(c => c.ReviewedAt).HasColumnType("datetime2");
            entity.Property(c => c.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue(StudentTeacherConnectionStatus.Pending);

            entity.HasOne(c => c.StudentUser)
                .WithMany()
                .HasForeignKey(c => c.StudentUserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.TeacherUser)
                .WithMany()
                .HasForeignKey(c => c.TeacherUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(c => new { c.StudentUserId, c.TeacherUserId })
                .IsUnique();

            entity.HasIndex(c => c.Status);
        });
        
        // Fix foreign key cascade issues
        modelBuilder.Entity<SecurityEvent>()
            .HasOne(se => se.ExamSession)
            .WithMany()
            .HasForeignKey(se => se.ExamSessionId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<SecurityEvent>()
            .HasOne(se => se.User)
            .WithMany()
            .HasForeignKey(se => se.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        // Global query filter for soft delete
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
                var filter = Expression.Lambda(Expression.Equal(property, Expression.Constant(false)), parameter);
                
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }

        modelBuilder.Entity<Paper>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<Subject>().HasQueryFilter(s => !s.IsDeleted);
        modelBuilder.Entity<Question>().HasQueryFilter(q => !q.IsDeleted);
    }

    /// <summary>
    /// Override SaveChanges to automatically handle audit fields
    /// </summary>
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        OnBeforeSaving();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    /// <summary>
    /// Override SaveChangesAsync to automatically handle audit fields
    /// </summary>
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        OnBeforeSaving();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// Automatically set audit fields before saving
    /// </summary>
    private void OnBeforeSaving()
    {
        var entries = ChangeTracker.Entries<IAuditable>();
        var utcNow = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = utcNow;
                    // TODO: Set CreatedBy from current user context
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = utcNow;
                    // TODO: Set UpdatedBy from current user context
                    break;
            }
        }

        // Handle soft delete
        var deletedEntries = ChangeTracker.Entries<ISoftDeletable>()
            .Where(e => e.State == EntityState.Deleted);

        foreach (var entry in deletedEntries)
        {
            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedAt = utcNow;
            // TODO: Set DeletedBy from current user context
        }
    }
}
