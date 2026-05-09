namespace ToeicBackend.Domain.Entities;

public class User
{
    public string Uid { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public int TargetScore { get; set; }
    public string CurrentLevel { get; set; } = "beginner";
    public string Plan { get; set; } = "free";
    public int StreakDays { get; set; }
    public int ExperiencePoints { get; set; }
    public DateTime? LastStudyDate { get; set; }
    public int TotalStudyMinutes { get; set; }
    public List<string> PreferredSkills { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
