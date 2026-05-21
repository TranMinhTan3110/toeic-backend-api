using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Helpers;

public static class EngagementRules
{
    public const int VocabReviewBaseEp      = 5;
    public const int VocabMasteredBonusEp   = 10;
    public const int VocabQuizEpPerCorrect  = 2;   // 2 EP / câu đúng (Quiz chọn từ/định nghĩa)
    public const int VocabMatchingEpPerPair = 2;   // 2 EP / cặp ghép đúng
    public const int VocabSentenceBaseEp    = 15;  // Đặt câu AI (mỗi lượt đặt câu)
    public const int DailyEpCap             = 500;
    public const double MaxStreakMultiplierBonus = 0.5;

    public static double GetStreakMultiplier(int streakDays)
    {
        if (streakDays <= 0) return 1.0;
        var bonus = Math.Min(streakDays * 0.02, MaxStreakMultiplierBonus);
        return 1.0 + bonus;
    }

    public static int CalculateVocabReviewBaseEp(bool newlyMastered)
    {
        var total = VocabReviewBaseEp;
        if (newlyMastered)
        {
            total += VocabMasteredBonusEp;
        }
        return total;
    }

    public static int ApplyStreakForStudyDay(User user, string todayDateKey)
    {
        if (string.IsNullOrEmpty(user.LastStudyDate))
        {
            return 1;
        }

        if (user.LastStudyDate == todayDateKey)
        {
            return Math.Max(1, user.StreakDays);
        }

        var gapDays = VietnamTimeHelper.GetDaysBetweenDateKeys(user.LastStudyDate, todayDateKey);
        if (gapDays == 1)
        {
            return Math.Max(1, user.StreakDays) + 1;
        }

        return 1;
    }
}
