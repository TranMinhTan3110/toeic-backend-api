# Kế hoạch Triển khai: Thiết kế 3 File Excel và Trình Import Đề thi TOEIC (L&R, Speaking, Writing)

Tài liệu này chi tiết hóa cấu trúc 3 mẫu file Excel (Listening & Reading, Speaking, Writing) được tối giản hóa (bỏ cột AI Prompt và Scoring Criteria), quy tắc đặt tên và đối chiếu (map) tệp đa phương tiện tự động, giải pháp chống trùng lặp ID câu hỏi giữa các đề thi, cùng phương án triển khai cụ thể trên Frontend và Backend.

---

## 1. Thiết kế cấu trúc 3 mẫu Excel (CSV) tối giản

Các cột phức tạp như **ScoringCriteria** và **AiPrompt** được loại bỏ khỏi Excel để người quản trị dễ nhập liệu. Hệ thống sẽ tự động sinh các giá trị mặc định cho các trường này dựa trên loại câu hỏi/Task.

### Template 1: Listening & Reading (L&R)
*Mẫu này áp dụng cho đề thi Đọc - Nghe (200 câu, Part 1 đến Part 7). Hỗ trợ ghép nhóm câu hỏi (Part 3, 4, 6, 7) qua `GroupId`.*

| Tên Cột | Kiểu Dữ Liệu | Ví dụ | Giải thích |
| :--- | :--- | :--- | :--- |
| **Part** | Số nguyên | `1` | Số Part (từ 1 đến 7) |
| **Id** | Chuỗi (Unique) | `t6_q_part1_1` | ID câu hỏi trong đề. Để tránh ghi đè dữ liệu (như đề `ets_2024_test_1` đã có sẵn ID `q_part1_1` trên Firestore), bạn nên chủ động điền ID kèm tiền tố mã đề (ví dụ: `t6_q_part1_1`). Nếu bạn điền ID ngắn dạng `q_part1_1`, hệ thống sẽ tự động ghép tiền tố mã đề khi import. |
| **QuestionText** | Chuỗi | `What does the man say about...?` | Nội dung câu hỏi (để trống với Part 1 & 2) |
| **OptionA** | Chuỗi | `A. It is free for customers.` | Phương án lựa chọn A |
| **OptionB** | Chuỗi | `B. It is under construction.` | Phương án lựa chọn B |
| **OptionC** | Chuỗi | `C. It closes soon.` | Phương án lựa chọn C |
| **OptionD** | Chuỗi | `D. It offers monthly contracts.` | Phương án lựa chọn D (để trống ở Part 2) |
| **CorrectAnswer** | Chuỗi | `A` | Đáp án đúng (`A`, `B`, `C` hoặc `D`) |
| **Explanation** | Chuỗi | `The man says...` | Giải thích chi tiết bằng tiếng Anh |
| **ExplanationVi** | Chuỗi | `Người đàn ông nói...` | Giải thích chi tiết bằng tiếng Việt |
| **ScriptOrPassage** | Chuỗi | `W: Thank you for... \n M: ...` | Script nghe (Part 1-4) hoặc Đoạn văn đọc (Part 6-7) |
| **Translation** | Chuỗi | `Cảm ơn bạn đã...` | Dịch tiếng Việt của Script/Đoạn văn |
| **ImageFileName** | Chuỗi | `lr_t6_p1_q1.png` | Tên file hình ảnh cục bộ (nếu có) |
| **AudioFileName** | Chuỗi | `lr_t6_p3_g1.mp3` | Tên file âm thanh cục bộ (nếu có) |
| **GroupId** | Chuỗi | `t6_group_part3_01` | ID nhóm câu hỏi (cho Part 3, 4, 6, 7 để gộp chung đoạn văn/audio). Nên thêm tiền tố mã đề (ví dụ: `t6_group_part3_01`) để phân biệt nhóm giữa các đề. Nếu điền ngắn, hệ thống tự động ghép tiền tố mã đề. |

#### Hướng dẫn & Ví dụ trực quan về ID và GroupId cho từng Part (Ví dụ với mã đề `t6`):

