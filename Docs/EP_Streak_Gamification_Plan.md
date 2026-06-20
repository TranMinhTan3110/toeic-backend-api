# Kế hoạch EP, Streak & Tương tác User

> Tài liệu kế hoạch gamification (EP, streak, leaderboard, profile). MVP: module **Từ vựng** trước; luyện tập / thi thử / ngữ pháp bổ sung sau qua `EngagementService`.

## Đánh giá ý tưởng

Ý tưởng **hợp lý và phù hợp** với mục tiêu giữ chân user (mô hình quen thuộc: Duolingo / Khan Academy). Điểm mạnh:

- **Một “tiền tệ” thống nhất (EP)** gắn mọi hành vi (từ vựng, luyện Part, thi thử) → user luôn thấy tiến bộ dù học module nào.
- **Streak nhân EP** khuyến khích quay lại hàng ngày mà không phải “phạt” quá nặng khi miss 1 ngày (có thể cấu hình freeze sau này).
- **Leaderboard tuần + profile** tạo cảm giác cộng đồng và mục tiêu ngắn hạn.

**Rủi ro cần xử lý sớm** (code hiện tại):

| Vấn đề | Hiện trạng |
|--------|------------|
| EP / streak | Có field trên `User.cs` nhưng **không bao giờ cập nhật** sau `AuthService` tạo user |
| `last_study_date` | Có trong design doc nhưng **thiếu trên entity** |
| Bảo mật | API nhận `userId` từ body/query, **không verify Firebase token** → leaderboard dễ bị spam EP |
| Nguồn EP | Chỉ vocab progress hoạt động; practice / thi thử chưa có API |

## Quyết định đã chốt

- **Leaderboard:** bảng **tuần toàn app** — top EP kiếm trong tuần hiện tại.
- **Streak:** ngày học tính theo **Asia/Ho_Chi_Minh** (00:00 VN = sang ngày mới).
- **Phạm vi MVP (nhánh hiện tại):** chỉ **Từ vựng**; luyện 4 kỹ năng / thi thử / ngữ pháp → nhánh khác, bổ sung EP sau.

## Phạm vi MVP: Từ vựng trước

| Module | Nhánh hiện tại | EP / streak |
|--------|----------------|-------------|
| **Flashcard / SRS** | `POST /api/vocabularies/progress` | **MVP:** hook ngay |
| Luyện gõ, nói, ghép thẻ, đặt câu | Chưa API / nhánh vocab | Sau merge |
| Luyện Part / thi / ngữ pháp | Nhánh khác | Phase 4 |

**ActivityType (mở rộng):** `VocabFlashcardReview` (MVP) → `VocabTyping`, … → `PracticeComplete`, `ExamComplete`, …

## Kiến trúc

### User — field bổ sung

- `LastStudyDate` — `yyyy-MM-dd` (VN)
- `BestStreakDays`
- `WeeklyEp`, `WeeklyEpPeriodKey` (vd `2026-W20`)

### Collection `ep_events`

`user_id`, `activity_type`, `base_ep`, `streak_multiplier`, `ep_awarded`, `reference_id`, `created_at`

### EngagementService

1. Load user → streak VN → EP × multiplier → cập nhật `ExperiencePoints`, `WeeklyEp` → ghi `ep_events`

**EP vocab (MVP):**

| Hoạt động | Base EP |
|-----------|---------|
| SRS quality ≥ 3 | 5 (tối đa 1 lần/từ/ngày) |
| Vừa mastered | +10 |
| Cap/ngày | 500 |

Multiplier: `1 + min(streakDays × 0.02, 0.5)` (max 1.5×)

### API

| Endpoint | Mô tả |
|----------|--------|
| `GET /api/users/me` | Profile + EP + streak |
| `PATCH /api/users/me` | Onboarding: target score, level, skills |
| `GET /api/leaderboard/weekly` | Top tuần |
| `POST /api/vocabularies/progress` | Bearer token; trả progress + EP |

### Bảo mật

Login Google + Firebase giữ nguyên. API học/EP: `Authorization: Bearer <ID token>`; `userId` lấy từ claims, không tin body.

## Lộ trình

1. **Phase 1:** Auth Bearer, EngagementService, hook vocab SRS, Users API  
2. **Phase 2:** Leaderboard weekly, Firestore index  
3. **Phase 3:** Chế độ vocab khác (sau merge nhánh vocab)  
4. **Phase 4:** Practice / exam / grammar (sau merge nhánh khác)

## Firestore index (leaderboard)

Collection `users`: `weekly_ep_period_key` ASC + `weekly_ep` DESC — tạo trong Firebase Console khi deploy.

---

*Chi tiản đầy đủ (diagram, luồng auth): bản gốc lưu trong Cursor plan; file này là bản làm việc trong repo.*
