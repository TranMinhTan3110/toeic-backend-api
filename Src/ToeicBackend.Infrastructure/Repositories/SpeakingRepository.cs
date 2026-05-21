using Google.Cloud.Firestore;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Infrastructure.Repositories;

public class SpeakingRepository : ISpeakingRepository
{
    private readonly FirestoreDb _firestoreDb;
    private const string CollectionName = "speaking_questions";

    public SpeakingRepository(FirestoreDb firestoreDb)
    {
        _firestoreDb = firestoreDb;
    }

    public async Task<IEnumerable<SpeakingQuestion>> GetAllAsync()
    {
        var snapshot = await _firestoreDb.Collection(CollectionName).GetSnapshotAsync();
        return snapshot.Documents.Select(MapToDomain);
    }

    public async Task<IEnumerable<SpeakingQuestion>> GetByTaskNumberAsync(int taskNumber)
    {
        var snapshot = await _firestoreDb.Collection(CollectionName)
            .WhereEqualTo("task_number", taskNumber)
            .GetSnapshotAsync();
        return snapshot.Documents.Select(MapToDomain);
    }

    public async Task<SpeakingQuestion?> GetByIdAsync(string id)
    {
        var docRef = _firestoreDb.Collection(CollectionName).Document(id);
        var snapshot = await docRef.GetSnapshotAsync();
        
        if (!snapshot.Exists) return null;
        
        return MapToDomain(snapshot);
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
        if (doc.ContainsField("preparation_time")) question.PreparationTime = doc.GetValue<int>("preparation_time");
        if (doc.ContainsField("response_time")) question.ResponseTime = doc.GetValue<int>("response_time");
        if (doc.ContainsField("difficulty")) question.Difficulty = doc.GetValue<string>("difficulty");
        if (doc.ContainsField("ai_prompt")) question.AiPrompt = doc.GetValue<string>("ai_prompt");
        if (doc.ContainsField("scoring_criteria")) question.ScoringCriteria = doc.GetValue<List<string>>("scoring_criteria") ?? new();
        if (doc.ContainsField("exam_set_id")) question.ExamSetId = doc.GetValue<string?>("exam_set_id");
        if (doc.ContainsField("topic")) question.Topic = doc.GetValue<string?>("topic");
        if (doc.ContainsField("is_practice")) question.IsPractice = doc.GetValue<bool>("is_practice");
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

        return question;
    }
}
