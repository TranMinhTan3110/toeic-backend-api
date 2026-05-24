using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Interfaces;

public interface ISpeakingRepository
{
    Task<IEnumerable<SpeakingQuestion>> GetAllAsync();
    Task<IEnumerable<SpeakingQuestion>> GetByTaskNumberAsync(int taskNumber);
    Task<SpeakingQuestion?> GetByIdAsync(string id);
}
