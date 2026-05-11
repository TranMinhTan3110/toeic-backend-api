# THIẾT KẾ DATABASE FIREBASE TOÀN DIỆN — TOEIC APP & WEB

## Tổng quan hệ thống

Ứng dụng TOEIC gồm các module chính:
- **Từ vựng**: Flashcard, Luyện Gõ, Luyện Nói, Ghép Thẻ, Đặt Câu (AI chữa), Import từ vựng
- **Ngữ pháp**: Bài học theo chủ đề, Bài tập, Giải thích AI
- **4 Kỹ năng**: Nghe, Nói, Đọc, Viết (có AI hỗ trợ)
- **Luyện tập**: Tự chọn Part, số câu, thời gian linh hoạt
- **Thi thử**: Đúng format TOEIC (200 câu / 120 phút)

---

## Tổng quan Collections

```
Firestore Database
├── users                    ← Hồ sơ người dùng
├── questions                ← Câu hỏi Part 1–7 (Nghe + Đọc)
├── question_groups          ← Nhóm câu Part 3,4,6,7
├── exams                    ← Bộ đề thi thử hoàn chỉnh
├── practice_sessions        ← Phiên luyện tập linh hoạt (không cố định số câu)
├── user_results             ← Kết quả thi thử L&R
├── vocabulary               ← Từ vựng hệ thống (system)
├── user_vocabulary          ← Từ vựng người dùng tự import / thêm
├── vocab_learning_progress  ← Tiến độ học từng từ của user
├── vocab_sentence_practice  ← Lịch sử luyện đặt câu + AI chữa
├── grammar_topics           ← Chủ đề ngữ pháp (nhóm bài học)
├── grammar_lessons          ← Bài học ngữ pháp chi tiết
├── grammar_exercises        ← Câu hỏi bài tập ngữ pháp
├── user_grammar_progress    ← Tiến độ ngữ pháp của user
├── speaking_questions       ← Đề Speaking (11 task)
├── speaking_submissions     ← Bài làm Speaking + AI score
├── speaking_results         ← Kết quả tổng hợp 1 lần thi Speaking
├── writing_questions        ← Đề Writing (8 task)
├── writing_submissions      ← Bài làm Writing + AI score
├── writing_results          ← Kết quả tổng hợp 1 lần thi Writing
├── user_saved_items         ← Bookmark từ vựng / câu hỏi / ngữ pháp
└── notifications            ← Thông báo hệ thống
```


---

## Firebase Storage Structure

```
storage/
├── audio/
│   ├── part1/              ← Audio Part 1
│   ├── part2/              ← Audio Part 2
│   ├── part3/              ← Audio Part 3
│   ├── part4/              ← Audio Part 4
│   └── vocab/              ← Audio phát âm từ vựng
├── images/
│   ├── part1/              ← Ảnh Part 1
│   ├── part3/              ← Hình minh họa Part 3
│   ├── writing/            ← Ảnh đề Writing
│   └── speaking/           ← Ảnh đề Speaking
├── user_audio/             ← Audio user record (Speaking)
│   └── {uid}/              ← Phân theo user
└── user_vocab_import/      ← File import từ vựng (Excel/CSV)
    └── {uid}/
```

---

## 1. Collection: `users`

**Mô tả:** Hồ sơ người dùng, đồng bộ với Firebase Authentication.

| Field | Kiểu | Mô tả |
|---|---|---|
| uid | string | Firebase Auth UID (document ID) |
| display_name | string | Tên hiển thị |
| email | string | Địa chỉ email |
| avatar_url | string? | URL ảnh đại diện |
| target_score | number | Mục tiêu điểm TOEIC |
| current_level | string | `beginner` \| `intermediate` \| `advanced` |
| plan | string | `free` \| `premium` |
| streak_days | number | Số ngày học liên tiếp |
| last_study_date | timestamp | Ngày học gần nhất |
| total_study_minutes | number | Tổng thời gian học (phút) |
| preferred_skills | array | Kỹ năng ưu tiên: `["listening","reading"]` |
| created_at | timestamp | Ngày tạo tài khoản |

**Data mẫu:**
```json
{
  "uid": "uid_firebase_001",
  "display_name": "Nguyễn Văn A",
  "email": "a@gmail.com",
  "target_score": 700,
  "current_level": "intermediate",
  "plan": "free",
  "streak_days": 12,
  "last_study_date": "2024-02-20T00:00:00Z",
  "total_study_minutes": 450,
  "preferred_skills": ["listening", "vocabulary"],
  "created_at": "2024-01-15T09:00:00Z"
}
```

---

## 2. Collection: `questions`

**Mô tả:** Câu hỏi Part 1–7 (Listening & Reading). Dùng cho thi thử và luyện tập.

> ⚠️ Part 1: image_url + audio_url, KHÔNG có question_text
> Part 2,3,4: audio_url, KHÔNG có image_url
> Part 3,4,6,7: BẮT BUỘC có group_id

