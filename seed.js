// Seed script for browser console (F12)
// Usage: Open poll page -> F12 -> Console -> paste this code
// Change POLL_ID and NUM_USERS below

const POLL_ID = new URLSearchParams(window.location.search).get('id') || 'YOUR_POLL_ID_HERE';
const NUM_USERS = 15;

async function seedVotes() {
    console.log(`Generating ${NUM_USERS} random votes for poll ${POLL_ID}...`);
    
    // 1. Get poll options
    const poll = await fetch(`http://localhost:5000/api/polls/${POLL_ID}`, {
        headers: { 'Authorization': `Bearer ${localStorage.getItem('accessToken')}` }
    }).then(r => r.json());
    
    const options = poll.options;
    console.log(`Options: ${options.map((o,i) => `[${i}] ${o.text}`).join(', ')}`);
    
    const votes = new Array(options.length).fill(0);
    
    for (let i = 1; i <= NUM_USERS; i++) {
        const email = `seeduser${i}@demo.local`;
        
        // Register
        const reg = await fetch('http://localhost:5000/api/auth/register', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, password: 'SeedPass123!', fullName: `Seed User ${i}` })
        });
        const data = await reg.json();
        
        if (!data.accessToken) {
            console.log(`[${i}/${NUM_USERS}] Failed to register ${email}`);
            continue;
        }
        
        // Random option
        const randIdx = Math.floor(Math.random() * options.length);
        const optionId = options[randIdx].id;
        
        // Vote
        await fetch(`http://localhost:5000/api/polls/${POLL_ID}/vote`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${data.accessToken}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ optionIds: [optionId] })
        });
        
        votes[randIdx]++;
        console.log(`[${i}/${NUM_USERS}] ${email} -> voted for [${options[randIdx].text}]`);
    }
    
    console.log('\n--- Results ---');
    let total = 0;
    options.forEach((opt, i) => {
        console.log(`  ${opt.text}: ${votes[i]} votes`);
        total += votes[i];
    });
    console.log(`\nTotal: ${total} votes`);
    console.log('Refresh page to see updated results!');
}

seedVotes();
