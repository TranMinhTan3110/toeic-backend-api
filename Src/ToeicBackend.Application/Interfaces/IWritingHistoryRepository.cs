using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Interfaces;

public interface IWritingHistoryRepository
{
    Task<string> AddAsync(WritingHistory history);
    Task<IEnumerable<WritingHistory>> GetByUserIdAsync(string userId, string? sessionType = null);
    Task<WritingHistory?> GetByIdAsync(string id);
}
