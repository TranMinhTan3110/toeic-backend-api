namespace ToeicBackend.Application.DTOs;

public class UserProfileDto
{
    public string Uid { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public int TargetScore { get; set; }
    public string CurrentLevel { get; set; } = string.Empty;
    public string Plan { get; set; } = string.Empty;
    public List<string> PreferredSkills { get; set; } = new();
    public int ExperiencePoints { get; set; }
    public int WeeklyEp { get; set; }
    public string WeeklyEpPeriodKey { get; set; } = string.Empty;
    public int StreakDays { get; set; }
    public int BestStreakDays { get; set; }
    public int TotalStudyMinutes { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsLocked { get; set; }
    public string Role { get; set; } = "user";
}