Để tránh bị ghi đè dữ liệu trên Firestore, toàn bộ ID câu hỏi (`Id`) và ID nhóm (`GroupId`) nên có tiền tố mã đề (ví dụ: `t6_`). Dưới đây là cách đặt ID mẫu từ Part 1 đến Part 7:

*   **Part 1 (Nghe tranh):**
    *   **Id:** `t6_q_part1_1`, `t6_q_part1_2`, `t6_q_part1_3`...
    *   **GroupId:** *Để trống* (Part 1 không gộp nhóm).
*   **Part 2 (Hỏi đáp):**
    *   **Id:** `t6_q_part2_7`, `t6_q_part2_8`, `t6_q_part2_9`...
    *   **GroupId:** *Để trống* (Part 2 không gộp nhóm).
*   **Part 3 (Hội thoại ngắn - Chung script và audio):**
    *   **Id:** `t6_q_part3_32`, `t6_q_part3_33`, `t6_q_part3_34`
    *   **GroupId:** `t6_group_part3_32_34` (hoặc `t6_group_part3_01`)
    *(3 câu hỏi này có chung GroupId để hệ thống biết chúng cùng thuộc một đoạn hội thoại).*
*   **Part 4 (Bài nói chuyện ngắn - Chung script và audio):**
    *   **Id:** `t6_q_part4_71`, `t6_q_part4_72`, `t6_q_part4_73`
    *   **GroupId:** `t6_group_part4_71_73` (hoặc `t6_group_part4_01`)
    *(3 câu hỏi này chung GroupId để gộp chung bài nói).*
*   **Part 5 (Điền vào chỗ trống):**
    *   **Id:** `t6_q_part5_101`, `t6_q_part5_102`, `t6_q_part5_103`...
    *   **GroupId:** *Để trống* (Part 5 làm riêng lẻ từng câu).
*   **Part 6 (Điền vào đoạn văn - Chung bài đọc):**
    *   **Id:** `t6_q_part6_131`, `t6_q_part6_132`, `t6_q_part6_133`, `t6_q_part6_134`
    *   **GroupId:** `t6_group_part6_131_134` (hoặc `t6_group_part6_01`)
    *(4 câu hỏi điền từ chung một bài đọc sẽ dùng chung GroupId này).*
*   **Part 7 (Đọc hiểu - Chung đoạn văn đơn/kép/ba):**
    *   **Id:** `t6_q_part7_147`, `t6_q_part7_148`, `t6_q_part7_149`...
    *   **GroupId:** `t6_group_part7_147_149` (hoặc `t6_group_part7_01`)
    *(Các câu hỏi cùng trả lời cho một đoạn văn đọc sẽ dùng chung GroupId này).*

---

### Template 2: Speaking (11 câu)
*Mẫu này áp dụng cho đề thi Nói (Task 1 đến Task 5). Sử dụng ký tự `|` (pipe) để phân tách các câu hỏi và câu trả lời mẫu con ở Task 3 & 4.*

