using Google.Cloud.Firestore;
using System.Text.Json;

// ====================================================
// TOEIC Firebase Seeder
// Chạy: dotnet run
// Mục đích: Đẩy seed data từ JSON lên Firestore 1 lần
// ====================================================

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

// --- Seed từng collection ---
await SeedCollection("vocabulary", "vocabulary.json");
await SeedCollection("grammar_topics", "grammar_topics.json");
await SeedCollection("question_groups", "question_groups.json");
await SeedCollection("questions", "questions.json");
await SeedCollection("exams", "exams.json");
await SeedCollection("speaking_questions", "speaking_questions.json");
await SeedCollection("writing_questions", "writing_questions.json");

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("\n Seed data hoàn tất!");
Console.ResetColor();

// ====================================================
// Hàm seed tổng quát: đọc JSON → push lên Firestore
// ====================================================
async Task SeedCollection(string collectionName, string fileName)
{
    string seedDir = Path.Combine(AppContext.BaseDirectory, "SeedData");

    // Nếu chạy từ thư mục project (dotnet run), tìm trong project folder
    if (!Directory.Exists(seedDir))
    {
        seedDir = Path.Combine(projectRoot, "ToeicBackend.Seeder", "SeedData");
    }

    string filePath = Path.Combine(seedDir, fileName);

    if (!File.Exists(filePath))
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  Bỏ qua '{collectionName}' — không tìm thấy file: {filePath}");
        Console.ResetColor();
        return;
    }

    string json = await File.ReadAllTextAsync(filePath);
    if (string.IsNullOrWhiteSpace(json))
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  Bỏ qua '{fileName}' — file rỗng.");
        Console.ResetColor();
        return;
    }

    var items = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(json);

    if (items == null || items.Count == 0)
    {
        Console.WriteLine($"  File '{fileName}' rỗng hoặc sai định dạng.");
        return;
    }

    Console.WriteLine($" Seeding '{collectionName}' ({items.Count} docs)...");

    int count = 0;
    foreach (var item in items)
    {
        // Lấy "id" làm document ID, nếu không có thì dùng auto-ID
        string? docId = item.ContainsKey("id") ? item["id"].GetString() : null;

        DocumentReference docRef = docId != null
            ? db.Collection(collectionName).Document(docId)
            : db.Collection(collectionName).Document();

        // Convert JsonElement → object đơn giản để Firestore nhận
        var data = ConvertToFirestoreDict(item);

        await docRef.SetAsync(data, SetOptions.Overwrite);
        count++;
    }

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"   Đã import {count} documents vào '{collectionName}'");
    Console.ResetColor();
}

// Convert Dictionary<string, JsonElement> → Dictionary<string, object?>
Dictionary<string, object?> ConvertToFirestoreDict(Dictionary<string, JsonElement> source)
{
    var result = new Dictionary<string, object?>();
    foreach (var kv in source)
    {
        result[kv.Key] = ConvertJsonElement(kv.Value);
    }
    return result;
}

object? ConvertJsonElement(JsonElement el)
{
    return el.ValueKind switch
    {
        JsonValueKind.String => el.GetString(),
        JsonValueKind.Number when el.TryGetInt64(out long l) => l,
        JsonValueKind.Number => el.GetDouble(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Null => null,
        JsonValueKind.Array => el.EnumerateArray()
            .Select(ConvertJsonElement)
            .ToList(),
        JsonValueKind.Object => el.EnumerateObject()
            .ToDictionary(p => p.Name, p => ConvertJsonElement(p.Value)),
        _ => el.ToString()
    };
}
