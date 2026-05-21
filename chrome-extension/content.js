let siteEngine = null;
let siteConfig = null;
let prevState = null;
let active = false;

async function isEnabled() {
    const data = await chrome.storage.sync.get({ enabled: true });
    return Boolean(data.enabled);
}

function getSiteConfigPath() {
    const parts = location.hostname
        .toLowerCase()
        .replace(/^www\./, "")
        .split(".");

    let siteName = parts.length >= 2
        ? parts[parts.length - 2]
        : parts[0];

    return `sites-config/${siteName}.config.js`;
}

async function loadMainEngine() {
    const path = chrome.runtime.getURL("common/site-engine.js");
    siteEngine = await import(path);
}

async function loadSiteConfig() {
    const path = chrome.runtime.getURL(getSiteConfigPath());
    const mod = await import(path);
    siteConfig = mod.default;
}

function syncFullscreenBlocker(enabled) {
    window.postMessage({ type: "EXT_FS_BLOCK", enabled: !!enabled }, "*");
}

async function applyIfEnabled() {
    active = await isEnabled();
    syncFullscreenBlocker(active);

    if (!active || !siteConfig) return;

    if (!prevState) {
        prevState = siteEngine.applyLayout(siteConfig);
    } else {
        siteEngine.applyLayout(siteConfig, prevState);
    }
}

async function restoreIfDisabled() {
    syncFullscreenBlocker(false);

    if (siteConfig && prevState) {
        siteEngine.restoreLayout(siteConfig, prevState);
        prevState = null;
    }
}

async function restoreIfDisabled() {
    syncFullscreenBlocker(false);

    if (siteConfig && prevState) {
        siteEngine.restoreLayout(prevState);
        prevState = null;
    }
}

(async () => {
    await loadMainEngine();
    await loadSiteConfig();
    await applyIfEnabled();

    const obs = new MutationObserver(async () => {
        if (active && siteConfig) {
            if (prevState) {
                siteEngine.applyLayout(siteConfig, prevState);
            }
        }
    });

    obs.observe(document.documentElement, {
        childList: true,
        subtree: true
    });

    chrome.storage.onChanged.addListener((changes, area) => {
        if (area !== "sync" || !changes.enabled) return;
        const enabledNow = Boolean(changes.enabled.newValue);

        if (!enabledNow) restoreIfDisabled();
        else applyIfEnabled();
    });
})();