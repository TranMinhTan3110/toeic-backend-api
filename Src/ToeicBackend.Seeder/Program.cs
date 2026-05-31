using Google.Cloud.Firestore;
using System.Text.Json;

// ====================================================
// TOEIC Firebase Seeder
// Chạy: dotnet run --project Src/ToeicBackend.Seeder
// Mục đích: Đẩy seed data từ JSON lên Firestore
// ====================================================

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

// Chỉ seed 1 collection: dotnet run --project Src/ToeicBackend.Seeder -- user_speaking_history
if (args.Length > 0 && args[0] == "user_speaking_history")
{
    await SeedCollection("user_speaking_history", "user_speaking_history.json");
}
else
{
    await SeedCollection("vocabulary", "vocabulary.json");
    await SeedCollection("grammar_topics", "grammar_topics.json");
    await SeedCollection("grammar_lessons", "grammar_lessons.json");
    await SeedCollection("question_groups", "question_groups.json");
    await SeedCollection("question_groups", "question_group_listening.json");
    await SeedCollection("questions", "questions.json");
    await SeedCollection("questions", "practiceListening.json");
    await SeedCollection("questions", "grammar_questions.json");
    await SeedCollection("exams", "exams.json");
    await SeedCollection("speaking_questions", "speaking_questions.json");
    await SeedCollection("writing_questions", "writing_questions.json");
    await SeedCollection("user_speaking_history", "user_speaking_history.json");
}

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("\n Seed data hoàn tất!");
Console.ResetColor();

async Task SeedCollection(string collectionName, string fileName)
{
    string? seedDir = new[]
    {
        Path.Combine(AppContext.BaseDirectory, "SeedData"),
        Path.Combine(projectRoot, "Src", "ToeicBackend.Seeder", "SeedData"),
        Path.Combine(projectRoot, "ToeicBackend.Seeder", "SeedData"),
    }.FirstOrDefault(Directory.Exists);

    if (seedDir == null)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  Không tìm thấy thư mục SeedData (projectRoot={projectRoot})");
        Console.ResetColor();
        return;
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
        Console.WriteLine($"  Bỏ qua '{collectionName}' — file rỗng: {fileName}");
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
        string? docId = item.ContainsKey("id") ? item["id"].GetString() : null;
        DocumentReference docRef = docId != null
            ? db.Collection(collectionName).Document(docId)
            : db.Collection(collectionName).Document();

        var data = ConvertToFirestoreDict(item);
        await docRef.SetAsync(data, SetOptions.Overwrite);
        count++;
    }

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"   Đã import {count} documents vào '{collectionName}'");
    Console.ResetColor();
}

Dictionary<string, object?> ConvertToFirestoreDict(Dictionary<string, JsonElement> source)
{
    var result = new Dictionary<string, object?>();
    foreach (var kv in source)
    {
        if (IsTimestampField(kv.Key) &&
            kv.Value.ValueKind == JsonValueKind.String &&
            DateTime.TryParse(kv.Value.GetString(), out var dt))
        {
            result[kv.Key] = Timestamp.FromDateTime(dt.ToUniversalTime());
            continue;
        }

        result[kv.Key] = ConvertJsonElement(kv.Value);
    }
    return result;
}

static bool IsTimestampField(string fieldName) =>
    fieldName is "submitted_at" or "created_at" or "date" or "started_at" or "completed_at";

object? ConvertJsonElement(JsonElement el) => el.ValueKind switch
{
    JsonValueKind.String => el.GetString(),
    JsonValueKind.Number when el.TryGetInt64(out long l) => l,
    JsonValueKind.Number => el.GetDouble(),
    JsonValueKind.True => true,
    JsonValueKind.False => false,
    JsonValueKind.Null => null,
    JsonValueKind.Array => el.EnumerateArray().Select(ConvertJsonElement).ToList(),
    JsonValueKind.Object => el.EnumerateObject()
        .ToDictionary(p => p.Name, p => ConvertJsonElement(p.Value)),
    _ => el.ToString()
};
