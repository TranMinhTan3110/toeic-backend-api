using ToeicBackend.Application.Interfaces;
using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Infrastructure.Services;

public class SpacedRepetitionService : ISpacedRepetitionService
{
    public UserVocabularyProgress CalculateNextReview(UserVocabularyProgress progress, int quality)
    {
        // Thuật toán SM-2 (SuperMemo-2)
        
        // 1. Cập nhật Easiness Factor (EF)
        // EF = EF + (0.1 - (5 - quality) * (0.08 + (5 - quality) * 0.02))
        progress.EasinessFactor = progress.EasinessFactor + (0.1 - (5 - quality) * (0.08 + (5 - quality) * 0.02));
        
        if (progress.EasinessFactor < 1.3)
        {
            progress.EasinessFactor = 1.3;
        }

        // 2. Cập nhật Repetitions và Interval
        if (quality >= 3) // Trả lời đúng (Từ "Đạt" trở lên)
        {
            if (progress.Repetitions == 0)
            {
                progress.Interval = 1;
            }
            else if (progress.Repetitions == 1)
            {
                progress.Interval = 6;
            }
            else
            {
                progress.Interval = Math.Round(progress.Interval * progress.EasinessFactor);
            }
            
            progress.Repetitions++;
        }
        else // Trả lời sai (Quên từ)
        {
            progress.Repetitions = 0;
            progress.Interval = 1;
        }

        // 3. Cập nhật các thông tin khác
        progress.LastReviewedAt = DateTime.UtcNow;
        progress.NextReviewDate = DateTime.UtcNow.AddDays(progress.Interval);
        
        // Tính Mastery Level (Mức độ tinh thông)
        // Giả sử sau 5 lần đúng liên tiếp là 100%
        progress.MasteryLevel = Math.Min(100, progress.Repetitions * 20);
        progress.IsMastered = progress.MasteryLevel >= 100;

        return progress;
    }
}
