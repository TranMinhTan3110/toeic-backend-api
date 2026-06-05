using Google.Cloud.Firestore;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Infrastructure.Repositories;

public class ExamRepository : IExamRepository
{
    private readonly FirestoreDb _firestoreDb;
    private const string QuestionsCollection = "questions";
    private const string GroupsCollection = "question_groups";
    private const string CollectionName = "exams";

    public ExamRepository(FirestoreDb firestoreDb)
    {
        _firestoreDb = firestoreDb;
    }

    public async Task<IEnumerable<ListeningQuestion>> GetExamQuestionsAsync(string examId)
    {
        var snapshot = await _firestoreDb.Collection(QuestionsCollection)
            .WhereEqualTo("exam_id", examId)
            .WhereEqualTo("is_for_exam", true)
            .GetSnapshotAsync();

        return snapshot.Documents
            .Select(MapToQuestion)
            .Where(q => q.IsForPractice == false);
    }

    public async Task<IEnumerable<QuestionGroup>> GetExamGroupsAsync(string examId)
    {
        // 1. Lấy tất cả các câu hỏi thuộc đề thi này
        var questionsSnapshot = await _firestoreDb.Collection(QuestionsCollection)
            .WhereEqualTo("exam_id", examId)
            .WhereEqualTo("is_for_exam", true)
            .GetSnapshotAsync();

        var groupIds = questionsSnapshot.Documents
            .Select(MapToQuestion)
            .Where(q => q.IsForPractice == false)
            .Where(q => q.Part == 3 || q.Part == 4 || q.Part == 6 || q.Part == 7)
            .Where(q => !string.IsNullOrEmpty(q.GroupId))
            .Select(q => q.GroupId!)
            .Distinct()
            .ToList();

        var groups = new List<QuestionGroup>();

        // 2. Query groups theo chuỗi id (Firestore IN query max 10 items)
        for (int i = 0; i < groupIds.Count; i += 10)
        {
            var chunk = groupIds.Skip(i).Take(10).ToList();
            if (chunk.Any())
            {
                var groupSnapshot = await _firestoreDb.Collection(GroupsCollection)
                    .WhereIn(FieldPath.DocumentId, chunk)
                    .GetSnapshotAsync();
                
                groups.AddRange(groupSnapshot.Documents.Select(MapToGroup));
            }
        }

        return groups;
    }

    public async Task<IEnumerable<Exam>> GetAllAsync()
    {
        var snapshot = await _firestoreDb.Collection(CollectionName).GetSnapshotAsync();
        var exams = snapshot.Documents.Select(MapToDomain).ToList();
        
        var tasks = exams.Select(async exam => {
            exam.Attempts = await GetAttemptsCountAsync(exam.Id);
        });
        await Task.WhenAll(tasks);
        
        return exams;
    }

    public async Task<Exam?> GetByIdAsync(string id)
    {
        var docRef = _firestoreDb.Collection(CollectionName).Document(id);
        var snapshot = await docRef.GetSnapshotAsync();

        if (!snapshot.Exists) return null;

        var exam = MapToDomain(snapshot);
        exam.Attempts = await GetAttemptsCountAsync(exam.Id);
        return exam;
    }

    public async Task<IEnumerable<Exam>> GetByFilterAsync(bool? isExam, bool? isPractice)
    {
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
        var exams = snapshot.Documents.Select(MapToDomain).ToList();
        
        var tasks = exams.Select(async exam => {
            exam.Attempts = await GetAttemptsCountAsync(exam.Id);
        });
        await Task.WhenAll(tasks);
        
        return exams;
    }

    private async Task<int> GetAttemptsCountAsync(string examId)
    {
        try
        {
            var speakingSnap = await _firestoreDb.Collection("speaking_exam_history")
                .WhereEqualTo("exam_set_id", examId)
                .Count().GetSnapshotAsync();
            
            var writingSnap = await _firestoreDb.Collection("writing_exam_history")
                .WhereEqualTo("exam_set_id", examId)
                .Count().GetSnapshotAsync();
                
            return (int)((speakingSnap.Count ?? 0) + (writingSnap.Count ?? 0));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ExamRepository] Error getting attempts for {examId}: {ex.Message}");
            return 0;
        }
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

