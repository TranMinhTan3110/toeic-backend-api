namespace ToeicBackend.Application.DTOs;

public class WritingHistoryAiFeedbackDto
{
    public int? GrammarScore { get; set; }
    public int? VocabularyScore { get; set; }
    public int? CohesionScore { get; set; }
    public string? CorrectionsVi { get; set; }
    public string? SuggestedImprovement { get; set; }
}
