using ToeicBackend.Application.DTOs;

namespace ToeicBackend.Application.Interfaces;

public interface IWritingExamHistoryRepository
{
    Task<string> AddAsync(WritingExamHistoryDto history);
    Task<IEnumerable<WritingExamHistoryDto>> GetByUserIdAsync(string userId);
    Task<WritingExamHistoryDto?> GetByIdAsync(string id);
}
