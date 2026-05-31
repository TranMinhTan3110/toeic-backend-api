namespace ToeicBackend.Application.DTOs;

public class UpdateUserProfileDto
{
    public int? TargetScore { get; set; }
    public string? CurrentLevel { get; set; }
    public List<string>? PreferredSkills { get; set; }
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Gender { get; set; }
    public string? BirthDate { get; set; }
}
