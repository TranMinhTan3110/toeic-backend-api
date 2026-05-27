using ToeicBackend.Application.DTOs;

namespace ToeicBackend.Application.Interfaces;

public interface IGrammarService
{
    Task<IEnumerable<GrammarTopicDto>> GetTopicsAsync();
    Task<GrammarLessonDto?> GetLessonByTopicIdAsync(string topicId);
    Task<IEnumerable<ListeningQuestionDto>> GetExercisesByTopicIdAsync(string topicId, bool random = false, int limit = 0);
    Task<GrammarTopicDto> CreateTopicAsync(CreateGrammarTopicDto dto);
    Task<GrammarTopicDto?> UpdateTopicAsync(string id, CreateGrammarTopicDto dto);
    Task<bool> DeleteTopicAsync(string id);
    Task<GrammarLessonDto> SaveLessonAsync(string topicId, CreateGrammarLessonDto dto);
    Task<ListeningQuestionDto> AddExerciseAsync(string topicId, ListeningQuestionDto dto);
    Task<ListeningQuestionDto?> UpdateExerciseAsync(string id, ListeningQuestionDto dto);
    Task<bool> DeleteExerciseAsync(string id);
}
