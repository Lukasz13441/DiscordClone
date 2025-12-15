document.addEventListener('DOMContentLoaded', () => {
console.log("Chathub.js loaded");

    // ==========================================================
    // 1. KONFIGURACJA I MAPOWANIE (Liczby <-> Emoji)
    // ==========================================================

    // To pozwala serwerowi operować na liczbach (Enum), a klientowi wyświetlać grafiki
    const REACTION_MAP = {
        1: "👍",
        2: "❤️",
        3: "😂",
        4: "🔥",
        5: "😎"
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
        // Generujemy opcje wyboru reakcji używając naszego mapowania (ID zamiast samego emoji w data atrybucie)
        let reactionPickerHtml = '';
        for (const [key, emoji] of Object.entries(REACTION_MAP)) {
            reactionPickerHtml += `<span class="reaction-option" data-reaction-id="${key}">${emoji}</span>`;
        }

    const html = `
        <div class="message-row" data-message-id="${id}">
                <img src="/img/default-avatar.png" class="avatar" alt="Avatar"/> <!-- Ustaw domyślny avatar -->
            <div class="message-content">
                <div class="message-header">
                    <span class="username">${username}</span>
                    <span class="timestamp">${new Date(date).toLocaleTimeString()}</span>
                </div>
                <div class="message-text">${message}</div>
                    
                    <!-- Kontener na wyświetlanie dodanych reakcji -->
                <div class="message-reactions"></div>
                    
                    <!-- Przycisk dodawania reakcji -->
                <div class="add-reaction-btn">➕</div>
                    
                    <!-- Przyciski akcji (edycja/usuwanie) -->
                <div class="message-actions">
                    <span class="action-button edit-message-btn">✏️</span>
                    <span class="action-button delete-message-btn">🗑️</span>
                </div>

                    <!-- Menu wyboru reakcji (generowane dynamicznie) -->
                <div class="reaction-picker" style="display:none;">
                        ${reactionPickerHtml}
                </div>
            </div>
        </div>`;

    document.getElementById("messagesList").innerHTML += html;
});

    // --- B. Aktualizacja reakcji (Odbieramy ID reakcji i licznik) ---
    connection.on("UpdateReaction", (messageId, reactionId, newCount) => {
        const messageRow = document.querySelector(`.message-row[data-message-id='${messageId}']`);
        if (!messageRow) return;

        const reactionsContainer = messageRow.querySelector('.message-reactions');

        // Pobieramy wygląd emotki na podstawie ID
        const emojiSymbol = REACTION_MAP[reactionId];

        // Szukamy, czy ta konkretna reakcja (po ID) już istnieje w wiadomości
        let existingBadge = reactionsContainer.querySelector(`.reaction[data-reaction-id='${reactionId}']`);

        if (newCount > 0) {
            if (existingBadge) {
                // Aktualizujemy licznik
                existingBadge.querySelector('.count').innerText = newCount;
                // Animacja pulsowania
                existingBadge.classList.add('pulse-anim');
                setTimeout(() => existingBadge.classList.remove('pulse-anim'), 300);
        } else {
                // Tworzymy nową plakietkę
                const newBadge = document.createElement('div');
                newBadge.className = 'reaction';
                newBadge.setAttribute('data-reaction-id', reactionId); // Przechowujemy ID
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
});

    // --- C. Edycja i usuwanie (potwierdzenia z serwera) ---
connection.on("MessageEdited", (id, newText) => {
    const row = document.querySelector(`[data-message-id="${id}"]`);
        // Jeśli edycja jest aktywna (input), nadpisz input, jeśli nie - nadpisz div
        const textContainer = row?.querySelector(".message-text") || row?.querySelector("input.edit-input");

        if (textContainer) {
            // Przywracamy div z nowym tekstem
            const newDiv = document.createElement("div");
            newDiv.className = "message-text";
            newDiv.textContent = newText;
            textContainer.replaceWith(newDiv);
        }
    }
});

connection.on("MessageDeleted", id => {
        const row = document.querySelector(`[data-message-id="${id}"]`);
        if (row) {
            row.style.opacity = '0';
            setTimeout(() => row.remove(), 300); // Mała animacja usuwania
        }
});
//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
document.addEventListener('DOMContentLoaded', () => {

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
    start();

    // ==========================================================
    // 5. OBSŁUGA ZDARZEŃ (Wysyłanie, Kliknięcia)
    // ==========================================================

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
            // Szukamy pickera w pobliżu przycisku (wewnątrz tego samego rodzica message-content)
            const picker = btn.parentElement.querySelector('.reaction-picker');

            // Zamknij inne
                document.querySelectorAll('.reaction-picker').forEach(p => {
                    if (p !== picker) p.style.display = 'none';
                });

            if (picker) {
                picker.style.display = (picker.style.display === 'none') ? 'flex' : 'none';
            }
            return;
        }
        return;
    }

    // B. Kliknięcie w konkretną emotkę w menu (Wyślij do serwera)
    if (e.target.classList.contains('reaction-option')) {
        const reactionKey = e.target.getAttribute('data-reaction-key'); // np. "1", "2"

        // 2. WYBÓR REAKCJI (Wysyłanie ID do serwera)
        if (e.target.classList.contains('reaction-option')) {
            const emoji = e.target.innerHTML; // np. 👍
            const picker = e.target.closest('.reaction-picker');

            // Pobieramy ID wiadomości
            const messageRow = picker.closest('.message-row');
            const messageId = messageRow.getAttribute('data-message-id');

            const messageId = parseInt(messageRow.getAttribute('data-message-id'));
            const reactionId = parseInt(e.target.getAttribute('data-reaction-id')); // Pobieramy np. 1, 2, 3...
            const userIdInput = document.getElementById('UserProfileId');

            if (!userIdInput) return console.error("Brak UserProfileId");
            // Uwaga: zakładam, że userId w bazie to int, jeśli string (Guid) usuń parseInt
            const userId = parseInt(userIdInput.value);

            picker.style.display = 'none'; // Schowaj menu

            try {
                // Wysyłamy ID (int) zamiast Emoji (string)
                await connection.invoke("ToggleReaction", messageId, reactionId, userId);
            } catch (err) {
                console.error("Błąd wysyłania reakcji:", err);
            }
                return;
            }

        // 3. Edycja Wiadomości - Kliknięcie "Ołówek"
        if (e.target.classList.contains("edit-message-btn")) {
            const row = e.target.closest(".message-row");
            const textEl = row.querySelector(".message-text");
            if (!textEl) return;

            const oldText = textEl.innerText; // innerText jest bezpieczniejszy dla inputa

            const input = document.createElement("input");
            input.type = "text";
            input.value = oldText;
            input.className = "edit-input";

            // Zdarzenia dla inputa edycji
            input.addEventListener("blur", () => {
                if (input.value.trim() !== oldText && input.value.trim() !== "") {
                    connection.invoke("EditMessage", parseInt(row.dataset.messageId), input.value)
                        .catch(err => console.error(err));
                } else {
                    // Anulowano lub puste - przywróć tekst
                    const div = document.createElement("div");
                    div.className = "message-text";
                    div.textContent = oldText;
                    input.replaceWith(div);
                }
            });

            input.addEventListener("keydown", (ev) => {
                if (ev.key === "Enter") input.blur();
                if (ev.key === "Escape") {
                    input.value = oldText; // Przywróć wartość
                    input.blur(); // Wywołaj blur, który przywróci diva
                }
            });

            textEl.replaceWith(input);
            input.focus();
            return;
        }

        // 4. Usuwanie Wiadomości - Kliknięcie "Kosz"
        if (e.target.classList.contains("delete-message-btn")) {
            if (confirm("Czy na pewno chcesz usunąć wiadomość?")) {
                const row = e.target.closest(".message-row");
                connection.invoke("DeleteMessage", parseInt(row.dataset.messageId))
                    .catch(err => console.error(err));
            }
            return;
        }
        const userId = userIdInput.value;

        // 5. Kliknięcie w tło - zamyka wszystkie pickery
        if (!e.target.closest('.reaction-picker') && !e.target.closest('.add-reaction-btn')) {
            document.querySelectorAll('.reaction-picker').forEach(p => {
                p.style.display = 'none';
            });
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