using ToeicBackend.Application.DTOs;

namespace ToeicBackend.Application.Interfaces;

public interface IExamService
{
    Task<IEnumerable<ListeningQuestionDto>> GetExamQuestionsAsync(string examId);
    Task<IEnumerable<ListeningGroupDto>> GetExamGroupsAsync(string examId);
    Task<IEnumerable<ExamDto>> GetAllAsync();
    Task<ExamDto?> GetByIdAsync(string id);
    Task<IEnumerable<ExamDto>> GetByFilterAsync(bool? isExam, bool? isPractice);
}
