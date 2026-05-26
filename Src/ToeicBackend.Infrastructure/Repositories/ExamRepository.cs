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
            .WhereEqualTo("skill", "listening")
            .WhereEqualTo("exam_id", examId)
            .WhereEqualTo("is_for_exam", true)
            .GetSnapshotAsync();

        return snapshot.Documents.Select(MapToQuestion);
    }

    public async Task<IEnumerable<QuestionGroup>> GetExamGroupsAsync(string examId)
    {
        // 1. Lấy tất cả các câu hỏi của part 3, 4 thuộc đề thi này để lấy danh sách group_id
        var questionsSnapshot = await _firestoreDb.Collection(QuestionsCollection)
            .WhereEqualTo("skill", "listening")
            .WhereEqualTo("exam_id", examId)
            .WhereEqualTo("is_for_exam", true)
            .WhereIn("part", new[] { 3, 4 })
            .GetSnapshotAsync();

        var groupIds = questionsSnapshot.Documents
            .Where(d => d.ContainsField("group_id"))
            .Select(d => d.GetValue<string>("group_id"))
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
        return snapshot.Documents.Select(MapToDomain);
    }

    public async Task<Exam?> GetByIdAsync(string id)
    {
        var docRef = _firestoreDb.Collection(CollectionName).Document(id);
        var snapshot = await docRef.GetSnapshotAsync();

        if (!snapshot.Exists) return null;

        return MapToDomain(snapshot);
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
        return snapshot.Documents.Select(MapToDomain);
    }

    private ListeningQuestion MapToQuestion(DocumentSnapshot doc)
    {
        var question = new ListeningQuestion { Id = doc.Id };

        if (doc.ContainsField("part")) question.Part = doc.GetValue<int>("part");
        if (doc.ContainsField("question_text")) question.QuestionText = doc.GetValue<string?>("question_text");
        if (doc.ContainsField("image_url")) question.ImageUrl = doc.GetValue<string?>("image_url");
        if (doc.ContainsField("audio_url")) question.AudioUrl = doc.GetValue<string?>("audio_url");
        if (doc.ContainsField("correct_answer")) question.CorrectAnswer = doc.GetValue<string>("correct_answer");
        if (doc.ContainsField("explanation")) question.Explanation = doc.GetValue<string?>("explanation");
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
}
