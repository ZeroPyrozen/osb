const changeLang = document.getElementById('changeLang');
const arrowLang = document.getElementById('arrowLang');
const langShow = document.getElementById('langShow');
const elem = document.querySelector(".change-language");

let wait;
elem.style.animationName = "example-off"

changeLang.addEventListener("click", () => {
    setTimeout(() => {
        if (elem.style.animationName === "example-off") {
            clearTimeout(wait);
            langShow.style.display = "inline-grid";
            elem.style.animationName = "example";
            elem.style.animationDuration = "0.5s";
            arrowLang.classList.add("arrow-rotated");
            return;
        }
        clearTimeout(wait);
        elem.style.animationName = "example-off";
        elem.style.animationDuration = "0.5s";
        wait = setTimeout(() => {
            langShow.style.display = "none";
        }, 450);
        arrowLang.classList.remove("arrow-rotated");
    }, 10);
});

document.addEventListener('click', (e) => {
    if (elem.style.animationName === "example-off" && changeLang.click) {
        return;
    }
    setTimeout(() => {
        if (elem.style.animationName === "example") {
            clearTimeout(wait);
            elem.style.animationName = "example-off";
            elem.style.animationDuration = "0.5s";
            wait = setTimeout(() => {
                langShow.style.display = "none";
            }, 450);
            arrowLang.classList.remove("arrow-rotated");
        }
    }, 100);
});


const buttons = document.querySelectorAll('#langShow button');

buttons.forEach(button => {
    button.addEventListener('click', event => {
        buttons.forEach(btn => btn.classList.remove('select'));
        event.target.classList.add('select');

        const lang = event.target.textContent;
        changeLanguage(lang);

        location.reload();
    });
});

function changeLanguage(lang) {
    localStorage.setItem('language', lang);
}

const lang = localStorage.getItem('language');

buttons.forEach(button => {
    if (button.textContent === lang) {
        button.classList.add('select');
    }
});