// wwwroot/js/health-check.js (iframe 逾時偵測版)

window.iframeWatchers = window.iframeWatchers || {};

// 監視一個 iframe 的載入狀態
window.watchIframeLoad = (dotNetHelper, iframeId, timeoutMs) => {
    stopIframeWatch(iframeId); // 先停止任何可能已存在的舊監視

    const iframe = document.getElementById(iframeId);
    if (!iframe) {
        console.error(`[IframeWatch] Iframe with id '${iframeId}' not found.`);
        return;
    }

    let isLoaded = false;

    // 設定一個計時器，如果在指定時間內 iframe 沒有載入成功，就回報「離線」
    const timeoutHandle = setTimeout(() => {
        if (!isLoaded) {
            dotNetHelper.invokeMethodAsync('ReportStatus', false);
        }
    }, timeoutMs);

    // 設定 iframe 的 onload 事件
    iframe.onload = () => {
        isLoaded = true;
        clearTimeout(timeoutHandle); // 清除逾時的計時器
        dotNetHelper.invokeMethodAsync('ReportStatus', true); // 載入成功，回報「在線」
    };

    window.iframeWatchers[iframeId] = { timeoutHandle };
};

// 停止監視
window.stopIframeWatch = (iframeId) => {
    const watcher = window.iframeWatchers[iframeId];
    if (watcher) {
        clearTimeout(watcher.timeoutHandle);
        delete window.iframeWatchers[iframeId];
    }
};