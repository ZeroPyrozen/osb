let elements = document.querySelectorAll('.beatmap');

if (elements.length === 0) {
    elements = document.querySelectorAll('.user-card');
}
function isInViewport(element) {
    const rect = element.getBoundingClientRect();
    return (
        rect.top >= -200 &&
        rect.left >= -200 &&
        rect.bottom <= ((window.innerHeight + 200) || document.documentElement.clientHeight) &&
        rect.right <= (window.innerWidth + 200 || document.documentElement.clientWidth)
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