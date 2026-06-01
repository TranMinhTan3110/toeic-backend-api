using Google.Cloud.Firestore;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Infrastructure.Repositories;

public class WritingQuestionRepository : IWritingQuestionRepository
{
    private readonly FirestoreDb _firestoreDb;
    private const string CollectionName = "writing_questions";

    public WritingQuestionRepository(FirestoreDb firestoreDb)
    {
        _firestoreDb = firestoreDb;
    }

    public async Task<IEnumerable<WritingQuestion>> GetAllAsync()
    {
        Console.WriteLine($"[DEBUG] Querying Firestore - Collection: '{CollectionName}'");
        var snapshot = await _firestoreDb.Collection(CollectionName).GetSnapshotAsync();
        Console.WriteLine($"[DEBUG] Firestore returned {snapshot.Documents.Count} documents.");
        return snapshot.Documents.Select(MapToDomain);
    }

    public async Task<WritingQuestion?> GetByIdAsync(string id)
    {
        var docRef = _firestoreDb.Collection(CollectionName).Document(id);
        var snapshot = await docRef.GetSnapshotAsync();
        
        if (!snapshot.Exists) return null;
        
        return MapToDomain(snapshot);
    }

    public async Task<IEnumerable<WritingQuestion>> GetByTaskTypeAsync(string taskType)
    {
        taskType = taskType?.Trim().ToLowerInvariant() ?? "";
        Console.WriteLine($"[DEBUG] Querying by task_type: '{taskType}'");
        
        var query = _firestoreDb.Collection(CollectionName).WhereEqualTo("task_type", taskType);
        var snapshot = await query.GetSnapshotAsync();
        
        return snapshot.Documents.Select(MapToDomain);
    }

    public async Task<IEnumerable<WritingQuestion>> GetByTaskNumberAsync(int taskNumber)
    {
        Console.WriteLine($"[DEBUG] Querying by task_number: {taskNumber}");
        
        var query = _firestoreDb.Collection(CollectionName).WhereEqualTo("task_number", taskNumber);
        var snapshot = await query.GetSnapshotAsync();
        
        return snapshot.Documents.Select(MapToDomain);
    }

    public async Task<IEnumerable<WritingQuestion>> GetByDifficultyAsync(string difficulty)
    {
        difficulty = difficulty?.Trim().ToLowerInvariant() ?? "";
        Console.WriteLine($"[DEBUG] Querying by difficulty: '{difficulty}'");
        
        var query = _firestoreDb.Collection(CollectionName).WhereEqualTo("difficulty", difficulty);
        var snapshot = await query.GetSnapshotAsync();
        
        return snapshot.Documents.Select(MapToDomain);
    }

    public async Task<IEnumerable<WritingQuestion>> GetPracticeQuestionsAsync()
    {
        Console.WriteLine($"[DEBUG] Querying practice questions");
        
        var query = _firestoreDb.Collection(CollectionName).WhereEqualTo("is_practice", true);
        var snapshot = await query.GetSnapshotAsync();
        
        return snapshot.Documents.Select(MapToDomain);
    }

    public async Task<IEnumerable<WritingQuestion>> GetPracticeByTaskTypeAsync(string taskType)
    {
        taskType = taskType?.Trim().ToLowerInvariant() ?? "";
        Console.WriteLine($"[DEBUG] Querying practice questions by task_type: '{taskType}'");
        
        var query = _firestoreDb.Collection(CollectionName)
            .WhereEqualTo("task_type", taskType)
            .WhereEqualTo("is_practice", true);
        var snapshot = await query.GetSnapshotAsync();
        
        return snapshot.Documents.Select(MapToDomain);
    }