| Field | Kiểu | Mô tả |
|---|---|---|
| id | string | Auto-ID. VD: `q_part5_101` |
| part | number | Part 1–7 |
| question_text | string? | Nội dung câu hỏi |
| image_url | string? | URL ảnh |
| audio_url | string? | URL audio |
| options | array | `["A. ...", "B. ...", "C. ...", "D. ..."]` |
| correct_answer | string | `A` \| `B` \| `C` \| `D` |
| explanation | string | Giải thích đáp án |
| explanation_vi | string | Giải thích tiếng Việt |
| group_id | string? | ID nhóm (Part 3,4,6,7) |
| tags | array | `["grammar","office","past-tense"]` |
| grammar_topic_id | string? | Liên kết với grammar_topics |
| is_for_exam | boolean | true = xuất hiện trong bài thi thử |
| is_for_practice | boolean | true = xuất hiện trong phòng luyện tập |
| difficulty | string | `easy` \| `medium` \| `hard` |
| skill | string | `listening` \| `reading` |
| created_at | timestamp | Thời điểm tạo |

**Data mẫu Part 5:**
```json
{
  "id": "q_part5_101",
  "part": 5,
  "is_for_exam": true,
  "is_for_practice": false,
  "question_text": "The manager _______ the report before the deadline.",
  "options": ["A. submit", "B. submitted", "C. submitting", "D. to submit"],
  "correct_answer": "B",
  "explanation": "Past simple tense — submitted.",
  "explanation_vi": "Dùng V2 (submitted) vì câu ở thì quá khứ đơn.",
  "group_id": null,
  "tags": ["grammar", "past-tense"],
  "grammar_topic_id": "grammar_topic_003",
  "difficulty": "medium",
  "skill": "reading"
}
```

---

## 3. Collection: `question_groups`

**Mô tả:** Nội dung chung (bài đọc / hội thoại) cho nhóm câu Part 3, 4, 6, 7.

