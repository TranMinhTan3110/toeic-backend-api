namespace ToeicBackend.Application.DTOs.Reading;

public class Part6PassageDto
{
    public string Id { get; set; } = string.Empty;
    public string? PassageText { get; set; }
    public string? PassageTranslation { get; set; }
    public List<object>? Passages { get; set; }
    public string? ImageUrl { get; set; }
    public string? AudioUrl { get; set; }
    public List<Part6QuestionDto> Questions { get; set; } = new();
}
