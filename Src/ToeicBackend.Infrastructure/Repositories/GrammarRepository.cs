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
        if (doc.ContainsField("grammar_topic_id")) question.GrammarTopicId = doc.GetValue<string?>("grammar_topic_id");

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

    public async Task<GrammarTopic> CreateTopicAsync(GrammarTopic topic)
    {
        if (string.IsNullOrEmpty(topic.Id))
        {
            topic.Id = "topic_" + Guid.NewGuid().ToString().Substring(0, 8);
        }
        var docRef = _firestoreDb.Collection(TopicsCollection).Document(topic.Id);
        var data = MapTopicToFirestore(topic);
        await docRef.SetAsync(data);
        return topic;
    }

    public async Task<GrammarTopic?> UpdateTopicAsync(string id, GrammarTopic topic)
    {
        topic.Id = id;
        var docRef = _firestoreDb.Collection(TopicsCollection).Document(id);
        var snapshot = await docRef.GetSnapshotAsync();
        if (!snapshot.Exists) return null;

        var data = MapTopicToFirestore(topic);
        await docRef.SetAsync(data, SetOptions.Overwrite);
        return topic;
    }

    public async Task<bool> DeleteTopicAsync(string id)
    {
        var docRef = _firestoreDb.Collection(TopicsCollection).Document(id);
        var snapshot = await docRef.GetSnapshotAsync();
        if (!snapshot.Exists) return false;

        // Xóa chủ đề
        await docRef.DeleteAsync();

        // Xóa bài học lý thuyết liên quan
        var lessonSnapshot = await _firestoreDb.Collection(LessonsCollection)
            .WhereEqualTo("topic_id", id)
            .GetSnapshotAsync();
        
        foreach (var doc in lessonSnapshot.Documents)
        {
            await doc.Reference.DeleteAsync();
        }

        // Xóa bài tập thực hành liên quan
        var questionsSnapshot = await _firestoreDb.Collection(QuestionsCollection)
            .WhereEqualTo("grammar_topic_id", id)
            .GetSnapshotAsync();

        foreach (var doc in questionsSnapshot.Documents)
        {
            await doc.Reference.DeleteAsync();
        }

        return true;
    }

    public async Task<GrammarLesson> SaveLessonAsync(GrammarLesson lesson)
    {
        if (string.IsNullOrEmpty(lesson.TopicId))
        {
            throw new ArgumentException("TopicId must be provided to save a lesson.");
        }

        // Tìm bài học lý thuyết cũ của Topic
        var snapshot = await _firestoreDb.Collection(LessonsCollection)
            .WhereEqualTo("topic_id", lesson.TopicId)
            .Limit(1)
            .GetSnapshotAsync();

        DocumentReference docRef;
        if (snapshot.Documents.Count > 0)
        {
            lesson.Id = snapshot.Documents[0].Id;
            docRef = snapshot.Documents[0].Reference;
        }
        else
        {
            lesson.Id = "lesson_" + Guid.NewGuid().ToString().Substring(0, 8);
            docRef = _firestoreDb.Collection(LessonsCollection).Document(lesson.Id);
        }

        var data = MapLessonToFirestore(lesson);
        await docRef.SetAsync(data, SetOptions.Overwrite);
        return lesson;
    }

    public async Task<ListeningQuestion> AddExerciseAsync(ListeningQuestion question)
    {
        if (string.IsNullOrEmpty(question.Id))
        {
            question.Id = "q_grammar_" + Guid.NewGuid().ToString().Substring(0, 8);
        }
        question.CreatedAt = DateTime.UtcNow;
        var docRef = _firestoreDb.Collection(QuestionsCollection).Document(question.Id);
        var data = MapQuestionToFirestore(question);
        await docRef.SetAsync(data);
        return question;
    }

    public async Task<ListeningQuestion?> UpdateExerciseAsync(string id, ListeningQuestion question)
    {
        question.Id = id;
        var docRef = _firestoreDb.Collection(QuestionsCollection).Document(id);
        var snapshot = await docRef.GetSnapshotAsync();
        if (!snapshot.Exists) return null;

        var data = MapQuestionToFirestore(question);
        await docRef.SetAsync(data, SetOptions.Overwrite);
        return question;
    }

    public async Task<bool> DeleteExerciseAsync(string id)
    {
        var docRef = _firestoreDb.Collection(QuestionsCollection).Document(id);
        var snapshot = await docRef.GetSnapshotAsync();
        if (!snapshot.Exists) return false;

        string? topicId = null;
        if (snapshot.ContainsField("grammar_topic_id"))
        {
            topicId = snapshot.GetValue<string?>("grammar_topic_id");
        }

        await docRef.DeleteAsync();

        if (!string.IsNullOrEmpty(topicId))
        {
            var topicRef = _firestoreDb.Collection(TopicsCollection).Document(topicId);
            var topicSnap = await topicRef.GetSnapshotAsync();
            if (topicSnap.Exists)
            {
                var topic = MapToTopic(topicSnap);
                topic.ExerciseCount = Math.Max(0, topic.ExerciseCount - 1);
                await topicRef.SetAsync(MapTopicToFirestore(topic), SetOptions.Overwrite);
            }
        }

        return true;
    }

    private Dictionary<string, object> MapTopicToFirestore(GrammarTopic topic)
    {
        return new Dictionary<string, object>
        {
            { "id", topic.Id },
            { "title", topic.Title },
            { "title_en", topic.TitleEn },
            { "category", topic.Category },
            { "description", topic.Description },
            { "icon", topic.Icon },
            { "lesson_count", topic.LessonCount },
            { "exercise_count", topic.ExerciseCount },
            { "related_parts", topic.RelatedParts },
            { "difficulty", topic.Difficulty },
            { "order", topic.Order },
            { "is_published", topic.IsPublished }
        };
    }

    private Dictionary<string, object> MapLessonToFirestore(GrammarLesson lesson)
    {
        return new Dictionary<string, object>
        {
            { "id", lesson.Id },
            { "topic_id", lesson.TopicId },
            { "title", lesson.Title },
            { "content", lesson.Content },
            { "order", lesson.Order }
        };
    }

    private Dictionary<string, object> MapQuestionToFirestore(ListeningQuestion question)
    {
        var dict = new Dictionary<string, object>
        {
            { "part", question.Part },
            { "options", question.Options },
            { "correct_answer", question.CorrectAnswer },
            { "difficulty", question.Difficulty },
            { "skill", question.Skill },
            { "is_for_exam", question.IsForExam },
            { "is_for_practice", question.IsForPractice }
        };

        if (question.QuestionText != null) dict["question_text"] = question.QuestionText;
        if (question.ImageUrl != null) dict["image_url"] = question.ImageUrl;
        if (question.AudioUrl != null) dict["audio_url"] = question.AudioUrl;
        if (question.Explanation != null) dict["explanation"] = question.Explanation;
        if (question.ExplanationVi != null) dict["explanation_vi"] = question.ExplanationVi;
        if (question.Script != null) dict["script"] = question.Script;
        if (question.GroupId != null) dict["group_id"] = question.GroupId;
        if (question.GrammarTopicId != null) dict["grammar_topic_id"] = question.GrammarTopicId;
        if (question.CreatedAt != null) dict["created_at"] = Timestamp.FromDateTime(question.CreatedAt.Value.ToUniversalTime());

        return dict;
    }
}
