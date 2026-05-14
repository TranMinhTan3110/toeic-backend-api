using ToeicBackend.Application.DTOs.Reading;

namespace ToeicBackend.Application.Interfaces;

public interface IReadingService
{
    Task<IEnumerable<Part5QuestionDto>> GetPart5QuestionsAsync(int? count = null);
    Task<IEnumerable<Part6PassageDto>> GetPart6PassagesAsync();
    Task<IEnumerable<Part6PassageDto>> GetPart7PassagesAsync();
    Task<Part5SubmitResponseDto> SubmitPart5AnswersAsync(Part5SubmitRequestDto request);
    Task<Part5SubmitResponseDto> SubmitPart6AnswersAsync(Part5SubmitRequestDto request);
    Task<Part5SubmitResponseDto> SubmitPart7AnswersAsync(Part5SubmitRequestDto request);
}
