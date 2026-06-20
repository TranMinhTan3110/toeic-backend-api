namespace ToeicBackend.Application.DTOs;

public class VocabularyDto
{
    public string Id { get; set; } = string.Empty;
    public string Word { get; set; } = string.Empty;
    public string? Phonetic { get; set; }
    public string WordType { get; set; } = string.Empty;
    public string DefinitionEn { get; set; } = string.Empty;
    public string DefinitionVi { get; set; } = string.Empty;
    public List<VocabularyExampleDto> Examples { get; set; } = new();
    public string? AudioUrl { get; set; }
    public string? ImageUrl { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public List<string> Synonyms { get; set; } = new();
    public List<string> Antonyms { get; set; } = new();
    public List<string> Collocations { get; set; } = new();
    public string Frequency { get; set; } = string.Empty;
    public bool IsStarred { get; set; } = false;
}

public class VocabularyExampleDto
{
    public string Sentence { get; set; } = string.Empty;
    public string SentenceVi { get; set; } = string.Empty;
}
