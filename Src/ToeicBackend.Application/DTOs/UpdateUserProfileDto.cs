namespace ToeicBackend.Application.DTOs;

public class UpdateUserProfileDto
{
    public int? TargetScore { get; set; }
    public string? CurrentLevel { get; set; }
    public List<string>? PreferredSkills { get; set; }
}
