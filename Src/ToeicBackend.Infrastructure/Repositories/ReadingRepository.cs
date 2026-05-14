using Google.Cloud.Firestore;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Domain.Entities.Reading;

namespace ToeicBackend.Infrastructure.Repositories;

public class ReadingRepository : IReadingRepository
{
    private readonly FirestoreDb _firestoreDb;
    private const string CollectionName = "questions"; // use existing questions collection (part field)

    public ReadingRepository(FirestoreDb firestoreDb)
    {
        _firestoreDb = firestoreDb;
    }

    public async Task<IEnumerable<Part5Question>> GetRandomPart5QuestionsAsync(int count)
    {
        // Query only documents where part == 5
        Query query = _firestoreDb.Collection(CollectionName).WhereEqualTo("part", 5);
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
                // ensure it's part 5
                if (snap.ContainsField("part") && snap.GetValue<int>("part") == 5)
                {
                    list.Add(MapToDomain(snap));
                }
            }
        }
        return list;
    }

    public async Task<IEnumerable<Part5Question>> GetQuestionsByIdsAsync(IEnumerable<string> ids)
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
                    if (qsnap.Exists && qsnap.ContainsField("part") && qsnap.GetValue<int>("part") == 6)
                    {
                        passage.Questions.Add(MapToDomain(qsnap));
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
                    if (qsnap.Exists && qsnap.ContainsField("part") && qsnap.GetValue<int>("part") == 7)
                    {
                        passage.Questions.Add(MapToDomain(qsnap));
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

    private Part5Question MapToDomain(DocumentSnapshot doc)
    {
        var q = new Part5Question { Id = doc.Id };
        if (doc.ContainsField("id")) q.Id = doc.GetValue<string>("id");
        if (doc.ContainsField("question_text")) q.QuestionText = doc.GetValue<string>("question_text");

        // Many entries store options as an array: ["A. ...","B. ...",...]
        if (doc.ContainsField("options"))
        {
            try
            {
                var opts = doc.GetValue<List<string>>("options") ?? new List<string>();
                if (opts.Count > 0) q.OptionA = TrimOptionPrefix(opts.ElementAtOrDefault(0));
                if (opts.Count > 1) q.OptionB = TrimOptionPrefix(opts.ElementAtOrDefault(1));
                if (opts.Count > 2) q.OptionC = TrimOptionPrefix(opts.ElementAtOrDefault(2));
                if (opts.Count > 3) q.OptionD = TrimOptionPrefix(opts.ElementAtOrDefault(3));
            }
            catch
            {
                // fallback to individual fields
                if (doc.ContainsField("option_a")) q.OptionA = doc.GetValue<string>("option_a");
                if (doc.ContainsField("option_b")) q.OptionB = doc.GetValue<string>("option_b");
                if (doc.ContainsField("option_c")) q.OptionC = doc.GetValue<string>("option_c");
                if (doc.ContainsField("option_d")) q.OptionD = doc.GetValue<string>("option_d");
            }
        }
        else
        {
            if (doc.ContainsField("option_a")) q.OptionA = doc.GetValue<string>("option_a");
            if (doc.ContainsField("option_b")) q.OptionB = doc.GetValue<string>("option_b");
            if (doc.ContainsField("option_c")) q.OptionC = doc.GetValue<string>("option_c");
            if (doc.ContainsField("option_d")) q.OptionD = doc.GetValue<string>("option_d");
        }
        if (doc.ContainsField("correct_answer")) q.CorrectAnswer = doc.GetValue<string>("correct_answer");
        if (doc.ContainsField("explanation"))
            q.Explanation = doc.GetValue<string?>("explanation");

        return q;
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
