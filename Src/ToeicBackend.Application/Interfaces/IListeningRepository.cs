using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Interfaces;

public interface IListeningRepository
{
    Task<IEnumerable<ListeningQuestion>> GetQuestionsByPartAsync(int part);
    Task<ListeningQuestion?> GetQuestionByIdAsync(string id);
    Task<IEnumerable<QuestionGroup>> GetGroupsByPartAsync(int part);
    Task<QuestionGroup?> GetGroupByIdAsync(string groupId);
    Task<IEnumerable<ListeningQuestion>> GetQuestionsByIdsAsync(List<string> ids);
    Task<IEnumerable<ListeningQuestion>> GetAllQuestionsAdminAsync();
    Task<string> AddQuestionAsync(ListeningQuestion question);
    Task<string> AddGroupAsync(QuestionGroup group);

    /// <summary>Đếm số câu hỏi Part 1/2 bằng Firestore Count Aggregation — chỉ tốn 1 read.</summary>
    Task<int> GetQuestionCountByPartAsync(int part);

    /// <summary>Đếm số nhóm Part 3/4 bằng Firestore Count Aggregation — chỉ tốn 1 read.</summary>
    Task<int> GetGroupCountByPartAsync(int part);

    // --- History Practice Methods ---
    Task<string> AddHistoryAsync(ListeningHistory history);
    Task<IEnumerable<ListeningHistory>> GetHistoryByUserIdAsync(string userId);
    Task<ListeningHistory?> GetHistoryByIdAsync(string id);
    Task<bool> DeleteQuestionAsync(string id);
    Task<bool> UpdateQuestionAsync(ListeningQuestion question);
    Task<bool> UpdateGroupAsync(QuestionGroup group);
}

