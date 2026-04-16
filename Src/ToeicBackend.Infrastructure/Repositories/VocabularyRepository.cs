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

    public async Task<IEnumerable<Vocabulary>> GetAllAsync()
    {
        var snapshot = await _firestoreDb.Collection(CollectionName).GetSnapshotAsync();
        return snapshot.Documents.Select(MapToDomain);
    }

    public async Task<IEnumerable<Vocabulary>> GetByTopicAsync(string topic)
    {
        var snapshot = await _firestoreDb.Collection(CollectionName)
            .WhereEqualTo("topic", topic)
            .GetSnapshotAsync();
            
        return snapshot.Documents.Select(MapToDomain);
    }

    public async Task<IEnumerable<Vocabulary>> GetByTopicAndLevelAsync(string topic, string level)
    {
        var snapshot = await _firestoreDb.Collection(CollectionName)
            .WhereEqualTo("topic", topic)
            .WhereEqualTo("level", level)
            .GetSnapshotAsync();
            
        return snapshot.Documents.Select(MapToDomain);
    }

    public async Task<Vocabulary?> GetByIdAsync(string id)
    {
        var docRef = _firestoreDb.Collection(CollectionName).Document(id);
        var snapshot = await docRef.GetSnapshotAsync();
        
        if (!snapshot.Exists) return null;
        
        return MapToDomain(snapshot);
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
