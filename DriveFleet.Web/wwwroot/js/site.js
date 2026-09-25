// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener("DOMContentLoaded", function () {
    if (!window.jQuery || !$.validator) {
        return;
    }

    // Allows decimal values entered with either a comma or a dot.
    $.validator.methods.number = function (value, element) {
        return this.optional(element) ||
            /^-?\d+([.,]\d+)?$/.test(value.trim());
    };

    // Applies range validation to decimal values using
    // either a comma or a dot as the decimal separator.
    $.validator.methods.range = function (value, element, param) {
        if (this.optional(element)) {
            return true;
        }

        const normalizedValue =
            Number(value.replace(",", "."));

        return normalizedValue >= Number(param[0]) &&
            normalizedValue <= Number(param[1]);
    };
});