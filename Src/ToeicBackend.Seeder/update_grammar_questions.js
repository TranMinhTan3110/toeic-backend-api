const fs = require('fs');
const path = require('path');

const filePath = path.join(__dirname, 'SeedData', 'questions.json');

if (!fs.existsSync(filePath)) {
  console.error(`[ERROR] File not found: ${filePath}`);
  process.exit(1);
}

const data = JSON.parse(fs.readFileSync(filePath, 'utf8'));
console.log(`Loaded ${data.length} questions from ${filePath}`);

let gtopic_001_count = 0; // Simple Past
let gtopic_002_count = 0; // Simple Present
let gtopic_003_count = 0; // Word Forms
let gtopic_004_count = 0; // Prepositions

data.forEach(q => {
  if (q.part === 5) {
    const tags = (q.tags || []).map(t => t.toLowerCase());
    const text = (q.question_text || '').toLowerCase();
    const explanation = JSON.stringify(q.explanation || '').toLowerCase();

    // 1. Simple Past
    if (tags.includes('past-tense') || tags.includes('past') || tags.includes('simple-past') || 
        explanation.includes('past simple') || explanation.includes('quá khứ đơn') || explanation.includes('thì quá khứ')) {
      q.grammar_topic_id = 'gtopic_001';
      gtopic_001_count++;
    }
    // 2. Simple Present
    else if (tags.includes('present-tense') || tags.includes('present') || tags.includes('simple-present') ||
             explanation.includes('present simple') || explanation.includes('hiện tại đơn') || explanation.includes('thì hiện tại')) {
      q.grammar_topic_id = 'gtopic_002';
      gtopic_002_count++;
    }
    // 3. Word Forms
    else if (tags.includes('word-form') || tags.includes('wordform') || tags.includes('noun') || 
             tags.includes('adjective') || tags.includes('adverb') || tags.includes('pronoun') || 
             tags.includes('conjunction') || explanation.includes('từ loại') || explanation.includes('danh từ') || 
             explanation.includes('tính từ') || explanation.includes('trạng từ')) {
      q.grammar_topic_id = 'gtopic_003';
      gtopic_003_count++;
    }
    // 4. Prepositions
    else if (tags.includes('preposition') || explanation.includes('giới từ') || explanation.includes('preposition')) {
      q.grammar_topic_id = 'gtopic_004';
      gtopic_004_count++;
    }
  }
});

console.log(`Mapping results:`);
console.log(`- Simple Past (gtopic_001): ${gtopic_001_count} questions`);
console.log(`- Simple Present (gtopic_002): ${gtopic_002_count} questions`);
console.log(`- Word Forms (gtopic_003): ${gtopic_003_count} questions`);
console.log(`- Prepositions (gtopic_004): ${gtopic_004_count} questions`);

fs.writeFileSync(filePath, JSON.stringify(data, null, 2), 'utf8');
console.log(`Successfully updated and saved ${filePath}`);
