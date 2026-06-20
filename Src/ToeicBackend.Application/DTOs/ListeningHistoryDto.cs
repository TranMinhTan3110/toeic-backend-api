using System;
using System.Collections.Generic;

namespace ToeicBackend.Application.DTOs;

public class ListeningHistoryDto
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
}

public class SaveListeningHistoryRequestDto
{
    public int Part { get; set; }
    public int CorrectCount { get; set; }
    public int TotalCount { get; set; }
    public double Percent { get; set; }
    public List<string> IncorrectQuestionIds { get; set; } = new();
    public Dictionary<string, string> SelectedAnswers { get; set; } = new();
}
