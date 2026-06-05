using System.Collections.Generic;

namespace ToeicBackend.Application.DTOs;

public class ReadingSubmitResultDto
{
    public int CorrectCount { get; set; }
    public int TotalQuestions { get; set; }
    public List<ReadingQuestionResultDto> Results { get; set; } = new();
}

public class ReadingQuestionResultDto
{
    public string QuestionId { get; set; } = string.Empty;
    public string? SelectedOption { get; set; }
    public string? CorrectAnswer { get; set; }
    public bool IsCorrect { get; set; }
}
