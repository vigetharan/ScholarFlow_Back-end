using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScholarFlow.Domain.Entities;

namespace ScholarFlow.Infrastructure.Persistence.Configurations;

public class StreamSubjectConfiguration : IEntityTypeConfiguration<StreamSubject>
{
    public void Configure(EntityTypeBuilder<StreamSubject> builder)
    {
        // Configure table name
        builder.ToTable("StreamSubjects");
        
        // Configure primary key using the Id from AuditableEntity
        builder.HasKey(ss => ss.Id);
        
        // Configure RowVersion as concurrency token
        builder.Property(ss => ss.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken()
            .ValueGeneratedOnAddOrUpdate();

        // Configure foreign key to Stream (one-to-many)
        builder.HasOne(ss => ss.Stream)
            .WithMany(s => s.StreamSubjects)
            .HasForeignKey(ss => ss.StreamId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure foreign key to Subject (one-to-many)
        builder.HasOne(ss => ss.Subject)
            .WithMany(s => s.StreamSubjects)
            .HasForeignKey(ss => ss.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure indexes for performance
        builder.HasIndex(ss => ss.StreamId);
        builder.HasIndex(ss => ss.SubjectId);
        builder.HasIndex(ss => new { ss.StreamId, ss.SubjectId }).IsUnique();
        
        // Configure soft delete filter
        builder.HasQueryFilter(ss => !ss.IsDeleted);
    }
}
