# 📘 Hướng Dẫn Dự Án `toeic-api` — Dành Cho Nhóm

> **Mục tiêu:** Giúp toàn bộ nhóm hiểu rõ cấu trúc thư mục, vai trò từng phần, và cách làm việc cùng nhau hiệu quả.

---

## 🗂️ Toàn Bộ Cấu Trúc Thư Mục (Có File Bổ Sung)

```
/ (Root)
│── .gitignore
│── README.md
│── ToeicBackend.sln
│── global.json                        ← [MỚI] Ghim phiên bản .NET SDK
│
├── Src/
│   ├── ToeicBackend.API/
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs      ← [MỚI] Đăng nhập / đăng ký
│   │   │   ├── QuestionController.cs  ← [MỚI] CRUD câu hỏi TOEIC
│   │   │   └── ExamController.cs      ← [MỚI] Tạo & nộp bài thi
│   │   ├── Middlewares/               ← [MỚI] Xử lý lỗi toàn cục
│   │   │   └── ExceptionMiddleware.cs
│   │   ├── Program.cs                 ← [MỚI] Entry point, cấu hình DI
│   │   └── appsettings.json
│   │
│   ├── ToeicBackend.Application/
│   │   ├── Services/
│   │   │   ├── AuthService.cs         ← [MỚI]
│   │   │   ├── ExamService.cs         ← [MỚI]
│   │   │   └── QuestionService.cs     ← [MỚI]
│   │   ├── Interfaces/
│   │   │   ├── IAuthService.cs        ← [MỚI]
│   │   │   ├── IExamService.cs        ← [MỚI]
│   │   │   └── IQuestionRepository.cs ← [MỚI]
│   │   └── DTOs/
│   │       ├── LoginRequestDto.cs     ← [MỚI]
│   │       ├── ExamResultDto.cs       ← [MỚI]
│   │       └── QuestionDto.cs         ← [MỚI]
│   │
│   ├── ToeicBackend.Domain/
│   │   ├── Entities/
│   │   │   ├── Question.cs            ← [MỚI]
│   │   │   ├── User.cs                ← [MỚI]
│   │   │   └── ExamResult.cs          ← [MỚI]
│   │   └── Enums/
│   │       └── ToeicPart.cs           ← [MỚI] Part1..Part7
│   │
│   └── ToeicBackend.Infrastructure/
│       ├── Firebase/
│       │   └── FirebaseConfig.cs      ← [MỚI] Khởi tạo Firestore SDK
│       ├── Repositories/
│       │   ├── QuestionRepository.cs  ← [MỚI]
│       │   └── ExamRepository.cs      ← [MỚI]
│       └── ExternalServices/
│           └── EmailService.cs        ← [MỚI] (Dùng sau nếu cần)
│
├── Docs/
│   ├── DatabaseDesign/
│   │   └── firestore_schema.json      ← [MỚI] Mẫu dữ liệu Firestore
│   └── API_Doc.md                     ← Danh sách API cho Web & Mobile
│
└── Tests/
    └── ToeicBackend.Tests/            ← [MỚI] Unit Test project (làm sau)
        └── ExamServiceTests.cs
```

---

## 🧠 Kiến Trúc: Clean Architecture — Tại Sao Lại Chia Như Vậy?

Dự án này dùng **Clean Architecture** — một cách tổ chức code chuyên nghiệp theo từng "tầng" (layer). Mỗi tầng chỉ làm **một việc duy nhất**, giúp code dễ sửa và dễ test.

```
┌─────────────────────────────┐
│        API Layer            │  ← Nhận request từ App/Web
├─────────────────────────────┤
│     Application Layer       │  ← Xử lý logic nghiệp vụ
├─────────────────────────────┤
│       Domain Layer          │  ← Định nghĩa dữ liệu, quy tắc
├─────────────────────────────┤
│   Infrastructure Layer      │  ← Kết nối Firebase, gửi email
└─────────────────────────────┘
```

> [!NOTE]
> **Quy tắc vàng:** Tầng trên có thể gọi tầng dưới, nhưng **KHÔNG** chiều ngược lại. Domain không biết gì về Firebase hay API.

---

## 📂 Giải Thích Chi Tiết Từng Phần

