using Google.Cloud.Firestore;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Infrastructure.Repositories;

public class ListeningRepository : IListeningRepository
{
    private readonly FirestoreDb _firestoreDb;
    private const string QuestionsCollection = "questions";
    private const string GroupsCollection = "question_groups";

    public ListeningRepository(FirestoreDb firestoreDb)
    {
        _firestoreDb = firestoreDb;
    }

    public async Task<IEnumerable<ListeningQuestion>> GetQuestionsByPartAsync(int part)
    {
         var snapshot = await _firestoreDb.Collection(QuestionsCollection)
             .WhereEqualTo("skill", "listening")
             .WhereEqualTo("part", part)
             .WhereEqualTo("is_for_exam", false)      
             .WhereEqualTo("is_for_practice", true)   
             .GetSnapshotAsync();
 
         return snapshot.Documents.Select(MapToQuestion);
    }


    public async Task<ListeningQuestion?> GetQuestionByIdAsync(string id)
    {
        var docRef = _firestoreDb.Collection(QuestionsCollection).Document(id);
        var snapshot = await docRef.GetSnapshotAsync();
        return snapshot.Exists ? MapToQuestion(snapshot) : null;
    }

    public async Task<IEnumerable<ListeningQuestion>> GetAllQuestionsAdminAsync()
    {
        var snapshot = await _firestoreDb.Collection(QuestionsCollection)
            .WhereEqualTo("skill", "listening")
            .GetSnapshotAsync();
        return snapshot.Documents.Select(MapToQuestion);
    }

    public async Task<IEnumerable<QuestionGroup>> GetGroupsByPartAsync(int part)
    {
        var snapshot = await _firestoreDb.Collection(GroupsCollection)
            .WhereEqualTo("part", part)
            .WhereEqualTo("is_for_exam", false)
            .WhereEqualTo("is_for_practice", true)
            .GetSnapshotAsync();

        return snapshot.Documents.Select(MapToGroup);
    }

    /// <summary>
    /// Dùng Firestore Count Aggregation — chỉ tốn 1 read, không tải document nào.
    /// Dùng để hiển thị nhanh số câu ở DetailScreen mà không cần fetch toàn bộ data.
    /// </summary>
    public async Task<int> GetQuestionCountByPartAsync(int part)
    {
        var aggregateQuery = _firestoreDb.Collection(QuestionsCollection)
            .WhereEqualTo("skill", "listening")
            .WhereEqualTo("part", part)
            .WhereEqualTo("is_for_exam", false)
            .WhereEqualTo("is_for_practice", true)
            .Count();

        var snapshot = await aggregateQuery.GetSnapshotAsync();
        return (int)(snapshot.Count ?? 0);
    }

    /// <summary>
    /// Dùng Firestore Count Aggregation — chỉ tốn 1 read, không tải document nào.
    /// </summary>
    public async Task<int> GetGroupCountByPartAsync(int part)
    {
        var aggregateQuery = _firestoreDb.Collection(GroupsCollection)
            .WhereEqualTo("part", part)
            .WhereEqualTo("is_for_exam", false)
            .WhereEqualTo("is_for_practice", true)
            .Count();

        var snapshot = await aggregateQuery.GetSnapshotAsync();
        return (int)(snapshot.Count ?? 0);
    }

    public async Task<QuestionGroup?> GetGroupByIdAsync(string groupId)
    {
        var docRef = _firestoreDb.Collection(GroupsCollection).Document(groupId);
        var snapshot = await docRef.GetSnapshotAsync();
        return snapshot.Exists ? MapToGroup(snapshot) : null;
    }

