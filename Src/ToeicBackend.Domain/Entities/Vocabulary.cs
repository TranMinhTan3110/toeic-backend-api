namespace ToeicBackend.Domain.Entities;

public class Vocabulary
{
    public string Id { get; set; } = string.Empty;
    public string Word { get; set; } = string.Empty;
    public string? Phonetic { get; set; }
    public string WordType { get; set; } = string.Empty;
    public string DefinitionEn { get; set; } = string.Empty;
    public string DefinitionVi { get; set; } = string.Empty;
    public List<VocabularyExample> Examples { get; set; } = new();
    public string? AudioUrl { get; set; }
    public string? ImageUrl { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public List<string> Synonyms { get; set; } = new();
    public List<string> Antonyms { get; set; } = new();
    public List<string> Collocations { get; set; } = new();
    public List<string> RelatedQuestionIds { get; set; } = new();
    public string Frequency { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
}

public class VocabularyExample
{
    public string Sentence { get; set; } = string.Empty;
    public string SentenceVi { get; set; } = string.Empty;
}