| Tên Cột | Kiểu Dữ Liệu | Ví dụ | Giải thích |
| :--- | :--- | :--- | :--- |
| **TaskNumber** | Số nguyên | `3` | Số thứ tự Task (từ 1 đến 5) |
| **TaskType** | Chuỗi | `respond_questions` | Loại task (`read_aloud`, `describe_picture`, `respond_questions`, `respond_questions_with_info`, `express_opinion`) |
| **Id** | Chuỗi (Unique) | `t6_spk_task3_005` | ID câu hỏi Speaking. Nên thêm tiền tố mã đề để tránh trùng lặp giữa các đề thi (ví dụ: `t6_spk_task3_005`). Hệ thống tự động ghép tiền tố nếu bạn điền ID ngắn. |
| **PromptText** | Chuỗi | `A shuttle bus is presently en route...` | Đoạn văn cần đọc (Task 1) hoặc Văn cảnh/Đề bài (Task 3, 4, 5) |
| **Translation** | Chuỗi | `Một xe buýt trung chuyển hiện đang...` | Dịch tiếng Việt của văn cảnh/đề bài |
| **PromptImage** | Chuỗi | `spk_t6_t2_q3.png` | Tên file ảnh minh họa (mô tả tranh Task 2, lịch trình Task 4) |
| **PromptAudio** | Chuỗi | `spk_t6_t1_q1.mp3` | Tên file audio đề bài/bài đọc mẫu (Task 1, 3) |
| **PrepTime** | Số nguyên | `30` | Thời gian chuẩn bị làm bài (giây) |
| **RespTime** | Số nguyên | `45` | Thời gian trả lời (giây) |
| **Difficulty** | Chuỗi | `easy` | Độ khó (`easy`, `medium`, `hard`) |
| **MaxScore** | Số nguyên | `5` | Điểm số tối đa (Mặc định: `5`) |
| **Topic** | Chuỗi | `bus delay notice` | Chủ đề câu hỏi |
| **Questions** | Chuỗi (Dùng `\|`) | `Q5? \| Q6? \| Q7?` | Danh sách 3 câu hỏi con (dành riêng cho Task 3 & 4) |
| **QuestionsTranslation** | Chuỗi (Dùng `\|`) | `Dịch Q5? \| Dịch Q6? \| Dịch Q7?` | Dịch tiếng Việt của 3 câu hỏi con (Task 3 & 4) |
| **AnswerTimes** | Chuỗi (Dùng `\|`) | `15 \| 15 \| 30` | Thời gian trả lời tương ứng từng câu hỏi con (Task 3 & 4) |
| **SampleAnswers** | Chuỗi (Dùng `\|`) | `Ans5 \| Ans6 \| Ans7` | Câu trả lời mẫu (Nếu Task 1/2/5 thì nhập 1 chuỗi, Task 3/4 nhập 3 câu phân tách bằng `\|`) |
| **SampleAnswersTranslation**| Chuỗi (Dùng `\|`) | `Dịch Ans5 \| Dịch Ans6 \| Dịch Ans7` | Bản dịch tiếng Việt của câu trả lời mẫu |
| **Keywords** | Chuỗi (Dùng `\|`) | `word1:ipa1:mean1 \| word2:ipa2:mean2` | Từ vựng giải thích theo định dạng `Từ:Phiên Âm:Nghĩa` |

---

### Template 3: Writing (8 câu)
*Mẫu này áp dụng cho đề thi Viết (Task 1 đến Task 3). Hỗ trợ Viết tranh, Viết email và Viết luận.*

| Tên Cột | Kiểu Dữ Liệu | Ví dụ | Giải thích |
| :--- | :--- | :--- | :--- |
| **TaskNumber** | Số nguyên | `6` | Thứ tự câu (từ 1 đến 8: 1-5 viết câu, 6-7 email, 8 viết luận) |
| **TaskType** | Chuỗi | `respond_email` | Loại task (`write_sentence`, `respond_email`, `opinion_essay`) |
| **Id** | Chuỗi (Unique) | `t6_wrt_task6_001` | ID câu hỏi Writing. Nên theo định dạng `[exam_id]_wrt_task[TaskNumber]_001` (ví dụ: `t6_wrt_task6_001`). Hệ thống tự động chuẩn hóa ID dựa trên TaskNumber nếu bạn để trống hoặc điền ID ngắn. |
| **PromptText** | Chuỗi | `Respond to the written request.` | Đề bài/Hướng dẫn yêu cầu |
| **PromptImage** | Chuỗi | `wrt_t6_t1_q1.png` | Tên file ảnh minh họa (Chỉ dùng cho Task 1) |
| **GivenWords** | Chuỗi | `hard hat, arm` | Từ cho sẵn (phân cách bằng dấu phẩy, Chỉ dùng cho Task 1) |
| **EmailContent** | Chuỗi | `From: Emily Clark... Dear Mr. Wilson...` | Nội dung Email nhận được (Chỉ dùng cho Task 2) |
| **EmailQuestions** | Chuỗi (Dùng `\|`) | `Confirm meeting \| Suggest time` | Yêu cầu viết phản hồi email (Phân cách bằng `\|`, Chỉ dùng cho Task 2) |
| **ExplanationVi** | Chuỗi | `Từ: Emily Clark... Kính gửi ông...` | Bản dịch tiếng Việt của Email hoặc giải thích thêm |
| **MinWords** | Số nguyên | `300` | Số từ tối thiểu yêu cầu (Chỉ dùng cho Task 3: luận 300 từ) |
| **TimeLimit** | Số nguyên | `10` | Thời gian giới hạn làm bài (phút: 8ph cho Task 1, 10ph Task 2, 30ph Task 3) |
| **MaxScore** | Số nguyên | `4` | Điểm số tối đa (3 cho Task 1, 4 cho Task 2, 5 cho Task 3) |
| **Topic** | Chuỗi | `meeting schedule change email` | Chủ đề câu hỏi |
| **Difficulty** | Chuỗi | `medium` | Độ khó (`easy`, `medium`, `hard`) |
| **SampleAnswer** | Chuỗi | `Dear Ms. Clark, Thank you...` | Bài viết mẫu |
| **SampleAnswerTranslation**| Chuỗi | `Kính gửi cô Clark, Cảm ơn...` | Bản dịch tiếng Việt của bài viết mẫu |

