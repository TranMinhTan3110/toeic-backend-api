using Microsoft.Extensions.DependencyInjection;
using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.Interfaces;

namespace ToeicBackend.Infrastructure.Services;

public class HybridAiService : IAiService
{
    private readonly IAiService _geminiService;
    private readonly IAiService _openAiService;

    public HybridAiService(
        [FromKeyedServices("gemini")] IAiService geminiService,
        [FromKeyedServices("openai")] IAiService openAiService)
    {
        _geminiService = geminiService;
        _openAiService = openAiService;
    }

    public Task<string> AnalyzeSentenceAsync(string sentence, string targetWord, string situation)
    {
        return _openAiService.AnalyzeSentenceAsync(sentence, targetWord, situation);
    }

    public Task<string> GenerateScenarioAsync(string word, string meaning)
    {
        return _openAiService.GenerateScenarioAsync(word, meaning);
    }

    public Task<string> GenerateGrammarLessonAsync(string topicTitle, string topicTitleEn)
    {
        return _openAiService.GenerateGrammarLessonAsync(topicTitle, topicTitleEn);
    }

    public Task<string> GenerateGrammarExercisesAsync(string topicTitle, string topicTitleEn, int count)
    {
        return _openAiService.GenerateGrammarExercisesAsync(topicTitle, topicTitleEn, count);
    }

    public async Task<SpeakingEvaluationDto> EvaluateSpeakingAsync(
        string taskPrompt,
        IReadOnlyList<string> sampleAnswers,
        string userTranscript,
        int taskNumber,
        byte[]? audioBytes = null,
        string? mimeType = null)
    {
        try
        {
            Console.WriteLine("[HYBRID AI] Đang thử chấm Speaking bằng Gemini...");
            var result = await _geminiService.EvaluateSpeakingAsync(taskPrompt, sampleAnswers, userTranscript, taskNumber, audioBytes, mimeType);
            
            // Nếu kết quả trả về bị lỗi hoặc rỗng (do lỗi 429 hoặc lỗi xử lý nội bộ nhưng bắt được trong GeminiService)
            if (result == null || (result.Feedback != null && result.Feedback.Contains("Không thể phân tích phản hồi AI")))
            {
                throw new Exception("Gemini returned a failed/empty evaluation (likely due to 429 or quota limit).");
            }
            
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HYBRID AI WARNING] Gemini gặp sự cố hoặc quá tải: {ex.Message}. Tự động chuyển hướng (fallback) sang chấm bằng OpenAI...");
            return await _openAiService.EvaluateSpeakingAsync(taskPrompt, sampleAnswers, userTranscript, taskNumber, audioBytes, mimeType);
        }
    }

    public Task<WritingEvaluationDto> EvaluateWritingAsync(
        string taskPrompt,
        string taskType,
        IReadOnlyList<string> givenWords,
        string? emailContent,
        IReadOnlyList<string> emailQuestions,
        IReadOnlyList<string> sampleAnswers,
        string userAnswer)
    {
        return _openAiService.EvaluateWritingAsync(taskPrompt, taskType, givenWords, emailContent, emailQuestions, sampleAnswers, userAnswer);
    }
}
