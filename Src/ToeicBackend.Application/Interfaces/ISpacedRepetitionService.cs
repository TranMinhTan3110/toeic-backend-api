using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Interfaces;

public interface ISpacedRepetitionService
{
    /// <summary>
    /// Cập nhật tiến độ học tập dựa trên kết quả ôn tập của người dùng
    /// </summary>
    /// <param name="progress">Tiến độ hiện tại</param>
    /// <param name="quality">Đánh giá chất lượng (0-5). 0: Quên hoàn toàn, 5: Nhớ cực tốt</param>
    /// <returns>Tiến độ đã được cập nhật</returns>
    UserVocabularyProgress CalculateNextReview(UserVocabularyProgress progress, int quality);
}
