using System;
using System.Collections.Generic;
using ToeicBackend.Application.DTOs.Reading;

namespace ToeicBackend.Application.DTOs;

public class ReadingHistoryDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public int Part { get; set; }
    public int CorrectCount { get; set; }
    public int TotalCount { get; set; }
    public double Percent { get; set; }
    public DateTime Date { get; set; }
    public List<string> IncorrectQuestionIds { get; set; } = new();
    public Dictionary<string, string> SelectedAnswers { get; set; } = new();
    // QuestionDetails keyed by question id. Filled on history detail requests.
    public Dictionary<string, Part5QuestionDto> QuestionDetails { get; set; } = new();
}

public class SaveReadingHistoryRequestDto
{
    public int Part { get; set; }
    public int CorrectCount { get; set; }
    public int TotalCount { get; set; }
    public double Percent { get; set; }
    public List<string> IncorrectQuestionIds { get; set; } = new();
    public Dictionary<string, string> SelectedAnswers { get; set; } = new();
}
