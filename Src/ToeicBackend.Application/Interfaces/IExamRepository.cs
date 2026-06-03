using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Interfaces;

public interface IExamRepository
{
    Task<IEnumerable<ListeningQuestion>> GetExamQuestionsAsync(string examId);
    Task<IEnumerable<QuestionGroup>> GetExamGroupsAsync(string examId);
    Task<IEnumerable<Exam>> GetAllAsync();
    Task<Exam?> GetByIdAsync(string id);
    Task<IEnumerable<Exam>> GetByFilterAsync(bool? isExam, bool? isPractice);
    Task<string> AddFullTestHistoryAsync(FullTestHistory history);
    Task<IEnumerable<FullTestHistory>> GetFullTestHistoryByUserIdAsync(string userId);
    Task<FullTestHistory?> GetFullTestHistoryByIdAsync(string id);
}

