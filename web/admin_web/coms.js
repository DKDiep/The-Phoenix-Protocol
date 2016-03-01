var serverSocket;

$( document ).ready(function() {
    initSocket()
});

// Initialise the web socket connection
function initSocket() {
    if(typeof serverSocket === 'undefined') {
        serverSocket = new WebSocket("ws://" + window.location.host + "/web_socket");

        // After connection initialisation
        serverSocket.onopen = function() {
            enableAdminPanel()
            console.log("Web Socket Connection initialised");
        }

        // Receiving from server
        serverSocket.onmessage = function(e) { onMessage(e) }

        // Web Socket Error
        serverSocket.onerror = function(e) {
            conosle.log("Web Socket Error: " + e.data);
        }

        // After closing the connection
        serverSocket.onclose = function() {
            serverSocket = undefined;
            disableAdminPanel()
            console.log("Web Socket Connection Closed");
        }
    }
}

// Act based on type of command
function onMessage(event) {
    var msg = JSON.parse(event.data);

    // TODO: implement commented functions
    switch (msg.type) {
        default:
            console.log("Received unexpected message type: "+msg.type)
    }
}

function sendStartGameSignal() {
        var msg = {
            type: "GM_STRT"
        }

        serverSocket.send(JSON.stringify(msg));
}
