const fs = require('fs');
const path = require('path');

function updatePart(partNum) {
    const questionsPath = path.join(__dirname, 'SeedData', `practice_part${partNum}.json`);
    const groupsPath = path.join(__dirname, 'SeedData', `practice_part${partNum}_group.json`);

    console.log(`\n--- Processing Part ${partNum} ---`);

    // 1. Update Questions
    if (fs.existsSync(questionsPath)) {
        const content = fs.readFileSync(questionsPath, 'utf8').trim();
        if (content.length > 0) {
            try {
                let questions = JSON.parse(content);
                let qUpdated = 0;
                let gRefUpdated = 0;

                questions.forEach(q => {
                    // Update question ID
                    if (q.id && q.id.startsWith(`q_part${partNum}_`) && !q.id.startsWith(`p_`)) {
                        q.id = `p_${q.id}`;
                        qUpdated++;
                    }
                    // Update group_id reference
                    if (q.group_id && q.group_id.startsWith(`group_part${partNum}_`) && !q.group_id.startsWith(`p_`)) {
                        q.group_id = `p_${q.group_id}`;
                        gRefUpdated++;
                    }
                });

                fs.writeFileSync(questionsPath, JSON.stringify(questions, null, 4), 'utf8');
                console.log(`✔ Successfully updated ${questionsPath}:`);
                console.log(`  - Question IDs updated: ${qUpdated}`);
                console.log(`  - Group ID references updated: ${gRefUpdated}`);
            } catch (err) {
                console.error(` Error parsing ${questionsPath}:`, err.message);
            }
        } else {
            console.log(` ${questionsPath} is empty. Skipping.`);
        }
    } else {
        console.log(` ${questionsPath} does not exist. Skipping.`);
    }

    // 2. Update Question Groups
    if (fs.existsSync(groupsPath)) {
        const content = fs.readFileSync(groupsPath, 'utf8').trim();
        if (content.length > 0) {
            try {
                let groups = JSON.parse(content);
                let gUpdated = 0;
                let qRefUpdated = 0;

                groups.forEach(g => {
                    // Update group ID
                    if (g.id && g.id.startsWith(`group_part${partNum}_`) && !g.id.startsWith(`p_`)) {
                        g.id = `p_${g.id}`;
                        gUpdated++;
                    }
                    // Update question_ids array
                    if (g.question_ids && Array.isArray(g.question_ids)) {
                        g.question_ids = g.question_ids.map(qid => {
                            if (qid && qid.startsWith(`q_part${partNum}_`) && !qid.startsWith(`p_`)) {
                                qRefUpdated++;
                                return `p_${qid}`;
                            }
                            return qid;
                        });
                    }
                });

                fs.writeFileSync(groupsPath, JSON.stringify(groups, null, 4), 'utf8');
                console.log(`✔ Successfully updated ${groupsPath}:`);
                console.log(`  - Group IDs updated: ${gUpdated}`);
                console.log(`  - Question references updated: ${qRefUpdated}`);
            } catch (err) {
                console.error(` Error parsing ${groupsPath}:`, err.message);
            }
        } else {
            console.log(` ${groupsPath} is empty. Skipping.`);
        }
    } else {
        console.log(`ℹ ${groupsPath} does not exist. Skipping.`);
    }
}

// Update both Part 6 and Part 7
updatePart(6);
updatePart(7);
console.log('\n--- ALL DONE ---');
