using System.Collections.Generic;
using System.Threading.Tasks;
using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.DTOs.Reading;
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

    Task<IEnumerable<Part5QuestionDto>> GetPart5QuestionsAsync(int? count = null);
    Task<IEnumerable<Part6PassageDto>> GetPart6PassagesAsync();
    Task<IEnumerable<Part7PassageDto>> GetPart7PassagesAsync();
    Task<Part5SubmitResponseDto> SubmitPart5AnswersAsync(Part5SubmitRequestDto request);
    Task<Part6SubmitResponseDto> SubmitPart6AnswersAsync(Part6SubmitRequestDto request);
    Task<Part7SubmitResponseDto> SubmitPart7AnswersAsync(Part7SubmitRequestDto request);
    // convenience: fetch passages for part6 (alias)
    Task<IEnumerable<Part6PassageDto>> GetPart6QuestionsAsync();
    Task<IEnumerable<Part7PassageDto>> GetPart7QuestionsAsync();
    // History practice for Reading
    Task<string> SaveHistoryAsync(string userId, SaveReadingHistoryRequestDto request);
    Task<string> SavePart6HistoryAsync(string userId, SaveReadingHistoryRequestDto request);
    Task<string> SavePart7HistoryAsync(string userId, SaveReadingHistoryRequestDto request);
    Task<IEnumerable<ReadingHistoryDto>> GetUserHistoryAsync(string userId);
    Task<IEnumerable<ReadingHistoryDto>> GetPart5HistoryAsync(string userId);
    Task<IEnumerable<ReadingHistoryDto>> GetPart6HistoryAsync(string userId);
    Task<IEnumerable<ReadingHistoryDto>> GetPart7HistoryAsync(string userId);
    Task<ReadingHistoryDto?> GetHistoryByIdAsync(string id);
}
