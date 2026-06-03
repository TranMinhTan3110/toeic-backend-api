using System;
using System.Collections.Generic;

namespace ToeicBackend.Application.DTOs.Reading;

public class Part7HistoryDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public int CorrectCount { get; set; }
    public int TotalCount { get; set; }
    public double Percentage { get; set; }
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, string> SelectedAnswers { get; set; } = new();
    public List<string> IncorrectQuestionIds { get; set; } = new();
}
