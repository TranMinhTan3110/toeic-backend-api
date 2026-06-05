using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Caching.Memory;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Infrastructure.Repositories;

public class SpeakingRepository : ISpeakingRepository
{
    private readonly FirestoreDb _firestoreDb;
    private readonly IMemoryCache _cache;
    private const string CollectionName = "speaking_questions";

    public SpeakingRepository(FirestoreDb firestoreDb, IMemoryCache cache)
    {
        _firestoreDb = firestoreDb;
        _cache = cache;
    }

    public async Task<IEnumerable<SpeakingQuestion>> GetAllAsync()
    {
        const string cacheKey = "speaking_questions_all";
        if (_cache.TryGetValue(cacheKey, out List<SpeakingQuestion>? cachedList) && cachedList != null)
        {
            return cachedList;
        }

        var snapshot = await _firestoreDb.Collection(CollectionName).GetSnapshotAsync();
        var results = snapshot.Documents.Select(MapToDomain).ToList();
        
        _cache.Set(cacheKey, results, TimeSpan.FromHours(4));
        return results;
    }
    public async Task<IEnumerable<SpeakingQuestion>> GetByTaskNumberAsync(
        int taskNumber,
        bool? isExam = null,
        bool? isPractice = null)
    {
        var cacheKey = $"speaking_questions_task_{taskNumber}";
        if (!_cache.TryGetValue(cacheKey, out List<SpeakingQuestion>? cachedList) || cachedList == null)
        {
            var snapshot = await _firestoreDb.Collection(CollectionName)
                .WhereEqualTo("task_number", taskNumber)
                .GetSnapshotAsync();

            cachedList = snapshot.Documents.Select(MapToDomain).ToList();
            _cache.Set(cacheKey, cachedList, TimeSpan.FromHours(4));
        }

        IEnumerable<SpeakingQuestion> results = cachedList;

        if (isExam.HasValue)
            results = results.Where(q => q.IsExam == isExam.Value);

        if (isPractice.HasValue)
            results = results.Where(q => q.IsPractice == isPractice.Value);

        return results;
    }

    public async Task<IEnumerable<SpeakingQuestion>> GetByFilterAsync(bool? isExam, bool? isPractice)
    {
        var cacheKey = $"speaking_questions_filter_{isExam}_{isPractice}";
        if (_cache.TryGetValue(cacheKey, out List<SpeakingQuestion>? cachedList) && cachedList != null)
        {
            return cachedList;
        }

        Query query = _firestoreDb.Collection(CollectionName);

        if (isExam.HasValue)
        {
            query = query.WhereEqualTo("is_exam", isExam.Value);
        }

        if (isPractice.HasValue)
        {
            query = query.WhereEqualTo("is_practice", isPractice.Value);
        }

        var snapshot = await query.GetSnapshotAsync();
        var results = snapshot.Documents.Select(MapToDomain).ToList();
        
        _cache.Set(cacheKey, results, TimeSpan.FromHours(4));
        return results;
    }

    public async Task<IEnumerable<SpeakingQuestion>> GetByExamSetIdAsync(string examSetId)
    {
        var cacheKey = $"speaking_questions_exam_set_{examSetId}";
        if (_cache.TryGetValue(cacheKey, out List<SpeakingQuestion>? cachedList) && cachedList != null)
        {
            return cachedList;
        }

        Console.WriteLine($"[DEBUG] Querying speaking questions by exam_set_id: '{examSetId}'");
        var query = _firestoreDb.Collection(CollectionName)
            .WhereEqualTo("exam_set_id", examSetId);
        
        var snapshot = await query.GetSnapshotAsync();
        var results = snapshot.Documents.Select(MapToDomain).OrderBy(q => q.TaskNumber).ToList();
        
        _cache.Set(cacheKey, results, TimeSpan.FromHours(4));
        return results;
    }

    public async Task<int> GetCountByFilterAsync(bool? isExam, bool? isPractice)
    {
        var cacheKey = $"speaking_questions_count_{isExam}_{isPractice}";
        if (_cache.TryGetValue(cacheKey, out int count))
        {
            return count;
        }

        Query query = _firestoreDb.Collection(CollectionName);

        if (isExam.HasValue)
        {
            query = query.WhereEqualTo("is_exam", isExam.Value);
        }

        if (isPractice.HasValue)
        {
            query = query.WhereEqualTo("is_practice", isPractice.Value);
        }

        var aggregateQuery = query.Count();
        var snapshot = await aggregateQuery.GetSnapshotAsync();
        var resultCount = (int)(snapshot.Count ?? 0);
        
        _cache.Set(cacheKey, resultCount, TimeSpan.FromHours(4));
        return resultCount;
    }

