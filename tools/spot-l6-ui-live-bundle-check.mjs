/**
 * Verifica stringhe Step 5 nel bundle Firebase live (entry + lazy chunks).
 */
const FRONTEND = (process.env.FRONTEND_URL || 'https://eugenio-dashboard-2a.web.app').replace(/\/$/, '');
const NEED = ['L5 perse per generare crediti', 'Crediti L6 generati', 'Salva crediti L6'];
const OLD = ['Soglia L6 per bot', 'Salva soglia'];

async function fetchText(url) {
  const r = await fetch(url);
  if (!r.ok) throw new Error(`${url} -> ${r.status}`);
  return r.text();
}

async function collectAssetPaths(seedText) {
  const paths = new Set();
  const re = /\/assets\/[A-Za-z0-9_.-]+\.js/g;
  for (const m of seedText.matchAll(re)) paths.add(m[0]);
  const queue = [...paths];
  while (queue.length) {
    const path = queue.pop();
    const text = await fetchText(`${FRONTEND}${path}`);
    for (const m of text.matchAll(re)) {
      if (!paths.has(m[0])) {
        paths.add(m[0]);
        queue.push(m[0]);
      }
    }
  }
  return paths;
}

async function main() {
  const html = await fetchText(`${FRONTEND}/`);
  const entry = [...html.matchAll(/\/assets\/index-[A-Za-z0-9_.-]+\.js/g)].map((m) => m[0]);
  console.log(`entry scripts: ${entry.join(', ') || 'none'}`);

  const assets = await collectAssetPaths(html + '\n' + (await Promise.all(entry.map((p) => fetchText(`${FRONTEND}${p}`)))).join('\n'));
  console.log(`assets discovered: ${assets.size}`);

  const foundNeed = Object.fromEntries(NEED.map((s) => [s, false]));
  const foundOld = Object.fromEntries(OLD.map((s) => [s, false]));

  for (const asset of [...assets].sort()) {
    const text = await fetchText(`${FRONTEND}${asset}`);
    for (const s of NEED) if (text.includes(s)) foundNeed[s] = true;
    for (const s of OLD) if (text.includes(s)) foundOld[s] = true;
    const hits = [...NEED, ...OLD].filter((s) => text.includes(s));
    if (hits.length) console.log(`${asset} (${text.length}b): ${hits.join(' | ')}`);
  }

  console.log('\n--- NEW strings ---');
  for (const [k, v] of Object.entries(foundNeed)) console.log(`${v ? 'FOUND' : 'MISS'} ${k}`);
  console.log('\n--- OLD strings ---');
  for (const [k, v] of Object.entries(foundOld)) console.log(`${v ? 'FOUND' : 'MISS'} ${k}`);

  const pass = NEED.every((s) => foundNeed[s]) && OLD.every((s) => !foundOld[s]);
  console.log(`\nVERDICT ${pass ? 'PASS' : 'FAIL'} — live bundle Step 5 UI`);
  process.exit(pass ? 0 : 1);
}

main().catch((err) => {
  console.error(err);
  process.exit(1);
});
