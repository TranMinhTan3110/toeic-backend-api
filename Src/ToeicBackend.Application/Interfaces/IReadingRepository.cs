using ToeicBackend.Domain.Entities.Reading;

namespace ToeicBackend.Application.Interfaces;

public interface IReadingRepository
{
    Task<IEnumerable<Part5Question>> GetRandomPart5QuestionsAsync(int count);
    Task<IEnumerable<Part5Question>> GetPart5QuestionsByIdsAsync(IEnumerable<string> ids);
    Task<IEnumerable<Part5Question>> GetQuestionsByIdsAsync(IEnumerable<string> ids);
    Task<IEnumerable<ToeicBackend.Domain.Entities.Reading.Part6Passage>> GetPart6PassagesAsync();
    Task<IEnumerable<ToeicBackend.Domain.Entities.Reading.Part6Passage>> GetPart7PassagesAsync();
}
