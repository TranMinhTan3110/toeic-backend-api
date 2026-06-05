using System.Collections.Generic;

namespace ToeicBackend.Application.DTOs;

public class ReadingGroupDto
{
    public string Id { get; set; } = string.Empty;
    public int Part { get; set; }
    public string? PassageText { get; set; }
    public string? Script { get; set; }
    public string? ImageUrl { get; set; }
    public List<ReadingQuestionDto> Questions { get; set; } = new();
    public string? Source { get; set; }
}