    public async Task<IEnumerable<WritingQuestion>> GetExamByTaskTypeAsync(string taskType)
    {
        taskType = taskType?.Trim().ToLowerInvariant() ?? "";
        Console.WriteLine($"[DEBUG] Querying exam questions by task_type: '{taskType}'");
        
        var query = _firestoreDb.Collection(CollectionName)
            .WhereEqualTo("task_type", taskType)
            .WhereEqualTo("is_practice", false);
        var snapshot = await query.GetSnapshotAsync();
        
        return snapshot.Documents.Select(MapToDomain);
    }

    public async Task<IEnumerable<WritingQuestion>> GetByExamSetIdAsync(string examSetId)
    {
        Console.WriteLine($"[DEBUG] Querying writing questions by exam_set_id: '{examSetId}'");
        var query = _firestoreDb.Collection(CollectionName).WhereEqualTo("exam_set_id", examSetId);
        var snapshot = await query.GetSnapshotAsync();
        return snapshot.Documents.Select(MapToDomain);
    }

    public async Task<IEnumerable<string>> GetAvailableTaskTypesAsync()
    {
        var snapshot = await _firestoreDb.Collection(CollectionName).Select("task_type").GetSnapshotAsync();
        return snapshot.Documents
            .Where(doc => doc.ContainsField("task_type"))
            .Select(doc => doc.GetValue<string>("task_type"))
            .Distinct()
            .OrderBy(t => t);
    }

    public async Task<IEnumerable<WritingQuestion>> GetQuestionsByExamSetIdAsync(string examSetId)
    {
        Console.WriteLine($"[DEBUG] Querying writing questions by exam_set_id: '{examSetId}'");
        var query = _firestoreDb.Collection(CollectionName)
            .WhereEqualTo("exam_set_id", examSetId);
        
        var snapshot = await query.GetSnapshotAsync();
        return snapshot.Documents.Select(MapToDomain).OrderBy(wq => wq.TaskNumber).ToList();
    }

    public async Task<Dictionary<string, int>> GetPracticeCountsByTaskTypeAsync()
    {
        var taskTypes = new[] { "write_sentence", "respond_email", "opinion_essay" };
        var result = new Dictionary<string, int>();

        foreach (var taskType in taskTypes)
        {
            var aggregateQuery = _firestoreDb.Collection(CollectionName)
                .WhereEqualTo("task_type", taskType)
                .WhereEqualTo("is_practice", true)
                .Count();

            var snapshot = await aggregateQuery.GetSnapshotAsync();
            result[taskType] = (int)(snapshot.Count ?? 0);
        }

        return result;
    }

    public async Task<string> AddAsync(WritingQuestion question)
    {
        var docRef = string.IsNullOrWhiteSpace(question.Id)
            ? _firestoreDb.Collection(CollectionName).Document()
            : _firestoreDb.Collection(CollectionName).Document(question.Id);

        question.Id = docRef.Id;
        var data = ToFirestoreData(question, includeCreatedAt: true);
        await docRef.SetAsync(data, SetOptions.Overwrite);
        return docRef.Id;
    }

    public async Task<bool> UpdateAsync(string id, WritingQuestion question)
    {
        var docRef = _firestoreDb.Collection(CollectionName).Document(id);
        var snapshot = await docRef.GetSnapshotAsync();
        if (!snapshot.Exists) return false;

        question.Id = id;
        var data = ToFirestoreData(question, includeCreatedAt: false);
        data["updated_at"] = FieldValue.ServerTimestamp;
        await docRef.SetAsync(data, SetOptions.MergeAll);
        return true;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var docRef = _firestoreDb.Collection(CollectionName).Document(id);
        var snapshot = await docRef.GetSnapshotAsync();
        if (!snapshot.Exists) return false;

        await docRef.DeleteAsync();
        return true;
    }

