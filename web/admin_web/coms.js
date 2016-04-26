var serverSocket;
var sendAction;

// "enum" for the role types
roles = {
    OFFICER: 0,
    SPECTATOR: 1,
    COMMANDER: 2
}

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
    console.log(msg)

    displayState(msg.State)
    displayStatus(msg.State, msg.Ready)
    displayCommander(msg.Commander)
    displayOfficers(msg.Officers)
    displaySpectators(msg.Spectators)


    switch (msg.State) {
        case "RUN":
            disableAllButtons()
            sendAction = sendEnterSetupSignal
            $('#nextStateButton').html("Enter Setup")
            if (msg.Ready) {enableNextStateButton()}
            break;
        case "STP":
            sendAction = sendInviteOfficersSingnal
            $('#nextStateButton').html("Invite Officers")
            if (msg.Ready) {enableNextStateButton()}
            break;
        case "INV":
            disableAllButtons()
            sendAction = sendStartGameSignal
            $('#nextStateButton').html("Start Game")
            if (msg.Ready) {enableAllButtons()}
            break;
    }
}

function enableAllButtons() {
    $('.btn').each(function() {
        $(this).attr("disabled", false)
    });
}

function disableAllButtons() {
    $('.btn').each(function() {
        $(this).attr("disabled", true)
    });
}

function nextStateButtonAction() {
    // block button until next update is received
    $('#nextStateButton').attr("disabled", true)
    if(sendAction != undefined) {
        sendAction()
    }
}

function sendInviteOfficersSingnal() {
    var msg = {
        type: "GM_INV"
    }
    serverSocket.send(JSON.stringify(msg));
}

function sendStartGameSignal() {
    // Get the team name from the textbox
    var team_name = $('#team-name').val()
    var stats_ip = $('#stats-ip').val()

    // If the team name is invalid we show an error message
    // and return
    if (team_name == "") {
        $('#team-name-error').css("color", "red")
        $('#team-name-error').html("Please enter a valid team name!")
        return
    } else {
        $('#team-name-error').html("")
        $('#team-name').val("")
    }

    var msg = {
        type: "GM_STRT",
        data: team_name + "," + stats_ip
    }
    serverSocket.send(JSON.stringify(msg));
}

function sendEnterSetupSignal() {
    var msg = {
        type: "GM_STP"
    }
    serverSocket.send(JSON.stringify(msg));
}

function sendSetPlayerSignal(userId, role) {
    typeStr = ""
    switch (role) {
        case roles.OFFICER:
            typeStr = "SET_OFFIC"
            break;
        case roles.SPECTATOR:
            typeStr = "SET_SPEC"
            break;
        case roles.COMMANDER:
            typeStr = "SET_COMND"
            break;
        default:
            console.log("Unknown role to request to be set.")
            return
    }

    var msg = {
        type: typeStr,
        data: userId
    }

    serverSocket.send(JSON.stringify(msg));
}
