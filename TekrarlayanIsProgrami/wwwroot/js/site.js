window.setInputType = (element, type) => {
    if (element) element.type = type;
};

(function () {
    let timeout;
    function applyMobileCardLabels() {
        document.querySelectorAll('.mobile-card-grid').forEach(function (grid) {
            var headers = [];
            grid.querySelectorAll('.rz-datatable-thead th').forEach(function (th) {
                headers.push(th.textContent.trim());
            });
            if (headers.length === 0) return;
            grid.querySelectorAll('.rz-datatable-data tr').forEach(function (tr) {
                if (tr.dataset.mcLabeled) return;
                var tds = tr.querySelectorAll('td');
                tds.forEach(function (td, i) {
                    if (i < headers.length && headers[i]) {
                        td.setAttribute('data-label', headers[i]);
                    }
                });
                tr.dataset.mcLabeled = '1';
            });
        });
    }
    var obs = new MutationObserver(function () {
        clearTimeout(timeout);
        timeout = setTimeout(applyMobileCardLabels, 150);
    });
    obs.observe(document.body, { childList: true, subtree: true });
    document.addEventListener('DOMContentLoaded', applyMobileCardLabels);
})();

//document.addEventListener("wheel", function (e) {
//    const tabs = e.target.closest(".my-tabs");
//    if (!tabs) return;

//    if (e.deltaY !== 0) {
//        e.preventDefault();
//        tabs.scrollLeft += e.deltaY;
//    }
//}, { passive: false });


//document.addEventListener("wheel", function (e) {
//    // RadzenDataGrid içinde mi kontrol et
//    const dataGrid = e.target.closest(".rz-data-grid");
//    if (!dataGrid) return;

//    // Scroll container'ı bul - önce wrapper, sonra scrollable
//    let gridScroll = dataGrid.querySelector(".rz-datatable-scrollable-wrapper");
//    if (!gridScroll) {
//        gridScroll = dataGrid.querySelector(".rz-datatable-scrollable");
//    }
//    if (!gridScroll) {
//        // Alternatif olarak, overflow-x: auto olan elementi bul
//        const allElements = dataGrid.querySelectorAll("*");
//        for (let el of allElements) {
//            const style = window.getComputedStyle(el);
//            if (style.overflowX === "auto" || style.overflowX === "scroll") {
//                gridScroll = el;
//                break;
//            }
//        }
//    }
    
//    if (!gridScroll) return;
    
//    // Yatay scroll varsa ve mouse grid üzerindeyse
//    if (gridScroll.scrollWidth > gridScroll.clientWidth && e.deltaY !== 0) {
//        e.preventDefault();
//        gridScroll.scrollLeft += e.deltaY;
//    }
//}, { passive: false });