### 1️⃣ `ToeicBackend.API` — Cửa Ngõ Của Hệ Thống

**Vai trò:** Nhận HTTP Request từ ReactJS Web và Flutter App, trả về Response.

| File/Folder | Làm gì? |
|---|---|
| `Controllers/AuthController.cs` | Xử lý `/api/auth/login`, `/api/auth/register` |
| `Controllers/QuestionController.cs` | Xử lý `/api/questions` — lấy câu hỏi theo Part |
| `Controllers/ExamController.cs` | Xử lý `/api/exam/submit` — nộp bài, tính điểm |
| `Middlewares/ExceptionMiddleware.cs` | Bắt lỗi toàn server, trả về thông báo lỗi chuẩn |
| `Program.cs` | Khởi động server, đăng ký tất cả services |
| `appsettings.json` | Lưu config: Firebase key, port, CORS |

---

### 2️⃣ `ToeicBackend.Application` — Bộ Não Của Hệ Thống

**Vai trò:** Chứa toàn bộ **logic nghiệp vụ** (business logic). Controller chỉ gọi vào đây, không tự xử lý.

| File/Folder | Làm gì? |
|---|---|
| `Services/ExamService.cs` | Tính điểm, random câu hỏi ngẫu nhiên |
| `Services/AuthService.cs` | Xác thực Firebase Token từ App/Web gửi lên |
| `Interfaces/IQuestionRepository.cs` | **Hợp đồng** — định nghĩa hàm cần có, không quan tâm cách làm |
| `DTOs/QuestionDto.cs` | Định dạng dữ liệu trả về cho App/Web (không trả full Entity) |

> [!TIP]
> **DTO** là "bản rút gọn" của dữ liệu. Ví dụ `Question` entity có 15 field, nhưng App chỉ cần 5 field → tạo `QuestionDto` với 5 field đó.

---

### 3️⃣ `ToeicBackend.Domain` — Linh Hồn Của Hệ Thống

**Vai trò:** Định nghĩa **dữ liệu** và **quy tắc cốt lõi**. Không phụ thuộc vào bất kỳ thư viện nào.

| File | Làm gì? |
|---|---|
| `Entities/Question.cs` | Class Question: Id, Content, Options, CorrectAnswer, Part |
| `Entities/User.cs` | Class User: FirebaseUid, Email, DisplayName |
| `Entities/ExamResult.cs` | Class kết quả: UserId, Score, ListeningScore, ReadingScore |
| `Enums/ToeicPart.cs` | Enum: `Part1, Part2, Part3, Part4, Part5, Part6, Part7` |

---

### 4️⃣ `ToeicBackend.Infrastructure` — Tay Chân Của Hệ Thống

**Vai trò:** Thực thi code thực tế để kết nối Firebase, gửi email, v.v.

| File | Làm gì? |
|---|---|
| `Firebase/FirebaseConfig.cs` | Khởi tạo kết nối Firestore bằng Service Account JSON |
| `Repositories/QuestionRepository.cs` | Code thực tế: đọc/ghi câu hỏi vào Firestore |
| `Repositories/ExamRepository.cs` | Code thực tế: lưu kết quả thi vào Firestore |
| `ExternalServices/EmailService.cs` | Gửi email (bỏ trống trước, làm sau nếu cần) |

---

### 5️⃣ `Docs/` — Tài Liệu Cho Cả Nhóm

| File | Nội dung |
|---|---|
| `DatabaseDesign/firestore_schema.json` | JSON mẫu cấu trúc Firestore (xem bên dưới) |
| `API_Doc.md` | Danh sách endpoint API: URL, method, request body, response |

---

## 🗃️ Mẫu Schema Firestore (`firestore_schema.json`)

```json
{
  "collections": {
    "users": {
      "document_id": "firebase_uid",
      "fields": {
        "email": "string",
        "displayName": "string",
        "role": "string (student | admin)",
        "createdAt": "timestamp"
      }
    },
    "questions": {
      "document_id": "auto_id",
      "fields": {
        "content": "string",
        "imageUrl": "string (nullable)",
        "audioUrl": "string (nullable)",
        "options": ["A. ...", "B. ...", "C. ...", "D. ..."],
        "correctAnswer": "string (A|B|C|D)",
        "part": "number (1-7)",
        "difficulty": "string (easy|medium|hard)"
      }
    },
    "examResults": {
      "document_id": "auto_id",
      "fields": {
        "userId": "string",
        "totalScore": "number (10-990)",
        "listeningScore": "number",
        "readingScore": "number",
        "submittedAt": "timestamp",
        "answers": { "questionId": "string (A|B|C|D)" }
      }
    }
  }
}
```

