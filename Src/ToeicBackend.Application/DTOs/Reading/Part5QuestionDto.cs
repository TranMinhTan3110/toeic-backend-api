namespace ToeicBackend.Application.DTOs.Reading;

public class Part5QuestionDto
{
    public string Id { get; set; } = string.Empty;
    public string QuestionText { get; set; } = string.Empty;
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;
    public string? CorrectAnswer { get; set; }
    public string? Explanation { get; set; }
    public string? ExplanationVi { get; set; }
    // Structured explanation fields
    public string? Translation { get; set; }
    public string? GrammarExplanation { get; set; }
    public string? GrammarPoint { get; set; }
    public Dictionary<string, string>? OptionExplanations { get; set; }
}
