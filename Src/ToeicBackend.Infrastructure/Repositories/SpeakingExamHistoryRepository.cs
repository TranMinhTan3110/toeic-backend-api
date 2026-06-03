using Google.Cloud.Firestore;
using Microsoft.Extensions.Caching.Memory;
using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.Interfaces;

namespace ToeicBackend.Infrastructure.Repositories;

public class SpeakingExamHistoryRepository : ISpeakingExamHistoryRepository
{
    private readonly FirestoreDb _firestoreDb;
    private readonly IMemoryCache _cache;
    private const string CollectionName = "speaking_exam_history";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public SpeakingExamHistoryRepository(FirestoreDb firestoreDb, IMemoryCache cache)
    {
        _firestoreDb = firestoreDb;
        _cache = cache;
    }

    public async Task<string> AddAsync(SpeakingExamHistoryDto history)
    {
        var data = new Dictionary<string, object>
        {
            ["user_id"]          = history.UserId,
            ["exam_set_id"]      = history.ExamSetId,
            ["exam_title"]       = history.ExamTitle,
            ["toeic_score"]      = history.ToeicScore,
            ["raw_average_score"]= history.RawAverageScore,
            ["total_tasks"]      = history.TotalTasks,
            ["date"]             = history.Date,
            ["task_results"]     = history.TaskResults.Select(t => new Dictionary<string, object?>
            {
                ["question_id"]       = t.QuestionId,
                ["sub_question_index"]= (object?)(t.SubQuestionIndex.HasValue ? (object)t.SubQuestionIndex.Value : null),
                ["transcript"]        = t.Transcript,
                ["score"]             = t.Score,
                ["feedback"]          = t.Feedback,
                ["criteria_scores"]   = t.CriteriaScores,
                ["passed"]            = t.Passed
            }).ToList<object>()
        };

        var docRef = await _firestoreDb.Collection(CollectionName).AddAsync(data);
        _cache.Remove($"speaking_exam_history_{history.UserId}");
        return docRef.Id;
    }

    public async Task<IEnumerable<SpeakingExamHistoryDto>> GetByUserIdAsync(string userId)
    {
        var cacheKey = $"speaking_exam_history_{userId}";
        if (_cache.TryGetValue(cacheKey, out List<SpeakingExamHistoryDto>? cached) && cached != null)
            return cached;

        var snapshot = await _firestoreDb.Collection(CollectionName)
            .WhereEqualTo("user_id", userId)
            .GetSnapshotAsync();

        var results = snapshot.Documents
            .Select(MapToDto)
            .OrderByDescending(h => h.Date)
            .Take(20)
            .ToList();

        _cache.Set(cacheKey, results, CacheDuration);
        return results;
    }

    public async Task<SpeakingExamHistoryDto?> GetByIdAsync(string id)
    {
        var cacheKey = $"speaking_exam_history_detail_{id}";
        if (_cache.TryGetValue(cacheKey, out SpeakingExamHistoryDto? cached) && cached != null)
            return cached;

        var docRef = _firestoreDb.Collection(CollectionName).Document(id);
        var snapshot = await docRef.GetSnapshotAsync();
        if (!snapshot.Exists) return null;

        var result = MapToDto(snapshot);
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(30));
        return result;
    }

    private static SpeakingExamHistoryDto MapToDto(DocumentSnapshot doc)
    {
        var taskResultsList = new List<SpeakingExamTaskResultDto>();
        if (doc.TryGetValue<IList<object>>("task_results", out var rawTasks) && rawTasks != null)
        {
            foreach (var rawTask in rawTasks)
            {
                if (rawTask is IDictionary<string, object> t)
                {
                    var criteriaRaw = t.TryGetValue("criteria_scores", out var cr) && cr is IDictionary<string, object> cd
                        ? cd.ToDictionary(kv => kv.Key, kv => Convert.ToDouble(kv.Value))
                        : new Dictionary<string, double>();

                    taskResultsList.Add(new SpeakingExamTaskResultDto
                    {
                        QuestionId       = t.TryGetValue("question_id", out var qid) ? qid?.ToString() ?? "" : "",
                        SubQuestionIndex = t.TryGetValue("sub_question_index", out var sqi) && sqi != null ? Convert.ToInt32(sqi) : null,
                        Transcript       = t.TryGetValue("transcript", out var tr) ? tr?.ToString() ?? "" : "",
                        Score            = t.TryGetValue("score", out var sc) ? Convert.ToDouble(sc) : 0,
                        Feedback         = t.TryGetValue("feedback", out var fb) ? fb?.ToString() ?? "" : "",
                        CriteriaScores   = criteriaRaw,
                        Passed           = t.TryGetValue("passed", out var p) && Convert.ToBoolean(p)
                    });
                }
            }
        }

        return new SpeakingExamHistoryDto
        {
            Id              = doc.Id,
            UserId          = doc.GetValue<string>("user_id"),
            ExamSetId       = doc.GetValue<string>("exam_set_id"),
            ExamTitle       = doc.GetValue<string>("exam_title"),
            ToeicScore      = doc.TryGetValue<double>("toeic_score", out var ts) ? ts : 0,
            RawAverageScore = doc.TryGetValue<double>("raw_average_score", out var ra) ? ra : 0,
            TotalTasks      = doc.TryGetValue<int>("total_tasks", out var tt) ? tt : 0,
            Date            = doc.TryGetValue<DateTime>("date", out var dt) ? dt : DateTime.MinValue,
            TaskResults     = taskResultsList
        };
    }
}
