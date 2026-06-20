using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Interfaces;

public interface IEpEventRepository
{
    Task AddAsync(EpEvent epEvent);
    Task<int> SumEpForUserOnDateAsync(string userId, string studyDateKey);
    Task<bool> ExistsForReferenceOnDateAsync(string userId, string activityType, string referenceId, string studyDateKey);
    /// <summary>Kiểm tra lifetime (không phải daily) — dùng cho Exam để chặn spam vĩnh viễn.</summary>
    Task<bool> ExistsForReferenceEverAsync(string userId, string activityType, string referenceId);
}
