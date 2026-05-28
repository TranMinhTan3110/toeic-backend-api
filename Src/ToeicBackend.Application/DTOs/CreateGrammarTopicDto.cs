using System.ComponentModel.DataAnnotations;

namespace ToeicBackend.Application.DTOs;

public class CreateGrammarTopicDto
{
    [Required(ErrorMessage = "Tiêu đề tiếng Việt không được để trống")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tiêu đề tiếng Anh không được để trống")]
    public string TitleEn { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phân loại không được để trống")]
    public string Category { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;

    public List<int> RelatedParts { get; set; } = new();

    [Required(ErrorMessage = "Độ khó không được để trống")]
    public string Difficulty { get; set; } = "basic"; // basic, intermediate, advanced

    public int Order { get; set; }
    public bool IsPublished { get; set; } = true;
}
