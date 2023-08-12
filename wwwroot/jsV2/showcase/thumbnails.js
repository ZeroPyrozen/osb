let sliderIndex = 0; // Start with index 0
showSlides(sliderIndex);

function currSlide(n) {
    stopCurrentVideo(); // Pause current video before changing slide
    playVideoOnSlide(n - 1); // Play video on the upcoming slide
    showSlides(sliderIndex = n - 1); // Adjust to 0-based index
}


function stopCurrentVideo() {
    playerInstances.forEach(player => {
        player.pauseVideo();
    });
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

let playerInstances = [];

function onYouTubeIframeAPIReady() {
    const iframes = document.querySelectorAll(".slide-container iframe");
    iframes.forEach((iframe, index) => {
        const player = new YT.Player(iframe);
        playerInstances.push(player);

        // Play the video on the first slide with a delay of 1 second
        if (index === 0) {
            setTimeout(function() {
                player.playVideo();
            }, 2500);
        }
    });
}



function playVideoOnSlide(index) {
    const player = playerInstances[index];
    if (player && typeof player.playVideo === "function") {
        player.playVideo();
    }
}

