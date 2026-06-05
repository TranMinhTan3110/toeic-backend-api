namespace ToeicBackend.Application.DTOs;

public class WritingQuestionDto
{
    public string Id { get; set; } = string.Empty;
    public int TaskNumber { get; set; }
    public string TaskType { get; set; } = string.Empty; // write_sentence | respond_email | opinion_essay
    public string PromptText { get; set; } = string.Empty;
    public string? PromptImageUrl { get; set; }
    public List<string> GivenWords { get; set; } = new();
    public string? EmailContent { get; set; }
    public List<string> EmailQuestions { get; set; } = new();
    public int TimeLimit { get; set; }
    public int? MinWords { get; set; }
    public int? MaxWords { get; set; }
    public int MaxScore { get; set; }
    public List<string> ScoringCriteria { get; set; } = new();
    public string? SampleAnswer { get; set; }
    public string? SampleAnswerTranslation { get; set; }
    public string? ExplanationVietnamese { get; set; }
    public string? AiPrompt { get; set; }
    public string? Topic { get; set; }
    public string Difficulty { get; set; } = string.Empty;
    public string? ExamSetId { get; set; }
    public bool IsPractice { get; set; }
    public bool IsExam { get; set; }
}
