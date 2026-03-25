(() => {
  const chatRoot = document.querySelector("[data-ai-chat]");
  if (!chatRoot) {
    return;
  }

  const form = chatRoot.querySelector("[data-chat-form]");
  const input = chatRoot.querySelector("[data-chat-input]");
  const thread = chatRoot.querySelector("[data-chat-thread]");
  const suggestions = chatRoot.querySelector("[data-chat-suggestions]");
  const quickPrompts = document.querySelectorAll("[data-suggestion]");
  const endpoint = chatRoot.getAttribute("data-endpoint");
  const submitButton = form?.querySelector("button[type='submit']");

  const renderMessage = (role, text) => {
    const article = document.createElement("article");
    article.className = `chat-message ${role === "user" ? "chat-message-user" : "chat-message-ai"}`;

    const badge = document.createElement("span");
    badge.className = "chat-role";
    badge.textContent = role === "user" ? "Ban" : "AI";

    const content = document.createElement("p");
    content.textContent = text;

    article.appendChild(badge);
    article.appendChild(content);
    thread?.appendChild(article);

    if (thread) {
      thread.scrollTop = thread.scrollHeight;
    }
  };

  const renderSuggestions = (items) => {
    if (!suggestions) {
      return;
    }

    suggestions.innerHTML = "";

    items.forEach((item) => {
      const button = document.createElement("button");
      button.type = "button";
      button.className = "assistant-pill";
      button.textContent = item;
      button.addEventListener("click", () => {
        if (input) {
          input.value = item;
          input.focus();
        }
      });
      suggestions.appendChild(button);
    });
  };

  const setBusy = (busy) => {
    if (input) {
      input.disabled = busy;
    }

    if (submitButton) {
      submitButton.disabled = busy;
    }
  };

  const submitMessage = async (message) => {
    if (!message || !endpoint) {
      return;
    }

    renderMessage("user", message);
    setBusy(true);

    try {
      const response = await fetch(endpoint, {
        method: "POST",
        headers: {
          "Content-Type": "application/json"
        },
        body: JSON.stringify(message)
      });

      if (!response.ok) {
        throw new Error("Assistant request failed.");
      }

      const payload = await response.json();
      renderMessage("ai", payload.reply || "Toi chua co cau tra loi phu hop.");
      renderSuggestions(Array.isArray(payload.suggestions) ? payload.suggestions : []);
    } catch (error) {
      renderMessage("ai", "He thong chat dang tam thoi gian gian doan. Ban vui long thu lai sau.");
    } finally {
      setBusy(false);

      if (input) {
        input.value = "";
        input.focus();
      }
    }
  };

  form?.addEventListener("submit", async (event) => {
    event.preventDefault();

    if (!input) {
      return;
    }

    await submitMessage(input.value.trim());
  });

  quickPrompts.forEach((button) => {
    button.addEventListener("click", async () => {
      const suggestion = button.getAttribute("data-suggestion");
      if (!suggestion) {
        return;
      }

      if (input) {
        input.value = suggestion;
      }

      await submitMessage(suggestion);
    });
  });

  renderSuggestions([
    "Showroom hien co bao nhieu xe?",
    "Hang nao dang co nhieu xe nhat?",
    "Xe nao dang ban chay?"
  ]);
})();
