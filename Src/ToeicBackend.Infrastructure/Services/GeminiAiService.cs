using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using ToeicBackend.Application.DTOs;
using ToeicBackend.Application.Interfaces;

namespace ToeicBackend.Infrastructure.Services;

public class GeminiAiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly string _apiKey;
    private readonly string _apiUrl;

    public GeminiAiService(HttpClient httpClient, IConfiguration configuration, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _cache = cache;
        _apiKey = configuration["Gemini:ApiKey"] ?? "";
        _apiUrl = configuration["Gemini:ApiUrl"] ?? "";
    }

    public async Task<string> AnalyzeSentenceAsync(string sentence, string targetWord, string situation)
    {
        // Tạo Cache Key dựa trên nội dung câu, từ vựng và tình huống
        var cacheKey = $"analyze_{sentence.ToLower().Trim()}_{targetWord.ToLower()}_{situation.ToLower()}";
        
        if (_cache.TryGetValue(cacheKey, out string? cachedResult))
        {
            Console.WriteLine($"[CACHE HIT] Trả về kết quả từ bộ nhớ tạm cho câu: {sentence}");
            return cachedResult ?? "";
        }

        var prompt = $@"
Bạn là chuyên gia ngôn ngữ TOEIC. Nhiệm vụ của bạn là chấm điểm câu của học viên.
- Tình huống: '{situation}'
- Từ vựng bắt buộc: '{targetWord}'
- Câu của học viên: '{sentence}'

TUYỆT ĐỐI KHÔNG CHÀO HỎI. KHÔNG GIỚI THIỆU BẢN THÂN.
HÃY TRẢ LỜI NGAY LẬP TỨC VỚI ĐÚNG ĐỊNH DẠNG MARKDOWN BÊN DƯỚI (Không được thiếu bất kỳ mục nào):

### [SCORE]
[X]/10

### [ANALYSIS]
- **Sử dụng từ vựng:** [Học viên có dùng đúng từ '{targetWord}' không?]
- **Trật tự từ & Ngữ pháp:** [Phân tích chi tiết lỗi sai nếu có]
- **Độ phù hợp:** [Có hợp với tình huống '{situation}' không?]

### [REVISION]
- [Câu của học viên sau khi được sửa cho đúng và tự nhiên]

### [SAMPLE]
- [BẮT BUỘC: Viết 1 câu hoàn toàn mới, cực hay, dùng từ '{targetWord}' và cực kỳ phù hợp với tình huống '{situation}']

### [EXPLANATION]
- [Giải thích cấu trúc của câu mẫu đề xuất ở trên để học viên học theo]";

        var result = await CallGeminiAsync(prompt, 4000); 

        // Lưu kết quả vào cache trong 1 tiếng để tái sử dụng
        if (!string.IsNullOrEmpty(result) && !result.Contains("Lỗi khi gọi AI"))
        {
            _cache.Set(cacheKey, result, TimeSpan.FromHours(1));
        }

        return result;
    }


    public async Task<string> GenerateScenarioAsync(string word, string meaning)
    {
        var prompt = $@"Bạn là một giáo viên TOEIC giàu kinh nghiệm. Hãy tạo một thử thách đặt câu cho học viên.
Yêu cầu:
1. Tạo ra một tình huống cụ thể, thực tế trong môi trường công sở (văn phòng, hội nghị, đi công tác) hoặc đời sống chuyên nghiệp.
2. Câu hỏi phải gợi ý một bối cảnh rõ ràng để người dùng dễ dàng đặt câu có chứa từ '{word}' ({meaning}).
3. KHÔNG chào hỏi, KHÔNG lặp lại ví dụ. Chỉ trả về duy nhất câu thử thách.

Ví dụ hay:
- 'Hãy tưởng tượng bạn đang ở sân bay và chuyến bay bị hoãn, hãy đặt một câu để thông báo cho đối tác về sự chậm trễ này.'
- 'Bạn vừa hoàn thành một bản báo cáo quan trọng, hãy đặt một câu để đề nghị sếp kiểm tra lại nó.'

Thử thách cho từ '{word}':";

        return await CallGeminiAsync(prompt, 1000); // Tăng kịch trần để tránh bị cụt câu
    }

    public async Task<string> GenerateGrammarLessonAsync(string topicTitle, string topicTitleEn)
    {
        var prompt = $@"
Bạn là một giáo viên TOEIC hàng đầu thế giới. Hãy biên soạn một bài học lý thuyết ngữ pháp chuyên sâu, khoa học và cực kỳ chi tiết cho chủ đề: '{topicTitle}' (tên tiếng Anh: '{topicTitleEn}').

Yêu cầu bài viết:
1. Viết hoàn toàn bằng định dạng Markdown chuẩn.
2. TUYỆT ĐỐI KHÔNG CHÀO HỎI. KHÔNG GIỚI THIỆU BẢN THÂN. KHÔNG CHÈN PHẦN CẢM ƠN.
3. Phải bao gồm các phần chính sau:
   - # {topicTitle}
   - ## 1. Khái niệm & Cách dùng (Usage): Giải thích ngắn gọn dễ hiểu, gắn liền với các tình huống giao tiếp TOEIC.
   - ## 2. Công thức (Formula): Trình bày rõ ràng công thức cho các thể Khẳng định, Phủ định, Nghi vấn. Dùng in đậm (**) cho công thức.
   - ## 3. Ví dụ mẫu kèm dịch nghĩa tiếng Việt chi tiết.
   - ## 4. Dấu hiệu nhận biết (Signal Words): Liệt kê các từ khóa nhận biết hay gặp trong đề thi TOEIC.
   - ## 5. Lưu ý / Mẹo làm bài thi TOEIC: Những bẫy ngữ pháp liên quan cần tránh trong Part 5 & Part 6.
4. Trình bày thật thoáng đãng, chia các dòng rõ ràng để dễ hiển thị.

Nội dung biên soạn lý thuyết cho '{topicTitle}':";

        return await CallGeminiAsync(prompt, 4000);
    }

    public async Task<string> GenerateGrammarExercisesAsync(string topicTitle, string topicTitleEn, int count)
    {
        var prompt = $@"
Bạn là một chuyên gia khảo thí TOEIC hàng đầu thế giới. Hãy tạo ra đúng {count} câu hỏi trắc nghiệm ngữ pháp TOEIC Part 5 để ôn tập cho chủ đề ngữ pháp: '{topicTitle}' (tên tiếng Anh: '{topicTitleEn}').

Yêu cầu định dạng đầu ra:
- TUYỆT ĐỐI KHÔNG CHÀO HỎI. KHÔNG GIỚI THIỆU BẢN THÂN.
- TRẢ VỀ DUY NHẤT một chuỗi JSON hợp lệ biểu diễn một mảng các đối tượng câu hỏi.
- Không bọc JSON trong các từ khóa ```json ```, chỉ trả về chuỗi văn bản thuần JSON.
- Mỗi câu hỏi phải là một đối tượng có các trường chính xác như sau:
  - ""questionText"": Chuỗi câu hỏi tiếng Anh chứa phần điền khuyết dạng ""_____"" (ví dụ: ""The manager asked his employees to submit their reports _____ Friday."")
  - ""options"": Một mảng gồm đúng 4 phần tử chuỗi, bắt buộc phải có tiền tố ""A. "", ""B. "", ""C. "", ""D. "" (ví dụ: [""A. for"", ""B. since"", ""C. during"", ""D. in""])
  - ""correctAnswer"": Đáp án đúng, nhận một trong các giá trị sau: ""A"", ""B"", ""C"", ""D""
  - ""difficulty"": Mức độ khó của câu hỏi, nhận một trong các giá trị: ""easy"", ""medium"", ""hard""
  - ""explanationVi"": Giải thích chi tiết câu hỏi bằng tiếng Việt (vì sao chọn đáp án này, dịch nghĩa toàn bộ câu, các bẫy cần tránh)

Nội dung JSON của bộ câu hỏi thực hành cho '{topicTitle}':";

        return await CallGeminiAsync(prompt, 4000);
    }

    public async Task<SpeakingEvaluationDto> EvaluateSpeakingAsync(
        string taskPrompt,
        IReadOnlyList<string> sampleAnswers,
        string userTranscript,
        int taskNumber,
        byte[]? audioBytes = null,
        string? mimeType = null)
    {
        bool hasAudio = audioBytes != null && audioBytes.Length > 0;
        bool hasTranscript = !string.IsNullOrWhiteSpace(userTranscript) && 
                              !userTranscript.Contains("Không nhận diện được giọng nói") && 
                              !userTranscript.Contains("bỏ qua") && 
                              userTranscript.Trim() != "...";

        if (!hasAudio && !hasTranscript)
        {
            return new SpeakingEvaluationDto
            {
                OverallScore = 0,
                Passed = false,
                Feedback = "Hệ thống không nhận diện được giọng nói của bạn hoặc bạn đã bỏ qua câu hỏi. Vui lòng nói to rõ hoặc kiểm tra lại micro và thử lại.",
                Transcript = string.IsNullOrWhiteSpace(userTranscript) ? "(Bỏ qua)" : userTranscript,
                CriteriaScores = new Dictionary<string, double>
                {
                    { "Phát âm", 0 },
                    { "Lưu loát", 0 },
                    { "Ngữ pháp", 0 },
                    { "Từ vựng", 0 }
                }
            };
        }

        var samplesBlock = sampleAnswers.Count > 0
            ? string.Join("\n---\n", sampleAnswers.Select((s, i) => $"[Mẫu {i + 1}]\n{s}"))
            : "(Không có bài mẫu — chấm theo tiêu chí TOEIC Speaking.)";

        var prompt = $@"
Bạn là giám khảo TOEIC Speaking chuyên nghiệp. Hãy lắng nghe trực tiếp và chấm câu trả lời của học viên bằng cách SO SÁNH với bài mẫu và đề bài.

- Part / Task: {taskNumber}
- Đề bài / Prompt: {taskPrompt}
- Bài mẫu tham chiếu:
{samplesBlock}
- Câu trả lời học viên (transcript để tham khảo): {userTranscript}

YÊU CẦU ĐÁNH GIÁ ĐA PHƯƠNG THỨC CHUYÊN SÂU:
1. Hãy LẮNG NGHE trực tiếp file ghi âm đính kèm để đánh giá chính xác Ngữ âm (Pronunciation), Ngữ điệu & Trọng âm (Intonation & Stress) và Độ lưu loát (Fluency).
2. Hãy chỉ rõ những từ bị phát âm sai, thiếu âm đuôi (ending sounds) hoặc nhấn sai trọng âm nghe thấy từ file âm thanh nếu có.
3. Chấm điểm theo thang điểm 10 cho từng tiêu chí và cho điểm trung bình tổng quan.

TUYỆT ĐỐI chỉ trả về JSON hợp lệ (không markdown, không giải thích thêm) theo schema:
{{
  ""overallScore"": <số 0-10, một chữ số thập phân>,
  ""passed"": <true nếu overallScore >= 6.0>,
  ""criteriaScores"": {{
    ""Phát âm"": <0-10>,
    ""Lưu loát"": <0-10>,
    ""Ngữ pháp"": <0-10>,
    ""Từ vựng"": <0-10>
  }},
  ""feedback"": ""<nhận xét tiếng Việt, 2-4 câu, nêu rõ thế mạnh và lỗi phát âm/ngữ điệu nghe thấy từ file audio để giúp học viên sửa đổi>""
}}";

        var cacheKey = $"speaking_eval_{taskNumber}_{userTranscript.ToLower().Trim().GetHashCode()}_{(audioBytes != null ? audioBytes.Length : 0)}";
        if (_cache.TryGetValue(cacheKey, out SpeakingEvaluationDto? cachedEval) && cachedEval != null)
        {
            return cachedEval;
        }

        var raw = await CallGeminiAsync(prompt, 2000, audioBytes, mimeType, isJson: true);
        var dto = ParseSpeakingEvaluationJson(raw, userTranscript);

        if (dto.OverallScore > 0)
        {
            _cache.Set(cacheKey, dto, TimeSpan.FromMinutes(30));
        }

        return dto;
    }

    private static SpeakingEvaluationDto ParseSpeakingEvaluationJson(string raw, string transcript)
    {
        try
        {
            var json = ExtractJsonObject(raw);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var criteria = new Dictionary<string, double>();
            if (root.TryGetProperty("criteriaScores", out var criteriaEl) &&
                criteriaEl.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in criteriaEl.EnumerateObject())
                {
                    if (prop.Value.TryGetDouble(out var val))
                    {
                        criteria[prop.Name] = val;
                    }
                }
            }

            var overall = root.TryGetProperty("overallScore", out var scoreEl) && scoreEl.TryGetDouble(out var s)
                ? s
                : 0;

            var passed = root.TryGetProperty("passed", out var passedEl) && passedEl.ValueKind == JsonValueKind.True
                ? true
                : overall >= 6.0;

            var feedback = root.TryGetProperty("feedback", out var fbEl)
                ? fbEl.GetString() ?? string.Empty
                : string.Empty;

            return new SpeakingEvaluationDto
            {
                OverallScore = overall,
                Passed = passed,
                CriteriaScores = criteria,
                Feedback = feedback,
                Transcript = transcript
            };
        }
        catch
        {
            return new SpeakingEvaluationDto
            {
                OverallScore = 0,
                Passed = false,
                Feedback = "Không thể phân tích phản hồi AI. Vui lòng thử lại.",
                Transcript = transcript,
                CriteriaScores = new Dictionary<string, double>()
            };
        }
    }

    public async Task<WritingEvaluationDto> EvaluateWritingAsync(
        string taskPrompt,
        string taskType,
        IReadOnlyList<string> givenWords,
        string? emailContent,
        IReadOnlyList<string> emailQuestions,
        IReadOnlyList<string> sampleAnswers,
        string userAnswer)
    {
        var samplesBlock = sampleAnswers.Count > 0
            ? string.Join("\n---\n", sampleAnswers.Select((s, i) => $"[Mẫu {i + 1}]\n{s}"))
            : "(Không có bài mẫu — chấm theo tiêu chí TOEIC Writing.)";

        var givenWordsBlock = givenWords.Count > 0
            ? string.Join(", ", givenWords)
            : "(Không có từ bắt buộc)";

        var emailContentBlock = !string.IsNullOrWhiteSpace(emailContent)
            ? $"Thư nhận được:\n{emailContent}"
            : "";

        var emailQuestionsBlock = emailQuestions.Count > 0
            ? $"Các câu hỏi/yêu cầu cần phản hồi:\n{string.Join("\n", emailQuestions.Select((q, i) => $"- Yêu cầu {i + 1}: {q}"))}"
            : "";

        var prompt = $@"
Bạn là giám khảo TOEIC Writing chuyên nghiệp. Hãy chấm điểm câu trả lời/bài viết của học viên dựa trên các tiêu chí TOEIC Writing.

- Dạng bài (Task Type): {taskType}
- Đề bài / Prompt: {taskPrompt}
- Từ vựng bắt buộc (nếu có): {givenWordsBlock}
{emailContentBlock}
{emailQuestionsBlock}

- Bài mẫu tham chiếu:
{samplesBlock}

- Bài viết của học viên: {userAnswer}

YÊU CẦU ĐÁNH GIÁ CHUYÊN SÂU:
1. Hãy đánh giá tính chính xác ngữ pháp, sự đa dạng của từ vựng, tính liên kết mạch lạc (cohesion), và tính liên quan/đáp ứng đầy đủ yêu cầu đề bài.
2. Đối với dạng 'write_sentence' (viết câu dựa trên tranh), học viên bắt buộc phải dùng đúng 2 từ cho sẵn ({givenWordsBlock}) và viết đúng ngữ pháp mô tả tranh.
3. Đối với dạng 'respond_email' (trả lời email), học viên phải phản hồi tất cả các câu hỏi/yêu cầu trong đề bài một cách tự nhiên, chuyên nghiệp.
4. Đối với dạng 'opinion_essay' (viết luận), học viên phải bày tỏ rõ ràng quan điểm, có các luận điểm và ví dụ minh họa chặt chẽ, mạch lạc.
5. Chấm điểm theo thang điểm 10 cho từng tiêu chí và cho điểm trung bình tổng quan.
6. Cung cấp chi tiết các lỗi sai, bản sửa lỗi (Corrections) bằng tiếng Việt, và bài viết đề xuất cải tiến (Suggested Improvement) để học viên học hỏi.

TUYỆT ĐỐI chỉ trả về JSON hợp lệ (không markdown, không giải thích thêm) theo schema:
{{
  ""overallScore"": <số 0-10, một chữ số thập phân>,
  ""passed"": <true nếu overallScore >= 6.0>,
  ""criteriaScores"": {{
    ""Ngữ pháp"": <0-10>,
    ""Từ vựng"": <0-10>,
    ""Bố cục & Liên kết"": <0-10>,
    ""Độ phù hợp"": <0-10>
  }},
  ""feedback"": ""<nhận xét tổng quan bằng tiếng Việt, 2-3 câu, nêu thế mạnh và điểm cần cải thiện>"",
  ""correctionsVi"": ""<phân tích chi tiết các lỗi ngữ pháp, dùng từ, chính tả bằng tiếng Việt và cách sửa tương ứng>"",
  ""suggestedImprovement"": ""<bài viết mẫu cải tiến nâng cấp từ chính bài làm của học viên, giúp câu văn tự nhiên và chuyên nghiệp hơn>""
}}";

        var cacheKey = $"writing_eval_{taskType}_{userAnswer.ToLower().Trim().GetHashCode()}_{taskPrompt.GetHashCode()}";
        if (_cache.TryGetValue(cacheKey, out WritingEvaluationDto? cachedEval) && cachedEval != null)
        {
            return cachedEval;
        }

        var raw = await CallGeminiAsync(prompt, 3000, isJson: true);
        var dto = ParseWritingEvaluationJson(raw, userAnswer);

        if (dto.OverallScore > 0)
        {
            _cache.Set(cacheKey, dto, TimeSpan.FromMinutes(30));
        }

        return dto;
    }

    private static WritingEvaluationDto ParseWritingEvaluationJson(string raw, string userAnswer)
    {
        try
        {
            var json = ExtractJsonObject(raw);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var criteria = new Dictionary<string, double>();
            if (root.TryGetProperty("criteriaScores", out var criteriaEl) &&
                criteriaEl.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in criteriaEl.EnumerateObject())
                {
                    if (prop.Value.TryGetDouble(out var val))
                    {
                        criteria[prop.Name] = val;
                    }
                }
            }

            var overall = root.TryGetProperty("overallScore", out var scoreEl) && scoreEl.TryGetDouble(out var s)
                ? s
                : 0;

            var passed = root.TryGetProperty("passed", out var passedEl) && passedEl.ValueKind == JsonValueKind.True
                ? true
                : overall >= 6.0;

            var feedback = root.TryGetProperty("feedback", out var fbEl)
                ? fbEl.GetString() ?? string.Empty
                : string.Empty;

            var corrections = root.TryGetProperty("correctionsVi", out var corrEl)
                ? corrEl.GetString() ?? string.Empty
                : (root.TryGetProperty("corrections_vi", out var corrEl2) ? corrEl2.GetString() ?? string.Empty : string.Empty);

            var suggested = root.TryGetProperty("suggestedImprovement", out var sugEl)
                ? sugEl.GetString() ?? string.Empty
                : (root.TryGetProperty("suggested_improvement", out var sugEl2) ? sugEl2.GetString() ?? string.Empty : string.Empty);

            return new WritingEvaluationDto
            {
                OverallScore = overall,
                Passed = passed,
                CriteriaScores = criteria,
                Feedback = feedback,
                CorrectionsVi = corrections,
                SuggestedImprovement = suggested,
                UserAnswer = userAnswer
            };
        }
        catch
        {
            return new WritingEvaluationDto
            {
                OverallScore = 0,
                Passed = false,
                Feedback = "Không thể phân tích phản hồi AI. Vui lòng thử lại.",
                CorrectionsVi = string.Empty,
                SuggestedImprovement = string.Empty,
                UserAnswer = userAnswer,
                CriteriaScores = new Dictionary<string, double>()
            };
        }
    }

    private static string ExtractJsonObject(string raw)
    {
        var trimmed = raw.Trim();
        var match = Regex.Match(trimmed, @"\{[\s\S]*\}");
        return match.Success ? match.Value : trimmed;
    }

    private async Task<string> CallGeminiAsync(string prompt, int maxTokens, byte[]? audioBytes = null, string? mimeType = null, bool isJson = false)
    {
        // Debug để xác nhận Backend đang chạy bản mới nhất
        Console.WriteLine($"[GEMINI DEBUG] Đang gọi AI với maxTokens: {maxTokens}, có audio: {audioBytes != null}, isJson: {isJson}");

        object partsArray;

        if (audioBytes != null && audioBytes.Length > 0)
        {
            var base64Audio = Convert.ToBase64String(audioBytes);
            var resolvedMime = string.IsNullOrWhiteSpace(mimeType) ? "audio/m4a" : mimeType;
            if (resolvedMime.IndexOf("webm", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                resolvedMime = "audio/webm";
            }
            partsArray = new object[]
            {
                new { text = prompt },
                new { 
                    inlineData = new {
                        mimeType = resolvedMime,
                        data = base64Audio
                    }
                }
            };
        }
        else
        {
            partsArray = new object[]
            {
                new { text = prompt }
            };
        }

        var requestBody = new
        {
            contents = new[]
            {
                new { parts = partsArray }
            },
            generationConfig = isJson
                ? (object)new
                {
                    maxOutputTokens = maxTokens,
                    temperature = 0.7,
                    responseMimeType = "application/json"
                }
                : new
                {
                    maxOutputTokens = maxTokens,
                    temperature = 0.7
                }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        int retryCount = 0;
        const int maxRetries = 3;

        while (retryCount <= maxRetries)
        {
            try
            {
                var response = await _httpClient.PostAsync($"{_apiUrl}?key={_apiKey}", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    // Log để debug xem Google thực sự trả về cái gì
                    Console.WriteLine($"[GEMINI RESPONSE] {responseString}");

                    using var doc = JsonDocument.Parse(responseString);
                    
                    // Kiểm tra xem có kết quả không trước khi truy cập
                    var root = doc.RootElement;
                    if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                    {
                        var candidate = candidates[0];
                        if (candidate.TryGetProperty("content", out var contentObj) && 
                            contentObj.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
                        {
                            return parts[0].GetProperty("text").GetString()?.Trim() ?? "AI không trả về văn bản.";
                        }
                    }
                    
                    return "AI không trả về kết quả hợp lệ (có thể do vi phạm chính sách nội dung).";
                }

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    retryCount++;
                    if (retryCount <= maxRetries)
                    {
                        int delay = retryCount * 2000; // 2s, 4s, 6s
                        Console.WriteLine($"[GEMINI] Gặp lỗi 429 (TooManyRequests). Đang thử lại lần {retryCount} sau {delay}ms...");
                        await Task.Delay(delay);
                        continue;
                    }
                }

                var errorMsg = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[GEMINI ERROR] {response.StatusCode}: {errorMsg}");
                return $"Lỗi khi gọi AI: {response.StatusCode}.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SYSTEM ERROR] {ex.Message}");
                return "Lỗi hệ thống khi kết nối với AI.";
            }
        }

        return "Hệ thống AI đang bận, vui lòng thử lại sau.";
    }
}
