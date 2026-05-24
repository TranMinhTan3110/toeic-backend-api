namespace ToeicBackend.Application.DTOs;

public class UpdateProgressDto
{
    public string VocabularyId { get; set; } = string.Empty;
    public int Quality { get; set; } // 0-5 (0: Quên, 5: Rất nhớ)
}
