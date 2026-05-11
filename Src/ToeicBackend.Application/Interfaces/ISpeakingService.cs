using ToeicBackend.Application.DTOs;

namespace ToeicBackend.Application.Interfaces;

public interface ISpeakingService
{
    Task<IEnumerable<SpeakingQuestionDto>> GetAllAsync();
    Task<IEnumerable<SpeakingQuestionDto>> GetByTaskNumberAsync(int taskNumber);
    Task<SpeakingQuestionDto?> GetByIdAsync(string id);
}
