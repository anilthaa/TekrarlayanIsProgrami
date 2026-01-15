window.setInputType = (element, type) => {
    if (element) element.type = type;
};

document.addEventListener("wheel", function (e) {
    const tabs = e.target.closest(".my-tabs");
    if (!tabs) return;

    if (e.deltaY !== 0) {
        e.preventDefault();
        tabs.scrollLeft += e.deltaY;
    }
}, { passive: false });


document.addEventListener("wheel", function (e) {
    // RadzenDataGrid içinde mi kontrol et
    const dataGrid = e.target.closest(".rz-data-grid");
    if (!dataGrid) return;

    // Scroll container'ı bul - önce wrapper, sonra scrollable
    let gridScroll = dataGrid.querySelector(".rz-datatable-scrollable-wrapper");
    if (!gridScroll) {
        gridScroll = dataGrid.querySelector(".rz-datatable-scrollable");
    }
    if (!gridScroll) {
        // Alternatif olarak, overflow-x: auto olan elementi bul
        const allElements = dataGrid.querySelectorAll("*");
        for (let el of allElements) {
            const style = window.getComputedStyle(el);
            if (style.overflowX === "auto" || style.overflowX === "scroll") {
                gridScroll = el;
                break;
            }
        }
    }
    
    if (!gridScroll) return;
    
    // Yatay scroll varsa ve mouse grid üzerindeyse
    if (gridScroll.scrollWidth > gridScroll.clientWidth && e.deltaY !== 0) {
        e.preventDefault();
        gridScroll.scrollLeft += e.deltaY;
    }
}, { passive: false });