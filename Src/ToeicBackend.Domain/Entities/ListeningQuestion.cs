namespace ToeicBackend.Domain.Entities;

public class ListeningQuestion
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
    public string Skill { get; set; } = "listening";
    public DateTime? CreatedAt { get; set; }
    public bool IsForExam { get; set; } = false;      
    public bool IsForPractice { get; set; } = true;   
    public string? ExamId { get; set; }
}
