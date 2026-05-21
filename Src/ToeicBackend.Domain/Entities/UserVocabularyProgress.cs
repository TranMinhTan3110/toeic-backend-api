using Google.Cloud.Firestore;

namespace ToeicBackend.Domain.Entities;

[FirestoreData]
public class UserVocabularyProgress
{
    [FirestoreDocumentId]
    public string Id { get; set; } = string.Empty; // Firebase Document ID

    [FirestoreProperty]
    public string UserId { get; set; } = string.Empty;

    [FirestoreProperty]
    public string VocabularyId { get; set; } = string.Empty;

    [FirestoreProperty]
    public int Repetitions { get; set; } = 0; // Số lần trả lời đúng liên tiếp

    [FirestoreProperty]
    public double Interval { get; set; } = 0; // Khoảng cách ôn tập tính bằng ngày

    [FirestoreProperty]
    public double EasinessFactor { get; set; } = 2.5; // Chỉ số độ dễ (Thuật toán SM-2)

    [FirestoreProperty]
    public DateTime? LastReviewedAt { get; set; }

    [FirestoreProperty]
    public DateTime NextReviewDate { get; set; } = DateTime.UtcNow;

    [FirestoreProperty]
    public int MasteryLevel { get; set; } = 0; // Mức độ tinh thông (0 - 100)

    [FirestoreProperty]
    public bool IsMastered { get; set; } = false;

    [FirestoreProperty]
    public bool IsStarred { get; set; } = false;
}

