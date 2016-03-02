var serverSocket;

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
    var msg = JSON.parse(event.data)
    console.log(event.data)

    if (msg.State == "STP" && msg.Ready) {
        enableStartGame()
    } else {
        disableStartGame()
    }

    displayState(msg.State)
    displayStatus(msg.Ready)
    displayOfficers(msg.Officers)
    displaySpectators(msg.Spectators)
}

function sendStartGameSignal() {
        // block button until next update is received
        $('#startGameButton').attr("disabled", true)
        var msg = {
            type: "GM_STRT"
        }

        serverSocket.send(JSON.stringify(msg));
}
