using ToeicBackend.Application.DTOs;

namespace ToeicBackend.Application.Interfaces;

public interface IEngagementService
{
    Task<EngagementResultDto?> RecordActivityAsync(string userId, RecordActivityRequest request);
}
