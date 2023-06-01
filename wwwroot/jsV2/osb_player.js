document.addEventListener('DOMContentLoaded', function() {
    const playerPositionTopCenter = document.querySelector('.player_position_top_center');
    const buttons = document.querySelectorAll(".event-button");
    let currentAudio = null;
    let currentPlayerSvg = null;
    let hidePlayerTimeout = null;
    let animationFrameId = null;
    const playButton = document.querySelector('.play_button');
    const playArrowIcon = document.querySelector('.play_arrow_icon');
    const currentTimeElement = document.getElementById('current_time');
    const timeSlider = document.querySelector('.time_slider');
    const volumePercentageElement = document.querySelector('.volume_percentage');
    const volumeSlider = document.querySelector('.volume_slider');

    volumeSlider.value = 15;
    volumePercentageElement.textContent = '15%';

    for (let i = 0; i < buttons.length; i++) {
        buttons[i].addEventListener("click", function() {
            const preview = this.getAttribute("data-preview");
            const playerSvg = this.querySelector(".player-svg");
            playerPositionTopCenter.style.pointerEvents = 'auto';
            playerPositionTopCenter.style.opacity = '1';
            playerPositionTopCenter.style.bottom = '13px';
            hidePlayer();
            if (currentAudio) {
                if (currentAudio.src === `https://b.ppy.sh/preview/${preview}.mp3`) {
                    if (currentAudio.paused) {
                        currentAudio.play();
                        playerSvg.setAttribute("src", "/staticV2/svg/pause.svg");
                        playArrowIcon.setAttribute("src", "/staticV2/svg/pause.svg");
                        updateSlider();
                    } else {
                        currentAudio.pause();
                        playerSvg.setAttribute("src", "/staticV2/svg/play_arrow.svg");
                        playArrowIcon.setAttribute("src", "/staticV2/svg/play_arrow.svg");
                        hidePlayer();
                    }
                    return;
                } else {
                    currentAudio.pause();
                    currentPlayerSvg.setAttribute("src", "/staticV2/svg/play_arrow.svg");
                    playArrowIcon.setAttribute("src", "/staticV2/svg/play_arrow.svg");
                    hidePlayer();
                }
            }
            currentAudio = new Audio(`https://b.ppy.sh/preview/${preview}.mp3`);
            currentAudio.volume = volumeSlider.value / 100;
            currentAudio.play();
            currentPlayerSvg = playerSvg;
            playerSvg.setAttribute("src", "/staticV2/svg/pause.svg");
            playArrowIcon.setAttribute("src", "/staticV2/svg/pause.svg");

            currentAudio.addEventListener("ended", function() {
                currentPlayerSvg.setAttribute("src", "/staticV2/svg/play_arrow.svg");
                playArrowIcon.setAttribute("src", "/staticV2/svg/play_arrow.svg");
                hidePlayer();
            });

            currentAudio.addEventListener('timeupdate', function() {
                currentTimeElement.textContent = formatTime(currentAudio.currentTime);
                updateSlider();
            });
        });
    }

    playButton.addEventListener('click', function() {
        if (currentAudio) {
            if (currentAudio.paused) {
                currentAudio.play();
                updateSlider();
                playArrowIcon.setAttribute("src", "/staticV2/svg/pause.svg");
                currentPlayerSvg.setAttribute("src", "/staticV2/svg/pause.svg");
            } else {
                currentAudio.pause();
                updateSlider();
                playArrowIcon.setAttribute("src", "/staticV2/svg/play_arrow.svg");
                currentPlayerSvg.setAttribute("src", "/staticV2/svg/play_arrow.svg");
            }
        }
    });

    timeSlider.addEventListener('input', function() {
        if (currentAudio) {
            currentAudio.currentTime = (timeSlider.value / 10000) * currentAudio.duration;
            hidePlayer();
        }
    });

    volumeSlider.addEventListener('input', function() {
        if (currentAudio) {
            currentAudio.volume = volumeSlider.value / 100;
            volumePercentageElement.textContent = `${volumeSlider.value}%`;
        }
    });

    function formatTime(time) {
        const minutes = Math.floor(time / 60);
        const seconds = Math.floor(time % 60);
        return `${minutes}:${seconds < 10 ? '0' + seconds : seconds}`;
    }

    function hidePlayer(delay = 10000) {
        clearTimeout(hidePlayerTimeout);
        hidePlayerTimeout = setTimeout(function() {
            playerPositionTopCenter.style.pointerEvents = 'none';
            playerPositionTopCenter.style.opacity = '0';
            playerPositionTopCenter.style.bottom = '-43px';
        }, delay);
    }

    // Helper function to update time slider value smoothly
    function updateSlider() {
        if (currentAudio) {
            timeSlider.value = (currentAudio.currentTime / currentAudio.duration) * 10000;
            animationFrameId = requestAnimationFrame(updateSlider);
        }
    }
})