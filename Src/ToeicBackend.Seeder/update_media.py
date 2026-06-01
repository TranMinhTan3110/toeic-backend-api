import json
import os

# Cấu hình đường dẫn file
QUESTIONS_FILE = r'd:\Toeic\Toeic_Backend\Src\ToeicBackend.Seeder\SeedData\questions.json'
GROUPS_FILE = r'd:\Toeic\Toeic_Backend\Src\ToeicBackend.Seeder\SeedData\question_groups.json'

# Cấu hình URL gốc của Cloudinary
IMAGE_BASE = 'https://res.cloudinary.com/dlfc5qwhj/image/upload/q_auto,f_auto/ETS-2024/Test-01/images/'
AUDIO_BASE = 'https://res.cloudinary.com/dlfc5qwhj/video/upload/q_auto,f_auto/ETS-2024/Test-01/audio/'

def update_questions():
    if not os.path.exists(QUESTIONS_FILE):
        print(f"Lỗi: Không tìm thấy file {QUESTIONS_FILE}")
        return

    with open(QUESTIONS_FILE, 'r', encoding='utf-8') as f:
        data = json.load(f)

    count_img = 0
    count_audio = 0

    for q in data:
        qid = q.get('id', '')
        part = q.get('part', 0)

        # Xử lý Part 1: Có cả Ảnh và Audio
        if part == 1 and qid.startswith('q_part1_'):
            q['image_url'] = f"{IMAGE_BASE}part1/{qid}.png"
            q['audio_url'] = f"{AUDIO_BASE}part1/{qid}.mp3"
            count_img += 1
            count_audio += 1
        
        # Xử lý Part 2: Chỉ có Audio
        elif part == 2 and qid.startswith('q_part2_'):
            q['audio_url'] = f"{AUDIO_BASE}part2/{qid}.mp3"
            count_audio += 1
        
        # Xử lý Graphic cho Part 3 & 4 (Nếu câu hỏi có tag graphic)
        elif part in [3, 4] and "graphic" in q.get('tags', []):
            q['image_url'] = f"{IMAGE_BASE}part{part}/{qid}.png"
            count_img += 1

    with open(QUESTIONS_FILE, 'w', encoding='utf-8') as f:
        json.dump(data, f, ensure_ascii=False, indent=2)
    
    print(f"--- KẾT QUẢ CẬP NHẬT QUESTIONS ---")
    print(f" Đã gán {count_img} link ảnh.")
    print(f" Đã gán {count_audio} link audio.")

def update_groups():
    if not os.path.exists(GROUPS_FILE):
        print(f"Lỗi: Không tìm thấy file {GROUPS_FILE}")
        return

    with open(GROUPS_FILE, 'r', encoding='utf-8') as f:
        groups = json.load(f)

    count_group_audio = 0
    count_group_img = 0

    for g in groups:
        gid = g.get('id', '')
        part = g.get('part', 0)

        # Đảm bảo trường passages luôn tồn tại (Dạng mảng)
        if 'passages' not in g or g['passages'] is None:
            g['passages'] = []
            # Nếu có passage_text cũ thì migrate sang passages
            if g.get('passage_text'):
                g['passages'].append({"type": "text", "content": g['passage_text']})
            # Nếu có image_url cũ thì migrate sang passages
            elif g.get('image_url'):
                g['passages'].append({"type": "image", "content": g['image_url']})

        # Xử lý Audio cho Part 3 & 4
        if part in [3, 4]:
            g['audio_url'] = f"{AUDIO_BASE}part{part}/{gid}.mp3"
            count_group_audio += 1
        
        # Xử lý Ảnh cho Part 7 (Tự động hóa)
        elif part == 7:
            # Gợi ý: Nếu gid là group_part7_12, 13, 15 thì mặc định dùng ảnh
            image_groups = ['group_part7_02', 'group_part7_03', 'group_part7_12', 'group_part7_13', 'group_part7_15']
            if gid in image_groups:
                # Nếu là nhóm Double/Triple (12, 14, 15) thì dự đoán có nhiều ảnh
                if gid in ['group_part7_12', 'group_part7_14', 'group_part7_15']:
                    # Reset lại passages để gán nhiều ảnh (Ví dụ 2 ảnh)
                    g['passages'] = [
                        {"type": "image", "content": f"{IMAGE_BASE}part7/{gid}_1.png"},
                        {"type": "image", "content": f"{IMAGE_BASE}part7/{gid}_2.png"}
                    ]
                    count_group_img += 2
                else:
                    g['image_url'] = f"{IMAGE_BASE}part7/{gid}.png"
                    g['passages'] = [{"type": "image", "content": g['image_url']}]
                    count_group_img += 1

    with open(GROUPS_FILE, 'w', encoding='utf-8') as f:
        json.dump(groups, f, ensure_ascii=False, indent=2)
    
    print(f"\n--- KẾT QUẢ CẬP NHẬT GROUPS ---")
    print(f" Đã gán {count_group_audio} link audio cho các nhóm Part 3 & 4.")
    print(f" Đã gán {count_group_img} link ảnh cho các nhóm Part 7.")

if __name__ == "__main__":
    update_questions()
    update_groups()
    print("\n TẤT CẢ ĐÃ XONG! Chúc mừng bạn!")
