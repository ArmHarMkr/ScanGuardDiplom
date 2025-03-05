// Ensure SignalR is properly loaded
if (typeof signalR === "undefined") {
    console.error("SignalR library is not loaded correctly!");
} else {
    console.log("SignalR is loaded!");
}

// Create the SignalR connection
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chathub") // Ensure this matches your Hub's URL in the backend
    .build();

// Start the connection
connection.start().then(function () {
    console.log("Connected to ChatHub!");
}).catch(function (err) {
    return console.error("Error connecting to ChatHub: " + err.toString());
});

// Handle receiving messages from the server
connection.on("ReceiveMessage", function (user, message, time) {
    const li = document.createElement("li");
    li.textContent = `${user} (${time}): ${message}`;
    document.getElementById("messagesList").appendChild(li);
});

// Handle sending message to the server when the button is clicked
document.getElementById("sendMessageButton").addEventListener("click", function () {
    const userName = document.getElementById("userName").value; // Get the username from input field
    const message = document.getElementById("messageInput").value; // Get the message from input field

    // Check if the message is not empty
    if (message.trim() !== "") {
        connection.invoke("SendMessage", userName, message).catch(function (err) {
            return console.error("Error sending message: " + err.toString());
        });
    } else {
        console.warn("Message cannot be empty!");
    }

    // Clear the message input field after sending
    document.getElementById("messageInput").value = "";
});
