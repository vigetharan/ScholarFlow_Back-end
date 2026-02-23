using Microsoft.EntityFrameworkCore;
using ScholarFlow.Domain.Entities;
using ScholarFlow.Domain.Interfaces;

namespace ScholarFlow.WebAPI.Data;

/// <summary>
/// Seeder for academic structure data
/// </summary>
public static class AcademicSeeder
{
    public static async Task SeedAcademicDataAsync(IApplicationDbContext context)
    {
        // Check if data already exists
        if (await context.Streams.AnyAsync())
        {
            return; // Database has been seeded
        }

        // Create Engineering Stream
        var engineeringStream = new AcademicStream
        {
            Id = Guid.NewGuid(),
            Name = "Engineering",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await context.Streams.AddAsync(engineeringStream);
        await context.SaveChangesAsync(CancellationToken.None);

        // Create Computer Science Subject
        var computerScienceSubject = new Subject
        {
            Id = Guid.NewGuid(),
            Name = "Computer Science",
            StreamId = engineeringStream.Id,  // Direct relationship
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await context.Subjects.AddAsync(computerScienceSubject);
        await context.SaveChangesAsync(CancellationToken.None);

        // Create Programming Topic
        var programmingTopic = new Topic
        {
            Id = Guid.NewGuid(),
            TopicName = "Programming",
            SubjectId = computerScienceSubject.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await context.Topics.AddAsync(programmingTopic);
        await context.SaveChangesAsync(CancellationToken.None);

        // Create SubTopics
        var subTopics = new[]
        {
            new SubTopic
            {
                Id = Guid.NewGuid(),
                SubTopicName = "JavaScript",
                TopicId = programmingTopic.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new SubTopic
            {
                Id = Guid.NewGuid(),
                SubTopicName = "Python",
                TopicId = programmingTopic.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new SubTopic
            {
                Id = Guid.NewGuid(),
                SubTopicName = "Java",
                TopicId = programmingTopic.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        await context.SubTopics.AddRangeAsync(subTopics);
        await context.SaveChangesAsync(CancellationToken.None);

        // Create Medical Stream
        var medicalStream = new AcademicStream
        {
            Id = Guid.NewGuid(),
            Name = "Medical",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await context.Streams.AddAsync(medicalStream);
        await context.SaveChangesAsync(CancellationToken.None);

        // Create Biology Subject
        var biologySubject = new Subject
        {
            Id = Guid.NewGuid(),
            Name = "Biology",
            StreamId = medicalStream.Id,  // Direct relationship
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await context.Subjects.AddAsync(biologySubject);
        await context.SaveChangesAsync(CancellationToken.None);

        // Create Anatomy Topic
        var anatomyTopic = new Topic
        {
            Id = Guid.NewGuid(),
            TopicName = "Anatomy",
            SubjectId = biologySubject.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await context.Topics.AddAsync(anatomyTopic);
        await context.SaveChangesAsync(CancellationToken.None);

        // Create Medical SubTopics
        var medicalSubTopics = new[]
        {
            new SubTopic
            {
                Id = Guid.NewGuid(),
                SubTopicName = "Human Anatomy",
                TopicId = anatomyTopic.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new SubTopic
            {
                Id = Guid.NewGuid(),
                SubTopicName = "Cell Biology",
                TopicId = anatomyTopic.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        await context.SubTopics.AddRangeAsync(medicalSubTopics);
        await context.SaveChangesAsync(CancellationToken.None);
    }
}
