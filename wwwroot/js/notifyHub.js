'use strict';

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notifyHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

connection.start().catch(err => console.error(err.toString()));

connection.on("ReceiveMessage", function (user, message) {
    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    var encodedMsg = "Naujas įrašas: " + msg + " - " + user;
    var li = document.createElement("li");
    li.textContent = encodedMsg;
    document.getElementById("messagesList").appendChild(li);
});