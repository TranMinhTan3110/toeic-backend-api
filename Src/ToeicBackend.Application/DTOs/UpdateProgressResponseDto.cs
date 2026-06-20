using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.DTOs;

public class UpdateProgressResponseDto
{
    public UserVocabularyProgress Progress { get; set; } = null!;
    public EngagementResultDto? Engagement { get; set; }
}
