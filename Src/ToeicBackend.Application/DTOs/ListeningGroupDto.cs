namespace ToeicBackend.Application.DTOs;

public class ListeningGroupDto
{
    public string Id { get; set; } = string.Empty;
    public int Part { get; set; }
    public string? PassageText { get; set; }
    public string? ImageUrl { get; set; }
    public string? AudioUrl { get; set; }
    public List<ListeningQuestionDto> Questions { get; set; } = new();
    public string? Source { get; set; }
}
