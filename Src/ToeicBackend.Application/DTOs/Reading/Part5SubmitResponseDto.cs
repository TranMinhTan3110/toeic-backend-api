namespace ToeicBackend.Application.DTOs.Reading;

public class Part5SubmitResponseDto
{
    public int TotalQuestions { get; set; }
    public int CorrectCount { get; set; }
    public List<Part5QuestionResultDto> Results { get; set; } = new();
}

public class Part5QuestionResultDto
{
    public string QuestionId { get; set; } = string.Empty;
    public string SelectedOption { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public string? Explanation { get; set; }
    public string? CorrectAnswer { get; set; }
}
