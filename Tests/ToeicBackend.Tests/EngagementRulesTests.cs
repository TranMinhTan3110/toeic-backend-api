using ToeicBackend.Application.Helpers;
using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Tests;

public class EngagementRulesTests
{
    [Fact]
    public void ApplyStreak_FirstStudyDay_ReturnsOne()
    {
        var user = new User { StreakDays = 0, LastStudyDate = null };
        var streak = EngagementRules.ApplyStreakForStudyDay(user, "2026-05-20");
        Assert.Equal(1, streak);
    }

    [Fact]
    public void ApplyStreak_SameDay_KeepsStreak()
    {
        var user = new User { StreakDays = 5, LastStudyDate = "2026-05-20" };
        var streak = EngagementRules.ApplyStreakForStudyDay(user, "2026-05-20");
        Assert.Equal(5, streak);
    }

    [Fact]
    public void ApplyStreak_ConsecutiveDay_Increments()
    {
        var user = new User { StreakDays = 3, LastStudyDate = "2026-05-19" };
        var streak = EngagementRules.ApplyStreakForStudyDay(user, "2026-05-20");
        Assert.Equal(4, streak);
    }

    [Fact]
    public void ApplyStreak_GapMoreThanOneDay_ResetsToOne()
    {
        var user = new User { StreakDays = 10, LastStudyDate = "2026-05-17" };
        var streak = EngagementRules.ApplyStreakForStudyDay(user, "2026-05-20");
        Assert.Equal(1, streak);
    }

    [Fact]
    public void GetStreakMultiplier_CapsAtOnePointFive()
    {
        Assert.Equal(1.0, EngagementRules.GetStreakMultiplier(0));
        Assert.Equal(1.1, EngagementRules.GetStreakMultiplier(5));
        Assert.Equal(1.5, EngagementRules.GetStreakMultiplier(100));
    }

    [Fact]
    public void CalculateVocabReviewBaseEp_IncludesMasteredBonus()
    {
        Assert.Equal(5, EngagementRules.CalculateVocabReviewBaseEp(false));
        Assert.Equal(15, EngagementRules.CalculateVocabReviewBaseEp(true));
    }
}
