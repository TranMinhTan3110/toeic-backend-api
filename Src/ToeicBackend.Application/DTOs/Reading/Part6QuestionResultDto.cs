namespace ToeicBackend.Application.DTOs.Reading;

public class Part6QuestionResultDto
{
    public string QuestionId { get; set; } = string.Empty;
    public string? SelectedAnswer { get; set; }
    public bool IsCorrect { get; set; }
    public string? CorrectAnswer { get; set; }
    public string? Explanation { get; set; }
    public string? ExplanationVi { get; set; }
    public string? Translation { get; set; }
    public string? GrammarExplanation { get; set; }
    public string? GrammarPoint { get; set; }
    public Dictionary<string, string>? OptionExplanations { get; set; }
}