    public async Task<IEnumerable<ListeningQuestion>> GetQuestionsByIdsAsync(List<string> ids)
    {
        if (ids == null || !ids.Any()) return Enumerable.Empty<ListeningQuestion>();

        // Firestore WhereIn tối đa 30 phần tử mỗi query.
        const int chunkSize = 30;
        var distinctIds = ids.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
        
        var tasks = new List<Task<QuerySnapshot>>();

        for (var i = 0; i < distinctIds.Count; i += chunkSize)
        {
            var chunk = distinctIds.Skip(i).Take(chunkSize).ToList();
            var task = _firestoreDb.Collection(QuestionsCollection)
                .WhereIn(FieldPath.DocumentId, chunk)
                .GetSnapshotAsync();
            tasks.Add(task);
        }

        var snapshots = await Task.WhenAll(tasks);
        return snapshots.SelectMany(s => s.Documents.Select(MapToQuestion)).ToList();
    }

    private ListeningQuestion MapToQuestion(DocumentSnapshot doc)
    {
        var question = new ListeningQuestion { Id = doc.Id };

        if (doc.ContainsField("part")) question.Part = doc.GetValue<int>("part");
        if (doc.ContainsField("question_text")) question.QuestionText = doc.GetValue<string?>("question_text");
        if (doc.ContainsField("image_url")) question.ImageUrl = doc.GetValue<string?>("image_url");
        if (doc.ContainsField("audio_url")) question.AudioUrl = doc.GetValue<string?>("audio_url");
        if (doc.ContainsField("correct_answer")) question.CorrectAnswer = doc.GetValue<string>("correct_answer");
        if (doc.ContainsField("explanation")) question.Explanation = doc.GetValue<object?>("explanation");
        if (doc.ContainsField("explanation_vi")) question.ExplanationVi = doc.GetValue<string?>("explanation_vi");
        if (doc.ContainsField("script")) question.Script = doc.GetValue<string?>("script");
        if (doc.ContainsField("group_id")) question.GroupId = doc.GetValue<string?>("group_id");
        if (doc.ContainsField("difficulty")) question.Difficulty = doc.GetValue<string>("difficulty");
        if (doc.ContainsField("skill")) question.Skill = doc.GetValue<string>("skill");
        if (doc.ContainsField("is_for_exam")) question.IsForExam = doc.GetValue<bool>("is_for_exam");
        if (doc.ContainsField("is_for_practice")) question.IsForPractice = doc.GetValue<bool>("is_for_practice");
        if (doc.ContainsField("exam_id")) question.ExamId = doc.GetValue<string?>("exam_id");

        if (doc.ContainsField("options"))
        {
            try {
                question.Options = doc.GetValue<List<string>>("options") ?? new();
            } catch {
                var list = doc.GetValue<object>("options") as List<object>;
                if (list != null) question.Options = list.Select(o => o.ToString() ?? "").ToList();
            }
        }

        if (doc.ContainsField("created_at"))
        {
            try {
                question.CreatedAt = doc.GetValue<Timestamp>("created_at").ToDateTime();
            } catch {
                if (DateTime.TryParse(doc.GetValue<string>("created_at"), out var dt))
                    question.CreatedAt = dt;
            }
        }

        return question;
    }

    private QuestionGroup MapToGroup(DocumentSnapshot doc)
    {
        var group = new QuestionGroup { Id = doc.Id };

        if (doc.ContainsField("part")) group.Part = doc.GetValue<int>("part");
        if (doc.ContainsField("passage_text")) group.PassageText = doc.GetValue<string?>("passage_text");
        if (doc.ContainsField("script")) group.Script = doc.GetValue<string?>("script");
        if (doc.ContainsField("image_url")) group.ImageUrl = doc.GetValue<string?>("image_url");
        if (doc.ContainsField("audio_url")) group.AudioUrl = doc.GetValue<string?>("audio_url");
        if (doc.ContainsField("question_count")) group.QuestionCount = doc.GetValue<int>("question_count");
        if (doc.ContainsField("source")) group.Source = doc.GetValue<string?>("source");

        if (doc.ContainsField("question_ids"))
        {
            try {
                group.QuestionIds = doc.GetValue<List<string>>("question_ids") ?? new();
            } catch {
                var list = doc.GetValue<object>("question_ids") as List<object>;
                if (list != null) group.QuestionIds = list.Select(o => o.ToString() ?? "").ToList();
            }
        }

        if (doc.ContainsField("created_at"))
        {
            try {
                group.CreatedAt = doc.GetValue<Timestamp>("created_at").ToDateTime();
            } catch {
                if (DateTime.TryParse(doc.GetValue<string>("created_at"), out var dt))
                    group.CreatedAt = dt;
            }
        }

        return group;
    }

