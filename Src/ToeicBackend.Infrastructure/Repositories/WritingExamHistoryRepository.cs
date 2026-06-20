using Google.Cloud.Firestore;
using Microsoft.Extensions.Caching.Memory;
using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.Interfaces;

namespace ToeicBackend.Infrastructure.Repositories;

public class WritingExamHistoryRepository : IWritingExamHistoryRepository
{
    private readonly FirestoreDb _firestoreDb;
    private readonly IMemoryCache _cache;
    private const string CollectionName = "writing_exam_history";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public WritingExamHistoryRepository(FirestoreDb firestoreDb, IMemoryCache cache)
    {
        _firestoreDb = firestoreDb;
        _cache = cache;
    }

    public async Task<string> AddAsync(WritingExamHistoryDto history)
    {
        var data = new Dictionary<string, object>
        {
            ["user_id"]           = history.UserId,
            ["exam_set_id"]       = history.ExamSetId,
            ["exam_title"]        = history.ExamTitle,
            ["toeic_score"]       = history.ToeicScore,
            ["raw_average_score"] = history.RawAverageScore,
            ["total_tasks"]       = history.TotalTasks,
            ["time_spent"]        = history.TimeSpent,
            ["date"]              = history.Date,
            ["task_results"]      = history.TaskResults.Select(t => new Dictionary<string, object>
            {
                ["question_id"]     = t.QuestionId,
                ["task_number"]     = t.TaskNumber,
                ["task_type"]       = t.TaskType,
                ["user_answer"]     = t.UserAnswer,
                ["word_count"]      = t.WordCount,
                ["ai_score"]        = t.AiScore,
                ["ai_feedback"]     = t.AiFeedback,
                ["criteria_scores"] = t.CriteriaScores
            }).ToList<object>()
        };

        var docRef = await _firestoreDb.Collection(CollectionName).AddAsync(data);
        _cache.Remove($"writing_exam_history_{history.UserId}");
        return docRef.Id;
    }

    public async Task<IEnumerable<WritingExamHistoryDto>> GetByUserIdAsync(string userId)
    {
        var cacheKey = $"writing_exam_history_{userId}";
        if (_cache.TryGetValue(cacheKey, out List<WritingExamHistoryDto>? cached) && cached != null)
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

    public async Task<WritingExamHistoryDto?> GetByIdAsync(string id)
    {
        var cacheKey = $"writing_exam_history_detail_{id}";
        if (_cache.TryGetValue(cacheKey, out WritingExamHistoryDto? cached) && cached != null)
            return cached;

        var docRef = _firestoreDb.Collection(CollectionName).Document(id);
        var snapshot = await docRef.GetSnapshotAsync();
        if (!snapshot.Exists) return null;

        var result = MapToDto(snapshot);
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(30));
        return result;
    }

    private static WritingExamHistoryDto MapToDto(DocumentSnapshot doc)
    {
        var taskResultsList = new List<WritingExamTaskResultDto>();
        if (doc.TryGetValue<IList<object>>("task_results", out var rawTasks) && rawTasks != null)
        {
            foreach (var rawTask in rawTasks)
            {
                if (rawTask is IDictionary<string, object> t)
                {
                    var criteriaRaw = t.TryGetValue("criteria_scores", out var cr) && cr is IDictionary<string, object> cd
                        ? cd.ToDictionary(kv => kv.Key, kv => Convert.ToDouble(kv.Value))
                        : new Dictionary<string, double>();

                    taskResultsList.Add(new WritingExamTaskResultDto
                    {
                        QuestionId     = t.TryGetValue("question_id", out var qid) ? qid?.ToString() ?? "" : "",
                        TaskNumber     = t.TryGetValue("task_number", out var tn) ? Convert.ToInt32(tn) : 0,
                        TaskType       = t.TryGetValue("task_type", out var ttype) ? ttype?.ToString() ?? "" : "",
                        UserAnswer     = t.TryGetValue("user_answer", out var ua) ? ua?.ToString() ?? "" : "",
                        WordCount      = t.TryGetValue("word_count", out var wc) ? Convert.ToInt32(wc) : 0,
                        AiScore        = t.TryGetValue("ai_score", out var ais) ? Convert.ToInt32(ais) : 0,
                        AiFeedback     = t.TryGetValue("ai_feedback", out var af) ? af?.ToString() ?? "" : "",
                        CriteriaScores = criteriaRaw
                    });
                }
            }
        }

        return new WritingExamHistoryDto
        {
            Id              = doc.Id,
            UserId          = doc.GetValue<string>("user_id"),
            ExamSetId       = doc.GetValue<string>("exam_set_id"),
            ExamTitle       = doc.GetValue<string>("exam_title"),
            ToeicScore      = doc.TryGetValue<double>("toeic_score", out var ts) ? ts : 0,
            RawAverageScore = doc.TryGetValue<double>("raw_average_score", out var ra) ? ra : 0,
            TotalTasks      = doc.TryGetValue<int>("total_tasks", out var tt) ? tt : 0,
            TimeSpent       = doc.TryGetValue<int>("time_spent", out var tsp) ? tsp : 0,
            Date            = doc.TryGetValue<DateTime>("date", out var dt) ? dt : DateTime.MinValue,
            TaskResults     = taskResultsList
        };
    }
}
