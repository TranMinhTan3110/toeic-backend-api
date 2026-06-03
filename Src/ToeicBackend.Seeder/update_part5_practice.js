const fs = require('fs');
const path = require('path');

const filePath = path.join(__dirname, 'SeedData', 'practice_part5.json');
console.log('Reading practice_part5.json from:', filePath);

if (!fs.existsSync(filePath)) {
    console.error('Error: File not found!');
    process.exit(1);
}

let data = JSON.parse(fs.readFileSync(filePath, 'utf8'));

let count_id = 0;

data.forEach(q => {
    let qid = q.id || '';
    if (qid.startsWith('q_part5_')) {
        let new_id = `p_${qid}`;
        q.id = new_id;
        count_id++;
    }
});

fs.writeFileSync(filePath, JSON.stringify(data, null, 4), 'utf8');
console.log(`--- SUCCESS ---`);
console.log(` Changed ${count_id} IDs to start with 'p_q_part5_'.`);
