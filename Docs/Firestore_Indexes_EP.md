# Firestore indexes cho EP / Leaderboard

Tạo trong [Firebase Console](https://console.firebase.google.com) → Firestore → Indexes nếu query báo lỗi thiếu index.

## Collection `users` — leaderboard tuần

| Collection | Fields | Query scope |
|------------|--------|-------------|
| users | `weekly_ep_period_key` Asc, `weekly_ep` Desc | Collection |

## Collection `ep_events` — kiểm tra EP theo ngày / theo từ

| Collection | Fields | Query scope |
|------------|--------|-------------|
| ep_events | `user_id` Asc, `study_date_key` Asc | Collection |
| ep_events | `user_id` Asc, `activity_type` Asc, `reference_id` Asc, `study_date_key` Asc | Collection |