---

## 2. Giải pháp chống trùng ID (Unique ID Generation)

Để giải quyết triệt để lỗi ghi đè dữ liệu trên Firestore khi các đề thi khác nhau trùng ID câu hỏi (ví dụ đề `ets_2024_test_1` và đề `t6` đều chứa ID câu hỏi là `q_part1_1` hoặc ID nhóm là `group_part3_01`):

1. **Công thức tự sinh ID độc nhất:** Khi lưu đề thi, hệ thống sẽ tự động ghép mã đề (`exam_id` - ví dụ `t6`) làm tiền tố cho toàn bộ ID câu hỏi và ID nhóm:
   - **ID câu hỏi thực tế trong Firestore:** `[exam_id]_[id_trong_excel]` (Ví dụ: `t6_q_part1_1`, `t6_spk_task3_005`).
   - **ID nhóm thực tế trong Firestore:** `[exam_id]_[group_id_trong_excel]` (Ví dụ: `t6_group_part3_01`).
2. **Đối chiếu nhóm câu hỏi:** Trong logic ghép nhóm, các ID câu hỏi con trong trường `question_ids` của nhóm cũng sẽ tự động được bổ sung tiền tố tương ứng để đảm bảo liên kết chính xác.
3. **Quy trình thực hiện:** Việc tự sinh ID độc nhất này sẽ được thực hiện trực tiếp ở **Frontend parser** (hoặc **Backend service**) để đảm bảo đồng nhất.

---

## 3. Quy tắc Đặt tên File & Map Media tự động

Tải lên thư mục media chứa hình ảnh/âm thanh và khớp tự động qua tên tệp đã định dạng sẵn:

* **Listening/Reading:**
  - Ảnh Part 1: `lr_[exam_id]_p1_q[number].[ext]` (Ví dụ: `lr_t6_p1_q1.png`)
  - Audio Part 1/2: `lr_[exam_id]_p[part]_q[number].mp3` (Ví dụ: `lr_t6_p2_q7.mp3`)
  - Audio nhóm Part 3/4: `lr_[exam_id]_p[part]_g[number].mp3` (Ví dụ: `lr_t6_p3_g1.mp3`)
  - **Ảnh minh họa hình vẽ Part 3/4 (Visual aids/graphics):** `lr_[exam_id]_p[part]_g[number].[ext]` (Ví dụ: `lr_t6_p3_g1.png`, `lr_t6_p4_g1.png`)
  - Ảnh nhóm Part 7: `lr_[exam_id]_p7_g[number].[ext]` (Ví dụ: `lr_t6_p7_g1.png`)
* **Speaking:**
  - Ảnh mô tả tranh / thông tin: `spk_[exam_id]_t[task]_q[number].[ext]` (Ví dụ: `spk_t6_t2_q3.png`, `spk_t6_t4_q8.png`)
  - Audio đề bài: `spk_[exam_id]_t[task]_q[number].mp3` (Ví dụ: `spk_t6_t1_q1.mp3`)
* **Writing:**
  - Ảnh viết câu tả tranh: `wrt_[exam_id]_t[task]_q[number].[ext]` (Ví dụ: `wrt_t6_t1_q1.png`)
