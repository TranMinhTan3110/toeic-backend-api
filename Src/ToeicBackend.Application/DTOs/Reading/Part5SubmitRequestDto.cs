namespace ToeicBackend.Application.DTOs.Reading;

public class Part5SubmitRequestDto
{
    // question id -> selected option ("A"|"B"|"C"|"D")
    public Dictionary<string, string> Answers { get; set; } = new();
}
