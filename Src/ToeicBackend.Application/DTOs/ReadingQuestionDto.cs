using System.Collections.Generic;

namespace ToeicBackend.Application.DTOs;

public class ReadingQuestionDto
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
    public bool IsForExam { get; set; }
    public bool IsForPractice { get; set; } = true;
    public string? GrammarTopicId { get; set; }
}
