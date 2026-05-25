using ToeicBackend.Application.DTOs;
using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Interfaces;

public interface IListeningService
{
    Task<IEnumerable<ListeningQuestionDto>> GetQuestionsByPartAsync(int part);
    Task<IEnumerable<ListeningGroupDto>> GetGroupsByPartAsync(int part);
    Task<IEnumerable<ListeningQuestionDto>> GetAllQuestionsAdminAsync();
    Task<string> AddQuestionAsync(ListeningQuestion question);
    Task<string> AddGroupAsync(QuestionGroup group);
}
