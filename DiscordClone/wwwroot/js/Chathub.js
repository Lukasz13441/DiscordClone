console.log("Chathub.js loaded");

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chathub")
    .withAutomaticReconnect()
    .build();

// ===============================
//  GLOBALNE ZMIENNE
// ===============================
const userReactions = {}; // Śledzi reakcje BIEŻĄCEGO użytkownika (messageId: {emoji: true})

// Mapowanie reakcji: klucz -> emoji
const REACTION_MAP = {
    '1': '👍',
    '2': '❤️',
    '3': '😂',
    '4': '🔥',
    '5': '😎'
};

// Odwrotne mapowanie: emoji -> klucz
const EMOJI_TO_KEY = {
    '👍': '1',
    '❤️': '2',
    '😂': '3',
    '🔥': '4',
    '😎': '5'
};

// ===============================
//  ODBIERANIE WIADOMOŚCI
// ===============================
connection.on("ReceiveMessage", (id, username, message, date) => {
    const html = `
        <div class="message-row" data-message-id="${id}">
            <img src="" class="avatar"/>
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
                    <span class="reaction-option" data-reaction-key="1">👍</span>
                    <span class="reaction-option" data-reaction-key="2">❤️</span>
                    <span class="reaction-option" data-reaction-key="3">😂</span>
                    <span class="reaction-option" data-reaction-key="4">🔥</span>
                    <span class="reaction-option" data-reaction-key="5">😎</span>
                </div>
            </div>
        </div>`;

    document.getElementById("messagesList").innerHTML += html;
});

// Start połączenia
connection.start()
    .then(() => console.log("✅ Połączono z SignalR"))
    .catch(err => console.error("❌ Błąd:", err));

