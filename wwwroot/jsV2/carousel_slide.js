let sliderIndex = 0; // Start with index 0
showSlides(sliderIndex);

function posSlide(n) {
    showSlides(sliderIndex += n);
    resetTimer();
}

function currSlide(n) {
    showSlides(sliderIndex = n - 1); // Adjust to 0-based index
    resetTimer();
}

function showSlides(n) {
    let i;
    let slides = document.getElementsByClassName("slide-container");
    let dots = document.getElementsByClassName("dot")

    if (n >= slides.length) {
        sliderIndex = 0; // Reset index if at the last slide
    }
    if (n < 0) {
        sliderIndex = slides.length - 1; // Go to the last slide if at the first slide
    }
    for (i = 0; i < slides.length; i++) {
        slides[i].classList.remove("active");
    }
    for (i = 0; i < dots.length; i++) {
        dots[i].classList.remove("active-dot");
    }
    slides[sliderIndex].classList.add("active");
    dots[sliderIndex].classList.add("active-dot");
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
