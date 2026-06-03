namespace ToeicBackend.Domain.Entities.Reading;

public class Part5Question
{
    public string Id { get; set; } = string.Empty;
    public string QuestionText { get; set; } = string.Empty;
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty; // "A"|"B"|"C"|"D"
    public string? Explanation { get; set; }
    // Vietnamese translation / explanation (lời dịch, lời giải)
    public string? ExplanationVi { get; set; }
    // Optional structured explanation fields (when explanation stored as map in Firestore)
    public string? Translation { get; set; }
    public string? GrammarExplanation { get; set; }
    public string? GrammarPoint { get; set; }
    public Dictionary<string, string>? OptionExplanations { get; set; }
    // Flag whether this question should be shown in practice flows
    public bool IsForPractice { get; set; } = true;
}
