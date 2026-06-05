namespace ToeicBackend.Application.DTOs;

public class ExamDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Difficulty { get; set; } = "medium";
    public int Duration { get; set; }
    public int Year { get; set; }
    public string? ImageUrl { get; set; }
    public string? AudioUrl { get; set; }
    public bool IsExam { get; set; }
    public bool IsPractice { get; set; }
    public bool IsPremium { get; set; }
    public bool IsPublished { get; set; }
    public string ExamType { get; set; } = "full";
    public List<string> QuestionIds { get; set; } = new();
    public int Attempts { get; set; }
}
