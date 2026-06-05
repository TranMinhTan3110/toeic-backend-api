namespace ToeicBackend.Application.DTOs;

public class UpdateExamDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Difficulty { get; set; }
    public int? Duration { get; set; }
    public int? Year { get; set; }
    public string? ImageUrl { get; set; }
    public string? AudioUrl { get; set; }
    public bool? IsExam { get; set; }
    public bool? IsPractice { get; set; }
    public bool? IsPremium { get; set; }
    public bool? IsPublished { get; set; }
    public string? ExamType { get; set; }
}
