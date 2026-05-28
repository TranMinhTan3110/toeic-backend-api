using ToeicBackend.Application.DTOs;

namespace ToeicBackend.Application.Interfaces;

public interface IWritingHistoryService
{
    Task<string> SaveHistoryAsync(string userId, SaveWritingHistoryRequestDto request);
    Task<string> SaveSessionAsync(string userId, SaveWritingSessionRequestDto request);
    Task<IEnumerable<WritingHistoryDto>> GetUserHistoryAsync(string userId, string? sessionType = null);
    Task<WritingHistoryDto?> GetHistoryByIdAsync(string id);
    Task<WritingEvaluationDto> EvaluateAsync(string questionId, string userAnswer);
}
