using System.ComponentModel.DataAnnotations;

namespace ToeicBackend.Application.DTOs;

public class CreateGrammarLessonDto
{
    [Required(ErrorMessage = "Tiêu đề bài học không được để trống")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nội dung bài học không được để trống")]
    public string Content { get; set; } = string.Empty; // Markdown text

    public int Order { get; set; }
}