    private Exam MapToDomain(DocumentSnapshot doc)
    {
        var exam = new Exam
        {
            Id = doc.Id
        };

        if (doc.ContainsField("title")) exam.Title = doc.GetValue<string>("title");
        if (doc.ContainsField("description")) exam.Description = doc.GetValue<string>("description");
        if (doc.ContainsField("difficulty")) exam.Difficulty = doc.GetValue<string>("difficulty");
        if (doc.ContainsField("duration")) exam.Duration = doc.GetValue<int>("duration");
        if (doc.ContainsField("year")) exam.Year = doc.GetValue<int>("year");
        if (doc.ContainsField("image_url")) exam.ImageUrl = doc.GetValue<string?>("image_url");
        if (doc.ContainsField("audio_url")) exam.AudioUrl = doc.GetValue<string?>("audio_url");
        if (doc.ContainsField("is_exam")) exam.IsExam = doc.GetValue<bool>("is_exam");
        if (doc.ContainsField("is_practice")) exam.IsPractice = doc.GetValue<bool>("is_practice");
        if (doc.ContainsField("is_premium")) exam.IsPremium = doc.GetValue<bool>("is_premium");
        if (doc.ContainsField("is_published")) exam.IsPublished = doc.GetValue<bool>("is_published");
        if (doc.ContainsField("exam_type")) exam.ExamType = doc.GetValue<string>("exam_type");
        if (doc.ContainsField("attempts")) exam.Attempts = doc.GetValue<int>("attempts");
        
        if (doc.ContainsField("question_ids"))
        {
            try {
                exam.QuestionIds = doc.GetValue<List<string>>("question_ids") ?? new();
            } catch {
                var list = doc.GetValue<object>("question_ids") as List<object>;
                if (list != null) exam.QuestionIds = list.Select(o => o.ToString() ?? "").ToList();
            }
        }

        return exam;
    }

    public async Task<string> AddExamAsync(Exam exam)
    {
        var docRef = string.IsNullOrEmpty(exam.Id)
            ? _firestoreDb.Collection(CollectionName).Document()
            : _firestoreDb.Collection(CollectionName).Document(exam.Id);

        var data = new Dictionary<string, object>
        {
            { "id", docRef.Id },
            { "title", exam.Title },
            { "description", exam.Description ?? "" },
            { "difficulty", exam.Difficulty },
            { "duration", exam.Duration },
            { "year", exam.Year },
            { "image_url", exam.ImageUrl ?? "" },
            { "audio_url", exam.AudioUrl ?? "" },
            { "is_exam", exam.IsExam },
            { "is_practice", exam.IsPractice },
            { "is_premium", exam.IsPremium },
            { "is_published", exam.IsPublished },
            { "exam_type", exam.ExamType ?? "full" },
            { "question_ids", exam.QuestionIds }
        };

        await docRef.SetAsync(data);
        return docRef.Id;
    }

    public async Task<bool> UpdateExamAsync(Exam exam)
    {
        var docRef = _firestoreDb.Collection(CollectionName).Document(exam.Id);
        var snapshot = await docRef.GetSnapshotAsync();
        if (!snapshot.Exists) return false;

        var updates = new Dictionary<string, object>();

        if (exam.Title != null)       updates["title"]       = exam.Title;
        if (exam.Description != null) updates["description"] = exam.Description;
        if (exam.Difficulty != null)  updates["difficulty"]  = exam.Difficulty;
        if (exam.Duration > 0)        updates["duration"]    = exam.Duration;
        if (exam.Year > 0)            updates["year"]        = exam.Year;
        if (exam.ImageUrl != null)    updates["image_url"]   = exam.ImageUrl;
        if (exam.AudioUrl != null)    updates["audio_url"]   = exam.AudioUrl;
        if (exam.ExamType != null)    updates["exam_type"]   = exam.ExamType;

        // bool fields — always update if explicitly set
        updates["is_exam"]      = exam.IsExam;
        updates["is_practice"]  = exam.IsPractice;
        updates["is_premium"]   = exam.IsPremium;
        updates["is_published"] = exam.IsPublished;

        await docRef.UpdateAsync(updates);
        return true;
    }

    public async Task<bool> DeleteExamAsync(string id)
    {
        var docRef = _firestoreDb.Collection(CollectionName).Document(id);
        var snapshot = await docRef.GetSnapshotAsync();
        if (!snapshot.Exists) return false;
        await docRef.DeleteAsync();
        return true;
    }

    private const string FullTestHistoryCollection = "user_results";

    public async Task<string> AddFullTestHistoryAsync(FullTestHistory history)
    {
        if (string.IsNullOrEmpty(history.Id))
        {
            var docRef = await _firestoreDb.Collection(FullTestHistoryCollection).AddAsync(history);
            history.Id = docRef.Id;
            return docRef.Id;
        }
        else
        {
            var docRef = _firestoreDb.Collection(FullTestHistoryCollection).Document(history.Id);
            await docRef.SetAsync(history, SetOptions.Overwrite);
            return history.Id;
        }
    }

    public async Task<IEnumerable<FullTestHistory>> GetFullTestHistoryByUserIdAsync(string userId)
    {
        var snapshot = await _firestoreDb.Collection(FullTestHistoryCollection)
            .WhereEqualTo("user_id", userId)
            .GetSnapshotAsync();

        return snapshot.Documents
            .Select(doc => doc.ConvertTo<FullTestHistory>())
            .OrderByDescending(h => h.CompletedAt);
    }

    public async Task<FullTestHistory?> GetFullTestHistoryByIdAsync(string id)
    {
        var docRef = _firestoreDb.Collection(FullTestHistoryCollection).Document(id);
        var snapshot = await docRef.GetSnapshotAsync();

        if (!snapshot.Exists) return null;

        return snapshot.ConvertTo<FullTestHistory>();
    }
}
