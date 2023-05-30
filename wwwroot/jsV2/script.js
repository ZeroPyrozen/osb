const discordBtn = document.getElementById('discord');
const helptranslateBtn = document.getElementById('helptranslate');


function openDiscord() {
    window.open('https://discord.gg/example', '_blank');
}

function openHelpTranslate() {
    window.open('https://helptranslate.com', '_blank');
}

discordBtn.addEventListener('click', openDiscord);

helptranslateBtn.addEventListener('click', openHelpTranslate);


