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

    /// <summary>Trả về số lượng câu/nhóm của 1 part — cực nhanh, dùng Firestore Count Aggregation.</summary>
    Task<int> GetCountByPartAsync(int part);

    // --- History Practice Methods ---
    Task<string> SaveHistoryAsync(string userId, SaveListeningHistoryRequestDto request);
    Task<IEnumerable<ListeningHistoryDto>> GetUserHistoryAsync(string userId);
    Task<ListeningHistoryDto?> GetHistoryByIdAsync(string id);
    Task<bool> DeleteQuestionAsync(string id);
}

