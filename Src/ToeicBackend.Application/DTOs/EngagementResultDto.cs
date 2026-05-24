namespace ToeicBackend.Application.DTOs;

public class EngagementResultDto
{
    public int EpAwarded { get; set; }
    public int TotalExperiencePoints { get; set; }
    public int WeeklyEp { get; set; }
    public int StreakDays { get; set; }
    public int BestStreakDays { get; set; }
    public double StreakMultiplier { get; set; }
    public bool DailyCapReached { get; set; }
    public bool AlreadyAwardedForReference { get; set; }
}