    public async Task<SpeakingQuestion?> GetByIdAsync(string id)
    {
        var cacheKey = $"speaking_question_id_{id}";
        if (_cache.TryGetValue(cacheKey, out SpeakingQuestion? cachedQuestion) && cachedQuestion != null)
        {
            return cachedQuestion;
        }

        var docRef = _firestoreDb.Collection(CollectionName).Document(id);
        var snapshot = await docRef.GetSnapshotAsync();
        
        if (!snapshot.Exists) return null;
        
        var result = MapToDomain(snapshot);
        _cache.Set(cacheKey, result, TimeSpan.FromHours(4));
        return result;
    }

    private SpeakingQuestion MapToDomain(DocumentSnapshot doc)
    {
        var question = new SpeakingQuestion
        {
            Id = doc.Id
        };

        if (doc.ContainsField("task_type")) question.TaskType = doc.GetValue<string>("task_type");
        if (doc.ContainsField("task_number")) question.TaskNumber = doc.GetValue<int>("task_number");
        if (doc.ContainsField("prompt_text")) question.PromptText = doc.GetValue<string>("prompt_text");
        if (doc.ContainsField("prompt_image_url")) question.PromptImageUrl = doc.GetValue<string?>("prompt_image_url");
        if (doc.ContainsField("prompt_audio_url")) question.PromptAudioUrl = doc.GetValue<string?>("prompt_audio_url");
        if (doc.ContainsField("image_url")) question.ImageUrl = doc.GetValue<string?>("image_url");
        if (doc.ContainsField("audio_url")) question.AudioUrl = doc.GetValue<string?>("audio_url");
        if (doc.ContainsField("preparation_time")) question.PreparationTime = doc.GetValue<int>("preparation_time");
        if (doc.ContainsField("response_time")) question.ResponseTime = doc.GetValue<int>("response_time");
        if (doc.ContainsField("difficulty")) question.Difficulty = doc.GetValue<string>("difficulty");
        if (doc.ContainsField("ai_prompt")) question.AiPrompt = doc.GetValue<string>("ai_prompt");
        if (doc.ContainsField("scoring_criteria")) question.ScoringCriteria = doc.GetValue<List<string>>("scoring_criteria") ?? new();
        if (doc.ContainsField("exam_set_id")) question.ExamSetId = doc.GetValue<string?>("exam_set_id");
        if (doc.ContainsField("topic")) question.Topic = doc.GetValue<string?>("topic");
        if (doc.ContainsField("is_practice")) question.IsPractice = doc.GetValue<bool>("is_practice");
        if (doc.ContainsField("is_exam")) question.IsExam = doc.GetValue<bool>("is_exam");
        if (doc.ContainsField("max_score")) question.MaxScore = doc.GetValue<int>("max_score");
        if (doc.ContainsField("sample_answer")) question.SampleAnswer = doc.GetValue<string?>("sample_answer");
        
        if (doc.ContainsField("questions")) 
        {
            try {
                question.Questions = doc.GetValue<List<string>>("questions") ?? new();
            } catch {
                // Fallback for different list types
                var list = doc.GetValue<object>("questions") as List<object>;
                if (list != null) question.Questions = list.Select(o => o.ToString() ?? "").ToList();
            }
        }

        if (doc.ContainsField("answer_times")) 
        {
            try {
                question.AnswerTimes = doc.GetValue<List<int>>("answer_times") ?? new();
            } catch {
                var list = doc.GetValue<object>("answer_times") as List<object>;
                if (list != null) question.AnswerTimes = list.Select(o => Convert.ToInt32(o)).ToList();
            }
        }
        
        if (doc.ContainsField("created_at"))
        {
            try 
            {
                question.CreatedAt = doc.GetValue<Timestamp>("created_at").ToDateTime();
            }
            catch 
            {
                // Fallback if it's stored as string
                if (DateTime.TryParse(doc.GetValue<string>("created_at"), out var dt))
                    question.CreatedAt = dt;
            }
        }

        question.Explanation = MapExplanation(doc);
        EnsureExplanationFromSampleAnswer(question);

        return question;
    }

