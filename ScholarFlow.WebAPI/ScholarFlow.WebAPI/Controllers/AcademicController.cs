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
                .Include(s => s.Subjects)
                .ThenInclude(s => s.Topics)
                .ThenInclude(t => t.SubTopics)
                .ToListAsync();

            var result = streams.Select(stream => new
            {
                id = stream.Id.ToString(),
                name = stream.Name,
                subjects = stream.Subjects.Select(subject => new
                {
                    id = subject.Id.ToString(),
                    name = subject.Name,
                    streamId = stream.Id.ToString(),
                    topics = subject.Topics.Select(topic => new
                    {
                        id = topic.Id.ToString(),
                        name = topic.TopicName,
                        subjectId = subject.Id.ToString(),
                        subTopics = topic.SubTopics.Select(subTopic => new
                        {
                            id = subTopic.Id.ToString(),
                            name = subTopic.SubTopicName,
                            topicId = topic.Id.ToString()
                        }).ToList()
                    }).ToList()
                }).ToList()
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading streams from database");
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
        var streamExists = await _context.Streams
            .AnyAsync(s => s.Id == request.StreamId);

        if (!streamExists)
            return BadRequest("Stream not found");

        var subject = new Subject
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            StreamId = request.StreamId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Subjects.Add(subject);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            id = subject.Id,
            name = subject.Name,
            streamId = subject.StreamId
        });
    }

    // PUT: api/academic/subjects/{id}
    [HttpPut("subjects/{id}")]
    public async Task<IActionResult> UpdateSubject(string id, [FromBody] UpdateSubjectRequest request)
    {

            var subject = await _context.Subjects.FindAsync(id);

            if (subject == null)
                return NotFound();

            subject.Name = request.Name;
            subject.StreamId = request.StreamId;
            subject.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(CancellationToken.None);

            return Ok(new { id = subject.Id.ToString(), name = subject.Name, streamId = subject.StreamId.ToString() });
        
       
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
    public Guid StreamId { get; set; }
}

public class UpdateSubjectRequest
{
    public string Name { get; set; } = string.Empty;
    public Guid StreamId { get; set; }
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
