namespace ToeicBackend.Application.Interfaces;

public interface IAiService
{
    Task<string> AnalyzeSentenceAsync(string sentence, string targetWord, string situation);
    Task<string> GenerateScenarioAsync(string word, string meaning);
}
