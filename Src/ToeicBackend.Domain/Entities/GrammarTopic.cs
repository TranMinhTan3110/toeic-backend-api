namespace ToeicBackend.Domain.Entities;

public class GrammarTopic
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int LessonCount { get; set; }
    public int ExerciseCount { get; set; }
    public List<int> RelatedParts { get; set; } = new();
    public string Difficulty { get; set; } = "basic"; // basic, intermediate, advanced
    public int Order { get; set; }
    public bool IsPublished { get; set; } = true;
}
