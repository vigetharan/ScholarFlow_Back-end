using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScholarFlow.Domain.Entities;

namespace ScholarFlow.Infrastructure.Persistence.Configurations;

public class SubjectConfiguration : AuditableEntityConfiguration<Subject>
{
    public override void Configure(EntityTypeBuilder<Subject> builder)
    {
        base.Configure(builder);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(s => s.Name)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        // Configure one-to-many relationship with StreamSubject (explicit join entity)
        builder.HasMany(s => s.StreamSubjects)
            .WithOne(ss => ss.Subject)
            .HasForeignKey(ss => ss.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure backing fields for navigation properties
        builder.Metadata.FindNavigation(nameof(Subject.StreamSubjects))!
            .SetField("_streamSubjects");
        builder.Metadata.FindNavigation(nameof(Subject.Topics))!
            .SetField("_topics");
        builder.Metadata.FindNavigation(nameof(Subject.Papers))!
            .SetField("_papers");
    }
}
