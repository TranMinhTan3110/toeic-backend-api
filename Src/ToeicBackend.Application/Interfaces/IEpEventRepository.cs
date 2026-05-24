using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Interfaces;

public interface IEpEventRepository
{
    Task AddAsync(EpEvent epEvent);
    Task<int> SumEpForUserOnDateAsync(string userId, string studyDateKey);
    Task<bool> ExistsForReferenceOnDateAsync(string userId, string activityType, string referenceId, string studyDateKey);
}
