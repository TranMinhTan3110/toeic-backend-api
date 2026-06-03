using System.Collections.Generic;
using System.Threading.Tasks;
using ToeicBackend.Domain.Entities;
using ToeicBackend.Domain.Entities.Reading;

namespace ToeicBackend.Application.Interfaces;

public interface IReadingRepository
{
    Task<IEnumerable<ReadingQuestion>> GetQuestionsByPartAsync(int part);
    Task<ReadingQuestion?> GetQuestionByIdAsync(string id);
    Task<IEnumerable<ReadingQuestion>> GetAllQuestionsAdminAsync();
    Task<IEnumerable<QuestionGroup>> GetGroupsByPartAsync(int part);
    Task<int> GetQuestionCountByPartAsync(int part);
    Task<int> GetGroupCountByPartAsync(int part);
    Task<QuestionGroup?> GetGroupByIdAsync(string groupId);
    Task<IEnumerable<ReadingQuestion>> GetQuestionsByIdsAsync(List<string> ids);
    Task<string> AddQuestionAsync(ReadingQuestion question);
    Task<string> AddGroupAsync(QuestionGroup group);
    Task<bool> DeleteQuestionAsync(string id);
    Task<bool> UpdateQuestionAsync(ReadingQuestion question);
    Task<bool> UpdateGroupAsync(QuestionGroup group);

    Task<IEnumerable<Part5Question>> GetRandomPart5QuestionsAsync(int count);
    Task<IEnumerable<Part5Question>> GetPart5QuestionsByIdsAsync(IEnumerable<string> ids);
    Task<IEnumerable<Part5Question>> GetQuestionsAsPart5ByIdsAsync(IEnumerable<string> ids);
    Task<IEnumerable<Part6Passage>> GetPart6PassagesAsync();
    Task<IEnumerable<Part6Passage>> GetPart7PassagesAsync();
    // History practice for Reading
    Task<string> AddReadingHistoryAsync(ReadingHistory history);
    Task<IEnumerable<ReadingHistory>> GetReadingHistoryByUserIdAsync(string userId);
    Task<ReadingHistory?> GetReadingHistoryByIdAsync(string id);
}
