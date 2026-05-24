using ToeicBackend.Application.DTOs;

namespace ToeicBackend.Application.Interfaces;

public interface IExamService
{
    Task<IEnumerable<ExamDto>> GetAllAsync();
    Task<ExamDto?> GetByIdAsync(string id);
    Task<IEnumerable<ExamDto>> GetByFilterAsync(bool? isExam, bool? isPractice);
}
