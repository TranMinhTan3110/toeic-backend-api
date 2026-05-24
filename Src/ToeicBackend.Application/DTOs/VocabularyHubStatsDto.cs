namespace ToeicBackend.Application.DTOs;

public class VocabularyHubStatsDto
{
    public int StarredCount { get; set; }
    public int DueCount { get; set; }
    public int StudiedCount { get; set; }
    public int MasteredCount { get; set; }
    public int TotalCount { get; set; }
    public int DailyTargetCount { get; set; } = 500;
}
