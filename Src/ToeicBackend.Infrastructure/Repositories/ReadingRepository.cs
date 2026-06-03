using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Domain.Entities;
using ToeicBackend.Domain.Entities.Reading;


namespace ToeicBackend.Infrastructure.Repositories;

public class ReadingRepository : IReadingRepository
{
    private readonly FirestoreDb _firestoreDb;
    private const string QuestionsCollection = "questions";
    private const string GroupsCollection = "question_groups";

    private const string CollectionName = "questions"; // use existing questions collection (part field)

    public ReadingRepository(FirestoreDb firestoreDb)
    {
        _firestoreDb = firestoreDb;
    }

    public async Task<IEnumerable<ReadingQuestion>> GetQuestionsByPartAsync(int part)
    {
        var snapshot = await _firestoreDb.Collection(QuestionsCollection)
            .WhereEqualTo("skill", "reading")
            .WhereEqualTo("part", part)
            .WhereEqualTo("is_for_exam", false)
            .WhereEqualTo("is_for_practice", true)
            .GetSnapshotAsync();

        return snapshot.Documents.Select(MapToQuestion);
    }

    public async Task<ReadingQuestion?> GetQuestionByIdAsync(string id)
    {
        var docRef = _firestoreDb.Collection(QuestionsCollection).Document(id);
        var snapshot = await docRef.GetSnapshotAsync();
        return snapshot.Exists ? MapToQuestion(snapshot) : null;
    }

