namespace ToeicBackend.Application.DTOs.Reading;

public class Part6SubmitResponseDto
{
    public int TotalQuestions { get; set; }
    public int CorrectCount { get; set; }
    public double Percentage => TotalQuestions == 0 ? 0 : (double)CorrectCount / TotalQuestions * 100.0;
    public List<Part6QuestionResultDto> Results { get; set; } = new();
}
