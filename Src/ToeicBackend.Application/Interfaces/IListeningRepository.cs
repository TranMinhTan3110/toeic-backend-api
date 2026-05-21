using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Interfaces;

public interface IListeningRepository
{
    Task<IEnumerable<ListeningQuestion>> GetQuestionsByPartAsync(int part);
    Task<ListeningQuestion?> GetQuestionByIdAsync(string id);
    Task<IEnumerable<QuestionGroup>> GetGroupsByPartAsync(int part);
    Task<QuestionGroup?> GetGroupByIdAsync(string groupId);
    Task<IEnumerable<ListeningQuestion>> GetQuestionsByIdsAsync(List<string> ids);
}
