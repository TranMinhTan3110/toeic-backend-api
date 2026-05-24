using Google.Cloud.Firestore;
using System.Text.Json;

// --- Setup credentials ---
string projectRoot = Directory.GetParent(AppContext.BaseDirectory)!
    .Parent!.Parent!.Parent!.Parent!.FullName;

string keyPath = Path.Combine(projectRoot, "ToeicBackend.API", "serviceAccountKey.json");

if (!File.Exists(keyPath))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"[LỖI] Không tìm thấy serviceAccountKey.json tại:\n{keyPath}");
    Console.ResetColor();
    return;
}

Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", keyPath);

FirestoreDb db = FirestoreDb.Create("toeic-80ff0");
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine(" Kết nối Firestore thành công!\n");
Console.ResetColor();

// ====================================================
// UPDATE TEST 6 SPEAKING QUESTIONS
// ====================================================
string seedDir = Path.Combine(projectRoot, "ToeicBackend.Seeder", "SeedData");
string jsonPath = Path.Combine(seedDir, "speaking_questions_t6.json");

if (!File.Exists(jsonPath))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"[LỖI] Không tìm thấy file: {jsonPath}");
    Console.ResetColor();
    return;
}

var newQuestions = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(
    await File.ReadAllTextAsync(jsonPath));

if (newQuestions == null)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("[LỖI] Không thể deserialize file speaking_questions_t6.json");
    Console.ResetColor();
    return;
}

Console.ForegroundColor = ConsoleColor.Blue;
Console.WriteLine($"Bắt đầu cập nhật {newQuestions.Count} câu hỏi speaking cho Test 6...");
Console.ResetColor();

int updatedCount = 0;
foreach (var item in newQuestions)
{
    string origId = item["id"].GetString()!;
    string docId = "t6_" + origId;

    Console.WriteLine($"Đang xử lý: {docId}...");

    DocumentReference docRef = db.Collection("speaking_questions").Document(docId);
    DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

    string? existingAudio = snapshot.Exists && snapshot.ContainsField("prompt_audio_url")
        ? snapshot.GetValue<string?>("prompt_audio_url") : null;
    string? existingImage = snapshot.Exists && snapshot.ContainsField("prompt_image_url")
        ? snapshot.GetValue<string?>("prompt_image_url") : null;

    string? newAudio = item.TryGetValue("prompt_audio_url", out var elAudio) && elAudio.ValueKind == JsonValueKind.String
        ? elAudio.GetString() : null;
    string? newImage = item.TryGetValue("prompt_image_url", out var elImage) && elImage.ValueKind == JsonValueKind.String
        ? elImage.GetString() : null;

    var data = ConvertToFirestoreDict(item);
    data["id"] = docId;
    data["prompt_audio_url"] = ResolveMediaUrl(newAudio, existingAudio);
    data["prompt_image_url"] = ResolveMediaUrl(newImage, existingImage);
    data["exam_set_id"] = "ETS-SPK-2024-06";

    await docRef.SetAsync(data, SetOptions.Overwrite);
    updatedCount++;
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"   ✓ {docId} cập nhật thành công!");
    Console.ResetColor();
}

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine($"\n Hoàn tất! Đã xử lý {updatedCount} tài liệu Test 6.\n");
Console.ResetColor();

// ====================================================
// VERIFICATION
// ====================================================
Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("=== VERIFYING ===");
Console.ResetColor();

string[] verifyIds = {
    "t6_spk_task1_001", "t6_spk_task1_002",
    "t6_spk_task2_003", "t6_spk_task2_004",
    "t6_spk_task3_005", "t6_spk_task4_006", "t6_spk_task5_007"
};

foreach (var testId in verifyIds)
{
    var snap = await db.Collection("speaking_questions").Document(testId).GetSnapshotAsync();
    if (snap.Exists)
    {
        Console.WriteLine($"\n[{testId}]");
        Console.WriteLine($"  audio: {snap.GetValue<string?>("prompt_audio_url")}");
        Console.WriteLine($"  image: {snap.GetValue<string?>("prompt_image_url")}");
        var exp = snap.ContainsField("explanation") ? snap.GetValue<object>("explanation") : null;
        Console.WriteLine($"  explanation: {(exp != null ? "✓ present" : "✗ missing")}");
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[CẢNH BÁO] {testId} KHÔNG tồn tại!");
        Console.ResetColor();
    }
}

