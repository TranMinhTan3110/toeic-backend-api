namespace ToeicBackend.Application.DTOs.Reading;

public class Part6SubmitRequestDto
{
    // map of questionId -> selected answer (A/B/C/D or raw text)
    public Dictionary<string, string> Answers { get; set; } = new();
}
