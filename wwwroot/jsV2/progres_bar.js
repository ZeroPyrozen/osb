const progressBar = document.getElementById("myBar");

const progressContainer = document.querySelector(".progress-container");

const progressIndicator = document.getElementById("myIndicator");

const scrollableElement = document.getElementById("scrollCheck");

scrollableElement.onscroll = function () {
    updateProgressBar()
};

progressContainer.onmousemove = function (event) {
    onProgressHover(event)
};

progressContainer.onmouseleave = function () {
    onProgressLeave()
};

progressContainer.onclick = function (event) {
    const docHeight = scrollableElement.scrollHeight - scrollableElement.clientHeight;
    const clickPosition = event.offsetX / progressContainer.offsetWidth;
    scrollableElement.scroll({
        top: clickPosition * docHeight, behavior: 'smooth'
    });
}

function updateProgressBar() {
    const docHeight = scrollableElement.scrollHeight - scrollableElement.clientHeight;
    const winScroll = scrollableElement.scrollTop;

    if (docHeight > 0) {
        progressContainer.style.display = "block";
        progressBar.style.display = "block";

        const scrolled = (winScroll / docHeight) * 101;

        progressBar.style.width = scrolled + "%";
    } else {
        progressContainer.style.display = "none";
        progressBar.style.display = "none";
    }
}

function onProgressHover(event) {
    progressIndicator.style.display = "block";

    const hoverPosition = event.offsetX;

    progressIndicator.style.left = hoverPosition - 2 + "px";
}

function onProgressLeave() {
    progressIndicator.style.display = "none";
}