// ===============================
//  WYSYŁANIE WIADOMOŚCI
// ===============================
document.getElementById("sendBtn").addEventListener("click", () => {
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

const messageInput = document.getElementById("messageInput");

messageInput.addEventListener("keydown", (e) => {
    // Enter without Shift
    if (e.key === "Enter" && !e.shiftKey) {
        e.preventDefault(); // stop newline / form submit

        const channelId = document.getElementById("channelId").value;
        const userProfileId = document.getElementById("UserProfileId").value;

        if (messageInput.value.trim()) {
            connection.invoke(
                "SendMessage",
                parseInt(channelId),
                parseInt(userProfileId),
                messageInput.value
            ).catch(err => console.error(err));

            messageInput.value = "";
        }
    }
});



// ===============================
//  EDYCJA WIADOMOŚCI
// ===============================
document.addEventListener("click", e => {
    if (!e.target.classList.contains("edit-message-btn")) return;

    const row = e.target.closest(".message-row");
    const textEl = row.querySelector(".message-text");
    if (!textEl) return;

    const oldText = textEl.textContent;

    // Zamień tekst na pole input
    const input = document.createElement("input");
    input.type = "text";
    input.value = oldText;
    input.className = "edit-input";
    textEl.replaceWith(input);
    input.focus();

    // Zapisz po utracie focusu / Enter
    input.addEventListener("blur", () => {
        if (input.value.trim() !== oldText) {
            connection.invoke("EditMessage", parseInt(row.dataset.messageId), input.value)
                .catch(err => console.error(err));
        } else {
            input.outerHTML = `<div class="message-text">${oldText}</div>`;
        }
    });

    input.addEventListener("keydown", (e) => {
        if (e.key === "Enter") input.blur();
        if (e.key === "Escape") {
            input.value = oldText;
            input.blur();
        }
    });
});

connection.on("MessageEdited", (id, newText) => {
    const row = document.querySelector(`[data-message-id="${id}"]`);
    const input = row?.querySelector("input.edit-input");
    if (input) {
        input.outerHTML = `<div class="message-text">${newText}</div>`;
    }
});

// ===============================
//  USUWANIE WIADOMOŚCI
// ===============================
document.addEventListener("click", e => {
    if (e.target.classList.contains("delete-message-btn")) {
        if (confirm("Czy na pewno chcesz usunąć wiadomość?")) {
            const row = e.target.closest(".message-row");
            connection.invoke("DeleteMessage", parseInt(row.dataset.messageId))
                .catch(err => console.error(err));
        }
    }
});

connection.on("MessageDeleted", id => {
    document.querySelector(`[data-message-id="${id}"]`)?.remove();
});

// ===============================
//  REAKCJE
// ===============================

// ---------------------------------------------------------------
// ODBIERANIE REAKCJI (Serwer -> Klient)
// ---------------------------------------------------------------
connection.on("UpdateReaction", (messageId, reactionKey, newCount) => {
    // Znajdź wiersz wiadomości po ID
    const messageRow = document.querySelector(`.message-row[data-message-id='${messageId}']`);
    if (!messageRow) return;

    const reactionsContainer = messageRow.querySelector('.message-reactions');

    // Pobierz emoji odpowiadające kluczowi
    const emoji = REACTION_MAP[reactionKey] || reactionKey;

    // Szukamy, czy taka reakcja już jest wyrenderowana
    let existingBadge = reactionsContainer.querySelector(`.reaction[data-reaction-key='${reactionKey}']`);

    if (newCount > 0) {
        // SCENARIUSZ A: Ktoś dodał reakcję (lub jest ich więcej)
        if (existingBadge) {
            // Tylko aktualizujemy liczbę
            existingBadge.querySelector('.count').innerText = newCount;

            // Efekt wizualny (pulsowanie)
            existingBadge.classList.add('pulse-anim');
            setTimeout(() => existingBadge.classList.remove('pulse-anim'), 300);
        } else {
            // Tworzymy nową "plakietkę" z reakcją
            const newBadge = document.createElement('div');
            newBadge.className = 'reaction';
            newBadge.setAttribute('data-reaction-key', reactionKey);
            // HTML wewnątrz: emotka + licznik
            newBadge.innerHTML = `${emoji} <span class="count">${newCount}</span>`;

            reactionsContainer.appendChild(newBadge);
        }
    } else {
        // SCENARIUSZ B: Licznik spadł do 0 -> USUŃ element
        if (existingBadge) {
            existingBadge.remove();
        }
    }
});

// ---------------------------------------------------------------
// OBSŁUGA KLIKNIĘĆ (UI)
// ---------------------------------------------------------------
document.body.addEventListener('click', async (e) => {

    // A. Kliknięcie w przycisk "PLUS" (Otwórz/Zamknij menu)
    if (e.target.closest('.add-reaction-btn')) {
        const btn = e.target.closest('.add-reaction-btn');
        // Szukamy pickera w pobliżu przycisku (wewnątrz tego samego rodzica message-content)
        const picker = btn.parentElement.querySelector('.reaction-picker');

        if (picker) {
            // Zamknij wszystkie inne otwarte pickery na stronie
            document.querySelectorAll('.reaction-picker').forEach(p => {
                if (p !== picker) p.style.display = 'none';
            });

            // Przełącz widoczność (Toggle)
            const isHidden = picker.style.display === 'none' || picker.style.display === '';
            picker.style.display = isHidden ? 'flex' : 'none';
        }
        return;
    }

    // B. Kliknięcie w konkretną emotkę w menu (Wyślij do serwera)
    if (e.target.classList.contains('reaction-option')) {
        const reactionKey = e.target.getAttribute('data-reaction-key'); // np. "1", "2"

        // Debug log to verify we're sending the key, not emoji
        console.log('Sending reaction key:', reactionKey);

        const picker = e.target.closest('.reaction-picker');

        // Pobieramy ID wiadomości
        const messageRow = picker.closest('.message-row');
        const messageId = messageRow.getAttribute('data-message-id');

        // Pobieramy ID aktualnie zalogowanego usera
        const userIdInput = document.getElementById('UserProfileId');
        if (!userIdInput) {
            console.error("Brak inputa #UserProfileId!");
            return;
        }
        const userId = userIdInput.value;

        // Ukryj picker natychmiast po wyborze
        picker.style.display = 'none';

        // Wyślij sygnał do serwera z kluczem reakcji (string)
        try {
            // Nazwa "ToggleReaction" musi pasować do metody w C#
            await connection.invoke("ToggleReaction", parseInt(messageId), reactionKey, userId);
        } catch (err) {
            console.error("Błąd wysyłania reakcji: ", err);
        }
        return;
    }

    // C. Kliknięcie gdziekolwiek indziej (Zamknij wszystkie pickery)
    if (!e.target.closest('.reaction-picker') && !e.target.closest('.add-reaction-btn')) {
        document.querySelectorAll('.reaction-picker').forEach(p => {
            p.style.display = 'none';
        });
    }
});