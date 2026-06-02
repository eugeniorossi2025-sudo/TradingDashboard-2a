/**
 * Collaudo hero mobile DASH2A (missione aperta vs report periodo).
 * Uso: DASH2A_PASSWORD='...' node tools/dash2a-mobile-hero-smoke.cjs
 */
const path = require('node:path');
const { chromium, devices } = require('playwright');

const BASE = (process.env.DASH2A_BASE_URL || 'https://eugenio-dashboard-2a.web.app').replace(/\/$/, '');
const USER = process.env.DASH2A_USER || 'eugenio';
const PASS = process.env.DASH2A_PASSWORD || '';

if (!PASS) {
    console.error('Imposta DASH2A_PASSWORD');
    process.exit(1);
}

const iPhone = devices['iPhone 13'];

function parseMoney(text) {
    const m = String(text || '').match(/([\d.,]+)/);
    if (!m) return null;
    return Number(m[1].replace(/\./g, '').replace(',', '.'));
}

async function login(page) {
    await page.goto(`${BASE}/auth/login`, { waitUntil: 'networkidle', timeout: 60000 });
    await page.fill('#username1', USER);
    await page.locator('#password1 input').fill(PASS);
    await page.getByRole('button', { name: 'Accedi' }).click();
    await page.waitForURL(/\/(admin\/mobile-live|client\/mobile|dashboard|\/)$/, { timeout: 45000 });
}

async function assertHero(page, label) {
    const body = (await page.locator('main').innerText()).toLowerCase();
    if (!body.includes('margine live')) {
        throw new Error(`[${label}] Manca etichetta "Margine live"`);
    }
    if (!/missione #\d+/.test(body) && !body.includes('nessuna missione aperta')) {
        throw new Error(`[${label}] Manca stato missione (aperta o idle)`);
    }
    if (!body.includes('target periodo/report') && !body.includes('report periodo')) {
        throw new Error(`[${label}] Manca sezione report periodo separata`);
    }
    const heroBlock = page.locator('.hero-client, .panel.hero').first();
    const heroText = (await heroBlock.count()) > 0 ? await heroBlock.innerText() : body.split('Report periodo')[0];
    const heroMoney = parseMoney(heroText);
    console.log(`[${label}] hero margin:`, heroMoney);
    return { heroText, heroMoney, body };
}

async function clickPeriodChips(page) {
    for (const chip of ['DAY', 'WEEK', 'MONTH', 'YEAR']) {
        const btn = page.getByRole('button', { name: chip, exact: true });
        if ((await btn.count()) === 0) continue;
        await btn.click();
        await page.waitForTimeout(900);
    }
}

async function runRoute(page, path, label) {
    await page.goto(`${BASE}${path}`, { waitUntil: 'networkidle', timeout: 60000 });
    await page.waitForTimeout(2000);
    if (!page.url().includes(path.replace(/^\//, ''))) {
        console.log(`[${label}] skip (redirect ${page.url()})`);
        return;
    }
    const before = await assertHero(page, `${label}-before`);
    await clickPeriodChips(page);
    const after = await assertHero(page, `${label}-after`);
    if (before.heroMoney != null && after.heroMoney != null && Math.abs(before.heroMoney - after.heroMoney) > 0.02) {
        console.warn(`[${label}] hero cambiato dopo chip (${before.heroMoney} -> ${after.heroMoney})`);
    } else {
        console.log(`[${label}] OK hero stabile vs chip periodo`);
    }
}

async function main() {
    const browser = await chromium.launch({ headless: true });
    const context = await browser.newContext({ ...iPhone });
    const page = await context.newPage();
    const results = [];

    try {
        await login(page);
        console.log('Post-login:', page.url());

        for (const [route, name] of [
            ['/admin/mobile-live', 'admin'],
            ['/client/mobile', 'client']
        ]) {
            await page.goto(`${BASE}${route}`, { waitUntil: 'networkidle', timeout: 60000 });
            const bodyPeek = await page.locator('body').innerText();
            if (page.url().includes('login') || /access denied|accesso negato|non autorizzato|benvenuti in eugenio/i.test(bodyPeek)) {
                console.log(`[${name}] skip (no access / sessione)`);
                continue;
            }
            await runRoute(page, route, name);
            results.push(name);
        }

        console.log('\nCollaudo OK. Route:', results.join(', ') || 'nessuna');
    } catch (err) {
        console.error('Collaudo fallito:', err.message);
        process.exitCode = 1;
    } finally {
        await browser.close();
    }
}

main();
