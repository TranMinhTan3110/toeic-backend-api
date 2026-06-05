using System;
using System.Collections.Generic;

namespace ToeicBackend.Application.DTOs;

public class FullTestHistoryDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string ExamId { get; set; } = string.Empty;
    public string ExamTitle { get; set; } = string.Empty;
    public int ScoreListening { get; set; }
    public int ScoreReading { get; set; }
    public int TotalScore { get; set; }
    public int CorrectCount { get; set; }
    public int TotalCount { get; set; }
    public int TimeSpent { get; set; }
    public DateTime CompletedAt { get; set; }
    public Dictionary<string, string> Answers { get; set; } = new();
    public Dictionary<string, int> PartScores { get; set; } = new();
}

public class SaveFullTestRequestDto
{
    public string ExamId { get; set; } = string.Empty;
    public string ExamTitle { get; set; } = string.Empty;
    public int ScoreListening { get; set; }
    public int ScoreReading { get; set; }
    public int TotalScore { get; set; }
    public int CorrectCount { get; set; }
    public int TotalCount { get; set; }
    public int TimeSpent { get; set; }
    public Dictionary<string, string> Answers { get; set; } = new();
    public Dictionary<string, int> PartScores { get; set; } = new();
}
