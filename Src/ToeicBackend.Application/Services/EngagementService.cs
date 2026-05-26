using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.Enums;
using ToeicBackend.Application.Helpers;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Application.Services;

public class EngagementService : IEngagementService
{
    private readonly IUserRepository _userRepository;
    private readonly IEpEventRepository _epEventRepository;

    public EngagementService(IUserRepository userRepository, IEpEventRepository epEventRepository)
    {
        _userRepository = userRepository;
        _epEventRepository = epEventRepository;
    }

    public async Task<EngagementResultDto?> RecordActivityAsync(string userId, RecordActivityRequest request)
    {
        // Chỉ cho phép các hoạt động được hỗ trợ
        var allowedTypes = new[]
        {
            ActivityType.VocabFlashcardReview,
            ActivityType.VocabTyping,
            ActivityType.VocabMatching,
            ActivityType.VocabSentence,
            ActivityType.VocabSpeaking,
            ActivityType.GrammarLessonComplete,
            ActivityType.GrammarExercise
        };
        if (!allowedTypes.Contains(request.ActivityType))
        {
            return null;
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return null;

        var todayKey = VietnamTimeHelper.GetTodayDateKey();
        var activityTypeName = request.ActivityType.ToString();

        if (!string.IsNullOrEmpty(request.ReferenceId) &&
            await _epEventRepository.ExistsForReferenceOnDateAsync(userId, activityTypeName, request.ReferenceId, todayKey))
        {
            return new EngagementResultDto
            {
                EpAwarded = 0,
                TotalExperiencePoints = user.ExperiencePoints,
                WeeklyEp = user.WeeklyEp,
                StreakDays = user.StreakDays,
                BestStreakDays = user.BestStreakDays,
                StreakMultiplier = EngagementRules.GetStreakMultiplier(user.StreakDays),
                AlreadyAwardedForReference = true
            };
        }

        var baseEp = request.ActivityType switch
        {
            ActivityType.VocabFlashcardReview => EngagementRules.CalculateVocabReviewBaseEp(request.NewlyMastered),
            // Quiz & Matching: EP tỉ lệ với số câu/cặp đúng
            ActivityType.VocabTyping   => request.CorrectAnswers * EngagementRules.VocabQuizEpPerCorrect,
            ActivityType.VocabMatching => request.CorrectAnswers * EngagementRules.VocabMatchingEpPerPair,
            ActivityType.VocabSentence => EngagementRules.VocabSentenceBaseEp,
            ActivityType.VocabSpeaking => EngagementRules.VocabSpeakingBaseEp,
            ActivityType.GrammarLessonComplete => EngagementRules.GrammarLessonBaseEp,
            ActivityType.GrammarExercise       => request.CorrectAnswers * EngagementRules.GrammarExerciseEpPerCorrect,
            _                          => 3,
        };

        // Đảm bảo ít nhất 1 EP nếu đã hoàn thành (tránh baseEp = 0 khi correctAnswers = 0)
        if (baseEp <= 0 && (request.ActivityType == ActivityType.VocabTyping ||
                            request.ActivityType == ActivityType.VocabMatching ||
                            request.ActivityType == ActivityType.GrammarExercise))
        {
            baseEp = 1;
        }

        if (user.DailyEpDateKey != todayKey)
        {
            user.DailyEpDateKey = todayKey;
            user.DailyEpEarned = 0;
        }

        var epToday = user.DailyEpEarned;
        if (epToday == 0)
        {
            epToday = await _epEventRepository.SumEpForUserOnDateAsync(userId, todayKey);
            user.DailyEpEarned = epToday;
        }

        if (epToday >= EngagementRules.DailyEpCap)
        {
            return new EngagementResultDto
            {
                EpAwarded = 0,
                TotalExperiencePoints = user.ExperiencePoints,
                WeeklyEp = user.WeeklyEp,
                StreakDays = user.StreakDays,
                BestStreakDays = user.BestStreakDays,
                StreakMultiplier = EngagementRules.GetStreakMultiplier(user.StreakDays),
                DailyCapReached = true
            };
        }

        var newStreak = EngagementRules.ApplyStreakForStudyDay(user, todayKey);
        var multiplier = EngagementRules.GetStreakMultiplier(newStreak);
        var epAwarded = (int)Math.Round(baseEp * multiplier);

        var remainingCap = EngagementRules.DailyEpCap - epToday;
        if (epAwarded > remainingCap)
        {
            epAwarded = remainingCap;
        }

        if (epAwarded <= 0)
        {
            return new EngagementResultDto
            {
                EpAwarded = 0,
                TotalExperiencePoints = user.ExperiencePoints,
                WeeklyEp = user.WeeklyEp,
                StreakDays = user.StreakDays,
                BestStreakDays = user.BestStreakDays,
                StreakMultiplier = multiplier,
                DailyCapReached = true
            };
        }

        var periodKey = VietnamTimeHelper.GetWeeklyPeriodKey();
        if (user.WeeklyEpPeriodKey != periodKey)
        {
            user.WeeklyEp = 0;
            user.WeeklyEpPeriodKey = periodKey;
        }

        user.StreakDays = newStreak;
        user.BestStreakDays = Math.Max(user.BestStreakDays, newStreak);
        user.LastStudyDate = todayKey;
        user.ExperiencePoints += epAwarded;
        user.WeeklyEp += epAwarded;
        user.DailyEpEarned += epAwarded;

        await _userRepository.UpdateAsync(user);
        await _epEventRepository.AddAsync(new EpEvent
        {
            UserId = userId,
            ActivityType = activityTypeName,
            BaseEp = baseEp,
            StreakMultiplier = multiplier,
            EpAwarded = epAwarded,
            ReferenceId = request.ReferenceId,
            StudyDateKey = todayKey,
            CreatedAt = DateTime.UtcNow
        });

        return new EngagementResultDto
        {
            EpAwarded = epAwarded,
            TotalExperiencePoints = user.ExperiencePoints,
            WeeklyEp = user.WeeklyEp,
            StreakDays = user.StreakDays,
            BestStreakDays = user.BestStreakDays,
            StreakMultiplier = multiplier
        };
    }
}
