let sliderIndex = 1;
showSlides(sliderIndex);

function posSlide(n) {
    showSlides(sliderIndex += n);
    resetTimer();
}

function currSlide(n) {
    showSlides(sliderIndex = n);
    resetTimer();
}

function showSlides(n) {
    let i;
    let slides = document.getElementsByClassName("slide-container");
    let dots = document.getElementsByClassName("dot")

    if (n > slides.length) {
        sliderIndex = 1
    }
    if (n < 1) {
        sliderIndex = slides.length
    }
    for (i = 0; i < slides.length; i++) {
        slides[i].classList.remove("active");
    }
    for (i = 0; i < dots.length; i++) {
        dots[i].className = dots[i].className.replace("active-dot", "");
    }
    slides[sliderIndex - 1].classList.add("active");
    dots[sliderIndex - 1].className += " active-dot";
}

let slideInterval = setInterval(function() {
    posSlide(1);
}, 7000);

function resetTimer() {
    clearInterval(slideInterval);
    slideInterval = setInterval(function() {
        posSlide(1);
    }, 7000);
}