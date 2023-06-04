function toggleDropdown() {
    const selectOptions = document.getElementById("select-options");
    if (selectOptions) {
        selectOptions.classList.toggle("show");
    }
}

function getURLParams() {
    const searchParams = new URLSearchParams(window.location.search);
    const params = {};
    for (let param of searchParams.entries()) {
        params[param[0]] = param[1];
    }
    return params;
}

function selectOption(value, label) {
    const selectedValue = document.getElementById("selected-value");
    if (selectedValue) {
        selectedValue.textContent = label;
        selectedValue.dataset.value = value;
        updateURL();
        toggleDropdown();

        const options = document.querySelectorAll('.option');
        options.forEach(option => option.classList.remove('active'));

        const selectedOption = document.querySelector('.option[data-value="' + value + '"]');
        if (selectedOption) {
            selectedOption.classList.add('active');
        }
    }
}

function updateURL() {
    const searchInput = document.getElementById("text-search");
    const searchText = searchInput.value;
    let selectedValue = document.getElementById("selected-value").dataset.value;

    if (selectedValue === undefined){
        selectedValue = "-1";
    }

    const url = "https://" + window.location.host + "/future/search?s=" + encodeURIComponent(searchText) + "&t=" + encodeURIComponent(selectedValue);
    window.history.replaceState({}, '', url);
}

function toggleDropdown() {
    const selectOptions = document.getElementById("select-options");
    if (selectOptions) {
        selectOptions.classList.toggle("show");
    }
}

document.getElementById("search-action").addEventListener("click", function(event) {
    event.preventDefault();
    updateURL();
    window.location.reload();
});

window.addEventListener('DOMContentLoaded', function() {
    const params = getURLParams();
    const selectedValue = document.getElementById("selected-value");
    if (selectedValue && params.t) {
        const option = document.querySelector('.option[data-value="' + params.t + '"]');
        if (option) {
            selectedValue.textContent = option.textContent;
            selectedValue.dataset.value = params.t;
            option.classList.add("active");
        }
    }
});