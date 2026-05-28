namespace ToeicBackend.Application.DTOs;

public class EvaluateWritingRequestDto
{
    public string QuestionId { get; set; } = string.Empty;
    public string UserAnswer { get; set; } = string.Empty;
}
