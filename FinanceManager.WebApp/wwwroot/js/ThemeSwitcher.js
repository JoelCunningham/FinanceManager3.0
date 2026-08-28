let systemThemeListenerAttached = false;
let systemThemeQuery = window.matchMedia("(prefers-color-scheme: dark)");

function applyTheme(theme) {
    if (theme === "system") {
        theme = getSystemTheme();
    }
    document.documentElement.setAttribute("data-bs-theme", theme);
}

function getSystemTheme() {
    return systemThemeQuery.matches ? "dark" : "light";
}

function getEffectiveTheme() {
    const storedTheme = localStorage.getItem("theme");
    if (storedTheme === "system" || !storedTheme) {
        return getSystemTheme();
    }
    return storedTheme;
}

function attachSystemThemeListener() {
    if (systemThemeListenerAttached) return;

    systemThemeQuery.addEventListener("change", () => {
        if (localStorage.getItem("theme") === "system") {
            applyTheme(getSystemTheme());
        }
    });

    systemThemeListenerAttached = true;
}

window.theme = {
    set: function (theme) {
        localStorage.setItem("theme", theme);
        applyTheme(theme);

        if (theme === "system") {
            attachSystemThemeListener();
        }
    },

    getSystemTheme: getSystemTheme,

    getEffectiveTheme: getEffectiveTheme
};
