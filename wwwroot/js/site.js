document.addEventListener("DOMContentLoaded", () => {
    initializeGalleryTabs();
    initializePaymentCards();
    initializeChatbot();
});

function initializeGalleryTabs() {
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
}

function initializePaymentCards() {
    const paymentSelect = document.querySelector("[data-payment-select]");
    const paymentCards = document.querySelectorAll("[data-payment-card]");

    if (!paymentSelect || paymentCards.length === 0) {
        return;
    }

    const syncPaymentCards = () => {
        const selectedValue = paymentSelect.value;

        paymentCards.forEach((card) => {
            card.classList.toggle("is-active", card.getAttribute("data-payment-card") === selectedValue);
        });
    };

    paymentSelect.addEventListener("change", syncPaymentCards);
    syncPaymentCards();
}

function initializeChatbot() {
    const chatbotRoot = document.querySelector("[data-chatbot]");

    if (!chatbotRoot) {
        return;
    }

    const endpoint = chatbotRoot.getAttribute("data-endpoint");
    const toggleButton = chatbotRoot.querySelector("[data-chatbot-toggle]");
    const closeButton = chatbotRoot.querySelector("[data-chatbot-close]");
    const panel = chatbotRoot.querySelector("[data-chatbot-panel]");
    const form = chatbotRoot.querySelector("[data-chatbot-form]");
    const input = chatbotRoot.querySelector("[data-chatbot-input]");
    const submitButton = chatbotRoot.querySelector("[data-chatbot-submit]");
    const messageList = chatbotRoot.querySelector("[data-chatbot-messages]");
    const promptButtons = chatbotRoot.querySelectorAll("[data-chatbot-prompt]");

    if (!endpoint || !toggleButton || !panel || !form || !input || !submitButton || !messageList) {
        return;
    }

    const setOpen = (isOpen) => {
        chatbotRoot.classList.toggle("is-open", isOpen);
        panel.hidden = !isOpen;
        toggleButton.setAttribute("aria-expanded", isOpen ? "true" : "false");

        if (isOpen) {
            window.setTimeout(() => input.focus(), 120);
        }
    };

    const appendMessage = (role, text) => {
        const message = document.createElement("article");
        message.className = `chatbot-message chatbot-message--${role}`;
        message.textContent = text;
        messageList.appendChild(message);
        messageList.scrollTop = messageList.scrollHeight;
    };

    const appendSuggestions = (suggestions) => {
        if (!Array.isArray(suggestions) || suggestions.length === 0) {
            return;
        }

        const wrapper = document.createElement("div");
        wrapper.className = "chatbot-suggestions";

        suggestions.forEach((suggestion) => {
            const link = document.createElement("a");
            link.className = "chatbot-suggestion";
            link.href = `/Cars/Details/${suggestion.carId}`;

            const image = document.createElement("img");
            image.className = "chatbot-suggestion__image";
            image.src = suggestion.image;
            image.alt = suggestion.carName;

            const body = document.createElement("div");
            body.className = "chatbot-suggestion__body";

            const title = document.createElement("strong");
            title.textContent = suggestion.carName;

            const meta = document.createElement("span");
            meta.textContent = `${suggestion.brand} ${suggestion.modelName} • ${suggestion.bodyType}`;

            const price = document.createElement("span");
            price.className = "chatbot-suggestion__price";
            price.textContent = `${Number(suggestion.price).toLocaleString("vi-VN")} VNĐ`;

            const reason = document.createElement("p");
            reason.textContent = suggestion.reason;

            body.append(title, meta, price, reason);
            link.append(image, body);
            wrapper.appendChild(link);
        });

        messageList.appendChild(wrapper);
        messageList.scrollTop = messageList.scrollHeight;
    };

    const setPending = (isPending) => {
        submitButton.disabled = isPending;
        submitButton.textContent = isPending ? "Đang tư vấn..." : "Gửi";
    };

    toggleButton.addEventListener("click", () => {
        setOpen(!chatbotRoot.classList.contains("is-open"));
    });

    closeButton?.addEventListener("click", () => {
        setOpen(false);
    });

    promptButtons.forEach((button) => {
        button.addEventListener("click", () => {
            const prompt = button.getAttribute("data-chatbot-prompt") ?? "";
            input.value = prompt;
            setOpen(true);
            form.requestSubmit();
        });
    });

    form.addEventListener("submit", async (event) => {
        event.preventDefault();

        const message = input.value.trim();
        if (!message) {
            input.focus();
            return;
        }

        appendMessage("user", message);
        input.value = "";
        setPending(true);

        try {
            const response = await fetch(endpoint, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({ message })
            });

            const data = await response.json();
            appendMessage("assistant", data.message || "Mình chưa lấy được dữ liệu tư vấn phù hợp.");
            appendSuggestions(data.suggestions);
        } catch (error) {
            appendMessage("assistant", "AI tư vấn đang bận hoặc chưa kết nối được dữ liệu showroom. Bạn thử lại sau nhé.");
        } finally {
            setPending(false);
        }
    });
}
