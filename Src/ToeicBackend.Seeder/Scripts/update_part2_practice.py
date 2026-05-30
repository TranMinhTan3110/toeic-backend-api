import json
import os

PRACTICE_FILE = r'd:\Toeic\Toeic_Backend\Src\ToeicBackend.Seeder\SeedData\practiceListening.json'
AUDIO_BASE = 'https://res.cloudinary.com/dlfc5qwhj/video/upload/q_auto/f_auto/v1779182400/LR/Practice/Part%202/Audio/'

def update_part2():
    if not os.path.exists(PRACTICE_FILE):
        print(f"Lỗi: Không tìm thấy file {PRACTICE_FILE}")
        return

    with open(PRACTICE_FILE, 'r', encoding='utf-8') as f:
        data = json.load(f)

    count_id = 0
    count_audio = 0

    for q in data:
        qid = q.get('id', '')
        # Nếu ID hiện tại là q_part2_x, đổi thành p_q_part2_x và gán audio
        if qid.startswith('q_part2_'):
            pure_id = qid
            new_id = f"p_{qid}"
            q['id'] = new_id
            q['audio_url'] = f"{AUDIO_BASE}{pure_id}.mp3"
            count_id += 1
            count_audio += 1
        elif qid.startswith('p_q_part2_'):
            # Nếu đã có tiền tố p_ rồi, vẫn đảm bảo audio_url đúng mẫu
            pure_id = qid[2:]  # Bỏ 'p_' để lấy 'q_part2_x'
            q['audio_url'] = f"{AUDIO_BASE}{pure_id}.mp3"
            count_audio += 1

    with open(PRACTICE_FILE, 'w', encoding='utf-8') as f:
        json.dump(data, f, ensure_ascii=False, indent=4)

    print(f"--- KẾT QUẢ CẬP NHẬT PART 2 PRACTICE ---")
    print(f"✔ Đã đổi {count_id} ID sang định dạng 'p_q_part2_x'.")
    print(f"✔ Đã gán/cập nhật {count_audio} link audio.")

if __name__ == '__main__':
    update_part2()
