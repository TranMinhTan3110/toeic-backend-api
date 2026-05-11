namespace ToeicBackend.Domain.Entities;

public class QuestionGroup
{
    public string Id { get; set; } = string.Empty;
    public int Part { get; set; }
    public string? PassageText { get; set; }
    public string? ImageUrl { get; set; }
    public string? AudioUrl { get; set; }
    public List<string> QuestionIds { get; set; } = new();
    public int QuestionCount { get; set; }
    public string? Source { get; set; }
    public DateTime? CreatedAt { get; set; }
}