// ====================================================
// PHÂN LOẠI is_practice / is_exam THEO BỘ ĐỀ
// Test 2–5: luyện tập (is_practice: true, is_exam: false)
// Test 6–10: thi thử (is_practice: false, is_exam: true)
// ====================================================
Console.ForegroundColor = ConsoleColor.Blue;
Console.WriteLine("\n=== PHÂN LOẠI is_practice / is_exam (Test 2–5 luyện | Test 6–10 thi) ===");
Console.ResetColor();

static bool TryGetTestNumber(string docId, out int testNumber)
{
    testNumber = 0;
    if (!docId.StartsWith('t')) return false;
    var rest = docId[1..];
    var underscore = rest.IndexOf('_');
    if (underscore <= 0) return false;
    return int.TryParse(rest[..underscore], out testNumber);
}

var allSpeaking = await db.Collection("speaking_questions").GetSnapshotAsync();
int practiceCount = 0;
int examCount = 0;

foreach (var doc in allSpeaking.Documents)
{
    if (!TryGetTestNumber(doc.Id, out var testNum)) continue;

    bool isExamSet = testNum is >= 6 and <= 10;
    bool isPracticeSet = testNum is >= 2 and <= 5;

    if (!isExamSet && !isPracticeSet) continue;

    var updates = new Dictionary<string, object>
    {
        ["is_practice"] = isPracticeSet,
        ["is_exam"] = isExamSet,
    };

    await doc.Reference.UpdateAsync(updates);

    if (isPracticeSet)
    {
        practiceCount++;
        Console.WriteLine($"  [Luyện tập] {doc.Id} → is_practice: true, is_exam: false");
    }
    else
    {
        examCount++;
        Console.WriteLine($"  [Thi thử]   {doc.Id} → is_practice: false, is_exam: true");
    }
}

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine($"\nĐã cập nhật {practiceCount} câu luyện tập (test 2–5) và {examCount} câu thi (test 6–10).\n");
Console.ResetColor();

// ====================================================
// HELPERS
// ====================================================
string? ResolveMediaUrl(string? newUrl, string? existingUrl)
{
    if (string.IsNullOrWhiteSpace(newUrl)) return existingUrl;
    if (!newUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
    {
        if (!string.IsNullOrWhiteSpace(existingUrl) && existingUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            int lastSlash = existingUrl.LastIndexOf('/');
            return existingUrl.Substring(0, lastSlash + 1) + newUrl;
        }
        return newUrl;
    }
    return newUrl;
}

Dictionary<string, object?> ConvertToFirestoreDict(Dictionary<string, JsonElement> source)
{
    var result = new Dictionary<string, object?>();
    foreach (var kv in source)
    {
        if (kv.Key == "created_at" && kv.Value.ValueKind == JsonValueKind.String
            && DateTime.TryParse(kv.Value.GetString(), out var dt))
            result[kv.Key] = Timestamp.FromDateTime(dt.ToUniversalTime());
        else
            result[kv.Key] = ConvertJsonElement(kv.Value);
    }
    return result;
}

object? ConvertJsonElement(JsonElement el) => el.ValueKind switch
{
    JsonValueKind.String => el.GetString(),
    JsonValueKind.Number when el.TryGetInt64(out long l) => l,
    JsonValueKind.Number => el.GetDouble(),
    JsonValueKind.True => true,
    JsonValueKind.False => false,
    JsonValueKind.Null => null,
    JsonValueKind.Array => el.EnumerateArray().Select(ConvertJsonElement).ToList(),
    JsonValueKind.Object => el.EnumerateObject().ToDictionary(p => p.Name, p => ConvertJsonElement(p.Value)),
    _ => el.ToString()
};
