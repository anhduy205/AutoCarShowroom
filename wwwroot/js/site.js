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
    const currentCarIdRaw = chatbotRoot.getAttribute("data-current-car-id");
    const currentCarId = Number.isFinite(Number(currentCarIdRaw)) && Number(currentCarIdRaw) > 0
        ? Number(currentCarIdRaw)
        : null;

    if (!endpoint || !toggleButton || !panel || !form || !input || !submitButton || !messageList) {
        return;
    }

    const storageKeys = {
        history: "autocar-showroom-chatbot-history-v2",
        entries: "autocar-showroom-chatbot-entries-v2",
        state: "autocar-showroom-chatbot-state-v2"
    };

    const initialAssistantMessage = messageList.textContent?.trim() || "Em có thể hỗ trợ chọn xe, so sánh xe, giải thích thanh toán và đặt lịch xem xe.";
    messageList.innerHTML = "";

    let history = safeParseJson(sessionStorage.getItem(storageKeys.history), []);
    let entries = safeParseJson(sessionStorage.getItem(storageKeys.entries), []);
    let conversationState = safeParseJson(sessionStorage.getItem(storageKeys.state), null);

    const persistConversation = () => {
        sessionStorage.setItem(storageKeys.history, JSON.stringify(history.slice(-8)));
        sessionStorage.setItem(storageKeys.entries, JSON.stringify(entries.slice(-40)));
        sessionStorage.setItem(storageKeys.state, JSON.stringify(conversationState));
    };

    const setOpen = (isOpen) => {
        chatbotRoot.classList.toggle("is-open", isOpen);
        panel.hidden = !isOpen;
        toggleButton.setAttribute("aria-expanded", isOpen ? "true" : "false");

        if (isOpen) {
            window.setTimeout(() => input.focus(), 120);
        }
    };

    const scrollMessagesToBottom = () => {
        messageList.scrollTop = messageList.scrollHeight;
    };

    const addEntry = (entry, persist = true) => {
        entries.push(entry);

        if (persist) {
            persistConversation();
        }
    };

    const appendMessage = (role, text, options = {}) => {
        const { persistUi = true, persistHistory = true } = options;
        const message = document.createElement("article");
        message.className = `chatbot-message chatbot-message--${role}`;
        message.textContent = text;
        messageList.appendChild(message);

        if (persistUi) {
            addEntry({ type: "message", role, text });
        }

        if (persistHistory) {
            history.push({ role, message: text });
            history = history.slice(-8);
            persistConversation();
        }

        scrollMessagesToBottom();
    };

    const appendSuggestions = (suggestions, persistUi = true) => {
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

            const status = document.createElement("span");
            status.className = "chatbot-suggestion__status";
            status.textContent = suggestion.status || "Đang mở bán";

            const reason = document.createElement("p");
            reason.textContent = suggestion.reason;

            body.append(title, meta, price, status, reason);
            link.append(image, body);
            wrapper.appendChild(link);
        });

        messageList.appendChild(wrapper);

        if (persistUi) {
            addEntry({ type: "suggestions", suggestions });
        }

        scrollMessagesToBottom();
    };

    const createChoiceButton = (label, message, className) => {
        const button = document.createElement("button");
        button.type = "button";
        button.className = className;
        button.textContent = label;
        button.addEventListener("click", () => {
            input.value = message;
            form.requestSubmit();
        });
        return button;
    };

    const appendQuickReplies = (replies, persistUi = true) => {
        if (!Array.isArray(replies) || replies.length === 0) {
            return;
        }

        const wrapper = document.createElement("div");
        wrapper.className = "chatbot-choice-group";

        const label = document.createElement("span");
        label.className = "chatbot-choice-group__label";
        label.textContent = "Gợi ý nhanh";
        wrapper.appendChild(label);

        const buttonRail = document.createElement("div");
        buttonRail.className = "chatbot-choice-rail";

        replies.forEach((reply) => {
            buttonRail.appendChild(createChoiceButton(reply.label, reply.message, "chatbot-choice chatbot-choice--quick"));
        });

        wrapper.appendChild(buttonRail);
        messageList.appendChild(wrapper);

        if (persistUi) {
            addEntry({ type: "quickReplies", replies });
        }

        scrollMessagesToBottom();
    };

    const appendActions = (actions, persistUi = true) => {
        if (!Array.isArray(actions) || actions.length === 0) {
            return;
        }

        const wrapper = document.createElement("div");
        wrapper.className = "chatbot-choice-group";

        const label = document.createElement("span");
        label.className = "chatbot-choice-group__label";
        label.textContent = "Bước tiếp theo";
        wrapper.appendChild(label);

        const actionRail = document.createElement("div");
        actionRail.className = "chatbot-choice-rail";

        actions.forEach((action) => {
            if (action.kind === "link" && action.url) {
                const link = document.createElement("a");
                link.className = `chatbot-choice chatbot-choice--action chatbot-choice--${action.variant || "secondary"}`;
                link.href = action.url;
                link.textContent = action.label;
                actionRail.appendChild(link);
                return;
            }

            if (action.kind === "message" && action.message) {
                actionRail.appendChild(createChoiceButton(
                    action.label,
                    action.message,
                    `chatbot-choice chatbot-choice--action chatbot-choice--${action.variant || "secondary"}`
                ));
            }
        });

        wrapper.appendChild(actionRail);
        messageList.appendChild(wrapper);

        if (persistUi) {
            addEntry({ type: "actions", actions });
        }

        scrollMessagesToBottom();
    };

    const renderStoredEntries = () => {
        if (!Array.isArray(entries) || entries.length === 0) {
            appendMessage("assistant", initialAssistantMessage, { persistUi: true, persistHistory: false });
            return;
        }

        entries.forEach((entry) => {
            switch (entry.type) {
                case "message":
                    appendMessage(entry.role || "assistant", entry.text || "", { persistUi: false, persistHistory: false });
                    break;
                case "suggestions":
                    appendSuggestions(entry.suggestions || [], false);
                    break;
                case "quickReplies":
                    appendQuickReplies(entry.replies || [], false);
                    break;
                case "actions":
                    appendActions(entry.actions || [], false);
                    break;
                default:
                    break;
            }
        });
    };

    const setPending = (isPending) => {
        submitButton.disabled = isPending;
        submitButton.textContent = isPending ? "Đang tư vấn..." : "Gửi";
    };

    const sendMessage = async (message) => {
        appendMessage("user", message);
        setPending(true);

        try {
            const response = await fetch(endpoint, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    message,
                    history,
                    currentCarId,
                    state: conversationState
                })
            });

            let data = null;
            try {
                data = await response.json();
            } catch (error) {
                data = null;
            }

            const replyText = data?.message || "Em chưa lấy được dữ liệu tư vấn phù hợp. Anh/chị thử lại giúp em nhé.";
            appendMessage("assistant", replyText);

            if (Array.isArray(data?.suggestions)) {
                appendSuggestions(data.suggestions);
            }

            if (Array.isArray(data?.quickReplies)) {
                appendQuickReplies(data.quickReplies);
            }

            if (Array.isArray(data?.actions)) {
                appendActions(data.actions);
            }

            conversationState = data?.state || null;
            persistConversation();
        } catch (error) {
            appendMessage("assistant", "AI tư vấn đang bận hoặc chưa kết nối được dữ liệu showroom. Anh/chị thử lại sau giúp em nhé.");
        } finally {
            setPending(false);
        }
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

        input.value = "";
        await sendMessage(message);
    });

    renderStoredEntries();
}

function safeParseJson(rawValue, fallbackValue) {
    if (!rawValue) {
        return fallbackValue;
    }

    try {
        return JSON.parse(rawValue);
    } catch (error) {
        return fallbackValue;
    }
}
