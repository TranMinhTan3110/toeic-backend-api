const fs = require('fs');
const path = require('path');

// Đường dẫn các file dữ liệu
const groupFilePath = path.join(__dirname, '..', 'SeedData', 'question_group_listening.json');
const questionFilePath = path.join(__dirname, '..', 'SeedData', 'practiceListening.json');

console.log('--- KHỞI CHẠY THAY ĐỔI DỮ LIỆU PART 3 LUYỆN TẬP ---');

// 1. Định nghĩa các base URL cho Cloudinary
const AUDIO_BASE = 'https://res.cloudinary.com/dlfc5qwhj/video/upload/q_auto/f_auto/v1779184538/LR/Practice/Part%203/Audio/';
const IMAGE_BASE = 'https://res.cloudinary.com/dlfc5qwhj/image/upload/q_auto/f_auto/v1779184179/LR/Practice/Part%203/Image/';

// Danh sách 30 nhóm có hình ảnh (dựa trên phân tích từ ảnh chụp thư mục luyen_tap_toiec)
const groupsWithImages = new Set([
    11, 12, 13, 
    24, 25, 26, 
    37, 38, 39, 
    50, 51, 52, 
    63, 64, 65, 
    76, 77, 78, 
    89, 90, 91, 
    102, 103, 104, 
    115, 116, 117, 
    128, 129, 130
]);

// ==========================================
// BƯỚC 1: Xử lý file question_group_listening.json
// ==========================================
if (!fs.existsSync(groupFilePath)) {
    console.error('Lỗi: Không tìm thấy file question_group_listening.json!');
    process.exit(1);
}

let groups = JSON.parse(fs.readFileSync(groupFilePath, 'utf8'));
let updatedGroupsCount = 0;
let updatedAudioCount = 0;
let updatedImageCount = 0;

groups.forEach(group => {
    const originalId = group.id;
    
    // Chỉ xử lý nếu ID chưa có tiền tố p_
    if (originalId && originalId.startsWith('group_part3_')) {
        // Lấy số thứ tự nhóm từ ID (ví dụ: "group_part3_001" -> 1)
        const match = originalId.match(/group_part3_(\d+)/);
        if (match) {
            const num = parseInt(match[1], 10);
            const paddedNum = String(num).padStart(3, '0');
            
            // 1. Thêm tiền tố p_ cho ID nhóm
            group.id = `p_${originalId}`;
            
            // 2. Gán audio URL chuẩn (pad 3 chữ số)
            group.audio_url = `${AUDIO_BASE}group_part3_${paddedNum}.mp3`;
            updatedAudioCount++;
            
            // 3. Gán image URL nếu nhóm này thuộc danh sách có ảnh (không pad chữ số)
            if (groupsWithImages.has(num)) {
                group.image_url = `${IMAGE_BASE}group_part3_${num}.png`;
                updatedImageCount++;
            } else {
                group.image_url = null;
            }
            
            // 4. Thêm tiền tố p_ cho toàn bộ ID câu hỏi thuộc nhóm
            if (Array.isArray(group.question_ids)) {
                group.question_ids = group.question_ids.map(qid => {
                    if (qid.startsWith('q_part3_')) {
                        return `p_${qid}`;
                    }
                    return qid;
                });
            }
            
            // 5. Thêm cờ để phân biệt practice / exam
            group.is_for_practice = true;
            group.is_for_exam = false;
            
            updatedGroupsCount++;
        }
    } else if (originalId && originalId.startsWith('p_group_part3_')) {
        // Trường hợp ID đã được cập nhật trước đó, đảm bảo có đủ trường
        const pureId = originalId.substring(2); // Lấy "group_part3_xxx"
        const match = pureId.match(/group_part3_(\d+)/);
        if (match) {
            const num = parseInt(match[1], 10);
            const paddedNum = String(num).padStart(3, '0');
            
            group.audio_url = `${AUDIO_BASE}group_part3_${paddedNum}.mp3`;
            
            if (groupsWithImages.has(num)) {
                group.image_url = `${IMAGE_BASE}group_part3_${num}.png`;
            } else {
                group.image_url = null;
            }
            
            if (Array.isArray(group.question_ids)) {
                group.question_ids = group.question_ids.map(qid => {
                    if (qid.startsWith('q_part3_')) {
                        return `p_${qid}`;
                    }
                    return qid;
                });
            }
            
            group.is_for_practice = true;
            group.is_for_exam = false;
        }
    }
});

fs.writeFileSync(groupFilePath, JSON.stringify(groups, null, 2), 'utf8');
console.log(`✔ Cập nhật thành công ${updatedGroupsCount} nhóm trong question_group_listening.json`);
console.log(`✔ Đã gán ${updatedAudioCount} link Audio và ${updatedImageCount} link Image cho các nhóm phù hợp.`);


// ==========================================
// BƯỚC 2: Xử lý file practiceListening.json
// ==========================================
if (!fs.existsSync(questionFilePath)) {
    console.error('Lỗi: Không tìm thấy file practiceListening.json!');
    process.exit(1);
}

let questions = JSON.parse(fs.readFileSync(questionFilePath, 'utf8'));
let updatedQuestionsCount = 0;
let updatedGroupRefsCount = 0;

questions.forEach(q => {
    if (q.part === 3) {
        // 1. Thêm tiền tố p_ vào ID câu hỏi nếu chưa có
        if (q.id && q.id.startsWith('q_part3_')) {
            q.id = `p_${q.id}`;
            updatedQuestionsCount++;
        }
        
        // 2. Thêm tiền tố p_ vào group_id tham chiếu nếu chưa có
        if (q.group_id && q.group_id.startsWith('group_part3_')) {
            q.group_id = `p_${q.group_id}`;
            updatedGroupRefsCount++;
        }
        
        // 3. Đảm bảo có đủ cờ phân biệt practice / exam
        q.is_for_practice = true;
        q.is_for_exam = false;
    }
});

fs.writeFileSync(questionFilePath, JSON.stringify(questions, null, 2), 'utf8');
console.log(`✔ Cập nhật thành công ${updatedQuestionsCount} câu hỏi Part 3 trong practiceListening.json`);
console.log(`✔ Cập nhật thành công ${updatedGroupRefsCount} tham chiếu group_id sang định dạng 'p_group_part3_xxx'.`);
console.log('--- HOÀN TẤT THAY ĐỔI DỮ LIỆU ---');
