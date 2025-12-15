console.log("Chathub.js loaded");

// ==========================================================
// 1. KONFIGURACJA I MAPOWANIE (Liczby <-> Emoji)
// ==========================================================
const REACTION_MAP = {
    "1": "👍",
    "2": "❤️",
    "3": "😂",
    "4": "🔥",
    "5": "😎"
};

// ==========================================================
// 2. POŁĄCZENIE Z SIGNALR
// ==========================================================
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chathub")
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();

// ==========================================================
// 3. ODBIERANIE DANYCH Z SERWERA
// ==========================================================

// --- A. Odbieranie nowej wiadomości ---
connection.on("ReceiveMessage", (id, username, message, date) => {
    // Generujemy opcje wyboru reakcji
    let reactionPickerHtml = '';
    for (const [key, emoji] of Object.entries(REACTION_MAP)) {
        reactionPickerHtml += `<span class="reaction-option" data-reaction-key="${key}">${emoji}</span>`;
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
                
                <div class="message-reactions"></div>
                
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

    document.getElementById("messagesList").innerHTML += html;
});

// --- B. Aktualizacja reakcji ---
connection.on("UpdateReaction", (messageId, reactionKey, newCount, userIds) => {
    const messageRow = document.querySelector(`.message-row[data-message-id='${messageId}']`);
    if (!messageRow) return;

    const reactionsContainer = messageRow.querySelector('.message-reactions');
    const emojiSymbol = REACTION_MAP[reactionKey];

    let existingBadge = reactionsContainer.querySelector(`.reaction[data-reaction-key='${reactionKey}']`);

    if (newCount > 0) {
        if (existingBadge) {
            // Aktualizujemy licznik
            existingBadge.querySelector('.count').innerText = newCount;
            existingBadge.classList.add('pulse-anim');
            setTimeout(() => existingBadge.classList.remove('pulse-anim'), 300);
        } else {
            // Tworzymy nową plakietkę
            const newBadge = document.createElement('div');
            newBadge.className = 'reaction';
            newBadge.setAttribute('data-reaction-key', reactionKey);
            newBadge.innerHTML = `${emojiSymbol} <span class="count">${newCount}</span>`;
            reactionsContainer.appendChild(newBadge);
        }
    } else {
        // Jeśli licznik spadł do 0, usuwamy plakietkę
        if (existingBadge) {
            existingBadge.remove();
        }
    }
});

// --- C. Edycja i usuwanie ---
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

// ==========================================================
// 4. START POŁĄCZENIA
// ==========================================================
async function start() {
    try {
        await connection.start();
        console.log("✅ Połączono z SignalR");
    } catch (err) {
        console.error("❌ Błąd połączenia:", err);
        setTimeout(start, 5000);
    }
}

// ==========================================================
// 5. OBSŁUGA ZDARZEŃ (Po załadowaniu DOM)
// ==========================================================
document.addEventListener('DOMContentLoaded', () => {
    start();

    // --- Wysyłanie nowej wiadomości ---
    const sendBtn = document.getElementById("sendBtn");
    if (sendBtn) {
        sendBtn.addEventListener("click", () => {
            const channelId = document.getElementById("channelId").value;
            const userProfileId = document.getElementById("UserProfileId").value;
            const messageInput = document.getElementById("messageInput");

            if (messageInput.value.trim()) {
                connection.invoke("SendMessage",
                    parseInt(channelId),
                    parseInt(userProfileId),
                    messageInput.value
                ).catch(err => console.error(err));

                messageInput.value = "";
            }
        });
    }

    // --- Globalny Listener (Delegacja zdarzeń) ---
    document.body.addEventListener('click', async (e) => {

        // 1. Otwieranie/Zamykanie Pickera Reakcji
        if (e.target.closest('.add-reaction-btn')) {
            const btn = e.target.closest('.add-reaction-btn');
            const picker = btn.parentElement.querySelector('.reaction-picker');

            // Zamknij inne pickery
            document.querySelectorAll('.reaction-picker').forEach(p => {
                if (p !== picker) p.style.display = 'none';
            });

            if (picker) {
                picker.style.display = (picker.style.display === 'none') ? 'flex' : 'none';
            }
            return;
        }

        // 2. WYBÓR REAKCJI
        if (e.target.classList.contains('reaction-option')) {
            const reactionKey = e.target.getAttribute('data-reaction-key'); // "1", "2", "3", etc.
            const picker = e.target.closest('.reaction-picker');
            const messageRow = picker.closest('.message-row');
            const messageId = parseInt(messageRow.getAttribute('data-message-id'));
            const userIdInput = document.getElementById('UserProfileId');

            if (!userIdInput) {
                console.error("Brak UserProfileId");
                return;
            }

            const userId = parseInt(userIdInput.value);
            picker.style.display = 'none';

            try {
                // Wysyłamy reactionKey jako string ("1", "2", etc.)
                await connection.invoke("ToggleReaction", messageId, reactionKey, userId);
            } catch (err) {
                console.error("Błąd wysyłania reakcji:", err);
            }
            return;
        }

        // 3. Edycja Wiadomości
        if (e.target.classList.contains("edit-message-btn")) {
            const row = e.target.closest(".message-row");
            const textEl = row.querySelector(".message-text");
            if (!textEl) return;

            const oldText = textEl.innerText;
            const input = document.createElement("input");
            input.type = "text";
            input.value = oldText;
            input.className = "edit-input";

            input.addEventListener("blur", () => {
                if (input.value.trim() !== oldText && input.value.trim() !== "") {
                    connection.invoke("EditMessage", parseInt(row.dataset.messageId), input.value)
                        .catch(err => console.error(err));
                } else {
                    const div = document.createElement("div");
                    div.className = "message-text";
                    div.textContent = oldText;
                    input.replaceWith(div);
                }
            });

            input.addEventListener("keydown", (ev) => {
                if (ev.key === "Enter") input.blur();
                if (ev.key === "Escape") {
                    input.value = oldText;
                    input.blur();
                }
            });

            textEl.replaceWith(input);
            input.focus();
            return;
        }

        // 4. Usuwanie Wiadomości
        if (e.target.classList.contains("delete-message-btn")) {
            if (confirm("Czy na pewno chcesz usunąć wiadomość?")) {
                const row = e.target.closest(".message-row");
                connection.invoke("DeleteMessage", parseInt(row.dataset.messageId))
                    .catch(err => console.error(err));
            }
            return;
        }

        // 5. Kliknięcie gdziekolwiek indziej - zamyka wszystkie pickery
        if (!e.target.closest('.reaction-picker') && !e.target.closest('.add-reaction-btn')) {
            document.querySelectorAll('.reaction-picker').forEach(p => {
                p.style.display = 'none';
            });
        }
    });
});