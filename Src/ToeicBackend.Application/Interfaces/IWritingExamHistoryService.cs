using ToeicBackend.Application.DTOs;

namespace ToeicBackend.Application.Interfaces;

public interface IWritingExamHistoryService
{
    /// <summary>Chấm điểm batch toàn bộ bài thi Writing → lưu history → trả về kết quả.</summary>
    Task<WritingExamHistoryDto> SubmitExamAsync(string userId, SaveWritingExamRequestDto request);
    Task<IEnumerable<WritingExamHistoryDto>> GetUserHistoryAsync(string userId);
    Task<WritingExamHistoryDto?> GetByIdAsync(string id);
}
