namespace ToeicBackend.Application.DTOs;

public class ListeningQuestionDto
{
    public string Id { get; set; } = string.Empty;
    public int Part { get; set; }
    public string? QuestionText { get; set; }
    public string? ImageUrl { get; set; }
    public string? AudioUrl { get; set; }
    public List<string> Options { get; set; } = new();
    public string CorrectAnswer { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public string? ExplanationVi { get; set; }
    public string? Script { get; set; }
    public string? GroupId { get; set; }
    public string Difficulty { get; set; } = "medium";
    public bool IsForExam { get; set; }
    public bool IsForPractice { get; set; }
}
