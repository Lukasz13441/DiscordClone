// Connect to VoiceHub
const voiceConnection = new signalR.HubConnectionBuilder()
    .withUrl("/voiceHub")
    .build();

// Start connection
voiceConnection.start().catch(err => console.error(err));

// Join voice channel
function joinVoiceChannel(voiceChannelId, userId) {
    voiceConnection.invoke("JoinVoiceChannel", voiceChannelId, userId)
        .catch(err => console.error(err));
}

// Leave voice channel
function leaveVoiceChannel(userId) {
    voiceConnection.invoke("LeaveVoiceChannel", userId)
        .catch(err => console.error(err));
}

// Listen for users joining
voiceConnection.on("UserJoinedVoice", (voiceChannelId, users) => {
    updateVoiceChannelUI(voiceChannelId, users);
});

// Listen for users leaving
voiceConnection.on("UserLeftVoice", (voiceChannelId, users) => {
    updateVoiceChannelUI(voiceChannelId, users);
});

// Update UI to show users in voice channel
function updateVoiceChannelUI(voiceChannelId, users) {
    const container = document.querySelector(`#voice-channel-${voiceChannelId} .users`);
    if (!container) return;

    container.innerHTML = '';
    users.forEach(user => {
        const userDiv = document.createElement('div');
        userDiv.className = 'voice-user';
        userDiv.innerHTML = `
            <img src="${user.avatarURL || '/images/default-avatar.png'}" alt="${user.username}">
            <span>${user.username}</span>
        `;
        container.appendChild(userDiv);
    });
}