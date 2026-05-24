using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.Enums;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Services;

public class VocabularyProgressService : IVocabularyProgressService
{
    private readonly IVocabularyProgressRepository _repository;
    private readonly ISpacedRepetitionService _srsService;
    private readonly IEngagementService _engagementService;
    private readonly IVocabularyRepository _vocabRepository;

    public VocabularyProgressService(
        IVocabularyProgressRepository repository,
        ISpacedRepetitionService srsService,
        IEngagementService engagementService,
        IVocabularyRepository vocabRepository)
    {
        _repository = repository;
        _srsService = srsService;
        _engagementService = engagementService;
        _vocabRepository = vocabRepository;
    }

    public async Task<UpdateProgressResponseDto> UpdateProgressAsync(string userId, UpdateProgressDto dto)
    {
        var progress = await _repository.GetAsync(userId, dto.VocabularyId);
        var wasMastered = progress?.IsMastered ?? false;

        if (progress == null)
        {
            progress = new UserVocabularyProgress
            {
                UserId = userId,
                VocabularyId = dto.VocabularyId,
                Repetitions = 0,
                Interval = 0,
                EasinessFactor = 2.5,
                NextReviewDate = DateTime.UtcNow
            };
        }

        progress = _srsService.CalculateNextReview(progress, dto.Quality);
        await _repository.UpsertAsync(progress);

        EngagementResultDto? engagement = null;
        if (dto.Quality >= 3)
        {
            var newlyMastered = !wasMastered && progress.IsMastered;
            engagement = await _engagementService.RecordActivityAsync(userId, new RecordActivityRequest
            {
                ActivityType = ActivityType.VocabFlashcardReview,
                ReferenceId = dto.VocabularyId,
                NewlyMastered = newlyMastered
            });
        }

        return new UpdateProgressResponseDto
        {
            Progress = progress,
            Engagement = engagement
        };
    }

    public async Task<IEnumerable<UserVocabularyProgress>> GetUserProgressAsync(string userId)
    {
        return await _repository.GetUserProgressAsync(userId);
    }

    public async Task<IEnumerable<string>> GetDueVocabularyIdsAsync(string userId)
    {
        return await _repository.GetDueVocabularyIdsAsync(userId);
    }

    public async Task<VocabularyHubStatsDto> GetHubStatsAsync(string userId)
    {
        var allProgress = await _repository.GetUserProgressAsync(userId);
        var totalVocab = await _vocabRepository.GetCountAsync();
        var now = DateTime.UtcNow;

        return new VocabularyHubStatsDto
        {
            StarredCount = allProgress.Count(p => p.IsStarred),
            DueCount = allProgress.Count(p => p.NextReviewDate <= now),
            StudiedCount = allProgress.Count(p => p.Repetitions > 0), // Chỉ đếm từ đã thực sự học (có ít nhất 1 lần review)
            MasteredCount = allProgress.Count(p => p.IsMastered),
            TotalCount = totalVocab,
            DailyTargetCount = 500
        };
    }

    public async Task<UserVocabularyProgress> ToggleStarAsync(string userId, string vocabularyId)
    {
        var progress = await _repository.GetAsync(userId, vocabularyId);
        if (progress == null)
        {
            // Chưa từng học → NextReviewDate = MaxValue để KHÔNG vào "Cần ôn"
            // Từ sẽ vào lịch ôn tập sau khi user học lần đầu qua UpdateProgressAsync
            progress = new UserVocabularyProgress
            {
                UserId = userId,
                VocabularyId = vocabularyId,
                Repetitions = 0,
                Interval = 0,
                EasinessFactor = 2.5,
                NextReviewDate = new DateTime(9999, 1, 1, 0, 0, 0, DateTimeKind.Utc), // Ngày xa tương lai (Utc) → không vào "Cần ôn"
                IsStarred = true
            };
        }
        else
        {
            progress.IsStarred = !progress.IsStarred;
        }

        await _repository.UpsertAsync(progress);
        return progress;
    }

