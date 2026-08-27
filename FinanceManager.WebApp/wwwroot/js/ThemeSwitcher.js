window.theme = {
    set: function (theme) {
        document.documentElement.setAttribute("data-bs-theme", theme);
    },

    getSystemTheme: function () {
        return window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
    },

    getVariableValue: function (name) {
        return getComputedStyle(document.documentElement).getPropertyValue(name).trim();
    }
};