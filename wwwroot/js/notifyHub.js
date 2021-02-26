'use strict';

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notifyHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

connection.start().catch(err => console.error(err.toString()));

connection.on("ReceiveMessage", function (user, message) {
    var toastDiv = document.createElement("div");
    toastDiv.role = "alert";
    toastDiv.setAttribute("aria-live", "assertive");
    toastDiv.setAttribute("aria-atomic", "true");
    toastDiv.setAttribute("data-autohide", "false");
    toastDiv.className = "toast";

    var toastHeader = document.createElement("div");
    toastHeader.className = "toast-header";

    var toastHeaderStrong = document.createElement("strong");
    toastHeaderStrong.className = "mr-auto";
    toastHeaderStrong.textContent = "New Member";
    var toastHeaderSmall = document.createElement("small");
    toastHeaderSmall.className = "text-muded";
    toastHeaderSmall.textContent = "*";
    var toastHeaderButton = document.createElement("button");
    toastHeaderButton.className = "ml-2 mb-1 close";
    toastHeaderButton.type = "button";
    toastHeaderButton.setAttribute("data-dismiss", "toast");
    toastHeaderButton.setAttribute("aria-label", "Close");
    var toastHeaderSpan = document.createElement("span");
    toastHeaderSpan.setAttribute("aria-hidden", "true");
    toastHeaderSpan.textContent = "×";

    var toastBody = document.createElement("div");
    toastBody.className = "toast-body";
    toastBody.textContent = message;

    toastDiv.appendChild(toastHeader);
    toastDiv.appendChild(toastBody);

    toastHeader.appendChild(toastHeaderStrong);
    toastHeader.appendChild(toastHeaderSmall);
    toastHeader.appendChild(toastHeaderButton);

    toastHeaderButton.appendChild(toastHeaderSpan);

    document.getElementById("divToast").appendChild(toastDiv);
    $('.toast').toast('show');
});