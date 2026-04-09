using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;

var builder = WebApplication.CreateBuilder(args);



// --- PHẦN SETUP FIREBASE (DI) ---

// 1. Lấy đường dẫn file JSON từ appsettings.json
var keyPath = Path.Combine(Directory.GetCurrentDirectory(), builder.Configuration["Firebase:ApiKeyPath"]!);

// 2. Thiết lập biến môi trường để SDK của Google tự nhận diện chứng chỉ
Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", keyPath);

// 3. Khởi tạo Firebase Admin SDK (Dùng cho Auth, Cloud Messaging)
if (FirebaseApp.DefaultInstance == null)
{
    FirebaseApp.Create(new AppOptions()
    {
        Credential = GoogleCredential.FromFile(keyPath),
    });
}

// 4. Đăng ký FirestoreDb vào DI Container (AddSingleton)
// Singleton nghĩa là cả ứng dụng chỉ dùng chung 1 kết nối duy nhất, cực kỳ tiết kiệm tài nguyên.
builder.Services.AddSingleton(sp => 
{
    // Lấy Project ID từ file JSON 
    string projectId = "toeic-80ff0"; 
    return FirestoreDb.Create(projectId);
});
// --- KẾT THÚC SETUP FIREBASE ---

builder.Services.AddControllers();

// Đăng ký Repository và Service cho DI Container
builder.Services.AddScoped<ToeicBackend.Application.Interfaces.IVocabularyRepository, ToeicBackend.Infrastructure.Repositories.VocabularyRepository>();
builder.Services.AddScoped<ToeicBackend.Application.Interfaces.IVocabularyService, ToeicBackend.Application.Services.VocabularyService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAll");

// app.UseHttpsRedirection(); // Tạm tắt nếu test local cho máy ảo đỡ lỗi chứng chỉ

app.MapControllers();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
