using ToeicBackend.Application.DTOs.Reading;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Domain.Entities.Reading;

namespace ToeicBackend.Application.Services;

public class ReadingService : IReadingService
{
    private readonly IReadingRepository _repository;

    public ReadingService(IReadingRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Part5QuestionDto>> GetPart5QuestionsAsync(int? count = null)
    {
        var take = (count.HasValue && count.Value > 0) ? count.Value : int.MaxValue;
        var entities = await _repository.GetRandomPart5QuestionsAsync(take);
        return entities.Select(e => new Part5QuestionDto
        {
            Id = e.Id,
            QuestionText = e.QuestionText,
            OptionA = e.OptionA,
            OptionB = e.OptionB,
            OptionC = e.OptionC,
            OptionD = e.OptionD,
            Explanation = null // do not send explanation in the question fetch to avoid spoilers
        });
    }

    public async Task<Part5SubmitResponseDto> SubmitPart5AnswersAsync(Part5SubmitRequestDto request)
    {
        var ids = request.Answers.Keys;
        var questions = await _repository.GetPart5QuestionsByIdsAsync(ids);
        var map = questions.ToDictionary(q => q.Id, q => q);

        var response = new Part5SubmitResponseDto
        {
            TotalQuestions = request.Answers.Count,
            CorrectCount = 0
        };

        foreach (var kv in request.Answers)
        {
            var qid = kv.Key;
            var selected = kv.Value?.Trim().ToUpperInvariant() ?? string.Empty;
            if (map.TryGetValue(qid, out var question))
            {
                var isCorrect = string.Equals(selected, question.CorrectAnswer?.Trim().ToUpperInvariant(), StringComparison.Ordinal);
                if (isCorrect) response.CorrectCount++;

                response.Results.Add(new Part5QuestionResultDto
                {
                    QuestionId = qid,
                    SelectedOption = selected,
                    IsCorrect = isCorrect,
                    Explanation = question.Explanation,
                    CorrectAnswer = question.CorrectAnswer
                });
            }
            else
            {
                response.Results.Add(new Part5QuestionResultDto
                {
                    QuestionId = qid,
                    SelectedOption = selected,
                    IsCorrect = false,
                    Explanation = "Question not found",
                    CorrectAnswer = null
                });
            }
        }

        return response;
    }

    public async Task<IEnumerable<Part6PassageDto>> GetPart6PassagesAsync()
    {
        var groups = await _repository.GetPart6PassagesAsync();
        var result = new List<Part6PassageDto>();
        foreach (var g in groups)
        {
            var dto = new Part6PassageDto
            {
                Id = g.Id,
                PassageText = g.PassageText,
                ImageUrl = g.ImageUrl,
                AudioUrl = g.AudioUrl,
                Passages = g.Passages?.Cast<object>().ToList()
            };

            foreach (var q in g.Questions)
            {
                dto.Questions.Add(new Part5QuestionDto
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    OptionA = q.OptionA,
                    OptionB = q.OptionB,
                    OptionC = q.OptionC,
                    OptionD = q.OptionD,
                    Explanation = null
                });
            }

            result.Add(dto);
        }

        return result;
    }

    public async Task<IEnumerable<Part6PassageDto>> GetPart7PassagesAsync()
    {
        var groups = await _repository.GetPart7PassagesAsync();
        var result = new List<Part6PassageDto>();
        foreach (var g in groups)
        {
            var dto = new Part6PassageDto
            {
                Id = g.Id,
                PassageText = g.PassageText,
                ImageUrl = g.ImageUrl,
                AudioUrl = g.AudioUrl,
                Passages = g.Passages?.Cast<object>().ToList()
            };

            foreach (var q in g.Questions)
            {
                dto.Questions.Add(new Part5QuestionDto
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    OptionA = q.OptionA,
                    OptionB = q.OptionB,
                    OptionC = q.OptionC,
                    OptionD = q.OptionD,
                    Explanation = null
                });
            }

            result.Add(dto);
        }

        return result;
    }

    public async Task<Part5SubmitResponseDto> SubmitPart6AnswersAsync(Part5SubmitRequestDto request)
    {
        var ids = request.Answers.Keys;
        var questions = await _repository.GetQuestionsByIdsAsync(ids);
        var map = questions.ToDictionary(q => q.Id, q => q);

        var response = new Part5SubmitResponseDto
        {
            TotalQuestions = request.Answers.Count,
            CorrectCount = 0
        };

        foreach (var kv in request.Answers)
        {
            var qid = kv.Key;
            var selected = kv.Value?.Trim().ToUpperInvariant() ?? string.Empty;
            if (map.TryGetValue(qid, out var question))
            {
                var isCorrect = string.Equals(selected, question.CorrectAnswer?.Trim().ToUpperInvariant(), StringComparison.Ordinal);
                if (isCorrect) response.CorrectCount++;

                response.Results.Add(new Part5QuestionResultDto
                {
                    QuestionId = qid,
                    SelectedOption = selected,
                    IsCorrect = isCorrect,
                    Explanation = question.Explanation,
                    CorrectAnswer = question.CorrectAnswer
                });
            }
            else
            {
                response.Results.Add(new Part5QuestionResultDto
                {
                    QuestionId = qid,
                    SelectedOption = selected,
                    IsCorrect = false,
                    Explanation = "Question not found",
                    CorrectAnswer = null
                });
            }
        }

        return response;
    }

    public async Task<Part5SubmitResponseDto> SubmitPart7AnswersAsync(Part5SubmitRequestDto request)
    {
        // Reuse Part7 grading same as Part6: fetch without part filter
        var ids = request.Answers.Keys;
        var questions = await _repository.GetQuestionsByIdsAsync(ids);
        var map = questions.ToDictionary(q => q.Id, q => q);

        var response = new Part5SubmitResponseDto
        {
            TotalQuestions = request.Answers.Count,
            CorrectCount = 0
        };

        foreach (var kv in request.Answers)
        {
            var qid = kv.Key;
            var selected = kv.Value?.Trim().ToUpperInvariant() ?? string.Empty;
            if (map.TryGetValue(qid, out var question))
            {
                var isCorrect = string.Equals(selected, question.CorrectAnswer?.Trim().ToUpperInvariant(), StringComparison.Ordinal);
                if (isCorrect) response.CorrectCount++;

                response.Results.Add(new Part5QuestionResultDto
                {
                    QuestionId = qid,
                    SelectedOption = selected,
                    IsCorrect = isCorrect,
                    Explanation = question.Explanation,
                    CorrectAnswer = question.CorrectAnswer
                });
            }
            else
            {
                response.Results.Add(new Part5QuestionResultDto
                {
                    QuestionId = qid,
                    SelectedOption = selected,
                    IsCorrect = false,
                    Explanation = "Question not found",
                    CorrectAnswer = null
                });
            }
        }

        return response;
    }
}
