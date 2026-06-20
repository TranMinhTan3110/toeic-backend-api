using System.Collections.Generic;

namespace ToeicBackend.Application.DTOs;

public class WritingEvaluationDto
{
    public double OverallScore { get; set; }
    public bool Passed { get; set; }
    public Dictionary<string, double> CriteriaScores { get; set; } = new();
    public string Feedback { get; set; } = string.Empty;
    public string CorrectionsVi { get; set; } = string.Empty;
    public string SuggestedImprovement { get; set; } = string.Empty;
    public string UserAnswer { get; set; } = string.Empty;
}
