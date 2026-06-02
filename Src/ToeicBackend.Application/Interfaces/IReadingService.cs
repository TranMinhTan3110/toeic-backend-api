using ToeicBackend.Application.DTOs.Reading;

namespace ToeicBackend.Application.Interfaces;

public interface IReadingService
{
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
    Task<string> SaveHistoryAsync(string userId, ToeicBackend.Application.DTOs.SaveReadingHistoryRequestDto request);
    Task<string> SavePart6HistoryAsync(string userId, ToeicBackend.Application.DTOs.SaveReadingHistoryRequestDto request);
    Task<string> SavePart7HistoryAsync(string userId, ToeicBackend.Application.DTOs.SaveReadingHistoryRequestDto request);
    Task<IEnumerable<ToeicBackend.Application.DTOs.ReadingHistoryDto>> GetUserHistoryAsync(string userId);
    Task<IEnumerable<ToeicBackend.Application.DTOs.ReadingHistoryDto>> GetPart5HistoryAsync(string userId);
    Task<IEnumerable<ToeicBackend.Application.DTOs.ReadingHistoryDto>> GetPart6HistoryAsync(string userId);
    Task<IEnumerable<ToeicBackend.Application.DTOs.ReadingHistoryDto>> GetPart7HistoryAsync(string userId);
    Task<ToeicBackend.Application.DTOs.ReadingHistoryDto?> GetHistoryByIdAsync(string id);
}
