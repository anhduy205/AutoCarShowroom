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

    const initialAssistantMessage = normalizeChatbotText(
        messageList.textContent?.trim() || "Em có thể hỗ trợ chọn xe, so sánh xe, giải thích thanh toán và đặt lịch xem xe."
    );
    messageList.innerHTML = "";

    let history = normalizeStoredHistory(safeParseJson(sessionStorage.getItem(storageKeys.history), []));
    let entries = normalizeStoredEntries(safeParseJson(sessionStorage.getItem(storageKeys.entries), []));
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
        const resolvedText = normalizeChatbotText(text);
        const message = document.createElement("article");
        message.className = `chatbot-message chatbot-message--${role}`;
        message.textContent = resolvedText;
        messageList.appendChild(message);

        if (persistUi) {
            addEntry({ type: "message", role, text: resolvedText });
        }

        if (persistHistory) {
            history.push({ role, message: resolvedText });
            history = history.slice(-8);
            persistConversation();
        }

        scrollMessagesToBottom();
    };

    const appendSuggestions = (suggestions, persistUi = true) => {
        if (!Array.isArray(suggestions) || suggestions.length === 0) {
            return;
        }

        const normalizedSuggestions = suggestions.map(normalizeSuggestionData);
        const wrapper = document.createElement("div");
        wrapper.className = "chatbot-suggestions";

        normalizedSuggestions.forEach((suggestion) => {
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
            addEntry({ type: "suggestions", suggestions: normalizedSuggestions });
        }

        scrollMessagesToBottom();
    };

    const createChoiceButton = (label, message, className) => {
        const button = document.createElement("button");
        button.type = "button";
        button.className = className;
        button.textContent = normalizeChatbotText(label);
        button.addEventListener("click", () => {
            input.value = normalizeChatbotText(message);
            form.requestSubmit();
        });
        return button;
    };

    const appendQuickReplies = (replies, persistUi = true) => {
        if (!Array.isArray(replies) || replies.length === 0) {
            return;
        }

        const normalizedReplies = replies.map(normalizeQuickReplyData);
        const wrapper = document.createElement("div");
        wrapper.className = "chatbot-choice-group";

        const label = document.createElement("span");
        label.className = "chatbot-choice-group__label";
        label.textContent = "Gợi ý nhanh";
        wrapper.appendChild(label);

        const buttonRail = document.createElement("div");
        buttonRail.className = "chatbot-choice-rail";

        normalizedReplies.forEach((reply) => {
            buttonRail.appendChild(createChoiceButton(reply.label, reply.message, "chatbot-choice chatbot-choice--quick"));
        });

        wrapper.appendChild(buttonRail);
        messageList.appendChild(wrapper);

        if (persistUi) {
            addEntry({ type: "quickReplies", replies: normalizedReplies });
        }

        scrollMessagesToBottom();
    };

    const appendActions = (actions, persistUi = true) => {
        if (!Array.isArray(actions) || actions.length === 0) {
            return;
        }

        const normalizedActions = actions.map(normalizeActionData);
        const wrapper = document.createElement("div");
        wrapper.className = "chatbot-choice-group";

        const label = document.createElement("span");
        label.className = "chatbot-choice-group__label";
        label.textContent = "Bước tiếp theo";
        wrapper.appendChild(label);

        const actionRail = document.createElement("div");
        actionRail.className = "chatbot-choice-rail";

        normalizedActions.forEach((action) => {
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
            addEntry({ type: "actions", actions: normalizedActions });
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
        const normalizedMessage = normalizeChatbotText(message);
        appendMessage("user", normalizedMessage);
        setPending(true);

        try {
            const response = await fetch(endpoint, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    message: normalizedMessage,
                    history,
                    currentCarId,
                    state: conversationState
                })
            });

            let data = null;
            try {
                data = normalizeChatbotPayload(await response.json());
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
            const prompt = normalizeChatbotText(button.getAttribute("data-chatbot-prompt") ?? "");
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
    persistConversation();
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

function normalizeChatbotPayload(payload) {
    if (!payload || typeof payload !== "object") {
        return payload;
    }

    return {
        ...payload,
        message: normalizeChatbotText(payload.message ?? ""),
        suggestions: Array.isArray(payload.suggestions) ? payload.suggestions.map(normalizeSuggestionData) : [],
        quickReplies: Array.isArray(payload.quickReplies) ? payload.quickReplies.map(normalizeQuickReplyData) : [],
        actions: Array.isArray(payload.actions) ? payload.actions.map(normalizeActionData) : []
    };
}

function normalizeStoredHistory(history) {
    if (!Array.isArray(history)) {
        return [];
    }

    return history.map((entry) => ({
        ...entry,
        message: normalizeChatbotText(entry?.message ?? "")
    }));
}

function normalizeStoredEntries(entries) {
    if (!Array.isArray(entries)) {
        return [];
    }

    return entries.map((entry) => {
        if (!entry || typeof entry !== "object") {
            return entry;
        }

        switch (entry.type) {
            case "message":
                return {
                    ...entry,
                    text: normalizeChatbotText(entry.text ?? "")
                };
            case "suggestions":
                return {
                    ...entry,
                    suggestions: Array.isArray(entry.suggestions) ? entry.suggestions.map(normalizeSuggestionData) : []
                };
            case "quickReplies":
                return {
                    ...entry,
                    replies: Array.isArray(entry.replies) ? entry.replies.map(normalizeQuickReplyData) : []
                };
            case "actions":
                return {
                    ...entry,
                    actions: Array.isArray(entry.actions) ? entry.actions.map(normalizeActionData) : []
                };
            default:
                return entry;
        }
    });
}

function normalizeSuggestionData(suggestion) {
    if (!suggestion || typeof suggestion !== "object") {
        return suggestion;
    }

    return {
        ...suggestion,
        carName: normalizeChatbotText(suggestion.carName ?? ""),
        brand: normalizeChatbotText(suggestion.brand ?? ""),
        modelName: normalizeChatbotText(suggestion.modelName ?? ""),
        bodyType: normalizeChatbotText(suggestion.bodyType ?? ""),
        status: normalizeChatbotText(suggestion.status ?? ""),
        reason: normalizeChatbotText(suggestion.reason ?? "")
    };
}

function normalizeQuickReplyData(reply) {
    if (!reply || typeof reply !== "object") {
        return reply;
    }

    return {
        ...reply,
        label: normalizeChatbotText(reply.label ?? ""),
        message: normalizeChatbotText(reply.message ?? "")
    };
}

function normalizeActionData(action) {
    if (!action || typeof action !== "object") {
        return action;
    }

    return {
        ...action,
        label: normalizeChatbotText(action.label ?? ""),
        message: normalizeChatbotText(action.message ?? "")
    };
}

function normalizeChatbotText(value) {
    if (typeof value !== "string" || !value) {
        return "";
    }

    let current = value.normalize("NFC");

    for (let attempt = 0; attempt < 3; attempt += 1) {
        if (getMojibakeScore(current) === 0) {
            break;
        }

        const decoded = tryDecodeUtf8Mojibake(current);
        if (!decoded || getMojibakeScore(decoded) >= getMojibakeScore(current)) {
            break;
        }

        current = decoded.normalize("NFC");
    }

    return current;
}

function tryDecodeUtf8Mojibake(value) {
    try {
        const bytes = encodeWindows1252(value);
        if (!bytes) {
            return null;
        }

        return new TextDecoder("utf-8", { fatal: true }).decode(bytes);
    } catch (error) {
        return null;
    }
}

function encodeWindows1252(value) {
    const bytes = [];

    for (const character of value) {
        const codePoint = character.codePointAt(0);

        if (codePoint <= 0xff) {
            bytes.push(codePoint);
            continue;
        }

        const mappedValue = WINDOWS_1252_CODEPOINTS[codePoint];
        if (mappedValue === undefined) {
            return null;
        }

        bytes.push(mappedValue);
    }

    return Uint8Array.from(bytes);
}

function getMojibakeScore(value) {
    if (typeof value !== "string" || !value) {
        return 0;
    }

    return countOccurrences(value, "Ã")
        + countOccurrences(value, "Â")
        + countOccurrences(value, "Ä")
        + countOccurrences(value, "Æ")
        + countOccurrences(value, "�")
        + countOccurrences(value, "á»") * 2
        + countOccurrences(value, "áº") * 2
        + countOccurrences(value, "â€") * 2
        + countOccurrences(value, "â€¢") * 2;
}

function countOccurrences(value, fragment) {
    let count = 0;
    let index = 0;

    while ((index = value.indexOf(fragment, index)) !== -1) {
        count += 1;
        index += fragment.length;
    }

    return count;
}

const WINDOWS_1252_CODEPOINTS = {
    0x20ac: 0x80,
    0x201a: 0x82,
    0x0192: 0x83,
    0x201e: 0x84,
    0x2026: 0x85,
    0x2020: 0x86,
    0x2021: 0x87,
    0x02c6: 0x88,
    0x2030: 0x89,
    0x0160: 0x8a,
    0x2039: 0x8b,
    0x0152: 0x8c,
    0x017d: 0x8e,
    0x2018: 0x91,
    0x2019: 0x92,
    0x201c: 0x93,
    0x201d: 0x94,
    0x2022: 0x95,
    0x2013: 0x96,
    0x2014: 0x97,
    0x02dc: 0x98,
    0x2122: 0x99,
    0x0161: 0x9a,
    0x203a: 0x9b,
    0x0153: 0x9c,
    0x017e: 0x9e,
    0x0178: 0x9f
};