---

## 🚀 Quy Trình Làm Việc Cho Nhóm

### Bước 1 — Tân (Owner) setup ban đầu
```bash
# Pull repo về máy (đã có .gitignore)
git pull origin main

# Tạo Solution và các Project
dotnet new sln -n ToeicBackend
dotnet new webapi -n ToeicBackend.API -o Src/ToeicBackend.API
dotnet new classlib -n ToeicBackend.Application -o Src/ToeicBackend.Application
dotnet new classlib -n ToeicBackend.Domain -o Src/ToeicBackend.Domain
dotnet new classlib -n ToeicBackend.Infrastructure -o Src/ToeicBackend.Infrastructure

# Thêm vào Solution
dotnet sln add Src/ToeicBackend.API/ToeicBackend.API.csproj
dotnet sln add Src/ToeicBackend.Application/ToeicBackend.Application.csproj
dotnet sln add Src/ToeicBackend.Domain/ToeicBackend.Domain.csproj
dotnet sln add Src/ToeicBackend.Infrastructure/ToeicBackend.Infrastructure.csproj

# Kết nối các project với nhau
dotnet add Src/ToeicBackend.API reference Src/ToeicBackend.Application
dotnet add Src/ToeicBackend.Application reference Src/ToeicBackend.Domain
dotnet add Src/ToeicBackend.Infrastructure reference Src/ToeicBackend.Domain
dotnet add Src/ToeicBackend.API reference Src/ToeicBackend.Infrastructure
```

### Bước 2 — Cài NuGet packages cần thiết
```bash
# Firebase Admin SDK cho Infrastructure
dotnet add Src/ToeicBackend.Infrastructure package FirebaseAdmin

# Swagger để test API
dotnet add Src/ToeicBackend.API package Swashbuckle.AspNetCore
```

### Bước 3 — Thêm Firebase Service Account
1. Vào **Firebase Console** → Project Settings → Service Accounts
2. Download file `firebase-adminsdk.json`
3. Đặt vào `Src/ToeicBackend.API/` (⚠️ **KHÔNG** commit file này lên GitHub!)
4. Đảm bảo `.gitignore` có dòng: `**/firebase-adminsdk*.json`

### Bước 4 — Chạy thử
```bash
dotnet run --project Src/ToeicBackend.API
# Mở Swagger: https://localhost:5001/swagger
```

---

## 👥 Phân Công Công Việc Gợi Ý

| Thành viên | Phụ trách |
|---|---|
| Tân (Backend Lead) | Setup solution, Firebase config, `Program.cs`, `AuthController` |
| Thành viên 2 | `QuestionController` + `QuestionService` + `QuestionRepository` |
| Thành viên 3 | `ExamController` + `ExamService` + `ExamRepository` + Tính điểm |
| Cả nhóm | Cập nhật `API_Doc.md` sau khi làm xong mỗi endpoint |

> [!IMPORTANT]
> Mỗi người làm trên **branch riêng**: `feature/auth`, `feature/questions`, `feature/exam`. Xong thì tạo **Pull Request** để Tân review trước khi merge vào `main`.

> [!WARNING]
> **KHÔNG** bao giờ commit thẳng lên `main`. Luôn làm qua branch + Pull Request để tránh conflict code.

---

## 📋 Checklist Trước Khi Code

- [ ] Pull code mới nhất từ `main` về
- [ ] Tạo branch mới: `git checkout -b feature/ten-tinh-nang`
- [ ] Hiểu rõ Interface cần implement (xem `Interfaces/`)
- [ ] Viết code theo đúng tầng (Controller → Service → Repository)
- [ ] Test bằng Swagger trước khi push
- [ ] Cập nhật `API_Doc.md` nếu thêm endpoint mới
- [ ] Tạo Pull Request, tag Tân review
