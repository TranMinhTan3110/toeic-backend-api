namespace ToeicBackend.Application.DTOs;

public class GrammarLessonDto
{
    public string Id { get; set; } = string.Empty;
    public string TopicId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty; // Markdown text
    public int Order { get; set; }
}
