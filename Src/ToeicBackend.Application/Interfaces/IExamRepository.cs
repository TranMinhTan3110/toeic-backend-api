using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Interfaces;

public interface IExamRepository
{
    Task<IEnumerable<ListeningQuestion>> GetExamQuestionsAsync(string examId);
    Task<IEnumerable<QuestionGroup>> GetExamGroupsAsync(string examId);
}
