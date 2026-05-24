using ToeicBackend.Application.DTOs;

namespace ToeicBackend.Application.Interfaces;

public interface IListeningService
{
    Task<IEnumerable<ListeningQuestionDto>> GetQuestionsByPartAsync(int part);
    Task<IEnumerable<ListeningGroupDto>> GetGroupsByPartAsync(int part);
}
