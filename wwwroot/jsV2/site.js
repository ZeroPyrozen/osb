let sliderIndex = 1;
showSlides(sliderIndex);

function posSlide(n) {
    showSlides(sliderIndex += n);
}

function currSlide(n) {
    showSlides(sliderIndex = n);
}

function showSlides(n) {
    let i;
    let slides = document.getElementsByClassName("my-slide");
    let dots = document.getElementsByClassName("dot")

    if (n > slides.length) {
        sliderIndex = 1
    }
    if (n < 1) {
        sliderIndex = slides.length
    }
    for (i = 0; i < slides.length; i++) {
        slides[i].style.display = "none";
    }
    for (i = 0; i < dots.length; i++) {
        dots[i].className = dots[i].className.replace("active", "");
    }
    slides[sliderIndex - 1].style.display = "block";
    dots[sliderIndex - 1].className += " active";
}



const card = document.querySelector(".card");
const motionMatchMedia = window.matchMedia("(prefers-reduced-motion)");
const THRESHOLD = 3;

function handleHover(e) {
    const { clientX, clientY, currentTarget } = e;
    const { clientWidth, clientHeight, offsetLeft, offsetTop } = currentTarget;

    const horizontal = (clientX - offsetLeft) / clientWidth;
    const vertical = (clientY - offsetTop) / clientHeight;
    const rotateX = (THRESHOLD / 2 - horizontal * THRESHOLD).toFixed(2);
    const rotateY = (vertical * THRESHOLD - THRESHOLD / 2).toFixed(2);

    card.style.transform = `perspective(${clientWidth}px) rotateX(${rotateY}deg) rotateY(${rotateX}deg) scale3d(1, 1, 1)`;
}

function resetStyles(e) {
    card.style.transform = `perspective(${e.currentTarget.clientWidth}px) rotateX(0deg) rotateY(0deg)`;
}

if (!motionMatchMedia.matches) {
    card.addEventListener("mousemove", handleHover);
    card.addEventListener("mouseleave", resetStyles);
}


const navbar = document.getElementById('header');
window.onscroll = () => {
    if (window.scrollY > 90) {
        navbar.classList.add('nav-active');
        navbar.classList.remove('nav-bar');
    } else {
        navbar.classList.add('nav-bar');
        navbar.classList.remove('nav-active');
    }
};


const userDashboard = document.getElementById('userDashboard');
const dashboard = document.getElementById('dashboard');
const changeLangButton = document.getElementById('changeLangButton');
const dashboardLang = document.getElementById('dashboardLanguage');
let TF = true;
let CL = true;

document.body.addEventListener('click', () => {
    window.onclick = e => {
        if (e.target.id == 'trigger' || e.target.id == 'changeLangButton') return;
        if (TF == false) dashboard.style.display = 'none', TF = true
        if (CL == false) dashboardLang.style.display = 'none', CL = true
    }
})

if (userDashboard) {
    userDashboard.addEventListener('click', () => {
        if (CL == false) dashboardLang.style.display = 'none', CL = true
        if (TF == false) dashboard.style.display = 'none', TF = true
        else dashboard.style.display = 'flex', TF = false
    })
}

if (changeLangButton) {
    changeLangButton.addEventListener('click', () => {
        if (TF == false) dashboard.style.display = 'none', TF = true
        if (CL == false) dashboardLang.style.display = 'none', CL = true
        else dashboardLang.style.display = 'flex', CL = false
    })
}

const search = document.getElementById('search');
let current = search;
const toSB = document.getElementById('toSB');
const toOL = document.getElementById('toOL');

search.addEventListener('input', () => {
    check(current.value)
})

search.addEventListener('click', () => {
    check(current.value)
})

function check(e) {
    toSB.href = `http://127.0.0.1:5500/showcase/search?s=${e}`;
    toOL.href = `http://127.0.0.1:5500/learn/docs/search/?q=${e}`;
}
