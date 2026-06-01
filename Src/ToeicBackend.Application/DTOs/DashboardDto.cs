using System;
using System.Collections.Generic;

namespace ToeicBackend.Application.DTOs;

public class DashboardDto
{
    public int TotalUsers { get; set; }
    public string TotalUsersTrend { get; set; } = "+0.0%";
    
    public int TotalQuestions { get; set; }
    public string TotalQuestionsTrend { get; set; } = "+0";

    public int TotalVocabulary { get; set; }
    public string TotalVocabularyTrend { get; set; } = "+0";

    public int TotalGrammar { get; set; }
    public string TotalGrammarTrend { get; set; } = "+0";

    public List<int> MonthlyAttempts { get; set; } = new();
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
}

public class RecentActivityDto
{
    public string Text { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string Badge { get; set; } = string.Empty;
    public string Bc { get; set; } = string.Empty;
}
