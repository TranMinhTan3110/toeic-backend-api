using ToeicBackend.Application.DTOs;

namespace ToeicBackend.Application.Interfaces;

public interface IExamService
{
    Task<IEnumerable<ListeningQuestionDto>> GetExamQuestionsAsync(string examId);
    Task<IEnumerable<ListeningGroupDto>> GetExamGroupsAsync(string examId);
}
