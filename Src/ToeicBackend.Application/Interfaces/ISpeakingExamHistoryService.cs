using ToeicBackend.Application.DTOs;

namespace ToeicBackend.Application.Interfaces;

public interface ISpeakingExamHistoryService
{
    /// <summary>Chấm điểm batch toàn bộ bài thi → lưu history → trả về kết quả.</summary>
    Task<SpeakingExamHistoryDto> SubmitExamAsync(string userId, SaveSpeakingExamRequestDto request);
    Task<IEnumerable<SpeakingExamHistoryDto>> GetUserHistoryAsync(string userId);
    Task<SpeakingExamHistoryDto?> GetByIdAsync(string id);
}
