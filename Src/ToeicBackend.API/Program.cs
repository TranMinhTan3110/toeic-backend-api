using System.Security.Claims;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

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

var firebaseProjectId = builder.Configuration["Firebase:ProjectId"] ?? "toeic-80ff0";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://securetoken.google.com/{firebaseProjectId}";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"https://securetoken.google.com/{firebaseProjectId}",
            ValidateAudience = true,
            ValidAudience = firebaseProjectId,
            ValidateLifetime = true,
            NameClaimType = "user_id"
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = ctx =>
            {
                var identity = (ClaimsIdentity)ctx.Principal!.Identity!;
                var userId = ctx.Principal.FindFirst("user_id")?.Value
                    ?? ctx.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId) && identity.FindFirst("user_id") == null)
                {
                    identity.AddClaim(new Claim("user_id", userId));
                }
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddMemoryCache();
builder.Services.AddControllers();

// Đăng ký Repository và Service cho DI Container
builder.Services.AddScoped<ToeicBackend.Application.Interfaces.IVocabularyRepository, ToeicBackend.Infrastructure.Repositories.VocabularyRepository>();
builder.Services.AddScoped<ToeicBackend.Application.Interfaces.IVocabularyService, ToeicBackend.Application.Services.VocabularyService>();
builder.Services.AddScoped<ToeicBackend.Application.Interfaces.IUserRepository, ToeicBackend.Infrastructure.Repositories.UserRepository>();
builder.Services.AddScoped<ToeicBackend.Application.Interfaces.IAuthService, ToeicBackend.Infrastructure.Services.AuthService>();

// --- PHẦN SRS VÀ TIẾN ĐỘ HỌC TẬP ---
builder.Services.AddScoped<ToeicBackend.Application.Interfaces.ISpacedRepetitionService, ToeicBackend.Infrastructure.Services.SpacedRepetitionService>();
builder.Services.AddScoped<ToeicBackend.Application.Interfaces.IVocabularyProgressRepository, ToeicBackend.Infrastructure.Repositories.VocabularyProgressRepository>();
builder.Services.AddScoped<ToeicBackend.Application.Interfaces.IVocabularyProgressService, ToeicBackend.Application.Services.VocabularyProgressService>();
builder.Services.AddScoped<ToeicBackend.Application.Interfaces.IEpEventRepository, ToeicBackend.Infrastructure.Repositories.EpEventRepository>();
builder.Services.AddScoped<ToeicBackend.Application.Interfaces.IEngagementService, ToeicBackend.Application.Services.EngagementService>();
builder.Services.AddScoped<ToeicBackend.Application.Interfaces.IUserProfileService, ToeicBackend.Application.Services.UserProfileService>();
builder.Services.AddScoped<ToeicBackend.Application.Interfaces.ILeaderboardService, ToeicBackend.Application.Services.LeaderboardService>();
// ------------------------------------

// Đăng ký HttpClient và AI Service cho Gemini
builder.Services.AddHttpClient();
builder.Services.AddScoped<ToeicBackend.Application.Interfaces.IAiService, ToeicBackend.Infrastructure.Services.GeminiAiService>();

// Đăng ký Speaking (Luyện Nói) từ develop
builder.Services.AddScoped<ToeicBackend.Application.Interfaces.ISpeakingRepository, ToeicBackend.Infrastructure.Repositories.SpeakingRepository>();
builder.Services.AddScoped<ToeicBackend.Application.Interfaces.ISpeakingService, ToeicBackend.Application.Services.SpeakingService>();
builder.Services.AddScoped<ToeicBackend.Application.Interfaces.IListeningRepository, ToeicBackend.Infrastructure.Repositories.ListeningRepository>();
builder.Services.AddScoped<ToeicBackend.Application.Interfaces.IListeningService, ToeicBackend.Application.Services.ListeningService>();
builder.Services.AddScoped<ToeicBackend.Application.Interfaces.IGrammarRepository, ToeicBackend.Infrastructure.Repositories.GrammarRepository>();
builder.Services.AddScoped<ToeicBackend.Application.Interfaces.IGrammarService, ToeicBackend.Application.Services.GrammarService>();

builder.Services.AddScoped<ToeicBackend.Application.Interfaces.IWritingQuestionRepository, ToeicBackend.Infrastructure.Repositories.WritingQuestionRepository>();
builder.Services.AddScoped<ToeicBackend.Application.Interfaces.IWritingQuestionService, ToeicBackend.Application.Services.WritingQuestionService>();

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
    app.MapScalarApiReference(); // Dùng Scalar thay cho Swagger
}

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

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
