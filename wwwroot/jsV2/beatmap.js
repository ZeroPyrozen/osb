let elements = document.querySelectorAll('.beatmap');
function isInViewport(element) {
    const rect = element.getBoundingClientRect();
    return (
        rect.top >= -200 &&
        rect.left >= 0 &&
        rect.bottom <= ((window.innerHeight + 200) || document.documentElement.clientHeight) &&
        rect.right <= (window.innerWidth || document.documentElement.clientWidth)
    );
}
function onScroll() {
    elements.forEach(element => {
        if (isInViewport(element)) {
            element.style.opacity = 1;
            element.style.scale = 1;
        } else {
            element.style.opacity = 0;
            element.style.scale = 1.2;
        }
    });
}

window.addEventListener('scroll', onScroll);