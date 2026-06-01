using System;
using System.Collections.Generic;

namespace ToeicBackend.Domain.Entities;

public class ReadingQuestion
{
    public string Id { get; set; } = string.Empty;
    public int Part { get; set; }
    public string? QuestionText { get; set; }
    public string? ImageUrl { get; set; }
    public List<string> Options { get; set; } = new();
    public string CorrectAnswer { get; set; } = string.Empty;
    public object? Explanation { get; set; }
    public string? ExplanationVi { get; set; }
    public string? Script { get; set; }
    public string? GroupId { get; set; }
    public string Difficulty { get; set; } = "medium";
    public string Skill { get; set; } = "reading";
    public DateTime? CreatedAt { get; set; }
    public bool IsForExam { get; set; } = false;
    public bool IsForPractice { get; set; } = true;
    public string? ExamId { get; set; }
    public string? GrammarTopicId { get; set; }
}
