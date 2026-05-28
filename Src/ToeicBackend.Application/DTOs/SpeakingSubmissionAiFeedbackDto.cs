namespace ToeicBackend.Application.DTOs;

public class SpeakingSubmissionAiFeedbackDto
{
    public int Pronunciation { get; set; }
    public int Grammar { get; set; }
    public int Vocabulary { get; set; }
    public string FeedbackVi { get; set; } = string.Empty;
}
