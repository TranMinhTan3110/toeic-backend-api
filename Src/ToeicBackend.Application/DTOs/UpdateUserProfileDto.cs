using System.ComponentModel.DataAnnotations;

namespace ToeicBackend.Application.DTOs;

public class UpdateUserProfileDto
{
    [Range(0, 990)]
    public int? TargetScore { get; set; }

    [StringLength(50)]
    public string? CurrentLevel { get; set; }

    public List<string>? PreferredSkills { get; set; }

    [StringLength(100)]
    public string? DisplayName { get; set; }

    [StringLength(500)]
    public string? AvatarUrl { get; set; }

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [StringLength(20)]
    public string? Gender { get; set; }

    [StringLength(20)]
    public string? BirthDate { get; set; }
}
