const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

connection.start().then(() => {
    document.getElementById("sendButton").disabled = false;
}).catch(err => console.error(err.toString()));

document.getElementById("sendButton").addEventListener("click", function (event) {
    event.preventDefault();

    const user = document.getElementById("userInput").value;
    const message = document.getElementById("messageInput").value;

    if (user && message) {
        connection.invoke("SendMessage", user, message).catch(err => console.error(err.toString()));
    }
});

connection.on("ReceiveMessage", function (user, message) {
    const li = document.createElement("li");
    li.textContent = `${user} says ${message}`;
    document.getElementById("messagesList").appendChild(li);
});
