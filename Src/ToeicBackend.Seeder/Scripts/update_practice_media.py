import json
import os

PRACTICE_FILE = r'd:\Toeic\Toeic_Backend\Src\ToeicBackend.Seeder\SeedData\practiceListening.json'

IMAGE_BASE = 'https://res.cloudinary.com/dlfc5qwhj/image/upload/q_auto,f_auto/v1779181951/LR/Practice/Part1/Image/'
AUDIO_BASE = 'https://res.cloudinary.com/dlfc5qwhj/video/upload/q_auto,f_auto/v1779182004/LR/Practice/Part1/Audio/'

def update_practice_questions():
    if not os.path.exists(PRACTICE_FILE):
        print(f"Lỗi: Không tìm thấy file {PRACTICE_FILE}")
        return

    with open(PRACTICE_FILE, 'r', encoding='utf-8') as f:
        data = json.load(f)

    count_img = 0
    count_audio = 0

    for q in data:
        qid = q.get('id', '')
        # Extract the pure ID part without 'p_' prefix (e.g. 'q_part1_59')
        if qid.startswith('p_q_part1_'):
            pure_id = qid[2:]  # Remove the 'p_' prefix
            q['image_url'] = f"{IMAGE_BASE}{pure_id}.png"
            q['audio_url'] = f"{AUDIO_BASE}{pure_id}.mp3"
            count_img += 1
            count_audio += 1

    with open(PRACTICE_FILE, 'w', encoding='utf-8') as f:
        json.dump(data, f, ensure_ascii=False, indent=4)
    
    print(f"--- KẾT QUẢ CẬP NHẬT PRACTICE LISTENING ---")
    print(f"✔ Đã gán {count_img} link ảnh.")
    print(f"✔ Đã gán {count_audio} link audio.")

if __name__ == "__main__":
    update_practice_questions()
