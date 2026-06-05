using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Interfaces;

public interface ISpeakingRepository
{
    Task<IEnumerable<SpeakingQuestion>> GetAllAsync();
    Task<IEnumerable<SpeakingQuestion>> GetByTaskNumberAsync(int taskNumber, bool? isExam = null, bool? isPractice = null);
    Task<SpeakingQuestion?> GetByIdAsync(string id);
    Task<IEnumerable<SpeakingQuestion>> GetByFilterAsync(bool? isExam, bool? isPractice);
    Task<int> GetCountByFilterAsync(bool? isExam, bool? isPractice);
    Task<IEnumerable<SpeakingQuestion>> GetByExamSetIdAsync(string examSetId);
    Task AddAsync(SpeakingQuestion entity);
    Task<bool> UpdateAsync(string id, SpeakingQuestion entity);
    Task<bool> DeleteAsync(string id);
}
