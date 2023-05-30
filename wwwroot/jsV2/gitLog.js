const gitLog = document.getElementById('gitLog');
const arrowGit = document.getElementById('arrowGit');
const gitLogShow = document.getElementById('gitLogShow');
const elemGit = document.querySelector(".change-git");

let timerGit;
elemGit.style.animationName = "loadAnim-off"

gitLog.addEventListener("click", () => {
    setTimeout(() => {
        if (elemGit.style.animationName === "loadAnim-off") {
            clearTimeout(timerGit);
            gitLogShow.style.display = "inline-grid";
            elemGit.style.animationName = "example";
            elemGit.style.animationDuration = "0.5s";
            arrowGit.classList.add("arrow-rotated");
            return;
        }
        clearTimeout(timerGit);
        elemGit.style.animationName = "loadAnim-off";
        elemGit.style.animationDuration = "0.5s";
        timerGit = setTimeout(() => {
            gitLogShow.style.display = "none";
        }, 450);
        arrowGit.classList.remove("arrow-rotated");
    }, 10);
});

document.addEventListener('click', (e) => {
    if (elemGit.style.animationName === "loadAnim-off" && gitLog.click) {
        return;
    }
    setTimeout(() => {
        if (elemGit.style.animationName === "loadAnim") {
            clearTimeout(timerGit);
            elemGit.style.animationName = "loadAnim-off";
            elemGit.style.animationDuration = "0.5s";
            timerGit = setTimeout(() => {
                gitLogShow.style.display = "none";
            }, 450);
            arrowGit.classList.remove("arrow-rotated");
        }
    }, 100);
});

fetch("https://api.github.com/repos/Maks1mio/osb-laboratory/events")
    .then(response => response.json())
    .then(events => {
        let gitLogShow = document.querySelector("#gitLogShow");
        for (let event in events) {
            let userAvatar = events[event].actor.avatar_url;
            let userName = events[event].actor.display_login;
            let userCreateTime = events[event].created_at;

            // создание элементов для отображения информации
            let profileContainer = document.createElement("div");
            profileContainer.classList.add("profile-container");

            let profileContainerIn = document.createElement("div");
            profileContainerIn.classList.add("profile-container-in");

            let avatarImg = document.createElement("img");
            avatarImg.classList.add("avatar-img");

            let nameElement = document.createElement("p");
            nameElement.classList.add("name-element");

            let timeElement = document.createElement("p");
            timeElement.classList.add("time-element");

            // установка значений для элементов
            avatarImg.src = userAvatar;
            nameElement.textContent = userName;
            timeElement.textContent = userCreateTime;

            // создание контейнера для элементов имени и времени
            let nameTimeContainer = document.createElement("div");
            nameTimeContainer.appendChild(nameElement);
            nameTimeContainer.appendChild(timeElement);
            
            // добавление элементов в контейнер профиля
            profileContainerIn.appendChild(avatarImg);
            profileContainerIn.appendChild(nameTimeContainer);
            
            // добавление элементов в контейнер профиля
            profileContainer.appendChild(profileContainerIn);

            // добавление обработчика событий для открытия профиля пользователя на GitHub
            avatarImg.addEventListener('click', () => {
                window.open(`https://github.com/${userName}`, '_blank');
            });

            // проверка наличия коммитов в событии
            if (events[event].payload.commits) {
                console.log(events[event].payload.commits);
                for (let commit of events[event].payload.commits) {
                    // создание элементов для отображения информации о коммите
                    let commitContainer = document.createElement("div");
                    commitContainer.classList.add("commit-container");
                    let messageElement = document.createElement("p");
                    messageElement.classList.add("message-element");
                    let urlElement = document.createElement("a");
                    urlElement.classList.add("url-element");

                    // установка значений для элементов
                    urlElement.href = `https://github.com/Maks1mio/osb-laboratory/commit/${commit.sha}`; 
                    urlElement.target = "_blank";
                    urlElement.textContent = "View Commit";   
                    
                    messageElement.textContent = `Message: ${commit.message}`;

                    // добавление элементов в контейнер коммита
                    commitContainer.appendChild(messageElement);
                    commitContainer.appendChild(urlElement);

                    // получение дополнительной информации о коммите
                    fetch(commit.url)
                        .then(response => response.json())
                        .then(commitData => {
                            // создание элемента для отображения измененных файлов
                            let filesElement = document.createElement("ul");
                            filesElement.classList.add("files-element");

                            for (let file of commitData.files) {
                                let fileElement = document.createElement("li");
                                fileElement.classList.add("file-element");
                                fileElement.textContent = file.filename;
                                filesElement.appendChild(fileElement);
                            }

                            // добавление элемента с измененными файлами в контейнер коммита
                            commitContainer.appendChild(filesElement);
                        })

                    // добавление контейнера коммита в контейнер профиля
                    profileContainer.appendChild(commitContainer);
                }
            }

            // добавление контейнера профиля в элемент gitLogShow
            gitLogShow.appendChild(profileContainer);
        }
    })