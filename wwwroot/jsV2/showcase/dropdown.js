let searchTimer;

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

        // Reload the page after selecting an option
        window.location.reload();
    }
}

function updateURL() {
    const searchInput = document.getElementById("text-search");
    const searchText = searchInput.value;
    let selectedValue = document.getElementById("selected-value").dataset.value;

    if (selectedValue === undefined) {
        selectedValue = "-1";
    }

    const url = "https://" + window.location.host + "/future/search?s=" + encodeURIComponent(searchText) + "&t=" + encodeURIComponent(selectedValue);
    window.history.replaceState({}, '', url);
}

function onSearchInputChange() {
    clearTimeout(searchTimer);
    searchTimer = setTimeout(function () {
        updateURL();
        window.location.reload();
    }, 1000);
}

window.addEventListener('DOMContentLoaded', function () {
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

document.getElementById("text-search").addEventListener("input", onSearchInputChange);

document.addEventListener("keydown", function(event) {
    if (event.key === "Tab") {
        event.preventDefault();
        const inputElement = document.getElementById("text-search");
        if (inputElement) {
            inputElement.focus();
        }
    }
});

// Add the change event listener to the select element
document.getElementById("selected-value").addEventListener("change", function() {
    const selectedOption = this.options[this.selectedIndex];
    const value = selectedOption.value;
    const label = selectedOption.text;
    selectOption(value, label);
});
