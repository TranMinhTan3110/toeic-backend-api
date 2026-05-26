using System.ComponentModel.DataAnnotations;

namespace ToeicBackend.Application.DTOs;

public class CreateVocabularyDto
{
    [Required(ErrorMessage = "Từ vựng không được để trống")]
    public string Word { get; set; } = string.Empty;

    public string? Phonetic { get; set; }

    [Required(ErrorMessage = "Loại từ không được để trống")]
    public string WordType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Định nghĩa tiếng Anh không được để trống")]
    public string DefinitionEn { get; set; } = string.Empty;

    [Required(ErrorMessage = "Định nghĩa tiếng Việt không được để trống")]
    public string DefinitionVi { get; set; } = string.Empty;

    public string? AudioUrl { get; set; }
    public string? ImageUrl { get; set; }

    [Required(ErrorMessage = "Chủ đề không được để trống")]
    public string Topic { get; set; } = string.Empty;

    [Required(ErrorMessage = "Cấp độ không được để trống")]
    public string Level { get; set; } = string.Empty;

    public string Frequency { get; set; } = "medium";

    public List<string> Synonyms { get; set; } = new();
    public List<string> Antonyms { get; set; } = new();
    public List<string> Collocations { get; set; } = new();
    
    public List<VocabularyExampleDto> Examples { get; set; } = new();
}
