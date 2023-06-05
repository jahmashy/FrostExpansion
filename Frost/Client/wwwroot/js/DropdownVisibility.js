window.addDocumentClickListener = function (dotNetHelper,id) {
    document.addEventListener("click", function (event) {
        var dropdownElement = document.getElementById(id);
        var targetElement = event.target;
        if (dropdownElement && !dropdownElement.contains(targetElement) && dropdownElement.parentElement != targetElement && dropdownElement.previousElementSibling != targetElement) {
            dotNetHelper.invokeMethodAsync("ToggleDropdownVisibility", false);
        }
    });
};