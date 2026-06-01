using ToeicBackend.Application.DTOs;

namespace ToeicBackend.Application.Interfaces;

public interface IWritingQuestionService
{
    Task<IEnumerable<WritingQuestionDto>> GetAllAsync();
    Task<WritingQuestionDto?> GetByIdAsync(string id);
    Task<IEnumerable<WritingQuestionDto>> GetByTaskTypeAsync(string taskType);
    Task<IEnumerable<WritingQuestionDto>> GetByTaskNumberAsync(int taskNumber);
    Task<IEnumerable<WritingQuestionDto>> GetByDifficultyAsync(string difficulty);
    Task<IEnumerable<WritingQuestionDto>> GetPracticeQuestionsAsync();
    Task<IEnumerable<WritingQuestionDto>> GetPracticeByTaskTypeAsync(string taskType);
    Task<IEnumerable<WritingQuestionDto>> GetExamByTaskTypeAsync(string taskType);
    Task<IEnumerable<string>> GetAvailableTaskTypesAsync();
    Task<IEnumerable<WritingQuestionDto>> GetQuestionsByExamSetIdAsync(string examSetId);
    Task<Dictionary<string, int>> GetPracticeCountsByTaskTypeAsync();
    Task<string> AddAsync(WritingQuestionDto dto);
    Task<bool> UpdateAsync(string id, WritingQuestionDto dto);
    Task<bool> DeleteAsync(string id);
}
