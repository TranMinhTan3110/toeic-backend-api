using System.ComponentModel.DataAnnotations;

namespace ToeicBackend.Application.DTOs;

public class SaveExamDto
{
    [Required]
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Difficulty { get; set; } = "medium";
    public int Duration { get; set; } = 120;
    public int Year { get; set; } = DateTime.UtcNow.Year;
    public string? ImageUrl { get; set; }
    public string? AudioUrl { get; set; }
    public bool IsExam { get; set; } = true;
    public bool IsPractice { get; set; } = false;
    public bool IsPremium { get; set; } = false;
    public bool IsPublished { get; set; } = true;
    public string ExamType { get; set; } = "full";

    public List<CreateExamQuestionDto> Questions { get; set; } = new();
    public List<CreateExamGroupDto> QuestionGroups { get; set; } = new();
}

public class CreateExamQuestionDto
{
    [Required]
    public string Id { get; set; } = string.Empty;
    [Required]
    public int Part { get; set; }
    public string? QuestionText { get; set; }
    public string? ImageUrl { get; set; }
    public string? AudioUrl { get; set; }
    public List<string> Options { get; set; } = new();
    [Required]
    public string CorrectAnswer { get; set; } = string.Empty;
    public object? Explanation { get; set; }
    public string? ExplanationVi { get; set; }
    public string? Script { get; set; }
    public string? GroupId { get; set; }
    public string Difficulty { get; set; } = "medium";
    public string Skill { get; set; } = "listening";
}

public class CreateExamGroupDto
{
    [Required]
    public string Id { get; set; } = string.Empty;
    [Required]
    public int Part { get; set; }
    public string? PassageText { get; set; }
    public string? Script { get; set; }
    public string? ImageUrl { get; set; }
    public string? AudioUrl { get; set; }
    public List<string> QuestionIds { get; set; } = new();
    public int QuestionCount { get; set; }
    public string? Source { get; set; }
}
