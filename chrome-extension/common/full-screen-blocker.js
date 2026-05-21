(() => {
    const orig = {
        requestFullscreen: Element.prototype.requestFullscreen,
        webkitRequestFullscreen: Element.prototype.webkitRequestFullscreen,
    };

    let enabled = false;

    function setEnabled(v) {
        enabled = !!v;

        if (enabled && document.fullscreenElement) {
            document.exitFullscreen().catch(() => { });
        }
    }

    function blockedRequestFullscreen(...args) {
        if (enabled) {
            return Promise.reject(new DOMException("Fullscreen blocked by extension", "SecurityError"));
        }
        return orig.requestFullscreen.apply(this, args);
    }

    if (orig.requestFullscreen) {
        Element.prototype.requestFullscreen = blockedRequestFullscreen;
    }

    if (orig.webkitRequestFullscreen) {
        Element.prototype.webkitRequestFullscreen = function (...args) {
            if (enabled) {
                return Promise.reject(new DOMException("Fullscreen blocked by extension", "SecurityError"));
            }
            return orig.webkitRequestFullscreen.apply(this, args);
        };
    }

    document.addEventListener("fullscreenchange", () => {
        if (enabled && document.fullscreenElement) {
            document.exitFullscreen().catch(() => { });
        }
    });

    window.addEventListener("message", (event) => {
        if (event.source !== window) return;
        const msg = event.data;
        if (msg && msg.type === "EXT_FS_BLOCK") {
            setEnabled(msg.enabled);
        }
    });
})();