    public async Task<IEnumerable<ReadingQuestion>> GetAllQuestionsAdminAsync()
    {
        var snapshot = await _firestoreDb.Collection(QuestionsCollection)
            .WhereEqualTo("skill", "reading")
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

    public async Task<int> GetQuestionCountByPartAsync(int part)
    {
        var aggregateQuery = _firestoreDb.Collection(QuestionsCollection)
            .WhereEqualTo("skill", "reading")
            .WhereEqualTo("part", part)
            .WhereEqualTo("is_for_exam", false)
            .WhereEqualTo("is_for_practice", true)
            .Count();

        var snapshot = await aggregateQuery.GetSnapshotAsync();
        return (int)(snapshot.Count ?? 0);
    }

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

    public async Task<IEnumerable<ReadingQuestion>> GetQuestionsByIdsAsync(List<string> ids)
    {
        if (ids == null || !ids.Any()) return Enumerable.Empty<ReadingQuestion>();

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

    private ReadingQuestion MapToQuestion(DocumentSnapshot doc)
    {
        var question = new ReadingQuestion { Id = doc.Id };

        if (doc.ContainsField("part")) question.Part = doc.GetValue<int>("part");
        if (doc.ContainsField("question_text")) question.QuestionText = doc.GetValue<string?>("question_text");
        if (doc.ContainsField("image_url")) question.ImageUrl = doc.GetValue<string?>("image_url");
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
        if (doc.ContainsField("grammar_topic_id")) question.GrammarTopicId = doc.GetValue<string?>("grammar_topic_id");

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

    public async Task<string> AddQuestionAsync(ReadingQuestion question)
    {
        var docRef = string.IsNullOrEmpty(question.Id)
            ? _firestoreDb.Collection(QuestionsCollection).Document()
            : _firestoreDb.Collection(QuestionsCollection).Document(question.Id);

        var data = new Dictionary<string, object>
        {
            { "part", question.Part },
            { "question_text", question.QuestionText ?? "" },
            { "image_url", question.ImageUrl ?? "" },
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

        if (!string.IsNullOrEmpty(question.GrammarTopicId))
        {
            data.Add("grammar_topic_id", question.GrammarTopicId);
        }

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
            { "audio_url", "" }, // not used
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

    public async Task<bool> DeleteQuestionAsync(string id)
    {
        var docRef = _firestoreDb.Collection(QuestionsCollection).Document(id);
        var snapshot = await docRef.GetSnapshotAsync();
        if (!snapshot.Exists) return false;
        await docRef.DeleteAsync();
        return true;
    }

    public async Task<bool> UpdateQuestionAsync(ReadingQuestion question)
    {
        if (string.IsNullOrEmpty(question.Id)) return false;
        var docRef = _firestoreDb.Collection(QuestionsCollection).Document(question.Id);

        var data = new Dictionary<string, object>
        {
            { "part", question.Part },
            { "question_text", question.QuestionText ?? "" },
            { "image_url", question.ImageUrl ?? "" },
            { "options", question.Options },
            { "correct_answer", question.CorrectAnswer },
            { "explanation", question.Explanation ?? "" },
            { "explanation_vi", question.ExplanationVi ?? "" },
            { "script", question.Script ?? "" },
            { "group_id", question.GroupId ?? "" },
            { "difficulty", question.Difficulty },
            { "skill", question.Skill },
            { "is_for_exam", question.IsForExam },
            { "is_for_practice", question.IsForPractice }
        };

        if (!string.IsNullOrEmpty(question.GrammarTopicId))
        {
            data.Add("grammar_topic_id", question.GrammarTopicId);
        }

        await docRef.SetAsync(data, SetOptions.MergeAll);
        return true;
    }

    public async Task<bool> UpdateGroupAsync(QuestionGroup group)
    {
        if (string.IsNullOrEmpty(group.Id)) return false;
        var docRef = _firestoreDb.Collection(GroupsCollection).Document(group.Id);

        var data = new Dictionary<string, object>
        {
            { "part", group.Part },
            { "passage_text", group.PassageText ?? "" },
            { "script", group.Script ?? "" },
            { "image_url", group.ImageUrl ?? "" },
            { "question_count", group.QuestionCount },
            { "source", group.Source ?? "" },
            { "question_ids", group.QuestionIds }
        };

        await docRef.SetAsync(data, SetOptions.MergeAll);
        return true;
    }

    public async Task<IEnumerable<Part5Question>> GetRandomPart5QuestionsAsync(int count)
    {
        // Query only documents where part == 5 and is_for_practice == true
        Query query = _firestoreDb.Collection(CollectionName)
            .WhereEqualTo("part", 5)
            .WhereEqualTo("is_for_practice", true);
        var snapshot = await query.GetSnapshotAsync();
        var all = snapshot.Documents.Select(MapToDomain).ToList();

        var rnd = new Random();
        return all.OrderBy(_ => rnd.Next()).Take(count).ToList();
    }

    public async Task<IEnumerable<Part5Question>> GetPart5QuestionsByIdsAsync(IEnumerable<string> ids)
    {
        var list = new List<Part5Question>();
        foreach (var id in ids)
        {
            var docRef = _firestoreDb.Collection(CollectionName).Document(id);
            var snap = await docRef.GetSnapshotAsync();
            if (snap.Exists)
            {
                // ensure it's part 5 and marked for practice
                if (snap.ContainsField("part") && snap.GetValue<int>("part") == 5)
                {
                    if (snap.ContainsField("is_for_practice") && snap.GetValue<bool>("is_for_practice"))
                    {
                        list.Add(MapToDomain(snap));
                    }
                    // if the flag is missing, treat as not-for-practice (do not add)
                }
            }
        }
        return list;
    }

    public async Task<IEnumerable<Part5Question>> GetQuestionsAsPart5ByIdsAsync(IEnumerable<string> ids)
    {
        var list = new List<Part5Question>();
        foreach (var id in ids)
        {
            var docRef = _firestoreDb.Collection(CollectionName).Document(id);
            var snap = await docRef.GetSnapshotAsync();
            if (snap.Exists)
            {
                list.Add(MapToDomain(snap));
            }
        }
        return list;
    }

    public async Task<IEnumerable<ToeicBackend.Domain.Entities.Reading.Part6Passage>> GetPart6PassagesAsync()
    {
        var result = new List<ToeicBackend.Domain.Entities.Reading.Part6Passage>();

        // Read groups from 'question_groups' collection where part == 6
        var groupSnap = await _firestoreDb.Collection("question_groups").WhereEqualTo("part", 6).GetSnapshotAsync();
        foreach (var g in groupSnap.Documents)
        {
            var passage = new ToeicBackend.Domain.Entities.Reading.Part6Passage { Id = g.Id };
            if (g.ContainsField("passage_text")) passage.PassageText = g.GetValue<string>("passage_text");
            // prefer localized field if present (some docs use passage_translation_vi)
            if (g.ContainsField("passage_translation_vi")) passage.PassageTranslation = SafeGetString(g, "passage_translation_vi");
            else if (g.ContainsField("passage_translation")) passage.PassageTranslation = SafeGetString(g, "passage_translation");
            if (g.ContainsField("image_url")) passage.ImageUrl = g.GetValue<string?>("image_url");
            if (g.ContainsField("audio_url")) passage.AudioUrl = g.GetValue<string?>("audio_url");

            if (g.ContainsField("question_ids"))
            {
                var qids = g.GetValue<List<string>>("question_ids") ?? new List<string>();
                passage.QuestionIds = qids;

                // Fetch questions for this group
                foreach (var qid in qids)
                {
                    var qref = _firestoreDb.Collection(CollectionName).Document(qid);
                    var qsnap = await qref.GetSnapshotAsync();
                    if (qsnap.Exists)
                    {
                        // Soft check: if part field exists, verify it matches; otherwise load anyway since it's in question_ids
                        if (qsnap.ContainsField("part"))
                        {
                            try
                            {
                                var part = qsnap.GetValue<int>("part");
                                if (part != 6) continue;
                            }
                            catch
                            {
                                // If part field is not an int, still try to load
                            }
                        }
                            var qdom = MapToDomain(qsnap);
                            // Only include questions marked for practice
                            if (!qdom.IsForPractice) continue;
                            passage.Questions.Add(qdom);
                    }
                }
            }

            // also support 'passages' array field
            if (g.ContainsField("passages"))
            {
                try
                {
                    passage.Passages = g.GetValue<List<Dictionary<string, object?>>>("passages");
                }
                catch
                {
                    // ignore
                }
            }

            result.Add(passage);
        }

        return result;
    }

    public async Task<IEnumerable<ToeicBackend.Domain.Entities.Reading.Part6Passage>> GetPart7PassagesAsync()
    {
        var result = new List<ToeicBackend.Domain.Entities.Reading.Part6Passage>();

        // Read groups from 'question_groups' collection where part == 7
        var groupSnap = await _firestoreDb.Collection("question_groups").WhereEqualTo("part", 7).GetSnapshotAsync();
        foreach (var g in groupSnap.Documents)
        {
            var passage = new ToeicBackend.Domain.Entities.Reading.Part6Passage { Id = g.Id };
            if (g.ContainsField("passage_text")) passage.PassageText = g.GetValue<string>("passage_text");
            // prefer localized field if present (some docs use passage_translation_vi)
            if (g.ContainsField("passage_translation_vi")) passage.PassageTranslation = SafeGetString(g, "passage_translation_vi");
            else if (g.ContainsField("passage_translation")) passage.PassageTranslation = SafeGetString(g, "passage_translation");
            if (g.ContainsField("image_url")) passage.ImageUrl = g.GetValue<string?>("image_url");
            if (g.ContainsField("audio_url")) passage.AudioUrl = g.GetValue<string?>("audio_url");

            if (g.ContainsField("question_ids"))
            {
                var qids = g.GetValue<List<string>>("question_ids") ?? new List<string>();
                passage.QuestionIds = qids;

                foreach (var qid in qids)
                {
                    var qref = _firestoreDb.Collection(CollectionName).Document(qid);
                    var qsnap = await qref.GetSnapshotAsync();
                    if (qsnap.Exists)
                    {
                        // Soft check: if part field exists, verify it matches; otherwise load anyway since it's in question_ids
                        if (qsnap.ContainsField("part"))
                        {
                            try
                            {
                                var part = qsnap.GetValue<int>("part");
                                if (part != 7) continue;
                            }
                            catch
                            {
                                // If part field is not an int, still try to load
                            }
                        }
                            var qdom = MapToDomain(qsnap);
                            // Only include questions marked for practice
                            if (!qdom.IsForPractice) continue;
                            passage.Questions.Add(qdom);
                    }
                }
            }

            if (g.ContainsField("passages"))
            {
                try
                {
                    passage.Passages = g.GetValue<List<Dictionary<string, object?>>>("passages");
                }
                catch
                {
                    // ignore
                }
            }

            result.Add(passage);
        }

        return result;
    }

    // --- History for Reading Practice ---
    private const string ReadingHistoryCollection = "user_reading_history";

    public async Task<string> AddReadingHistoryAsync(ToeicBackend.Domain.Entities.Reading.ReadingHistory history)
    {
        if (string.IsNullOrEmpty(history.Id))
        {
            var docRef = await _firestoreDb.Collection(ReadingHistoryCollection).AddAsync(history);
            history.Id = docRef.Id;
            return docRef.Id;
        }
        else
        {
            var docRef = _firestoreDb.Collection(ReadingHistoryCollection).Document(history.Id);
            await docRef.SetAsync(history, SetOptions.Overwrite);
            return history.Id;
        }
    }

    public async Task<IEnumerable<ToeicBackend.Domain.Entities.Reading.ReadingHistory>> GetReadingHistoryByUserIdAsync(string userId)
    {
        var snapshot = await _firestoreDb.Collection(ReadingHistoryCollection)
            .WhereEqualTo("user_id", userId)
            .GetSnapshotAsync();

        return snapshot.Documents
            .Select(doc => doc.ConvertTo<ToeicBackend.Domain.Entities.Reading.ReadingHistory>())
            .OrderByDescending(h => h.Date);
    }

    public async Task<ToeicBackend.Domain.Entities.Reading.ReadingHistory?> GetReadingHistoryByIdAsync(string id)
    {
        var docRef = _firestoreDb.Collection(ReadingHistoryCollection).Document(id);
        var snapshot = await docRef.GetSnapshotAsync();
        return snapshot.Exists ? snapshot.ConvertTo<ToeicBackend.Domain.Entities.Reading.ReadingHistory>() : null;
    }

    private Part5Question MapToDomain(DocumentSnapshot doc)
    {
        var q = new Part5Question { Id = doc.Id };
        if (doc.ContainsField("id")) q.Id = SafeGetString(doc, "id") ?? q.Id;
        q.QuestionText = SafeGetString(doc, "question_text") ?? string.Empty;

        // Many entries store options as an array: ["A. ...","B. ...",...]
        if (doc.ContainsField("options"))
        {
            var opts = SafeGetStringList(doc, "options");
            if (opts.Count > 0) q.OptionA = TrimOptionPrefix(opts.ElementAtOrDefault(0));
            if (opts.Count > 1) q.OptionB = TrimOptionPrefix(opts.ElementAtOrDefault(1));
            if (opts.Count > 2) q.OptionC = TrimOptionPrefix(opts.ElementAtOrDefault(2));
            if (opts.Count > 3) q.OptionD = TrimOptionPrefix(opts.ElementAtOrDefault(3));
            // fallback to individual fields if options were not strings
            if (string.IsNullOrEmpty(q.OptionA) && doc.ContainsField("option_a")) q.OptionA = SafeGetString(doc, "option_a") ?? string.Empty;
            if (string.IsNullOrEmpty(q.OptionB) && doc.ContainsField("option_b")) q.OptionB = SafeGetString(doc, "option_b") ?? string.Empty;
            if (string.IsNullOrEmpty(q.OptionC) && doc.ContainsField("option_c")) q.OptionC = SafeGetString(doc, "option_c") ?? string.Empty;
            if (string.IsNullOrEmpty(q.OptionD) && doc.ContainsField("option_d")) q.OptionD = SafeGetString(doc, "option_d") ?? string.Empty;
        }
        else
        {
            if (doc.ContainsField("option_a")) q.OptionA = SafeGetString(doc, "option_a") ?? string.Empty;
            if (doc.ContainsField("option_b")) q.OptionB = SafeGetString(doc, "option_b") ?? string.Empty;
            if (doc.ContainsField("option_c")) q.OptionC = SafeGetString(doc, "option_c") ?? string.Empty;
            if (doc.ContainsField("option_d")) q.OptionD = SafeGetString(doc, "option_d") ?? string.Empty;
        }

        q.CorrectAnswer = SafeGetString(doc, "correct_answer");
        // explanation may be a map with subfields (grammar_explanation, grammar_point, translation, option_explanations)
        if (doc.ContainsField("explanation"))
        {
            try
            {
                var raw = doc.GetValue<object>("explanation");
                if (raw is IDictionary<string, object> exMap)
                {
                    if (exMap.TryGetValue("grammar_explanation", out var ge) && ge is string ges) q.GrammarExplanation = ges;
                    if (exMap.TryGetValue("grammar_point", out var gp) && gp is string gps) q.GrammarPoint = gps;
                    if (exMap.TryGetValue("translation", out var tr) && tr is string trs) q.Translation = trs;
                    if (exMap.TryGetValue("option_explanations", out var oe) && oe is IDictionary<string, object> oemap)
                    {
                        var dict = new Dictionary<string, string>();
                        foreach (var kv in oemap)
                        {
                            if (kv.Value is string vs) dict[kv.Key] = vs;
                        }
                        q.OptionExplanations = dict;
                    }
                    // keep raw serialized fallback in Explanation
                    q.Explanation = System.Text.Json.JsonSerializer.Serialize(exMap);
                }
                else
                {
                    q.Explanation = SafeGetString(doc, "explanation");
                }
            }
            catch
            {
                q.Explanation = SafeGetString(doc, "explanation");
            }
        }
        else
        {
            q.Explanation = SafeGetString(doc, "explanation");
        }
        q.ExplanationVi = SafeGetString(doc, "explanation_vi");
        if (doc.ContainsField("is_for_practice"))
        {
            try { q.IsForPractice = doc.GetValue<bool>("is_for_practice"); } catch { q.IsForPractice = false; }
        }

        return q;
    }

    private static string? SafeGetString(DocumentSnapshot doc, string field)
    {
        if (!doc.ContainsField(field)) return null;
        try
        {
            var obj = doc.GetValue<object>(field);
            if (obj == null) return null;
            if (obj is string s) return s;
            if (obj is IDictionary<string, object> dict)
            {
                // try common keys
                if (dict.TryGetValue("text", out var t) && t is string ts) return ts;
                if (dict.TryGetValue("en", out var te) && te is string tes) return tes;
                if (dict.TryGetValue("vi", out var tv) && tv is string tvs) return tvs;
                // fallback to JSON
                return System.Text.Json.JsonSerializer.Serialize(dict);
            }
            if (obj is IEnumerable<object> list)
            {
                return string.Join(" ", list.Select(x => x?.ToString()));
            }
            return obj.ToString();
        }
        catch
        {
            return null;
        }
    }

    private static List<string> SafeGetStringList(DocumentSnapshot doc, string field)
    {
        var result = new List<string>();
        if (!doc.ContainsField(field)) return result;
        try
        {
            var raw = doc.GetValue<object>(field);
            if (raw is IEnumerable<object> list)
            {
                foreach (var item in list)
                {
                    if (item is string s) result.Add(s);
                    else if (item is IDictionary<string, object> dict)
                    {
                        // try common text keys
                        if (dict.TryGetValue("text", out var t) && t is string ts) result.Add(ts);
                        else if (dict.TryGetValue("en", out var te) && te is string tes) result.Add(tes);
                        else result.Add(System.Text.Json.JsonSerializer.Serialize(dict));
                    }
                    else
                    {
                        result.Add(item?.ToString() ?? string.Empty);
                    }
                }
            }
        }
        catch
        {
            // ignore and return empty or partial results
        }
        return result;
    }

    private static string TrimOptionPrefix(string? raw)
    {
        if (string.IsNullOrEmpty(raw)) return string.Empty;
        // Remove leading 'A.' or 'A) ' patterns
        var idx = raw.IndexOf('.');
        if (idx > 0 && idx < 3)
        {
            return raw.Substring(idx + 1).Trim();
        }
        // fallback
        return raw.Trim();
    }
}
