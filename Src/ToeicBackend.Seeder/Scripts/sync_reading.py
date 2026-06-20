import json
import os

# Cấu hình đường dẫn
READING_FILE = r'd:\Toeic\Toeic_Backend\Src\ToeicBackend.Seeder\SeedData\reading_test1.json'
QUESTIONS_FILE = r'd:\Toeic\Toeic_Backend\Src\ToeicBackend.Seeder\SeedData\questions.json'

def process_reading_data():
    if not os.path.exists(READING_FILE):
        print(f"Lỗi: Không tìm thấy file {READING_FILE}")
        return

    # 1. Đọc dữ liệu Reading
    with open(READING_FILE, 'r', encoding='utf-8') as f:
        reading_data = json.load(f)

    print(f"Đang xử lý {len(reading_data)} câu Reading...")

    # 2. Bổ sung các trường còn thiếu
    for q in reading_data:
        q['is_for_exam'] = True
        q['is_for_practice'] = True
        q['exam_id'] = "ets_2024_test_1"
        # Đảm bảo các trường null có mặt để đồng bộ schema
        if 'explanation_vi' not in q: q['explanation_vi'] = None
        if 'explanation' not in q: q['explanation'] = None

    # 3. Gộp vào questions.json
    if os.path.exists(QUESTIONS_FILE):
        with open(QUESTIONS_FILE, 'r', encoding='utf-8') as f:
            main_data = json.load(f)
        
        # Tránh trùng lặp ID nếu chạy nhiều lần
        existing_ids = {q['id'] for q in main_data if 'id' in q}
        new_questions = [q for q in reading_data if q['id'] not in existing_ids]
        
        main_data.extend(new_questions)
        
        with open(QUESTIONS_FILE, 'w', encoding='utf-8') as f:
            json.dump(main_data, f, ensure_ascii=False, indent=2)
        
        print(f"✔ Đã gộp thêm {len(new_questions)} câu mới vào questions.json.")
    else:
        print(f"Lỗi: Không tìm thấy file gốc {QUESTIONS_FILE}")

if __name__ == "__main__":
    process_reading_data()
