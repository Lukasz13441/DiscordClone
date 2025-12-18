// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener('DOMContentLoaded', () => {
    console.log("✅ Chat script loaded");
    const UserId = document.getElementById("UserId").value;
    const connection = new signalR.HubConnectionBuilder()
        .withUrl(`/chathub`)
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)
        .build();
     //3. Start Pingowania (aktualizacja LastSeen co 15s)
    //
    
    const statusElement = document.getElementById("ActivityStatus");

    connection.on("ActivityStatus", (message, status) => {
        console.log(`${message}`);
        
        if (status == 1) {
            statusElement.classList.remove("online");
            statusElement.classList.add("offline");
            return;
        }
        statusElement.classList.remove("offline");
        statusElement.classList.add("online");
    });

    connection.on("FriendActivityStatus", (message, status, id) => {
        console.log(`${message}`);
        let statusElement = document.getElementById(`ActivityStatus-${id}`);
        if (status == 0) {
            statusElement.classList.remove("offline");
            statusElement.classList.add("online");
        } else {
            statusElement.classList.remove("online");
            statusElement.classList.add("offline");
        }
        
    });
    let pingTimer = null;
    window.addEventListener("beforeunload", function () {
        connection.invoke("LeaveApp", UserId)
    });

    let ping = () => {
        if (connection.state === signalR.HubConnectionState.Connected) {
            connection.invoke("Ping", UserId).catch(err => console.error("Ping error:", err));
        }
    }

    async function start() {
        try {
            await connection.start();
            console.log("✅ Połączono z SignalR");
             pingTimer = setInterval(() => {
                ping();
              }, 5000);
        } catch (err) {
            console.error("❌ Błąd połączenia:", err);
            // Ponawianie próby za 5 sekund
            setTimeout(start, 15000);
        }
    }

    start();
    setTimeout(ping,500)
    
});