window.setInputType = (element, type) => {
    if (element) element.type = type;
};
function selectTab(i) {
    document.querySelectorAll('.my-tab').forEach((t, idx) => {
        t.classList.toggle('active', idx === i);
    });
    document.querySelectorAll('[id^="tabContent"]').forEach((c, idx) => {
        c.style.display = idx === i ? 'block' : 'none';
    });
}