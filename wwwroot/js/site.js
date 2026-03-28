document.addEventListener("DOMContentLoaded", () => {
    const tabButtons = document.querySelectorAll("[data-gallery-tab]");
    const tabPanels = document.querySelectorAll("[data-gallery-panel]");

    tabButtons.forEach((button) => {
        button.addEventListener("click", () => {
            const target = button.getAttribute("data-gallery-tab");

            tabButtons.forEach((item) => {
                item.classList.toggle("is-active", item === button);
            });

            tabPanels.forEach((panel) => {
                panel.classList.toggle("is-active", panel.getAttribute("data-gallery-panel") === target);
            });
        });
    });
});
