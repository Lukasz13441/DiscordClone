document.addEventListener('DOMContentLoaded', () => {
    console.log("✅ Chat script loaded");

    // ==========================================================
    // 0. POBIERANIE DANYCH Z DOM
    // ==========================================================
    const channelIdInput = document.getElementById("channelId");
    const userProfileIdInput = document.getElementById("UserProfileId");

    // Jeśli nie ma channelId, nie możemy się połączyć z odpowiednim pokojem
    if (!channelIdInput) {
        console.warn("⚠️ Brak elementu #channelId - SignalR nie zostanie uruchomiony.");
        return;
    }

    const currentChannelId = channelIdInput.value;
    const currentUserId = userProfileIdInput ? userProfileIdInput.value : 0;

    const REACTION_MAP = {
        "1": "👍",
        "2": "❤️",
        "3": "😂",
        "4": "🔥",
        "5": "😎"
    };

    // ==========================================================
    // FUNKCJA AUTO-SCROLL
    // ==========================================================
    function scrollToBottom() {
        const messagesList = document.getElementById("messagesList");
        if (messagesList) {
            messagesList.scrollTop = messagesList.scrollHeight;
        }
    }

    // ==========================================================
    // 1. POŁĄCZENIE (Jedno wspólne dla czatu i obecności)
    // ==========================================================

    // Przekazujemy channelId w URL, aby Hub wiedział do jakiej grupy nas dodać (OnConnectedAsync)

    let pingTimer = null;

    const connection = new signalR.HubConnectionBuilder()
        .withUrl(`/chathub?channelId=${currentChannelId}`)
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)
        .build();
    // ==========================================================
    // 2. NASŁUCHIWANIE EVENTÓW (Odbieranie danych)
    // ==========================================================

    // A. Wiadomości
    connection.on("ReceiveMessage", (id, username, message, date, reactions) => {
        console.log("📨 Nowa wiadomość:", id);

        // Generowanie HTML dla pickera reakcji
        let reactionPickerHtml = '';
        for (const [key, emoji] of Object.entries(REACTION_MAP)) {
            reactionPickerHtml += `<span class="reaction-option" data-reaction-key="${key}">${emoji}</span>`;
        }

        // Generowanie HTML dla istniejących reakcji
        let existingReactionsHtml = '';
        if (reactions && reactions.length > 0) {
            for (const reaction of reactions) {
                const reactionKey = reaction.reactionKey;
                const emojiSymbol = REACTION_MAP[reactionKey] || '?';
                const count = reaction.count;
                existingReactionsHtml += `<div class="reaction" data-reaction-key="${reactionKey}">${emojiSymbol} <span class="count">${count}</span></div>`;
            }
        }

        const html = `
            <div class="message-row" data-message-id="${id}">
                <img src="/img/default-avatar.png" class="avatar" alt="Avatar"/>
                <div class="message-content">
                    <div class="message-header">
                        <span class="username">${username}</span>
                        <span class="timestamp">${new Date(date).toLocaleTimeString()}</span>
                    </div>
                    <div class="message-text">${message}</div>
                    <div class="message-reactions">${existingReactionsHtml}</div>
                    <div class="add-reaction-btn">➕</div>
                    <div class="message-actions">
                        <span class="action-button edit-message-btn">✏️</span>
                        <span class="action-button delete-message-btn">🗑️</span>
                    </div>
                    <div class="reaction-picker" style="display:none;">
                        ${reactionPickerHtml}
                    </div>
                </div>
            </div>`;

        const list = document.getElementById("messagesList");
        if (list) {
            list.innerHTML += html;
            // 🚀 AUTO-SCROLL PO DODANIU WIADOMOŚCI
            scrollToBottom();
        }
    });

    // B. Reakcje
    connection.on("UpdateReaction", (messageId, reactionKey, newCount) => {
        const messageRow = document.querySelector(`.message-row[data-message-id='${messageId}']`);
        if (!messageRow) return;

        const reactionsContainer = messageRow.querySelector('.message-reactions');
        const emojiSymbol = REACTION_MAP[reactionKey];
        let existingBadge = reactionsContainer.querySelector(`.reaction[data-reaction-key='${reactionKey}']`);

        if (newCount > 0) {
            if (existingBadge) {
                existingBadge.querySelector('.count').innerText = newCount;
                existingBadge.classList.add('pulse-anim');
                setTimeout(() => existingBadge.classList.remove('pulse-anim'), 300);
            } else {
                const newBadge = document.createElement('div');
                newBadge.className = 'reaction';
                newBadge.setAttribute('data-reaction-key', reactionKey);
                newBadge.innerHTML = `${emojiSymbol} <span class="count">${newCount}</span>`;
                reactionsContainer.appendChild(newBadge);
            }
        } else {
            if (existingBadge) existingBadge.remove();
        }
    });

    // C. Edycja i Usuwanie
    connection.on("MessageEdited", (id, newText) => {
        const row = document.querySelector(`[data-message-id="${id}"]`);
        const textContainer = row?.querySelector(".message-text") || row?.querySelector("input.edit-input");

        if (textContainer) {
            const newDiv = document.createElement("div");
            newDiv.className = "message-text";
            newDiv.textContent = newText;
            textContainer.replaceWith(newDiv);
        }
    });

    connection.on("MessageDeleted", id => {
        const row = document.querySelector(`[data-message-id="${id}"]`);
        if (row) {
            row.style.opacity = '0';
            setTimeout(() => row.remove(), 300);
        }
    });

    // D. Powiadomienia (Notification / Status)
    connection.on("ReceiveNotification", (message) => {
        console.log("🔔 Powiadomienie systemowe:", message);
        // Opcjonalnie wyświetl na ekranie
        // alert(message); 
    });

    connection.on("Error", (errorMessage) => {
        console.error("❌ Błąd serwera:", errorMessage);
    });


    // ==========================================================
    // 3. START APLIKACJI
    // ==========================================================

    async function start() {
        try {
            await connection.start();
            console.log("✅ Połączono z SignalR");

            // 1. Pobieramy ID z inputów (zabezpieczenie przed nullem)
            const cidInput = document.getElementById("channelId");
            const uidInput = document.getElementById("UserProfileId");

            const channelId = cidInput ? parseInt(cidInput.value) : 0;
            const userId = uidInput ? parseInt(uidInput.value) : 0;

            // 2. Rejestracja w bazie (JoinChannel)
            if (channelId > 0 && userId > 0) {
                console.log(`📤 Rejestracja: User=${userId}, Channel=${channelId}`);

                // 👇 TU CZĘSTO JEST BŁĄD. Muszą być przecinki między każdym argumentem!
                await connection.invoke("JoinChannel", userId, channelId);

                // 🚀 AUTO-SCROLL PO ZAŁADOWANIU STRONY
                setTimeout(scrollToBottom, 300);

            } else {
                console.warn("⚠️ Brak UserID lub ChannelID - pomijam rejestrację w bazie.");
            }

        } catch (err) {
            console.error("❌ Błąd połączenia:", err);
            // Ponawianie próby za 5 sekund
            setTimeout(start, 5000);
        }
    }

    start();

    // Sprzątanie przy zamykaniu strony
    window.addEventListener('beforeunload', () => {
        if (pingTimer) clearInterval(pingTimer);
    });


    // ==========================================================
    // 4. OBSŁUGA UI (Wysyłanie, Kliknięcia)
    // ==========================================================

    const sendBtn = document.getElementById("sendBtn");
    if (sendBtn) {
        sendBtn.addEventListener("click", () => {
            const messageInput = document.getElementById("messageInput");

            if (messageInput && messageInput.value.trim()) {
                connection.invoke("SendMessage",
                    parseInt(currentChannelId),
                    parseInt(currentUserId),
                    messageInput.value
                ).catch(err => console.error("❌ Błąd wysyłania:", err));

                messageInput.value = "";
            }
        });
    }

    // Event Delegation
    document.body.addEventListener('click', async (e) => {

        // 1. Picker Reakcji
        if (e.target.closest('.add-reaction-btn')) {
            const picker = e.target.closest('.message-content')?.querySelector('.reaction-picker');
            document.querySelectorAll('.reaction-picker').forEach(p => { if (p !== picker) p.style.display = 'none'; });
            if (picker) picker.style.display = (picker.style.display !== 'none') ? 'none' : 'flex';
            e.stopPropagation();
            return;
        }

        // 2. Dodanie reakcji
        if (e.target.classList.contains('reaction-option')) {
            const row = e.target.closest('.message-row');
            if (row) {
                const msgId = parseInt(row.dataset.messageId);
                const rId = parseInt(e.target.dataset.reactionKey);

                e.target.closest('.reaction-picker').style.display = 'none';
                connection.invoke("ToggleReaction", msgId, rId, parseInt(currentUserId)).catch(console.error);
            }
            e.stopPropagation();
            return;
        }

        // 3. Edycja
        if (e.target.classList.contains("edit-message-btn")) {
            const row = e.target.closest(".message-row");
            const textEl = row?.querySelector(".message-text");
            if (textEl) {
                const oldText = textEl.innerText;
                const input = document.createElement("input");
                input.type = "text";
                input.className = "edit-input";
                input.value = oldText;

                const save = () => {
                    if (input.value.trim() !== oldText && input.value.trim() !== "") {
                        connection.invoke("EditMessage", parseInt(row.dataset.messageId), input.value).catch(console.error);
                    } else {
                        const div = document.createElement("div");
                        div.className = "message-text";
                        div.textContent = oldText;
                        input.replaceWith(div);
                    }
                };
                input.addEventListener("blur", save);
                input.addEventListener("keydown", (ev) => { if (ev.key === "Enter") input.blur(); });
                textEl.replaceWith(input);
                input.focus();
            }
            e.stopPropagation();
            return;
        }

        // 4. Usuwanie
        if (e.target.classList.contains("delete-message-btn")) {
            if (confirm("Usunąć wiadomość?")) {
                const row = e.target.closest(".message-row");
                connection.invoke("DeleteMessage", parseInt(row.dataset.messageId)).catch(console.error);
            }
            e.stopPropagation();
            return;
        }

        // Zamykanie przy kliknięciu w tło
        if (!e.target.closest('.reaction-picker') && !e.target.closest('.add-reaction-btn')) {
            document.querySelectorAll('.reaction-picker').forEach(p => p.style.display = 'none');
        }
    });
});