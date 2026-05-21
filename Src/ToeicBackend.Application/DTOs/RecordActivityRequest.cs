using ToeicBackend.Application.Enums;

namespace ToeicBackend.Application.DTOs;

public class RecordActivityRequest
{
    public ActivityType ActivityType { get; set; }
    public string? ReferenceId { get; set; }
    public bool NewlyMastered { get; set; }
    /// <summary>Số câu đúng (cho Quiz/Matching để tính bonus EP)</summary>
    public int CorrectAnswers { get; set; }
    /// <summary>Tổng số câu (cho Quiz/Matching)</summary>
    public int TotalAnswers { get; set; }
}
