const enabledEl = document.getElementById("enabled");
const hideScrollbarsEl = document.getElementById("hideScrollbars");

(async () => {
    const { enabled, hideScrollbars } = await chrome.storage.sync.get({
        enabled: true,
        hideScrollbars: true,
    });
    enabledEl.checked = Boolean(enabled);
    hideScrollbarsEl.checked = Boolean(hideScrollbars);
})();

enabledEl.addEventListener("change", async () => {
    await chrome.storage.sync.set({ enabled: enabledEl.checked });
});

hideScrollbarsEl.addEventListener("change", async () => {
    await chrome.storage.sync.set({ hideScrollbars: hideScrollbarsEl.checked });
});
