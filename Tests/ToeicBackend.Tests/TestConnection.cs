using Google.Cloud.Firestore;
using Xunit;

namespace ToeicBackend.Tests
{
    public class UnitTest1
    {
        [Fact]
        public async Task TestConnection()
        {
      
            string projectRoot = Directory.GetParent(AppContext.BaseDirectory)!
                .Parent!.Parent!.Parent!.Parent!.Parent!.FullName;

            string path = Path.Combine(projectRoot, "Src", "ToeicBackend.API", "serviceAccountKey.json");

            // Kiểm tra file có tồn tại không
            Assert.True(File.Exists(path), $"Không tìm thấy file tại: {path}");

            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);

            FirestoreDb db = FirestoreDb.Create("toeic-80ff0");

            DocumentReference docRef = db.Collection("Tests").Document("ConnectionTest");
            await docRef.SetAsync(new { Message = "Lead Tan Beo da ket noi thanh cong!", Time = DateTime.UtcNow });

            // Đọc lại để xác nhận ghi thành công
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
            Assert.True(snapshot.Exists, "Ghi lên Firestore thất bại!");
        }
    }
}