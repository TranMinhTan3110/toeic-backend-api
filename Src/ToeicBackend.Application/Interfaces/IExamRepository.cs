using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Interfaces;

public interface IExamRepository
{
    Task<IEnumerable<Exam>> GetAllAsync();
    Task<Exam?> GetByIdAsync(string id);
    Task<IEnumerable<Exam>> GetByFilterAsync(bool? isExam, bool? isPractice);
}
