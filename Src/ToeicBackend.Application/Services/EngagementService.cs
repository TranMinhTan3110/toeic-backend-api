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
            ActivityType.GrammarExercise,
            ActivityType.ListeningComplete,
            ActivityType.ReadingComplete,
            ActivityType.SpeakingComplete,
            ActivityType.WritingComplete,
            ActivityType.SpeakingExamComplete,
            ActivityType.WritingExamComplete,
        };
        if (!allowedTypes.Contains(request.ActivityType))
        {
            return null;
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return null;

        var todayKey = VietnamTimeHelper.GetTodayDateKey();
        var activityTypeName = request.ActivityType.ToString();

        var isExamType = request.ActivityType is ActivityType.SpeakingExamComplete or ActivityType.WritingExamComplete;

        if (!string.IsNullOrEmpty(request.ReferenceId))
        {
            // Exam: kiểm tra lifetime (vĩnh viễn) — mỗi examSetId chỉ cộng 1 lần
            // Practice: kiểm tra theo ngày
            var alreadyAwarded = isExamType
                ? await _epEventRepository.ExistsForReferenceEverAsync(userId, activityTypeName, request.ReferenceId)
                : await _epEventRepository.ExistsForReferenceOnDateAsync(userId, activityTypeName, request.ReferenceId, todayKey);

            if (alreadyAwarded)
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
            
            // Listening, Reading, Speaking, Writing practices
            ActivityType.ListeningComplete => request.CorrectAnswers > 0
                ? (request.CorrectAnswers * EngagementRules.ListeningEpPerCorrect) + EngagementRules.ListeningCompleteBonusEp
                : 0,
            ActivityType.ReadingComplete => request.CorrectAnswers > 0
                ? (request.CorrectAnswers * EngagementRules.ReadingEpPerCorrect) + EngagementRules.ReadingCompleteBonusEp
                : 0,
            ActivityType.SpeakingComplete => request.CorrectAnswers > 0
                ? (request.CorrectAnswers * EngagementRules.SpeakingEpPerCorrect) + EngagementRules.SpeakingCompleteBonusEp
                : 0,
            ActivityType.WritingComplete => request.CorrectAnswers > 0
                ? (request.CorrectAnswers * EngagementRules.WritingEpPerPoint) + EngagementRules.WritingCompleteBonusEp
                : 0,

            // Exam: +30 EP flat (lifetime, không nhân nhân vật liệu gì cả)
            ActivityType.SpeakingExamComplete => EngagementRules.ExamCompleteEp,
            ActivityType.WritingExamComplete  => EngagementRules.ExamCompleteEp,

            _ => 3,
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
