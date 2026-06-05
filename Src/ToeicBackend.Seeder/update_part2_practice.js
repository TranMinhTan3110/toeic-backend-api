const fs = require('fs');
const path = require('path');

const filePath = path.join(__dirname, 'SeedData', 'practiceListening.json');
console.log('Reading practiceListening.json from:', filePath);

if (!fs.existsSync(filePath)) {
    console.error('Error: File not found!');
    process.exit(1);
}

let data = JSON.parse(fs.readFileSync(filePath, 'utf8'));

const AUDIO_BASE = 'https://res.cloudinary.com/dlfc5qwhj/video/upload/q_auto/f_auto/v1779182400/LR/Practice/Part%202/Audio/';

let count_id = 0;
let count_audio = 0;

data.forEach(q => {
    let qid = q.id || '';
    if (qid.startsWith('q_part2_')) {
        let pure_id = qid;
        let new_id = `p_${qid}`;
        q.id = new_id;
        q.audio_url = `${AUDIO_BASE}${pure_id}.mp3`;
        count_id++;
        count_audio++;
    } else if (qid.startsWith('p_q_part2_')) {
        let pure_id = qid.substring(2); 
        q.audio_url = `${AUDIO_BASE}${pure_id}.mp3`;
        count_audio++;
    }
});

fs.writeFileSync(filePath, JSON.stringify(data, null, 4), 'utf8');
console.log(`--- SUCCESS ---`);
console.log(`✔ Changed ${count_id} IDs to start with 'p_q_part2_'.`);
console.log(`✔ Set/updated ${count_audio} audio URLs.`);
