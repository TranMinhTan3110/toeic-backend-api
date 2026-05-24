using Google.Cloud.Firestore;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Infrastructure.Repositories;

public class ExamRepository : IExamRepository
{
    private readonly FirestoreDb _firestoreDb;
    private const string CollectionName = "exams";

    public ExamRepository(FirestoreDb firestoreDb)
    {
        _firestoreDb = firestoreDb;
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
