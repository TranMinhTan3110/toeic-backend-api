using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Interfaces;

public interface IGrammarRepository
{
    Task<IEnumerable<GrammarTopic>> GetTopicsAsync();
    Task<GrammarTopic?> GetTopicByIdAsync(string id);
    Task<GrammarLesson?> GetLessonByTopicIdAsync(string topicId);
    Task<IEnumerable<ListeningQuestion>> GetQuestionsByTopicIdAsync(string topicId);
}
