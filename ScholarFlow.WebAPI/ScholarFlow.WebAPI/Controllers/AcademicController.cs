using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ScholarFlow.Application.DTOs;
using ScholarFlow.Domain.Entities;
using ScholarFlow.Domain.Interfaces;

namespace ScholarFlow.WebAPI.Controllers;

[ApiController]
[Route("api/academic")]
public class AcademicController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<AcademicController> _logger;

    public AcademicController(IApplicationDbContext context, ILogger<AcademicController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/academic/streams
    [HttpGet("streams")]
    public async Task<IActionResult> GetStreams()
    {
        try
        {
            var streams = await _context.Streams
                .ToListAsync();

            var result = new List<object>();
            
            // Debug: Check total StreamSubjects count
            var totalStreamSubjects = await _context.StreamSubjects.CountAsync();
            _logger.LogInformation($"Total StreamSubjects in database: {totalStreamSubjects}");
            
            foreach (var stream in streams)
            {
                // Load StreamSubjects separately to avoid filter conflicts
                var streamSubjects = await _context.StreamSubjects
                    .Include(ss => ss.Subject)
                    .Where(ss => ss.StreamId == stream.Id && !ss.IsDeleted)
                    .ToListAsync();

                // Debug: Log stream and subject count
                _logger.LogInformation($"Stream '{stream.Name}' (ID: {stream.Id}) has {streamSubjects.Count} subjects");

                var streamResult = new
                {
                    id = stream.Id.ToString(),
                    name = stream.Name,
                    subjects = streamSubjects.Select(ss => new
                    {
                        id = ss.Subject.Id.ToString(),
                        name = ss.Subject.Name,
                        streamId = stream.Id.ToString()
                    }).ToList()
                };
                
                result.Add(streamResult);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting streams");
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/academic/streams
    [HttpPost("streams")]
    public async Task<IActionResult> CreateStream([FromBody] CreateStreamRequest request)
    {
        try
        {
            var stream = new AcademicStream
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Streams.Add(stream);
            await _context.SaveChangesAsync(CancellationToken.None);

            return Ok(new { id = stream.Id.ToString(), name = stream.Name });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating stream");
            return StatusCode(500, "Internal server error");
        }
    }

    // PUT: api/academic/streams/{id}
    [HttpPut("streams/{id}")]
    public async Task<IActionResult> UpdateStream(Guid id, [FromBody] UpdateStreamRequest request)
    {
        try
        {
            var stream = await _context.Streams.FindAsync(id);

            if (stream == null)
                return NotFound();

            stream.Name = request.Name;
            stream.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(CancellationToken.None);

            return Ok(new { id = stream.Id.ToString(), name = stream.Name });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stream");
            return StatusCode(500, "Internal server error");
        }
    }

    // DELETE: api/academic/streams/{id}
    [HttpDelete("streams/{id}")]
    public async Task<IActionResult> DeleteStream(Guid id)
    {
        try
        {
            var stream = await _context.Streams.FindAsync(id);

            if (stream == null)
                return NotFound();

            _context.Streams.Remove(stream);
            await _context.SaveChangesAsync(CancellationToken.None);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting stream");
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/academic/subjects
    [HttpPost("subjects")]
    public async Task<IActionResult> CreateSubject([FromBody] CreateSubjectRequest request)
    {
        var subject = new Subject
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Subjects.Add(subject);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            id = subject.Id,
            name = subject.Name
        });
    }

    // PUT: api/academic/subjects/{id}
    [HttpPut("subjects/{id}")]
    public async Task<IActionResult> UpdateSubject(string id, [FromBody] UpdateSubjectRequest request)
    {
        var subjectId = Guid.Parse(id);
        var subject = await _context.Subjects.FindAsync(subjectId);

        if (subject == null)
            return NotFound();

        subject.Name = request.Name;
        subject.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(CancellationToken.None);

        return Ok(new { id = subject.Id.ToString(), name = subject.Name });
    }

    // DELETE: api/academic/subjects/{id}
    [HttpDelete("subjects/{id}")]
    public async Task<IActionResult> DeleteSubject(string id)
    {
        try
        {
            var subjectId = Guid.Parse(id);
            var subject = await _context.Subjects.FindAsync(subjectId);

            if (subject == null)
                return NotFound();

            _context.Subjects.Remove(subject);
            await _context.SaveChangesAsync(CancellationToken.None);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting subject");
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/academic/streams/{streamId}/subjects/{subjectId}
    [HttpPost("streams/{streamId}/subjects/{subjectId}")]
    public async Task<IActionResult> AddSubjectToStream(string streamId, string subjectId)
    {
        try
        {
            var streamGuid = Guid.Parse(streamId);
            var subjectGuid = Guid.Parse(subjectId);
            
            _logger.LogInformation($"Attempting to add subject {subjectGuid} to stream {streamGuid}");
            
            var stream = await _context.Streams
                .FirstOrDefaultAsync(s => s.Id == streamGuid);
                
            var subject = await _context.Subjects.FindAsync(subjectGuid);
            
            if (stream == null || subject == null)
            {
                _logger.LogWarning($"Stream {streamGuid} or Subject {subjectGuid} not found");
                return NotFound();
            }
            
            // Check if the relationship already exists
            var existingRelationship = await _context.StreamSubjects
                .FirstOrDefaultAsync(ss => ss.StreamId == streamGuid && ss.SubjectId == subjectGuid);
                
            if (existingRelationship != null)
            {
                _logger.LogWarning($"Subject {subjectGuid} is already linked to stream {streamGuid}");
                return BadRequest("Subject is already linked to this stream");
            }
            
            // Create the relationship
            var streamSubject = new StreamSubject
            {
                Id = Guid.NewGuid(),
                StreamId = streamGuid,
                SubjectId = subjectGuid,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            
            _logger.LogInformation($"Creating StreamSubject: ID={streamSubject.Id}, StreamId={streamSubject.StreamId}, SubjectId={streamSubject.SubjectId}");
            
            _context.StreamSubjects.Add(streamSubject);
            var saveResult = await _context.SaveChangesAsync(CancellationToken.None);
            
            _logger.LogInformation($"SaveChanges result: {saveResult} rows affected");
            
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding subject to stream");
            return StatusCode(500, "Internal server error");
        }
    }

    // DELETE: api/academic/streams/{streamId}/subjects/{subjectId}
    [HttpDelete("streams/{streamId}/subjects/{subjectId}")]
    public async Task<IActionResult> RemoveSubjectFromStream(string streamId, string subjectId)
    {
        try
        {
            var streamGuid = Guid.Parse(streamId);
            var subjectGuid = Guid.Parse(subjectId);
            
            var streamSubject = await _context.StreamSubjects
                .FirstOrDefaultAsync(ss => ss.StreamId == streamGuid && ss.SubjectId == subjectGuid);
            
            if (streamSubject == null)
                return NotFound();
            
            _context.StreamSubjects.Remove(streamSubject);
            await _context.SaveChangesAsync(CancellationToken.None);
            
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing subject from stream");
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/academic/topics
    [HttpPost("topics")]
    public async Task<IActionResult> CreateTopic([FromBody] CreateTopicRequest request)
    {
        try
        {
            var topic = new Topic
            {
                Id = Guid.NewGuid(),
                TopicName = request.Name,
                SubjectId = Guid.Parse(request.SubjectId),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Topics.Add(topic);
            await _context.SaveChangesAsync(CancellationToken.None);

            return Ok(new { id = topic.Id.ToString(), name = topic.TopicName, subjectId = topic.SubjectId.ToString() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating topic");
            return StatusCode(500, "Internal server error");
        }
    }

    // PUT: api/academic/topics/{id}
    [HttpPut("topics/{id}")]
    public async Task<IActionResult> UpdateTopic(string id, [FromBody] UpdateTopicRequest request)
    {
        try
        {
            var topicId = Guid.Parse(id);
            var topic = await _context.Topics.FindAsync(topicId);

            if (topic == null)
                return NotFound();

            topic.TopicName = request.Name;
            topic.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(CancellationToken.None);

            return Ok(new { id = topic.Id.ToString(), name = topic.TopicName, subjectId = topic.SubjectId.ToString() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating topic");
            return StatusCode(500, "Internal server error");
        }
    }

    // DELETE: api/academic/topics/{id}
    [HttpDelete("topics/{id}")]
    public async Task<IActionResult> DeleteTopic(string id)
    {
        try
        {
            var topicId = Guid.Parse(id);
            var topic = await _context.Topics.FindAsync(topicId);

            if (topic == null)
                return NotFound();

            _context.Topics.Remove(topic);
            await _context.SaveChangesAsync(CancellationToken.None);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting topic");
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/academic/subtopics
    [HttpPost("subtopics")]
    public async Task<IActionResult> CreateSubTopic([FromBody] CreateSubTopicRequest request)
    {
        try
        {
            var subTopic = new SubTopic
            {
                Id = Guid.NewGuid(),
                SubTopicName = request.Name,
                TopicId = Guid.Parse(request.TopicId),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.SubTopics.Add(subTopic);
            await _context.SaveChangesAsync(CancellationToken.None);

            return Ok(new { id = subTopic.Id.ToString(), name = subTopic.SubTopicName, topicId = subTopic.TopicId.ToString() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subtopic");
            return StatusCode(500, "Internal server error");
        }
    }

    // PUT: api/academic/subtopics/{id}
    [HttpPut("subtopics/{id}")]
    public async Task<IActionResult> UpdateSubTopic(string id, [FromBody] UpdateSubTopicRequest request)
    {
        try
        {
            var subTopicId = Guid.Parse(id);
            var subTopic = await _context.SubTopics.FindAsync(subTopicId);

            if (subTopic == null)
                return NotFound();

            subTopic.SubTopicName = request.Name;
            subTopic.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(CancellationToken.None);

            return Ok(new { id = subTopic.Id.ToString(), name = subTopic.SubTopicName, topicId = subTopic.TopicId.ToString() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subtopic");
            return StatusCode(500, "Internal server error");
        }
    }

    // DELETE: api/academic/subtopics/{id}
    [HttpDelete("subtopics/{id}")]
    public async Task<IActionResult> DeleteSubTopic(string id)
    {
        try
        {
            var subTopicId = Guid.Parse(id);
            var subTopic = await _context.SubTopics.FindAsync(subTopicId);

            if (subTopic == null)
                return NotFound();

            _context.SubTopics.Remove(subTopic);
            await _context.SaveChangesAsync(CancellationToken.None);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting subtopic");
            return StatusCode(500, "Internal server error");
        }
    }
}

// DTOs for requests
public class CreateStreamRequest
{
    public string Name { get; set; } = string.Empty;
}

public class UpdateStreamRequest
{
    public string Name { get; set; } = string.Empty;
}

public class CreateSubjectRequest
{
    public string Name { get; set; } = string.Empty;
}

public class UpdateSubjectRequest
{
    public string Name { get; set; } = string.Empty;
}

public class CreateTopicRequest
{
    public string Name { get; set; } = string.Empty;
    public string SubjectId { get; set; } = string.Empty;
}

public class UpdateTopicRequest
{
    public string Name { get; set; } = string.Empty;
    public string SubjectId { get; set; } = string.Empty;
}

public class CreateSubTopicRequest
{
    public string Name { get; set; } = string.Empty;
    public string TopicId { get; set; } = string.Empty;
}

public class UpdateSubTopicRequest
{
    public string Name { get; set; } = string.Empty;
    public string TopicId { get; set; } = string.Empty;
}
