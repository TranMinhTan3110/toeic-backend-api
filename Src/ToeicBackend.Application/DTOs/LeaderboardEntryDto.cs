namespace ToeicBackend.Application.DTOs;

public class LeaderboardEntryDto
{
    public int Rank { get; set; }
    public string Uid { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public int WeeklyEp { get; set; }
    public int StreakDays { get; set; }
}
