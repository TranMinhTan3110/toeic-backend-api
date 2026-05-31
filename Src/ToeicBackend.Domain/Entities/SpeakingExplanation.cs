namespace ToeicBackend.Domain.Entities;

public class SpeakingExplanation
{
    public string? Translation { get; set; }
    public List<ExplanationKeyword> Keywords { get; set; } = new();
    public List<string> QuestionsTranslation { get; set; } = new();
    public List<string> SampleAnswers { get; set; } = new();
    public List<string> SampleAnswersTranslation { get; set; } = new();
    public string? ContextTranslation { get; set; }
}

public class ExplanationKeyword
{
    public string Word { get; set; } = string.Empty;
    public string Ipa { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
}