    private static readonly JsonSerializerOptions ExplanationJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };

    private static SpeakingExplanation? MapExplanation(DocumentSnapshot doc)
    {
        if (!doc.ContainsField("explanation")) return null;

        try
        {
            var raw = doc.GetValue<object>("explanation");
            if (raw == null) return null;

            var json = JsonSerializer.Serialize(raw);
            var explanation = JsonSerializer.Deserialize<SpeakingExplanation>(json, ExplanationJsonOptions)
                              ?? new SpeakingExplanation();

            // Firestore thường lưu sample_answer / sample_answer_translation (số ít),
            // không khớp property SampleAnswers khi deserialize JSON.
            FillExplanationSampleFieldsFromJson(json, explanation);

            explanation.SampleAnswers = NormalizeStringList(explanation.SampleAnswers);
            explanation.SampleAnswersTranslation = NormalizeStringList(explanation.SampleAnswersTranslation);
            explanation.QuestionsTranslation = NormalizeStringList(explanation.QuestionsTranslation);

            return explanation;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error mapping explanation for {doc.Id}: {ex.Message}");
            return MapExplanationManual(doc);
        }
    }

    /// <summary>
    /// Đọc sample_answer (string) và sample_answer_translation từ JSON explanation của Firestore.
    /// </summary>
    private static void FillExplanationSampleFieldsFromJson(string explanationJson, SpeakingExplanation explanation)
    {
        using var doc = JsonDocument.Parse(explanationJson);
        var root = doc.RootElement;

        if (!explanation.SampleAnswers.Any())
        {
            var answers = ReadStringListFromJson(root, "sample_answers", "sample_answer", "sampleAnswers");
            if (answers.Any()) explanation.SampleAnswers = answers;
        }

        if (!explanation.SampleAnswersTranslation.Any())
        {
            var translations = ReadStringListFromJson(
                root,
                "sample_answers_translation",
                "sample_answer_translation",
                "sampleAnswersTranslation");
            if (translations.Any()) explanation.SampleAnswersTranslation = translations;
        }
    }

    private static List<string> ReadStringListFromJson(JsonElement root, params string[] propertyNames)
    {
        foreach (var name in propertyNames)
        {
            if (!root.TryGetProperty(name, out var el)) continue;

            if (el.ValueKind == JsonValueKind.String)
            {
                var text = el.GetString()?.Trim();
                if (!string.IsNullOrEmpty(text)) return new List<string> { text };
            }

            if (el.ValueKind == JsonValueKind.Array)
            {
                return el.EnumerateArray()
                    .Select(i => i.GetString()?.Trim() ?? "")
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            }
        }

        return new List<string>();
    }

    private static SpeakingExplanation? MapExplanationManual(DocumentSnapshot doc)
    {
        try
        {
            if (!doc.TryGetValue<Dictionary<string, object>>("explanation", out var explanationObj)
                || explanationObj == null)
            {
                return null;
            }

            var explanation = new SpeakingExplanation();

            if (TryGetString(explanationObj, "translation", out var translation))
                explanation.Translation = translation;

            if (TryGetString(explanationObj, "context_translation", out var contextTranslation))
                explanation.ContextTranslation = contextTranslation;

            if (explanationObj.TryGetValue("keywords", out var keywordsRaw) && keywordsRaw is IEnumerable<object> keywordsList)
            {
                explanation.Keywords = keywordsList.Select(k =>
                {
                    if (k is not Dictionary<string, object> keywordDict) return new ExplanationKeyword();
                    return new ExplanationKeyword
                    {
                        Word = keywordDict.TryGetValue("word", out var w) ? w?.ToString() ?? "" : "",
                        Ipa = keywordDict.TryGetValue("ipa", out var ipa) ? ipa?.ToString() ?? "" : "",
                        Meaning = keywordDict.TryGetValue("meaning", out var m) ? m?.ToString() ?? "" : "",
                    };
                }).ToList();
            }

            explanation.QuestionsTranslation = ReadStringList(explanationObj, "questions_translation", "questionsTranslation");
            explanation.SampleAnswers = ReadStringList(explanationObj, "sample_answers", "sample_answer", "sampleAnswers");
            explanation.SampleAnswersTranslation = ReadStringList(
                explanationObj,
                "sample_answers_translation",
                "sample_answer_translation",
                "sampleAnswersTranslation");

            return explanation;
        }
        catch
        {
            return null;
        }
    }

    private static void EnsureExplanationFromSampleAnswer(SpeakingQuestion question)
    {
        var sample = question.SampleAnswer?.Trim();
        if (string.IsNullOrEmpty(sample)) return;

        question.Explanation ??= new SpeakingExplanation();

        if (!question.Explanation.SampleAnswers.Any())
        {
            question.Explanation.SampleAnswers = new List<string> { sample };
        }
    }

    private static List<string> NormalizeStringList(List<string> values)
    {
        return values
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v.Trim())
            .ToList();
    }

    private static List<string> ReadStringList(Dictionary<string, object> source, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (!source.TryGetValue(key, out var raw) || raw == null) continue;

            if (raw is string text && !string.IsNullOrWhiteSpace(text))
                return new List<string> { text.Trim() };

            if (raw is IEnumerable<object> list)
                return list.Select(v => v?.ToString() ?? "").Where(v => !string.IsNullOrWhiteSpace(v)).ToList();
        }

        return new List<string>();
    }

    private static bool TryGetString(Dictionary<string, object> source, string key, out string value)
    {
        value = string.Empty;
        if (!source.TryGetValue(key, out var raw) || raw == null) return false;
        value = raw.ToString() ?? string.Empty;
        return !string.IsNullOrWhiteSpace(value);
    }

    public async Task AddAsync(SpeakingQuestion question)
    {
        var docRef = string.IsNullOrEmpty(question.Id)
            ? _firestoreDb.Collection(CollectionName).Document()
            : _firestoreDb.Collection(CollectionName).Document(question.Id);

        if (string.IsNullOrEmpty(question.Id))
        {
            question.Id = docRef.Id;
        }

        var data = new Dictionary<string, object>
        {
            { "task_type", question.TaskType ?? "" },
            { "task_number", question.TaskNumber },
            { "prompt_text", question.PromptText ?? "" },
            { "prompt_image_url", question.PromptImageUrl ?? "" },
            { "prompt_audio_url", question.PromptAudioUrl ?? "" },
            { "image_url", question.ImageUrl ?? "" },
            { "audio_url", question.AudioUrl ?? "" },
            { "preparation_time", question.PreparationTime },
            { "response_time", question.ResponseTime },
            { "difficulty", question.Difficulty ?? "medium" },
            { "ai_prompt", question.AiPrompt ?? "" },
            { "scoring_criteria", question.ScoringCriteria ?? new List<string>() },
            { "exam_set_id", question.ExamSetId ?? "" },
            { "topic", question.Topic ?? "" },
            { "is_practice", question.IsPractice },
            { "is_exam", question.IsExam },
            { "max_score", question.MaxScore },
            { "sample_answer", question.SampleAnswer ?? "" },
            { "questions", question.Questions ?? new List<string>() },
            { "answer_times", question.AnswerTimes ?? new List<int>() },
            { "created_at", FieldValue.ServerTimestamp }
        };

        if (question.Explanation != null)
        {
            var explanationData = new Dictionary<string, object>
            {
                { "translation", question.Explanation.Translation ?? "" },
                { "context_translation", question.Explanation.ContextTranslation ?? "" },
                { "questions_translation", question.Explanation.QuestionsTranslation ?? new List<string>() },
                { "sample_answers", question.Explanation.SampleAnswers ?? new List<string>() },
                { "sample_answers_translation", question.Explanation.SampleAnswersTranslation ?? new List<string>() }
            };

            if (question.Explanation.Keywords != null)
            {
                var keywordsData = question.Explanation.Keywords.Select(k => new Dictionary<string, object>
                {
                    { "word", k.Word ?? "" },
                    { "ipa", k.Ipa ?? "" },
                    { "meaning", k.Meaning ?? "" }
                }).ToList();
                explanationData.Add("keywords", keywordsData);
            }
            else
            {
                explanationData.Add("keywords", new List<object>());
            }

            data.Add("explanation", explanationData);
        }

        await docRef.SetAsync(data, SetOptions.Overwrite);

        // Evict cache
        EvictCache(question);
    }

    public async Task<bool> UpdateAsync(string id, SpeakingQuestion question)
    {
        var docRef = _firestoreDb.Collection(CollectionName).Document(id);
        var snapshot = await docRef.GetSnapshotAsync();
        if (!snapshot.Exists) return false;

        var data = new Dictionary<string, object>
        {
            { "task_type",          question.TaskType ?? "" },
            { "task_number",        question.TaskNumber },
            { "prompt_text",        question.PromptText ?? "" },
            { "prompt_image_url",   question.PromptImageUrl ?? "" },
            { "prompt_audio_url",   question.PromptAudioUrl ?? "" },
            { "image_url",          question.ImageUrl ?? "" },
            { "audio_url",          question.AudioUrl ?? "" },
            { "preparation_time",   question.PreparationTime },
            { "response_time",      question.ResponseTime },
            { "difficulty",         question.Difficulty ?? "medium" },
            { "ai_prompt",          question.AiPrompt ?? "" },
            { "scoring_criteria",   question.ScoringCriteria ?? new List<string>() },
            { "exam_set_id",        question.ExamSetId ?? "" },
            { "topic",              question.Topic ?? "" },
            { "is_practice",        question.IsPractice },
            { "is_exam",            question.IsExam },
            { "max_score",          question.MaxScore },
            { "sample_answer",      question.SampleAnswer ?? "" },
            { "questions",          question.Questions ?? new List<string>() },
            { "answer_times",       question.AnswerTimes ?? new List<int>() },
        };

        if (question.Explanation != null)
        {
            var explanationData = new Dictionary<string, object>
            {
                { "translation",              question.Explanation.Translation ?? "" },
                { "context_translation",      question.Explanation.ContextTranslation ?? "" },
                { "questions_translation",    question.Explanation.QuestionsTranslation ?? new List<string>() },
                { "sample_answers",           question.Explanation.SampleAnswers ?? new List<string>() },
                { "sample_answers_translation", question.Explanation.SampleAnswersTranslation ?? new List<string>() },
                { "keywords", question.Explanation.Keywords?.Select(k => new Dictionary<string, object>
                    { { "word", k.Word ?? "" }, { "ipa", k.Ipa ?? "" }, { "meaning", k.Meaning ?? "" } }).ToList<object>()
                    ?? new List<object>() }
            };
            data["explanation"] = explanationData;
        }

        await docRef.UpdateAsync(data);
        EvictCache(question);
        return true;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var docRef = _firestoreDb.Collection(CollectionName).Document(id);
        var snapshot = await docRef.GetSnapshotAsync();
        if (!snapshot.Exists) return false;

        var question = MapToDomain(snapshot);
        await docRef.DeleteAsync();

        // Evict cache
        EvictCache(question);
        return true;
    }

    private void EvictCache(SpeakingQuestion question)
    {
        _cache.Remove("speaking_questions_all");
        _cache.Remove($"speaking_questions_task_{question.TaskNumber}");
        _cache.Remove($"speaking_question_id_{question.Id}");

        // Evict general list caches
        _cache.Remove("speaking_questions_filter_true_true");
        _cache.Remove("speaking_questions_filter_true_false");
        _cache.Remove("speaking_questions_filter_false_true");
        _cache.Remove("speaking_questions_filter_false_false");
        _cache.Remove("speaking_questions_filter_null_null");
        _cache.Remove("speaking_questions_filter_true_null");
        _cache.Remove("speaking_questions_filter_false_null");
        _cache.Remove("speaking_questions_filter_null_true");
        _cache.Remove("speaking_questions_filter_null_false");

        _cache.Remove("speaking_questions_count_true_true");
        _cache.Remove("speaking_questions_count_true_false");
        _cache.Remove("speaking_questions_count_false_true");
        _cache.Remove("speaking_questions_count_false_false");
        _cache.Remove("speaking_questions_count_null_null");
        _cache.Remove("speaking_questions_count_true_null");
        _cache.Remove("speaking_questions_count_false_null");
        _cache.Remove("speaking_questions_count_null_true");
        _cache.Remove("speaking_questions_count_null_false");

        if (!string.IsNullOrEmpty(question.ExamSetId))
        {
            _cache.Remove($"speaking_questions_exam_set_{question.ExamSetId}");
        }
    }
}
