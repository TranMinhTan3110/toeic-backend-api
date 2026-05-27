using ToeicBackend.Application.DTOs;

namespace ToeicBackend.Application.Interfaces;

public interface ISpeakingHistoryService
{
    Task<string> SaveHistoryAsync(string userId, SaveSpeakingHistoryRequestDto request);
    Task<IEnumerable<SpeakingHistoryDto>> GetUserHistoryAsync(string userId, string? sessionType = null);
    Task<SpeakingHistoryDto?> GetHistoryByIdAsync(string id);
    Task<SpeakingEvaluationDto> EvaluateAsync(
        string questionId,
        string transcript,
        int? subQuestionIndex);
}
