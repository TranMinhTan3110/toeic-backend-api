using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Services;

public class ExamService : IExamService
{
    private readonly IExamRepository _repository;

    public ExamService(IExamRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ExamDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto);
    }

    public async Task<ExamDto?> GetByIdAsync(string id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<IEnumerable<ExamDto>> GetByFilterAsync(bool? isExam, bool? isPractice)
    {
        var entities = await _repository.GetByFilterAsync(isExam, isPractice);
        return entities.Select(MapToDto);
    }

    private ExamDto MapToDto(Exam entity)
    {
        return new ExamDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            Difficulty = entity.Difficulty,
            Duration = entity.Duration,
            Year = entity.Year,
            ImageUrl = entity.ImageUrl,
            AudioUrl = entity.AudioUrl,
            IsExam = entity.IsExam,
            IsPractice = entity.IsPractice,
            IsPremium = entity.IsPremium,
            IsPublished = entity.IsPublished,
            QuestionIds = entity.QuestionIds
        };
    }
}
