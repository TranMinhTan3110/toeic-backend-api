const fs = require('fs');
const path = require('path');

const filePath = path.join(__dirname, '..', 'SeedData', 'questions.json');

console.log('Đang đọc file questions.json...');
try {
  const data = fs.readFileSync(filePath, 'utf8');
  const questions = JSON.parse(data);

  let updatedCount = 0;
  questions.forEach(q => {
    if (q.exam_id === 'ets_2024_test_1') {
      if (q.is_for_practice !== false) {
        q.is_for_practice = false;
        updatedCount++;
      }
    }
  });

  console.log(`Đã cập nhật ${updatedCount} câu hỏi thành is_for_practice = false.`);

  if (updatedCount > 0) {
    console.log('Đang ghi đè dữ liệu mới vào questions.json...');
    fs.writeFileSync(filePath, JSON.stringify(questions, null, 2), 'utf8');
    console.log('Hoàn tất ghi file!');
  } else {
    console.log('Không có câu hỏi nào cần cập nhật.');
  }
} catch (err) {
  console.error('Đã xảy ra lỗi:', err);
}