    private static Dictionary<string, object> ToFirestoreData(WritingQuestion question, bool includeCreatedAt)
    {
        var data = new Dictionary<string, object>
        {
            { "id", question.Id },
            { "task_number", question.TaskNumber },
            { "task_type", question.TaskType ?? "" },
            { "prompt_text", question.PromptText ?? "" },
            { "prompt_image_url", question.PromptImageUrl ?? "" },
            { "given_words", question.GivenWords ?? new List<string>() },
            { "email_content", question.EmailContent ?? "" },
            { "email_questions", question.EmailQuestions ?? new List<string>() },
            { "time_limit", question.TimeLimit },
            { "min_words", question.MinWords ?? 0 },
            { "max_words", question.MaxWords ?? 0 },
            { "max_score", question.MaxScore },
            { "scoring_criteria", question.ScoringCriteria ?? new List<string>() },
            { "sample_answer", question.SampleAnswer ?? "" },
            { "sample_answer_translation", question.SampleAnswerTranslation ?? "" },
            { "explanation_vietnamese", question.ExplanationVietnamese ?? "" },
            { "ai_prompt", question.AiPrompt ?? "" },
            { "topic", question.Topic ?? "" },
            { "difficulty", question.Difficulty ?? "medium" },
            { "exam_set_id", question.ExamSetId ?? "" },
            { "is_practice", question.IsPractice }
        };

        if (includeCreatedAt)
            data["created_at"] = FieldValue.ServerTimestamp;

        return data;
    }

    private WritingQuestion MapToDomain(DocumentSnapshot doc)
    {
        var wq = new WritingQuestion
        {
            Id = doc.Id
        };

        if (doc.ContainsField("id")) wq.Id = doc.GetValue<string>("id");
        if (doc.ContainsField("task_number")) wq.TaskNumber = doc.GetValue<int>("task_number");
        if (doc.ContainsField("task_type")) wq.TaskType = doc.GetValue<string>("task_type");
        if (doc.ContainsField("prompt_text")) wq.PromptText = doc.GetValue<string>("prompt_text");
        if (doc.ContainsField("prompt_image_url")) wq.PromptImageUrl = doc.GetValue<string?>("prompt_image_url");
        if (doc.ContainsField("email_content")) wq.EmailContent = doc.GetValue<string?>("email_content");
        if (doc.ContainsField("time_limit")) wq.TimeLimit = doc.GetValue<int>("time_limit");
        if (doc.ContainsField("min_words")) wq.MinWords = doc.GetValue<int?>("min_words");
        if (doc.ContainsField("max_words")) wq.MaxWords = doc.GetValue<int?>("max_words");
        if (doc.ContainsField("max_score")) wq.MaxScore = doc.GetValue<int>("max_score");
        if (doc.ContainsField("sample_answer")) wq.SampleAnswer = doc.GetValue<string?>("sample_answer");
        if (doc.ContainsField("sample_answer_translation")) wq.SampleAnswerTranslation = doc.GetValue<string?>("sample_answer_translation");
        if (doc.ContainsField("explanation_vietnamese")) wq.ExplanationVietnamese = doc.GetValue<string?>("explanation_vietnamese");
        if (doc.ContainsField("ai_prompt")) wq.AiPrompt = doc.GetValue<string?>("ai_prompt");
        if (doc.ContainsField("topic")) wq.Topic = doc.GetValue<string?>("topic");
        if (doc.ContainsField("difficulty")) wq.Difficulty = doc.GetValue<string>("difficulty");
        if (doc.ContainsField("exam_set_id")) wq.ExamSetId = doc.GetValue<string?>("exam_set_id");
        if (doc.ContainsField("is_practice")) wq.IsPractice = doc.GetValue<bool>("is_practice");
        
        // Skip created_at - Firestore Timestamp not directly convertible to DateTime
        // Frontend doesn't need this info anyway

        if (doc.ContainsField("given_words")) 
            wq.GivenWords = doc.GetValue<List<string>>("given_words") ?? new();
        
        if (doc.ContainsField("email_questions")) 
            wq.EmailQuestions = doc.GetValue<List<string>>("email_questions") ?? new();
        
        if (doc.ContainsField("scoring_criteria")) 
            wq.ScoringCriteria = doc.GetValue<List<string>>("scoring_criteria") ?? new();

        return wq;
    }
}
