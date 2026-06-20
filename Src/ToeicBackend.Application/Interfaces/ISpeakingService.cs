using ToeicBackend.Application.DTOs;

namespace ToeicBackend.Application.Interfaces;

public interface ISpeakingService
{
    Task<IEnumerable<SpeakingQuestionDto>> GetAllAsync();
    Task<IEnumerable<SpeakingQuestionDto>> GetByTaskNumberAsync(int taskNumber, bool? isExam = null, bool? isPractice = null);
    Task<SpeakingQuestionDto?> GetByIdAsync(string id);
    Task<IEnumerable<SpeakingQuestionDto>> GetByFilterAsync(bool? isExam, bool? isPractice);
    Task<int> GetCountByFilterAsync(bool? isExam, bool? isPractice);
    Task<IEnumerable<SpeakingQuestionDto>> GetByExamSetIdAsync(string examSetId);
    Task AddQuestionAsync(ToeicBackend.Domain.Entities.SpeakingQuestion entity);
    Task<bool> UpdateQuestionAsync(string id, SpeakingQuestionDto dto);
    Task<bool> DeleteQuestionAsync(string id);
}
