const fs = require('fs');
const path = require('path');

const filePath = path.join(__dirname, '..', 'SeedData', 'questions.json');
console.log('Reading questions.json from:', filePath);

if (!fs.existsSync(filePath)) {
    console.error('Error: File not found!');
    process.exit(1);
}

let fileContent = fs.readFileSync(filePath, 'utf8');

// Replace all instances of "is_for_practice": true with "is_for_practice": false
// globally within the file.
let updatedContent = fileContent.replace(/"is_for_practice"\s*:\s*true/g, '"is_for_practice": false');

fs.writeFileSync(filePath, updatedContent, 'utf8');
console.log('Successfully updated "is_for_practice": true to false in questions.json!');