    public async Task<IEnumerable<VocabularyDto>> GetStarredVocabulariesAsync(string userId)
    {
        var allProgress = await _repository.GetUserProgressAsync(userId);
        var starredIds = allProgress.Where(p => p.IsStarred).Select(p => p.VocabularyId);
        
        var entities = await _vocabRepository.GetByIdsAsync(starredIds);
        return entities.Select(entity => new VocabularyDto
        {
            Id = entity.Id,
            Word = entity.Word,
            Phonetic = entity.Phonetic,
            WordType = entity.WordType,
            DefinitionEn = entity.DefinitionEn,
            DefinitionVi = entity.DefinitionVi,
            AudioUrl = entity.AudioUrl,
            ImageUrl = entity.ImageUrl,
            Topic = entity.Topic,
            Level = entity.Level,
            Frequency = entity.Frequency,
            Synonyms = entity.Synonyms,
            Antonyms = entity.Antonyms,
            Collocations = entity.Collocations,
            Examples = entity.Examples.Select(e => new VocabularyExampleDto
            {
                Sentence = e.Sentence,
                SentenceVi = e.SentenceVi
            }).ToList()
        });
    }

    public async Task<IEnumerable<VocabularyDto>> GetDueVocabulariesAsync(string userId)
    {
        var dueIds = await _repository.GetDueVocabularyIdsAsync(userId);
        var entities = await _vocabRepository.GetByIdsAsync(dueIds);
        return entities.Select(entity => new VocabularyDto
        {
            Id = entity.Id,
            Word = entity.Word,
            Phonetic = entity.Phonetic,
            WordType = entity.WordType,
            DefinitionEn = entity.DefinitionEn,
            DefinitionVi = entity.DefinitionVi,
            AudioUrl = entity.AudioUrl,
            ImageUrl = entity.ImageUrl,
            Topic = entity.Topic,
            Level = entity.Level,
            Frequency = entity.Frequency,
            Synonyms = entity.Synonyms,
            Antonyms = entity.Antonyms,
            Collocations = entity.Collocations,
            Examples = entity.Examples.Select(e => new VocabularyExampleDto
            {
                Sentence = e.Sentence,
                SentenceVi = e.SentenceVi
            }).ToList()
        });
    }

    public async Task<IEnumerable<ReviewScheduleItemDto>> GetReviewScheduleAsync(string userId)
    {
        var now = DateTime.UtcNow;

        // Lấy tất cả từ đã thực sự học (Repetitions > 0)
        var allProgress = (await _repository.GetUserProgressAsync(userId))
            .Where(p => p.Repetitions > 0)
            .ToList();

        if (!allProgress.Any())
            return Enumerable.Empty<ReviewScheduleItemDto>();

        // Lấy thông tin vocabulary cho các từ đó
        var vocabIds = allProgress.Select(p => p.VocabularyId);
        var vocabs = (await _vocabRepository.GetByIdsAsync(vocabIds))
            .ToDictionary(v => v.Id);

        var result = allProgress
            .Where(p => vocabs.ContainsKey(p.VocabularyId))
            .Select(p =>
            {
                var vocab = vocabs[p.VocabularyId];
                var isDue = p.NextReviewDate <= now;
                var daysUntilDue = (int)Math.Round((p.NextReviewDate - now).TotalDays);

                return new ReviewScheduleItemDto
                {
                    VocabularyId  = p.VocabularyId,
                    Word          = vocab.Word,
                    DefinitionVi  = vocab.DefinitionVi,
                    WordType      = vocab.WordType,
                    Repetitions   = p.Repetitions,
                    MasteryLevel  = p.MasteryLevel,
                    IsMastered    = p.IsMastered,
                    LastReviewedAt = p.LastReviewedAt,
                    NextReviewDate = p.NextReviewDate,
                    IsDue         = isDue,
                    DaysUntilDue  = daysUntilDue
                };
            })
            .OrderBy(x => x.NextReviewDate) // Gần nhất lên trên
            .ToList();

        return result;
    }
}
