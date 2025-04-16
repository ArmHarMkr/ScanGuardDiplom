/*const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

// Start SignalR connection
connection.start().then(() => {
    document.getElementById("sendButton").disabled = false;
    loadMessages();
}).catch(err => console.error(err.toString()));

// Load past messages
function loadMessages() {
    fetch("/Chat/GetMessages")
        .then(response => response.json())
        .then(messages => {
            messages.forEach(msg => addMessageToChat(msg.userName, msg.message, msg.profilePhoto));
        })
        .catch(error => console.error("Error loading messages:", error));
}

// Send message on button click
document.getElementById("sendButton").addEventListener("click", function (event) {
    event.preventDefault();
    const userInput = document.getElementById("userInput");
    const user = userInput ? userInput.value : "Anonymous";
    const messageInput = document.getElementById("messageInput");
    const message = messageInput ? messageInput.value.trim() : "";
    if (message !== "") {
        connection.invoke("SendMessage", user, message, '@profilePhoto')
            .then(() => {
                messageInput.value = "";
            })
            .catch(err => console.error(err.toString()));
    }
});

// Send typing event
let typingTimeout;
document.getElementById("messageInput").addEventListener("input", function () {
    clearTimeout(typingTimeout);
    connection.invoke("SendTyping", document.getElementById("userInput").value)
        .catch(err => console.error(err.toString()));
    typingTimeout = setTimeout(() => {
        connection.invoke("StopTyping", document.getElementById("userInput").value)
            .catch(err => console.error(err.toString()));
    }, 2000);
});

// Add message to UI
function addMessageToChat(user, message, profilePhoto) {
    const isOwnMessage = document.getElementById("userInput").value === user;
    const li = document.createElement("li");
    li.classList.add("message-container", isOwnMessage ? "own-message" : "other-message");

    const header = document.createElement("div");
    header.classList.add("message-header");
    header.textContent = user;

    const messageDiv = document.createElement("div");
    messageDiv.classList.add("message");

    const avatar = document.createElement("img");
    avatar.src = profilePhoto || "/img/default.jpg";
    avatar.classList.add("avatar");

    const textDiv = document.createElement("div");
    textDiv.classList.add("text");
    textDiv.textContent = message;

    const timestamp = document.createElement("div");
    timestamp.classList.add("message-timestamp");
    const now = new Date();
    timestamp.textContent = `${now.getHours().toString().padStart(2, '0')}:${now.getMinutes().toString().padStart(2, '0')}`;
    if (isOwnMessage) {
        const status = document.createElement("span");
        status.classList.add("message-status");
        status.textContent = "✓✓";
        timestamp.appendChild(status);
    }

    if (!isOwnMessage) {
        messageDiv.appendChild(avatar);
    }
    messageDiv.appendChild(textDiv);
    if (isOwnMessage) {
        messageDiv.appendChild(avatar);
    }
    li.appendChild(header);
    li.appendChild(messageDiv);
    li.appendChild(timestamp);

    document.getElementById("messagesList").appendChild(li);
    document.getElementById("messagesList").scrollTo({ top: document.getElementById("messagesList").scrollHeight, behavior: 'smooth' });
}

// Receive message
connection.on("ReceiveMessage", function (user, message, profilePhoto) {
    addMessageToChat(user, message, profilePhoto);
});

// Handle typing indicator
connection.on("UserTyping", function (user) {
    const indicator = document.querySelector(".typing-indicator");
    indicator.textContent = `${user} is typing...`;
    indicator.classList.add("active");
    clearTimeout(typingTimeout);
    typingTimeout = setTimeout(() => {
        indicator.classList.remove("active");
    }, 2000);
});

connection.on("UserStoppedTyping", function () {
    document.querySelector(".typing-indicator").classList.remove("active");
});

// Send message on Enter
document.getElementById("messageInput").addEventListener("keypress", function (e) {
    if (e.key === "Enter" && this.value.trim() !== "") {
        document.getElementById("sendButton").click();
        this.value = "";
    }
});*/