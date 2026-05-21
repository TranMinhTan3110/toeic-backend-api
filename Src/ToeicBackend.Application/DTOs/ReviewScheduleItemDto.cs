namespace ToeicBackend.Application.DTOs;

public class ReviewScheduleItemDto
{
    public string VocabularyId { get; set; } = string.Empty;
    public string Word { get; set; } = string.Empty;
    public string DefinitionVi { get; set; } = string.Empty;
    public string WordType { get; set; } = string.Empty;
    public int Repetitions { get; set; }        // Số lần đúng liên tiếp
    public int MasteryLevel { get; set; }       // 0-100
    public bool IsMastered { get; set; }
    public DateTime? LastReviewedAt { get; set; }
    public DateTime NextReviewDate { get; set; }
    public bool IsDue { get; set; }             // Đã đến hạn chưa
    public int DaysUntilDue { get; set; }       // Số ngày còn lại (âm = quá hạn)
}
