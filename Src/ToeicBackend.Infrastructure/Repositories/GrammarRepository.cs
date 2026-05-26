using Google.Cloud.Firestore;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Infrastructure.Repositories;

public class GrammarRepository : IGrammarRepository
{
    private readonly FirestoreDb _firestoreDb;
    private const string TopicsCollection = "grammar_topics";
    private const string LessonsCollection = "grammar_lessons";
    private const string QuestionsCollection = "questions";

    public GrammarRepository(FirestoreDb firestoreDb)
    {
        _firestoreDb = firestoreDb;
    }

    public async Task<IEnumerable<GrammarTopic>> GetTopicsAsync()
    {
        Console.WriteLine($"[DEBUG] Querying Firestore - Collection: '{TopicsCollection}'");
        var snapshot = await _firestoreDb.Collection(TopicsCollection)
            .OrderBy("order")
            .GetSnapshotAsync();

        return snapshot.Documents
            .Select(MapToTopic)
            .Where(t => t.IsPublished);
    }

    public async Task<GrammarTopic?> GetTopicByIdAsync(string id)
    {
        var docRef = _firestoreDb.Collection(TopicsCollection).Document(id);
        var snapshot = await docRef.GetSnapshotAsync();
        if (!snapshot.Exists) return null;

        return MapToTopic(snapshot);
    }

    public async Task<GrammarLesson?> GetLessonByTopicIdAsync(string topicId)
    {
        Console.WriteLine($"[DEBUG] Querying Firestore - Collection: '{LessonsCollection}', TopicId: '{topicId}'");
        var snapshot = await _firestoreDb.Collection(LessonsCollection)
            .WhereEqualTo("topic_id", topicId)
            .Limit(1)
            .GetSnapshotAsync();

        if (snapshot.Documents.Count == 0) return null;

        return MapToLesson(snapshot.Documents[0]);
    }

    public async Task<IEnumerable<ListeningQuestion>> GetQuestionsByTopicIdAsync(string topicId)
    {
        Console.WriteLine($"[DEBUG] Querying Firestore - Collection: '{QuestionsCollection}', GrammarTopicId: '{topicId}'");
        var snapshot = await _firestoreDb.Collection(QuestionsCollection)
            .WhereEqualTo("grammar_topic_id", topicId)
            .WhereEqualTo("is_for_practice", true)
            .GetSnapshotAsync();

        return snapshot.Documents.Select(MapToQuestion);
    }

    private GrammarTopic MapToTopic(DocumentSnapshot doc)
    {
        var topic = new GrammarTopic
        {
            Id = doc.Id
        };

        if (doc.ContainsField("id")) topic.Id = doc.GetValue<string>("id");
        if (doc.ContainsField("title")) topic.Title = doc.GetValue<string>("title");
        if (doc.ContainsField("title_en")) topic.TitleEn = doc.GetValue<string>("title_en");
        if (doc.ContainsField("category")) topic.Category = doc.GetValue<string>("category");
        if (doc.ContainsField("description")) topic.Description = doc.GetValue<string>("description");
        if (doc.ContainsField("icon")) topic.Icon = doc.GetValue<string>("icon");
        if (doc.ContainsField("lesson_count")) topic.LessonCount = doc.GetValue<int>("lesson_count");
        if (doc.ContainsField("exercise_count")) topic.ExerciseCount = doc.GetValue<int>("exercise_count");
        if (doc.ContainsField("difficulty")) topic.Difficulty = doc.GetValue<string>("difficulty");
        if (doc.ContainsField("order")) topic.Order = doc.GetValue<int>("order");
        if (doc.ContainsField("is_published")) topic.IsPublished = doc.GetValue<bool>("is_published");

        if (doc.ContainsField("related_parts"))
        {
            try
            {
                topic.RelatedParts = doc.GetValue<List<int>>("related_parts") ?? new();
            }
            catch
            {
                var list = doc.GetValue<object>("related_parts") as List<object>;
                if (list != null)
                {
                    topic.RelatedParts = list.Select(o => Convert.ToInt32(o)).ToList();
                }
            }
        }

        return topic;
    }

    private GrammarLesson MapToLesson(DocumentSnapshot doc)
    {
        var lesson = new GrammarLesson
        {
            Id = doc.Id
        };

        if (doc.ContainsField("id")) lesson.Id = doc.GetValue<string>("id");
        if (doc.ContainsField("topic_id")) lesson.TopicId = doc.GetValue<string>("topic_id");
        if (doc.ContainsField("title")) lesson.Title = doc.GetValue<string>("title");
        if (doc.ContainsField("content")) lesson.Content = doc.GetValue<string>("content");
        if (doc.ContainsField("order")) lesson.Order = doc.GetValue<int>("order");

        return lesson;
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

        if (doc.ContainsField("options"))
        {
            try
            {
                question.Options = doc.GetValue<List<string>>("options") ?? new();
            }
            catch
            {
                var list = doc.GetValue<object>("options") as List<object>;
                if (list != null) question.Options = list.Select(o => o.ToString() ?? "").ToList();
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
                if (DateTime.TryParse(doc.GetValue<string>("created_at"), out var dt))
                {
                    question.CreatedAt = dt;
                }
            }
        }

        return question;
    }
}