    public async Task<string> AddQuestionAsync(ListeningQuestion question)
    {
        var docRef = string.IsNullOrEmpty(question.Id) 
            ? _firestoreDb.Collection(QuestionsCollection).Document() 
            : _firestoreDb.Collection(QuestionsCollection).Document(question.Id);
            
        var data = new Dictionary<string, object>
        {
            { "part", question.Part },
            { "question_text", question.QuestionText ?? "" },
            { "image_url", question.ImageUrl ?? "" },
            { "audio_url", question.AudioUrl ?? "" },
            { "options", question.Options },
            { "correct_answer", question.CorrectAnswer },
            { "explanation", question.Explanation ?? "" },
            { "explanation_vi", question.ExplanationVi ?? "" },
            { "script", question.Script ?? "" },
            { "group_id", question.GroupId ?? "" },
            { "difficulty", question.Difficulty },
            { "skill", question.Skill },
            { "is_for_exam", question.IsForExam },
            { "is_for_practice", question.IsForPractice },
            { "created_at", FieldValue.ServerTimestamp }
        };

        await docRef.SetAsync(data);
        return docRef.Id;
    }

    public async Task<string> AddGroupAsync(QuestionGroup group)
    {
        var docRef = string.IsNullOrEmpty(group.Id) 
            ? _firestoreDb.Collection(GroupsCollection).Document() 
            : _firestoreDb.Collection(GroupsCollection).Document(group.Id);
            
        var data = new Dictionary<string, object>
        {
            { "part", group.Part },
            { "passage_text", group.PassageText ?? "" },
            { "script", group.Script ?? "" },
            { "image_url", group.ImageUrl ?? "" },
            { "audio_url", group.AudioUrl ?? "" },
            { "question_count", group.QuestionCount },
            { "source", group.Source ?? "" },
            { "question_ids", group.QuestionIds },
            { "is_for_exam", false },
            { "is_for_practice", true },
            { "created_at", FieldValue.ServerTimestamp }
        };

        await docRef.SetAsync(data);
        return docRef.Id;
    }

    // --- History Practice Implementations ---
    private const string HistoryCollection = "user_listening_history";

    public async Task<string> AddHistoryAsync(ListeningHistory history)
    {
        if (string.IsNullOrEmpty(history.Id))
        {
            var docRef = await _firestoreDb.Collection(HistoryCollection).AddAsync(history);
            history.Id = docRef.Id;
            return docRef.Id;
        }
        else
        {
            var docRef = _firestoreDb.Collection(HistoryCollection).Document(history.Id);
            await docRef.SetAsync(history, SetOptions.Overwrite);
            return history.Id;
        }
    }

    public async Task<IEnumerable<ListeningHistory>> GetHistoryByUserIdAsync(string userId)
    {
        var snapshot = await _firestoreDb.Collection(HistoryCollection)
            .WhereEqualTo("user_id", userId)
            .GetSnapshotAsync();

        return snapshot.Documents
            .Select(doc => doc.ConvertTo<ListeningHistory>())
            .OrderByDescending(h => h.Date);
    }

    public async Task<ListeningHistory?> GetHistoryByIdAsync(string id)
    {
        var docRef = _firestoreDb.Collection(HistoryCollection).Document(id);
        var snapshot = await docRef.GetSnapshotAsync();
        return snapshot.Exists ? snapshot.ConvertTo<ListeningHistory>() : null;
    }

    public async Task<bool> DeleteQuestionAsync(string id)
    {
        var docRef = _firestoreDb.Collection(QuestionsCollection).Document(id);
        var snapshot = await docRef.GetSnapshotAsync();
        if (!snapshot.Exists) return false;
        await docRef.DeleteAsync();
        return true;
    }
}