| Field | Kiểu | Mô tả |
|---|---|---|
| id | string | Auto-ID. VD: `group_part7_042` |
| part | number | 3, 4, 6, hoặc 7 |
| passage_text | string? | Đoạn văn/hội thoại |
| image_url | string? | Hình minh họa |
| audio_url | string? | Audio (Part 3, 4) |
| question_ids | array | ['q_id1', 'q_id2'x`, ...] |
| question_count | number | Số câu trong nhóm |
| source | string? | Nguồn đề: `ETS 2024 Test 01` |
| created_at | timestamp | Thời điểm tạo |

---

## 4. Collection: `exams`

**Mô tả:** Bộ đề thi thử hoàn chỉnh 200 câu đúng format TOEIC.

| Field | Kiểu | Mô tả |
|---|---|---|
| exam_id | string | VD: `ETS-2024-Test-01` |
| title | string | Tên đề thi |
| description | string? | Mô tả ngắn |
| duration | number | Thời gian làm bài (phút), mặc định 120 |
| question_ids | array | 200 ID câu hỏi, theo thứ tự Part 1→7 |
| part_distribution | map | `{part1:6, part2:25, part3:39, part4:30, part5:30, part6:16, part7:54, total:200}` |
| year | number | Năm phát hành |
| difficulty | string | `easy` \| `medium` \| `hard` |
| is_published | boolean | Hiển thị cho user |
| is_premium | boolean | Chỉ dành premium user |
| attempt_count | number | Số lượt đã làm (aggregate) |
| created_at | timestamp | Thời điểm tạo |

---

## 5. Collection: `practice_sessions`

**Mô tả:** Phiên luyện tập linh hoạt — user tự chọn Part, số câu, thời gian. KHÁC với thi thử (không cố định 200 câu, không cố định thời gian).

>  Đây là điểm khác biệt chính với `user_results`: luyện tập thì số câu linh hoạt, thời gian tùy chọn.

| Field | Kiểu | Mô tả |
|---|---|---|
| id | string | Auto-ID |
| user_id | string | UID người dùng |
| session_type | string | `practice` \| `quick_review` \| `part_focus` |
| skill | string | `listening` \| `reading` \| `all` |
| parts_selected | array | Parts người dùng chọn: `[1,2]` hoặc `[5,6,7]` |
| question_ids | array | Danh sách câu hỏi trong phiên (bất kỳ số lượng) |
| question_count | number | Số câu trong phiên |
| time_limit | number? | Thời gian giới hạn (giây), null = không giới hạn |
| answers | map | `{"q_id": "A", "q_id2": "C"}` |
| correct_count | number | Số câu đúng |
| incorrect_ids | array | ID câu làm sai |
| time_spent | number | Thời gian thực tế (giây) |
| difficulty_filter | string? | Bộ lọc độ khó đã chọn |
| tags_filter | array? | Bộ lọc tags đã chọn |
| started_at | timestamp | Thời điểm bắt đầu |
| completed_at | timestamp? | Thời điểm hoàn thành (null nếu chưa xong) |
| status | string | `in_progress` \| `completed` \| `abandoned` |

**Data mẫu:**
```json
{
  "id": "practice_xyz001",
  "user_id": "uid_firebase_001",
  "session_type": "part_focus",
  "skill": "listening",
  "parts_selected": [1, 2],
  "question_count": 20,
  "time_limit": 1200,
  "answers": {"q_part1_001": "A", "q_part2_010": "B"},
  "correct_count": 16,
  "incorrect_ids": ["q_part2_010"],
  "time_spent": 950,
  "status": "completed",
  "started_at": "2024-02-20T14:00:00Z",
  "completed_at": "2024-02-20T14:16:00Z"
}
```

---

## 6. Collection: `user_results`

**Mô tả:** Kết quả thi thử Listening & Reading (đúng format 200 câu).

| Field | Kiểu | Mô tả |
|---|---|---|
| result_id | string | Auto-ID |
| user_id | string | UID người dùng |
| exam_id | string | ID đề thi |
| score_listening | number | Điểm Listening (5–495) |
| score_reading | number | Điểm Reading (5–495) |
| total_score | number | Tổng điểm TOEIC (10–990) |
| correct_count | number | Số câu đúng / 200 |
| answers | map | `{"q_id": "A"}` |
| part_scores | map | `{part1: {correct:5, total:6}, ...}` |
| time_spent | number | Thời gian làm bài (giây) |
| completed_at | timestamp | Thời điểm nộp bài |

---

## 7. Collection: `vocabulary`

**Mô tả:** Từ vựng TOEIC hệ thống (system-provided), chia theo chủ đề. Người dùng có thể học, bookmark, hoặc thêm vào danh sách cá nhân.

| Field | Kiểu | Mô tả |
|---|---|---|
| id | string | Auto-ID. VD: `vocab_001` |
| word | string | Từ hoặc cụm từ |
| phonetic | string? | Phiên âm IPA. VD: `/nɪˈɡoʊʃieɪt/` |
| word_type | string | `noun` \| `verb` \| `adjective` \| `adverb` \| `phrase` |
| definition_en | string | Nghĩa tiếng Anh |
| definition_vi | string | Nghĩa tiếng Việt |
| examples | array | Mảng `{sentence, sentence_vi, audio_url?}` |
| audio_url | string? | URL audio phát âm chuẩn |
| image_url | string? | URL hình ảnh minh họa |
| topic | string | `business` \| `office` \| `travel` \| `finance` \| `health` \| `technology` \| `legal` \| `marketing` |
| level | string | `basic` \| `intermediate` \| `advanced` |
| synonyms | array | Từ đồng nghĩa: `["bargain", "mediate"]` |
| antonyms | array | Từ trái nghĩa: `["agree", "accept"]` |
| collocations | array | Cụm đi kèm: `["negotiate a deal", "negotiate terms"]` |
| related_question_ids | array | ID câu hỏi có dùng từ này |
| frequency | string | Tần suất xuất hiện trong TOEIC: `high` \| `medium` \| `low` |
| created_at | timestamp | Thời điểm thêm |

**Data mẫu:**
```json
{
  "id": "vocab_001",
  "word": "negotiate",
  "phonetic": "/nɪˈɡoʊʃieɪt/",
  "word_type": "verb",
  "definition_en": "To discuss something in order to reach an agreement.",
  "definition_vi": "Đàm phán, thương lượng.",
  "examples": [
    {"sentence": "They negotiated a new contract with the supplier.", "sentence_vi": "Họ đã đàm phán một hợp đồng mới với nhà cung cấp."},
    {"sentence": "The union is negotiating better wages.", "sentence_vi": "Công đoàn đang đàm phán mức lương tốt hơn."}
  ],
  "audio_url": "https://storage.googleapis.com/.../vocab/negotiate.mp3",
  "topic": "business",
  "level": "intermediate",
  "synonyms": ["bargain", "mediate", "discuss"],
  "antonyms": ["agree", "accept"],
  "collocations": ["negotiate a deal", "negotiate terms", "negotiate a contract"],
  "frequency": "high"
}
```

---

## 8. Collection: `user_vocabulary`

**Mô tả:** Từ vựng do người dùng tự thêm (import file, chụp ảnh, bôi đen trong bài) hoặc clone từ system vocabulary. Đây là bộ từ riêng tư của từng user.

> 💡 Ba cách thêm từ:
> 1. **Import file**: User upload Excel/CSV theo format chuẩn
> 2. **Chụp ảnh**: User chụp ảnh → AI (Vision API) trích xuất từ → user xác nhận
> 3. **Bôi đen**: User bôi đen từ trong Part 5-6-7 → bấm "Thêm vào từ vựng"

| Field | Kiểu | Mô tả |
|---|---|---|
| id | string | Auto-ID |
| user_id | string | UID người dùng |
| word | string | Từ hoặc cụm từ |
| phonetic | string? | Phiên âm |
| word_type | string? | Loại từ |
| definition_vi | string | Nghĩa tiếng Việt (bắt buộc) |
| definition_en | string? | Nghĩa tiếng Anh |
| examples | array | Mảng `{sentence, sentence_vi}` |
| audio_url | string? | URL audio (nếu TTS đã tạo) |
| image_url | string? | URL ảnh (nếu user cung cấp) |
| source | string | Nguồn: `import_file` \| `photo_ocr` \| `highlight` \| `manual` \| `system_clone` |
| source_question_id | string? | ID câu hỏi nguồn (nếu bôi đen từ bài) |
| system_vocab_id | string? | ID trong `vocabulary` nếu clone từ system |
| topic | string? | Chủ đề tự phân loại |
| tags | array | Nhãn user tự đặt |
| is_active | boolean | true = đang học |
| added_at | timestamp | Thời điểm thêm |

**Data mẫu (bôi đen từ bài):**
```json
{
  "id": "uv_abc001",
  "user_id": "uid_firebase_001",
  "word": "reimburse",
  "definition_vi": "Hoàn tiền, bồi hoàn",
  "definition_en": "To pay back money to someone",
  "source": "highlight",
  "source_question_id": "q_part7_210",
  "topic": "finance",
  "tags": ["part7", "finance"],
  "is_active": true,
  "added_at": "2024-02-20T15:00:00Z"
}
```

**Format file import (Excel/CSV):**
```
word | word_type | definition_vi | definition_en | example | example_vi
negotiate | verb | Đàm phán | To discuss to reach an agreement | They negotiated | Họ đã đàm phán
```

---

## 9. Collection: `vocab_learning_progress`

**Mô tả:** Tiến độ học từng từ của user — dùng thuật toán Spaced Repetition (SRS) để lên lịch ôn tập. Hỗ trợ cả từ system lẫn từ user tự thêm.

| Field | Kiểu | Mô tả |
|---|---|---|
| id | string | Auto-ID |
| user_id | string | UID người dùng |
| vocab_id | string | ID trong `vocabulary` hoặc `user_vocabulary` |
| vocab_source | string | `system` \| `user` |
| status | string | `new` \| `learning` \| `reviewing` \| `mastered` |
| flashcard_score | number | Điểm Flashcard (0–5, SRS) |
| typing_score | number | Điểm Luyện Gõ (0–100) |
| speaking_score | number | Điểm Luyện Nói/phát âm (0–100) |
| matching_score | number | Điểm Ghép Thẻ (0–100) |
| sentence_attempts | number | Số lần đặt câu |
| next_review_at | timestamp | Thời điểm ôn tập tiếp theo (SRS) |
| review_count | number | Số lần đã ôn |
| last_practiced_at | timestamp | Lần luyện gần nhất |
| ease_factor | number | Hệ số SRS (mặc định 2.5) |
| interval_days | number | Khoảng cách ôn (ngày) |

**Data mẫu:**
```json
{
  "id": "vlp_001",
  "user_id": "uid_firebase_001",
  "vocab_id": "vocab_001",
  "vocab_source": "system",
  "status": "reviewing",
  "flashcard_score": 4,
  "typing_score": 80,
  "speaking_score": 75,
  "matching_score": 90,
  "sentence_attempts": 2,
  "next_review_at": "2024-02-21T08:00:00Z",
  "review_count": 5,
  "ease_factor": 2.5,
  "interval_days": 3
}
```

---

## 10. Collection: `vocab_sentence_practice`

**Mô tả:** Lịch sử luyện đặt câu với từ vựng — user đặt câu, AI chữa và nhận xét.

| Field | Kiểu | Mô tả |
|---|---|---|
| id | string | Auto-ID |
| user_id | string | UID người dùng |
| vocab_id | string | ID từ vựng (system hoặc user) |
| vocab_source | string | `system` \| `user` |
| word | string | Từ đã đặt câu (denormalized) |
| user_sentence | string | Câu do user viết |
| ai_feedback | string | Nhận xét AI tiếng Việt |
| ai_corrected | string? | Câu đã chữa (nếu sai) |
| is_correct | boolean | AI đánh giá đúng/sai |
| grammar_score | number | Điểm ngữ pháp (0–100) |
| relevance_score | number | Điểm phù hợp ngữ nghĩa (0–100) |
| ai_model | string | Model AI đã dùng |
| created_at | timestamp | Thời điểm nộp |

---

## 11. Collection: `grammar_topics`

**Mô tả:** Danh mục chủ đề ngữ pháp — nhóm các bài học liên quan. Được tổ chức theo category để dễ điều hướng.

| Field | Kiểu | Mô tả |
|---|---|---|
| id | string | Auto-ID. VD: `gtopic_001` |
| title | string | Tên chủ đề. VD: `Thì quá khứ đơn` |
| title_en | string | Tên tiếng Anh. VD: `Simple Past Tense` |
| category | string | `tense` \| `word_form` \| `conjunction` \| `preposition` \| `clause` \| `pronoun` \| `comparison` |
| description | string | Mô tả ngắn |
| icon | string? | Tên icon / emoji |
| lesson_count | number | Số bài học trong chủ đề |
| exercise_count | number | Số câu bài tập |
| related_parts | array | Part liên quan: `[5, 6]` |
| difficulty | string | `basic` \| `intermediate` \| `advanced` |
| order | number | Thứ tự hiển thị |
| is_published | boolean | Hiển thị cho user |

**Data mẫu:**
```json
{
  "id": "gtopic_001",
  "title": "Thì quá khứ đơn",
  "title_en": "Simple Past Tense",
  "category": "tense",
  "description": "Diễn tả hành động đã xảy ra và kết thúc trong quá khứ.",
  "lesson_count": 3,
  "exercise_count": 20,
  "related_parts": [5, 6],
  "difficulty": "basic",
  "order": 1,
  "is_published": true
}
```

---

## 12. Collection: `grammar_lessons`

**Mô tả:** Bài học ngữ pháp chi tiết — giải thích lý thuyết, công thức, ví dụ đúng/sai.

| Field | Kiểu | Mô tả |
|---|---|---|
| id | string | Auto-ID. VD: `glesson_001` |
| topic_id | string | ID chủ đề (grammar_topics) |
| title | string | Tên bài học |
| order | number | Thứ tự trong topic |
| explanation | string | Giải thích chi tiết (hỗ trợ markdown) |
| formula | string? | Công thức ngữ pháp. VD: `S + V2/ed + O` |
| signal_words | array | Từ nhận biết: `["yesterday", "last week", "ago"]` |
| examples_correct | array | `[{sentence, translation}]` |
| examples_wrong | array | `[{sentence, translation, correction}]` |
| tips | array | Mẹo ghi nhớ |
| related_vocab_ids | array | Từ vựng liên quan |
| related_question_ids | array | Câu hỏi Part 5/6 test quy tắc này |
| estimated_minutes | number | Thời gian học ước tính |

---

## 13. Collection: `grammar_exercises`

**Mô tả:** Câu hỏi bài tập ngữ pháp — hiển thị giải thích khi đúng hoặc sai. Khác với `questions` (Part 1-7), đây là bài tập tập trung vào ngữ pháp cụ thể.

| Field | Kiểu | Mô tả |
|---|---|---|
| id | string | Auto-ID. VD: `gex_001` |
| topic_id | string | ID chủ đề ngữ pháp |
| lesson_id | string? | ID bài học cụ thể |
| question_text | string | Câu hỏi / câu điền khuyết |
| options | array | 4 lựa chọn A-D |
| correct_answer | string | `A` \| `B` \| `C` \| `D` |
| explanation_correct | string | Giải thích khi trả lời đúng |
| explanation_wrong | map | `{"A": "...", "B": "...", "C": "..."}` — giải thích từng đáp án sai |
| grammar_rule_tag | string | Tag quy tắc cụ thể. VD: `past-simple-regular` |
| difficulty | string | `easy` \| `medium` \| `hard` |
| order | number | Thứ tự trong bài học |

**Data mẫu:**
```json
{
  "id": "gex_001",
  "topic_id": "gtopic_001",
  "question_text": "She _______ the project last Friday.",
  "options": ["A. finish", "B. finished", "C. finishing", "D. has finished"],
  "correct_answer": "B",
  "explanation_correct": "Đúng! 'last Friday' là signal word của thì quá khứ đơn → dùng V-ed (finished).",
  "explanation_wrong": {
    "A": "'finish' là động từ nguyên thể, không dùng trong quá khứ đơn với động từ thường.",
    "C": "'finishing' là V-ing, dùng trong thì tiếp diễn, không phải đơn.",
    "D": "'has finished' là hiện tại hoàn thành, không dùng với 'last Friday'."
  },
  "grammar_rule_tag": "past-simple-regular",
  "difficulty": "easy",
  "order": 1
}
```

---

## 14. Collection: `user_grammar_progress`

**Mô tả:** Tiến độ học ngữ pháp của từng user — theo dõi topic và exercise đã hoàn thành.

| Field | Kiểu | Mô tả |
|---|---|---|
| id | string | Auto-ID |
| user_id | string | UID người dùng |
| topic_id | string | ID chủ đề |
| lessons_completed | array | ID bài học đã xong |
| exercises_attempted | number | Số câu đã làm |
| exercises_correct | number | Số câu đúng |
| accuracy_rate | number | Tỷ lệ đúng (0–100%) |
| mastery_level | string | `not_started` \| `learning` \| `practiced` \| `mastered` |
| last_practiced_at | timestamp | Lần luyện gần nhất |
| weak_rules | array | Tags quy tắc hay sai |

---

## 15. Collection: `speaking_questions`

**Mô tả:** Câu hỏi Speaking TOEIC — 11 câu, 6 dạng task.

| Task | Dạng | Thời gian chuẩn bị | Thời gian trả lời |
|---|---|---|---|
| 1–2 | read_aloud | 45s | 45s |
| 3 | describe_picture | 30s | 45s |
| 4–6 | respond_questions | 0s | 15/15/30s |
| 7–9 | respond_with_info | 30s | 15/15/30s |
| 10 | propose_solution | 30s | 60s |
| 11 | express_opinion | 15s | 60s |

| Field | Kiểu | Mô tả |
|---|---|---|
| id | string | Auto-ID. VD: `spk_task3_001` |
| task_number | number | 1–11 |
| task_type | string | `read_aloud` \| `describe_picture` \| `respond_questions` \| `respond_with_info` \| `propose_solution` \| `express_opinion` |
| prompt_text | string? | Đoạn văn / câu hỏi / mô tả vấn đề dạng text |
| prompt_image_url | string? | URL ảnh (Task 3, 7–9) |
| prompt_audio_url | string? | URL audio đề bài (Task 7–10) |
| questions | array? | Danh sách câu hỏi (Task 4–9) |
| preparation_time | number | Thời gian chuẩn bị (giây) |
| response_time | number | Thời gian trả lời (giây) |
| answer_times | array? | `[15, 15, 30]` nếu mỗi câu khác nhau |
| scoring_criteria | array | `["pronunciation","intonation","grammar","content"]` |
| max_score | number | Điểm tối đa: 3 hoặc 5 |
| sample_answer | string? | Câu trả lời mẫu |
| ai_prompt | string | Prompt gửi AI chấm — có `{transcript}` placeholder |
| topic | string? | Chủ đề |
| difficulty | string | `easy` \| `medium` \| `hard` |
| exam_set_id | string? | Bộ đề: `ETS-SPK-2024-01` |
| is_practice | boolean | true = dùng cho luyện tập riêng lẻ |
| created_at | timestamp | Thời điểm tạo |

---

## 16. Collection: `speaking_submissions`

**Mô tả:** Bài làm Speaking của user — lưu audio URL, transcript STT, điểm AI.

| Field | Kiểu | Mô tả |
|---|---|---|
| id | string | Auto-ID |
| user_id | string | UID người dùng |
| question_id | string | ID câu hỏi Speaking |
| result_id | string? | ID kết quả tổng hợp (nếu thi thử) |
| session_type | string | `exam` \| `practice` |
| audio_url | string | URL file mp3 trên Firebase Storage |
| audio_duration | number | Độ dài audio (giây) |
| transcript | string | Văn bản từ Speech-to-Text |
| ai_score | number | Điểm AI chấm (0–max_score) |
| ai_feedback | map | `{pronunciation:4, grammar:3, feedback_vi:"..."}` |
| ai_model | string | `gpt-4o` \| `gemini-1.5-pro` |
| status | string | `pending` \| `processing` \| `scored` \| `error` |
| submitted_at | timestamp | Thời điểm nộp |
| scored_at | timestamp? | Thời điểm AI chấm xong |

---

## 17. Collection: `speaking_results`

**Mô tả:** Kết quả tổng hợp 1 lần thi Speaking (11 câu). Điểm quy đổi 0–200 theo ETS.

| Field | Kiểu | Mô tả |
|---|---|---|
| id | string | Auto-ID |
| user_id | string | UID người dùng |
| exam_set_id | string | Bộ đề đã thi |
| submission_ids | array | 11 ID từ speaking_submissions |
| raw_score | number | Tổng điểm thô |
| scaled_score | number | Điểm quy đổi 0–200 |
| level | string | `Advanced` \| `High-Intermediate` \| `Intermediate` \| `Low-Intermediate` \| `Basic` |
| score_by_task | map | `{task1:3, task2:4, ...}` |
| avg_pronunciation | number | Điểm phát âm TB |
| avg_grammar | number | Điểm ngữ pháp TB |
| avg_vocabulary | number | Điểm từ vựng TB |
| avg_content | number | Điểm nội dung TB |
| status | string | `in_progress` \| `completed` |
| completed_at | timestamp? | Thời điểm hoàn thành |

---

## 18. Collection: `writing_questions`

**Mô tả:** Câu hỏi Writing TOEIC — 8 câu, 3 dạng task.

| Task | Dạng | Thời gian |
|---|---|---|
| 1–5 | write_sentence | 8 phút (cả 5 câu) |
| 6–7 | respond_email | 10 phút/câu |
| 8 | opinion_essay | 30 phút |

| Field | Kiểu | Mô tả |
|---|---|---|
| id | string | Auto-ID. VD: `wrt_task8_001` |
| task_number | number | 1–8 |
| task_type | string | `write_sentence` \| `respond_email` \| `opinion_essay` |
| prompt_text | string | Đề bài |
| prompt_image_url | string? | URL ảnh (Task 1–5) |
| given_words | array? | 2 từ cho sẵn (Task 1–5): `["meeting","scheduled"]` |
| email_content | string? | Nội dung email cần phản hồi (Task 6–7) |
| email_questions | array? | Câu hỏi trong email |
| time_limit | number | Thời gian (phút) |
| min_words | number? | Số từ tối thiểu |
| max_score | number | Điểm tối đa: 3 \| 4 \| 5 |
| scoring_criteria | array | `["grammar","vocabulary","cohesion","relevance"]` |
| sample_answer | string? | Bài mẫu |
| ai_prompt | string | Prompt AI chấm — có `{user_answer}` placeholder |
| topic | string? | Chủ đề |
| difficulty | string | `easy` \| `medium` \| `hard` |
| exam_set_id | string? | Bộ đề: `ETS-WRT-2024-01` |
| is_practice | boolean | true = dùng luyện tập riêng lẻ |
| created_at | timestamp | Thời điểm tạo |

---

## 19. Collection: `writing_submissions`

**Mô tả:** Bài làm Writing của user — lưu text, điểm AI trả về ngay (không cần poll).

| Field | Kiểu | Mô tả |
|---|---|---|
| id | string | Auto-ID |
| user_id | string | UID người dùng |
| question_id | string | ID câu hỏi Writing |
| result_id | string? | ID kết quả tổng hợp (nếu thi thử) |
| session_type | string | `exam` \| `practice` |
| user_answer | string | Bài làm dạng text |
| word_count | number | Số từ |
| time_used | number | Thời gian thực tế (giây) |
| ai_score | number | Điểm AI chấm (0–max_score) |
| ai_feedback | map | `{grammar:4, vocabulary:4, feedback_vi:"..."}` |
| ai_model | string | Model AI đã dùng |
| status | string | `scored` \| `error` |
| submitted_at | timestamp | Thời điểm nộp |
| scored_at | timestamp | Thời điểm chấm xong |

---

## 20. Collection: `writing_results`

**Mô tả:** Kết quả tổng hợp 1 lần thi Writing (8 câu). Điểm quy đổi 0–200 theo ETS.

| Field | Kiểu | Mô tả |
|---|---|---|
| id | string | Auto-ID |


| user_id | string | UID người dùng |
| exam_set_id | string | Bộ đề đã thi |
| submission_ids | array | 8 ID từ writing_submissions |
| raw_score | number | Tổng điểm thô |
| scaled_score | number | Điểm quy đổi 0–200 |
| level | string | `Advanced` \| `High-Intermediate` \| `Intermediate` \| `Low-Intermediate` \| `Basic` |
| score_by_task | map | `{task1:3, task6:4, task8:4, ...}` |
| avg_grammar | number | Điểm ngữ pháp TB |
| avg_vocabulary | number | Điểm từ vựng TB |
| avg_cohesion | number | Điểm mạch lạc TB |
| total_words | number | Tổng số từ đã viết |
| status | string | `in_progress` \| `completed` |
| completed_at | timestamp? | Thời điểm hoàn thành |

---

## 21. Collection: `user_saved_items`

**Mô tả:** Bookmark — user lưu từ vựng / câu hỏi / ngữ pháp để xem lại. Nguồn bookmark đa dạng: từ bài thi, bài luyện, hoặc khi bôi đen từ.

| Field | Kiểu | Mô tả |
|---|---|---|
| id | string | Auto-ID |
| user_id | string | UID người dùng |
| item_id | string | ID của mục được lưu |
| item_type | string | `vocabulary` \| `user_vocabulary` \| `question` \| `grammar_topic` \| `grammar_lesson` |
| note | string? | Ghi chú của user |
| context | string? | Ngữ cảnh: `"Gặp trong Part 7 đề ETS 2024"` |
| saved_at | timestamp | Thời điểm lưu |

---

## Quan hệ giữa Collections

```
users (1)
  ├──→ user_results (nhiều)            user_id
  ├──→ practice_sessions (nhiều)       user_id
  ├──→ user_vocabulary (nhiều)         user_id
  ├──→ vocab_learning_progress (nhiều) user_id
  ├──→ vocab_sentence_practice (nhiều) user_id
  ├──→ user_grammar_progress (nhiều)   user_id
  ├──→ speaking_submissions (nhiều)    user_id
  ├──→ speaking_results (nhiều)        user_id
  ├──→ writing_submissions (nhiều)     user_id
  ├──→ writing_results (nhiều)         user_id
  └──→ user_saved_items (nhiều)        user_id

exams (1)
  ├──→ questions (nhiều)               question_ids[]
  └──→ user_results (nhiều)            exam_id

question_groups (1)
  └──→ questions (nhiều)               group_id

grammar_topics (1)
  ├──→ grammar_lessons (nhiều)         topic_id
  ├──→ grammar_exercises (nhiều)       topic_id
  └──→ user_grammar_progress (nhiều)   topic_id

vocabulary / user_vocabulary (1)
  └──→ vocab_learning_progress (nhiều) vocab_id

speaking_results (1)
  └──→ speaking_submissions (nhiều)    result_id

writing_results (1)
  └──→ writing_submissions (nhiều)     result_id
```

---

## Thứ tự tạo Collections

| Bước | Collection | Ghi chú |
|---|---|---|
| 1 | `users` | Tạo trước để test Auth |
| 2 | `vocabulary` | Dữ liệu hệ thống độc lập |
| 3 | `grammar_topics` | Cần trước grammar_lessons |
| 4 | `grammar_lessons` | Cần sau grammar_topics |
| 5 | `grammar_exercises` | Cần sau grammar_topics |
| 6 | `questions` | Core câu hỏi L&R |
| 7 | `question_groups` | Cần sau questions |
| 8 | `exams` | Cần sau questions |
| 9 | `speaking_questions` | Độc lập |
| 10 | `writing_questions` | Độc lập |
| 11–21 | Còn lại | Tự tạo khi có data từ user |

>  Collections từ `user_results` trở đi **không cần tạo thủ công** — BE/client tự tạo khi user thao tác.

---

## Composite Indexes cần tạo trên Firestore

| Collection | Index | Mục đích |
|---|---|---|
| `questions` | `part ASC + difficulty ASC` | Lọc câu theo Part và độ khó |
| `questions` | `tags ARRAY_CONTAINS + part ASC` | Lọc theo tag và part |
| `practice_sessions` | `user_id ASC + status ASC + started_at DESC` | Lịch sử luyện tập |
| `user_results` | `user_id ASC + completed_at DESC` | Lịch sử thi thử |
| `vocab_learning_progress` | `user_id ASC + status ASC + next_review_at ASC` | Hàng ôn tập hôm nay |
| `vocab_learning_progress` | `user_id ASC + vocab_source ASC` | Lọc từ system/user |
| `user_vocabulary` | `user_id ASC + topic ASC` | Lọc từ theo chủ đề |
| `grammar_exercises` | `topic_id ASC + difficulty ASC + order ASC` | Bài tập theo topic |
| `user_grammar_progress` | `user_id ASC + mastery_level ASC` | Lọc topic đã/chưa học |
| `speaking_submissions` | `user_id ASC + status ASC` | Bài đang chờ chấm |
| `speaking_submissions` | `result_id ASC + task_number ASC` | Câu trong 1 lần thi |
| `speaking_results` | `user_id ASC + completed_at DESC` | Lịch sử thi Speaking |
| `writing_results` | `user_id ASC + completed_at DESC` | Lịch sử thi Writing |
| `user_saved_items` | `user_id ASC + item_type ASC + saved_at DESC` | Bookmark theo loại |

---

## Security Rules (Firestore)

```javascript
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {

    function isAdmin() {
      return request.auth.token.admin == true;
    }

    // ===== PUBLIC READ / ADMIN WRITE =====
    match /questions/{id} {
      allow read: if request.auth != null;
      allow write: if isAdmin();
    }
    match /question_groups/{id} {
      allow read: if request.auth != null;
      allow write: if isAdmin();
    }
    match /exams/{id} {
      allow read: if request.auth != null;
      allow write: if isAdmin();
    }
    match /vocabulary/{id} {
      allow read: if request.auth != null;
      allow write: if isAdmin();
    }
    match /grammar_topics/{id} {
      allow read: if request.auth != null;
      allow write: if isAdmin();
    }
    match /grammar_lessons/{id} {
      allow read: if request.auth != null;
      allow write: if isAdmin();
    }
    match /grammar_exercises/{id} {
      allow read: if request.auth != null;
      allow write: if isAdmin();
    }
    match /speaking_questions/{id} {
      allow read: if request.auth != null;
      allow write: if isAdmin();
    }
    match /writing_questions/{id} {
      allow read: if request.auth != null;
      allow write: if isAdmin();
    }

    // ===== USER-OWNED DATA =====
    match /users/{userId} {
      allow read, write: if request.auth.uid == userId;
    }
    match /user_results/{id} {
      allow read: if request.auth.uid == resource.data.user_id;
      allow create: if request.auth.uid == request.resource.data.user_id;
    }
    match /practice_sessions/{id} {
      allow read, write: if request.auth.uid == resource.data.user_id;
      allow create: if request.auth.uid == request.resource.data.user_id;
    }
    match /user_vocabulary/{id} {
      allow read, write: if request.auth.uid == resource.data.user_id;
      allow create: if request.auth.uid == request.resource.data.user_id;
    }
    match /vocab_learning_progress/{id} {
      allow read, write: if request.auth.uid == resource.data.user_id;
      allow create: if request.auth.uid == request.resource.data.user_id;
    }
    match /vocab_sentence_practice/{id} {
      allow read: if request.auth.uid == resource.data.user_id;
      allow create: if request.auth.uid == request.resource.data.user_id;
    }
    match /user_grammar_progress/{id} {
      allow read, write: if request.auth.uid == resource.data.user_id;
      allow create: if request.auth.uid == request.resource.data.user_id;
    }
    match /speaking_submissions/{id} {
      allow read: if request.auth.uid == resource.data.user_id;
      allow create: if request.auth.uid == request.resource.data.user_id;
      allow update: if isAdmin(); // AI scoring via Admin SDK
    }
    match /speaking_results/{id} {
      allow read: if request.auth.uid == resource.data.user_id;
      allow create: if request.auth.uid == request.resource.data.user_id;
    }
    match /writing_submissions/{id} {
      allow read: if request.auth.uid == resource.data.user_id;
      allow create: if request.auth.uid == request.resource.data.user_id;
    }
    match /writing_results/{id} {
      allow read: if request.auth.uid == resource.data.user_id;
      allow create: if request.auth.uid == request.resource.data.user_id;
    }
    match /user_saved_items/{id} {
      allow read, write: if request.auth.uid == resource.data.user_id;
      allow create: if request.auth.uid == request.resource.data.user_id;
    }
  }
}
```

---

## Luồng hoạt động chính

### Từ vựng — Bôi đen từ trong bài
```
1. User đang làm Part 5/6/7 → bôi đen từ "reimburse"
2. Hiển thị popup: nghĩa tiếng Việt + "Thêm vào từ vựng"
3. Client tạo document trong user_vocabulary (source: "highlight")
4. Client tạo document trong vocab_learning_progress (status: "new")
5. Từ xuất hiện trong danh sách "Học hôm nay" của user
```

### Từ vựng — Import ảnh (OCR)
```
1. User chụp ảnh sách/vở/màn hình
2. Upload lên Firebase Storage → gọi API Vision (Google Cloud Vision)
3. API trả về danh sách từ được nhận diện
4. UI hiển thị danh sách để user xác nhận / chỉnh sửa
5. User bấm "Lưu" → tạo hàng loạt trong user_vocabulary (source: "photo_ocr")
```

### Luyện tập — Chọn Part linh hoạt
```
1. User vào Luyện Tập → chọn: Part 1+2, 20 câu, 15 phút
2. Backend query questions theo part IN [1,2], random 20 câu
3. Tạo practice_session (status: "in_progress")
4. User làm bài → submit từng câu real-time update practice_session.answers
5. Kết thúc → cập nhật status: "completed", correct_count, incorrect_ids
6. Với câu sai → gợi ý từ vựng/ngữ pháp liên quan
```

### Speaking — Submit và chấm AI
```
1. Flutter: ghi âm → upload mp3 lên Firebase Storage → nhận URL
2. POST /api/speaking/submit {audioUrl, questionId, resultId?}
3. .NET: Google STT → transcript
4. Tạo speaking_submission (status: "processing") → trả về submission_id ngay
5. .NET background: gửi transcript cho GPT-4o → nhận JSON điểm
6. Update submission (status: "scored", ai_feedback)
7. Flutter: poll GET /submissions/{id} mỗi 3s → khi scored → hiển thị điểm
```

### Writing — Submit và nhận điểm ngay
```
1. User gõ xong bài → đếm word_count → POST /api/writing/submit
2. .NET: gửi text cho GPT-4o → nhận JSON điểm (2-5 giây)
3. Lưu writing_submission (status: "scored") → trả về kết quả ngay
4. Flutter/React: nhận điểm ngay lập tức → hiển thị ScoreDialog
```

---

## Giới hạn Firestore cần biết

| Giới hạn | Giá trị | Giải pháp |
|---|---|---|
| `whereIn` tối đa | 30 phần tử | Load 200 câu phải chia batch |
| Document size | 1 MB | passage_text dài cần cắt bớt |
| Array field | 20.000 phần tử | question_ids an toàn |
| Write rate 1 doc | 1 req/giây | Dùng batch write khi import từ |

---

*Tài liệu này tổng hợp và mở rộng từ `TOEIC_Database_Design.md` và `database_speaking_writing.md`. Cập nhật: 2024-04-05.*
