using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
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

    private async Task<string> CallGeminiAsync(string prompt, int maxTokens)
    {
        // Debug để xác nhận Backend đang chạy bản mới nhất
        Console.WriteLine($"[GEMINI DEBUG] Đang gọi AI với maxTokens: {maxTokens}");

        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            },
            generationConfig = new
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
