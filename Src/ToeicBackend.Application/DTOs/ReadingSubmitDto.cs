using System.Collections.Generic;

namespace ToeicBackend.Application.DTOs;

public class ReadingSubmitDto
{
    public List<ReadingAnswerItemDto> Answers { get; set; } = new();
}

public class ReadingAnswerItemDto
{
    public string QuestionId { get; set; } = string.Empty;
    public int? SelectedIndex { get; set; }
    public string Answer { get; set; } = string.Empty; // e.g. "A", "B", "C", "D"
}
