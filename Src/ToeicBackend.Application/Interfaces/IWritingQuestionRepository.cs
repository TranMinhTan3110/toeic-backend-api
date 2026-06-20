using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Interfaces;

public interface IWritingQuestionRepository
{
    Task<IEnumerable<WritingQuestion>> GetAllAsync();
    Task<WritingQuestion?> GetByIdAsync(string id);
    Task<IEnumerable<WritingQuestion>> GetByTaskTypeAsync(string taskType);
    Task<IEnumerable<WritingQuestion>> GetByTaskNumberAsync(int taskNumber);
    Task<IEnumerable<WritingQuestion>> GetByDifficultyAsync(string difficulty);
    Task<IEnumerable<WritingQuestion>> GetPracticeQuestionsAsync();
    Task<IEnumerable<WritingQuestion>> GetPracticeByTaskTypeAsync(string taskType);
    Task<IEnumerable<WritingQuestion>> GetExamByTaskTypeAsync(string taskType);
    Task<IEnumerable<string>> GetAvailableTaskTypesAsync();
    Task<IEnumerable<WritingQuestion>> GetQuestionsByExamSetIdAsync(string examSetId);
    Task<Dictionary<string, int>> GetPracticeCountsByTaskTypeAsync();
    Task<string> AddAsync(WritingQuestion question);
    Task<bool> UpdateAsync(string id, WritingQuestion question);
    Task<bool> DeleteAsync(string id);
}
