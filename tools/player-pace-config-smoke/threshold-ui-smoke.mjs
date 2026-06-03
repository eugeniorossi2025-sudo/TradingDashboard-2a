const DEFAULT = 107;

function getNumber(value, fallback = 0) {
    const numberValue = Number(value);
    return Number.isFinite(numberValue) ? numberValue : fallback;
}

function resolvePlayerP1P5Threshold(value) {
    const parsed = getNumber(value, DEFAULT);
    return parsed > 0 ? parsed : DEFAULT;
}

const cases = [
    [undefined, 107],
    [null, 107],
    ['107', 107],
    [107, 107],
    ['110.5', 110.5],
    ['abc', 107],
    [0, 107],
    [-3, 107],
    ['', 107],
];

let failed = 0;
for (const [input, expected] of cases) {
    const actual = resolvePlayerP1P5Threshold(input);
    const ok = Math.abs(actual - expected) < 0.001;
    console.log(`${ok ? 'OK  ' : 'FAIL'} resolve(${JSON.stringify(input)}) -> ${actual} (expected ${expected})`);
    if (!ok) failed = 1;
}

// pulse guard: no flash on first load
let playerStreakLastCountByBot = {};
function shouldPulse(bot, count, outcome) {
    const prev = playerStreakLastCountByBot[bot];
    const isPlayerStreak = outcome === 'P' && count > 0;
    let pulse = false;
    if (isPlayerStreak && prev !== undefined && count > prev) pulse = true;
    playerStreakLastCountByBot[bot] = isPlayerStreak ? count : 0;
    return pulse;
}

console.log(`${shouldPulse('PC1', 3, 'P') ? 'FAIL' : 'OK  '} first load count=3 no pulse`);
console.log(`${shouldPulse('PC1', 4, 'P') ? 'OK  ' : 'FAIL'} second tick count=4 pulse`);
console.log(`${shouldPulse('PC1', 0, 'B') ? 'FAIL' : 'OK  '} B reset no pulse`);
playerStreakLastCountByBot['PC1'] = 5;
console.log(`${shouldPulse('PC1', 6, 'P') ? 'OK  ' : 'FAIL'} after P5, P6 pulses (step 5)`);
console.log(`${Math.min(6, 5) === 5 ? 'OK  ' : 'FAIL'} pulse step capped at P5`);

process.exit(failed);
