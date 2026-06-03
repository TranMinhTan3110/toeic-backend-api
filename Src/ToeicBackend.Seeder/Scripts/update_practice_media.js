const fs = require('fs');
const path = require('path');

const filePath = path.join(__dirname, '..', 'SeedData', 'practiceListening.json');
console.log('Reading practiceListening.json from:', filePath);

if (!fs.existsSync(filePath)) {
    console.error('Error: File not found!');
    process.exit(1);
}

let data = JSON.parse(fs.readFileSync(filePath, 'utf8'));

const IMAGE_BASE = 'https://res.cloudinary.com/dlfc5qwhj/image/upload/q_auto,f_auto/v1779181951/LR/Practice/Part1/Image/';
const AUDIO_BASE = 'https://res.cloudinary.com/dlfc5qwhj/video/upload/q_auto,f_auto/v1779182004/LR/Practice/Part1/Audio/';

let count_img = 0;
let count_audio = 0;

data.forEach(q => {
    let qid = q.id || '';
    if (qid.startsWith('p_q_part1_')) {
        let pure_id = qid.substring(2); // Remove the 'p_' prefix
        q.image_url = `${IMAGE_BASE}${pure_id}.png`;
        q.audio_url = `${AUDIO_BASE}${pure_id}.mp3`;
        count_img++;
        count_audio++;
    }
});

fs.writeFileSync(filePath, JSON.stringify(data, null, 4), 'utf8');
console.log(`Successfully updated practice questions! Image URLs: ${count_img}, Audio URLs: ${count_audio}`);
