using ToeicBackend.Application.DTOs;

namespace ToeicBackend.Application.Interfaces;

public interface IGrammarService
{
    Task<IEnumerable<GrammarTopicDto>> GetTopicsAsync();
    Task<GrammarLessonDto?> GetLessonByTopicIdAsync(string topicId);
    Task<IEnumerable<ListeningQuestionDto>> GetExercisesByTopicIdAsync(string topicId);
}
