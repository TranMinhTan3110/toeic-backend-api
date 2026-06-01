using System.Collections.Generic;
using System.Threading.Tasks;
using ToeicBackend.Application.DTOs;
using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Interfaces;

public interface IReadingService
{
    Task<IEnumerable<ReadingQuestionDto>> GetQuestionsByPartAsync(int part);
    Task<IEnumerable<ReadingQuestionDto>> GetAllQuestionsAdminAsync();
    Task<IEnumerable<ReadingGroupDto>> GetGroupsByPartAsync(int part);
    Task<string> AddQuestionAsync(ReadingQuestion question);
    Task<string> AddGroupAsync(QuestionGroup group);
    Task<int> GetCountByPartAsync(int part);
    Task<bool> DeleteQuestionAsync(string id);
    Task<bool> UpdateQuestionAsync(string id, ReadingQuestionDto dto);
}
