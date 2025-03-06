const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

// Start SignalR connection
connection.start().then(() => {
    document.getElementById("sendButton").disabled = false;
    loadMessages(); // Load previous messages on page load
}).catch(err => console.error(err.toString()));

// Function to load past messages from the database
function loadMessages() {
    fetch("/Chat/GetMessages")
        .then(response => response.json())
        .then(messages => {
            messages.forEach(msg => addMessageToChat(msg.userName, msg.message, msg.profilePhoto));
        })
        .catch(error => console.error("Error loading messages:", error));
}

// Event listener for send button
document.getElementById("sendButton").addEventListener("click", function (event) {
    event.preventDefault();

    const userInput = document.getElementById("userInput");
    const user = userInput ? userInput.value : "Anonymous";
    const messageInput = document.getElementById("messageInput");
    const message = messageInput ? messageInput.value.trim() : "";

    if (message !== "") {
        const avatarElement = document.getElementById("userAvatar");
        const avatarSrc = avatarElement ? avatarElement.src : "/img/default.jpg"; // Default avatar

        connection.invoke("SendMessage", user, message, avatarSrc)
            .then(() => {
                messageInput.value = ""; // Clear message input
            })
            .catch(err => console.error(err.toString()));
    }
});

// Function to add a message to the chat UI
function addMessageToChat(user, message, profilePhoto) {
    const isOwnMessage = document.getElementById("userInput").value === user;

    const li = document.createElement("li");
    li.classList.add("message", isOwnMessage ? "own-message" : "other-message");

    const avatar = document.createElement("img");
    avatar.src = profilePhoto ? profilePhoto : "/img/default.jpg"; // Handle missing avatar
    avatar.classList.add("avatar");

    const textDiv = document.createElement("div");
    textDiv.classList.add("text");
    textDiv.innerHTML = `<strong>${user}:</strong> ${message}`;

    li.appendChild(avatar);
    li.appendChild(textDiv);
    document.getElementById("messagesList").appendChild(li);

    // Auto-scroll to the bottom
    document.getElementById("messagesList").scrollTop = document.getElementById("messagesList").scrollHeight;
}

// Listen for new messages from SignalR
connection.on("ReceiveMessage", function (user, message, profilePhoto) {
    addMessageToChat(user, message, profilePhoto);
});
