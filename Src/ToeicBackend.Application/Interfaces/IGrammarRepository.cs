using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Interfaces;

public interface IGrammarRepository
{
    Task<IEnumerable<GrammarTopic>> GetTopicsAsync();
    Task<GrammarTopic?> GetTopicByIdAsync(string id);
    Task<GrammarLesson?> GetLessonByTopicIdAsync(string topicId);
    Task<IEnumerable<ListeningQuestion>> GetQuestionsByTopicIdAsync(string topicId);
    Task<GrammarTopic> CreateTopicAsync(GrammarTopic topic);
    Task<GrammarTopic?> UpdateTopicAsync(string id, GrammarTopic topic);
    Task<bool> DeleteTopicAsync(string id);
    Task<GrammarLesson> SaveLessonAsync(GrammarLesson lesson);
    Task<ListeningQuestion> AddExerciseAsync(ListeningQuestion question);
    Task<ListeningQuestion?> UpdateExerciseAsync(string id, ListeningQuestion question);
    Task<bool> DeleteExerciseAsync(string id);
}
