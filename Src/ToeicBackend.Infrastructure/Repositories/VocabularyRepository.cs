using Google.Cloud.Firestore;
using ToeicBackend.Application.Interfaces;
using ToeicBackend.Domain.Entities;

namespace ToeicBackend.Infrastructure.Repositories;

public class VocabularyRepository : IVocabularyRepository
{
    private readonly FirestoreDb _firestoreDb;
    private const string CollectionName = "vocabulary";

    public VocabularyRepository(FirestoreDb firestoreDb)
    {
        _firestoreDb = firestoreDb;
    }

    public async Task<IEnumerable<Vocabulary>> GetFilteredAsync(string? topic, string? level)
    {
     
        topic = topic?.Trim().ToLowerInvariant();
        level = level?.Trim();

        Console.WriteLine($"[DEBUG] Querying Firestore - Collection: '{CollectionName}', Topic: '{topic}', Level: '{level}'");

        Query query = _firestoreDb.Collection(CollectionName);

        if (!string.IsNullOrEmpty(topic))
        {
            query = query.WhereEqualTo("topic", topic);
        }

        if (!string.IsNullOrEmpty(level))
        {
            query = query.WhereEqualTo("level", level);
        }

        var snapshot = await query.GetSnapshotAsync();
        Console.WriteLine($"[DEBUG] Firestore returned {snapshot.Documents.Count} documents.");

        return snapshot.Documents.Select(MapToDomain);
    }

    public async Task<Vocabulary?> GetByIdAsync(string id)
    {
        var docRef = _firestoreDb.Collection(CollectionName).Document(id);
        var snapshot = await docRef.GetSnapshotAsync();
        
        if (!snapshot.Exists) return null;
        
        return MapToDomain(snapshot);
    }

    public async Task<IEnumerable<string>> GetTopicsAsync()
    {
        var snapshot = await _firestoreDb.Collection(CollectionName).Select("topic").GetSnapshotAsync();
        return snapshot.Documents
            .Where(doc => doc.ContainsField("topic"))
            .Select(doc => doc.GetValue<string>("topic"))
            .Distinct()
            .OrderBy(t => t);
    }

    public async Task<IEnumerable<string>> GetLevelsAsync()
    {
        var snapshot = await _firestoreDb.Collection(CollectionName).Select("level").GetSnapshotAsync();
        return snapshot.Documents
            .Where(doc => doc.ContainsField("level"))
            .Select(doc => doc.GetValue<string>("level"))
            .Distinct()
            .OrderBy(l => l);
    }

    public async Task<int> GetCountAsync()
    {
        var countSnapshot = await _firestoreDb.Collection(CollectionName).Count().GetSnapshotAsync();
        return (int)countSnapshot.Count;
    }

    public async Task<IEnumerable<Vocabulary>> GetByIdsAsync(IEnumerable<string> ids)
    {
        if (ids == null || !ids.Any()) return Enumerable.Empty<Vocabulary>();
        var tasks = ids.Select(GetByIdAsync);
        var results = await Task.WhenAll(tasks);
        return results.Where(r => r != null)!;
    }


    private Vocabulary MapToDomain(DocumentSnapshot doc)
    {
        var voc = new Vocabulary
        {
            Id = doc.Id
        };
        
        if (doc.ContainsField("id")) voc.Id = doc.GetValue<string>("id");
        if (doc.ContainsField("word")) voc.Word = doc.GetValue<string>("word");
        if (doc.ContainsField("phonetic")) voc.Phonetic = doc.GetValue<string>("phonetic");
        if (doc.ContainsField("word_type")) voc.WordType = doc.GetValue<string>("word_type");
        if (doc.ContainsField("definition_en")) voc.DefinitionEn = doc.GetValue<string>("definition_en");
        if (doc.ContainsField("definition_vi")) voc.DefinitionVi = doc.GetValue<string>("definition_vi");
        if (doc.ContainsField("audio_url")) voc.AudioUrl = doc.GetValue<string?>("audio_url");
        if (doc.ContainsField("image_url")) voc.ImageUrl = doc.GetValue<string?>("image_url");
        if (doc.ContainsField("topic")) voc.Topic = doc.GetValue<string>("topic");
        if (doc.ContainsField("level")) voc.Level = doc.GetValue<string>("level");
        if (doc.ContainsField("frequency")) voc.Frequency = doc.GetValue<string>("frequency");
        
        if (doc.ContainsField("synonyms")) voc.Synonyms = doc.GetValue<List<string>>("synonyms") ?? new();
        if (doc.ContainsField("antonyms")) voc.Antonyms = doc.GetValue<List<string>>("antonyms") ?? new();
        if (doc.ContainsField("collocations")) voc.Collocations = doc.GetValue<List<string>>("collocations") ?? new();

        if (doc.ContainsField("examples"))
        {
            var dictList = doc.GetValue<List<Dictionary<string, object>>>("examples");
            if (dictList != null)
            {
                foreach (var dict in dictList)
                {
                    var ex = new VocabularyExample();
                    if (dict.TryGetValue("sentence", out var sentenceObj) && sentenceObj is string sentenceStr) ex.Sentence = sentenceStr;
                    if (dict.TryGetValue("sentence_vi", out var sentenceViObj) && sentenceViObj is string sentenceViStr) ex.SentenceVi = sentenceViStr;
                    voc.Examples.Add(ex);
                }
            }
        }

        return voc;
    }
}
