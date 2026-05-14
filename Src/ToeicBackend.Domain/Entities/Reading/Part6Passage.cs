namespace ToeicBackend.Domain.Entities.Reading;

public class Part6Passage
{
    public string Id { get; set; } = string.Empty;
    public string? PassageText { get; set; }
    public List<Dictionary<string, object?>>? Passages { get; set; }
    public string? ImageUrl { get; set; }
    public string? AudioUrl { get; set; }
    public List<string> QuestionIds { get; set; } = new();
    public List<Part5Question> Questions { get; set; } = new();
}
