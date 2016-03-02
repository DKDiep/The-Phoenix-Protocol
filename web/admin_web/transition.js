$( document ).ready(function() {
    initSocket()
    addToLegend("officer-legend")
    addToLegend("spectator-legend")
});

function disableAdminPanel() {
    displayConnectionError()
    disableStartGame()
}

function enableAdminPanel() {
    displayConnectionEstablished()
    enableStartGame()
}

function displayState(state) {
    switch (state) {
        case "STP":
            $("#game-state-value").removeClass("game-state-running")
            $("#game-state-value").addClass("game-state-setup")
            $("#game-state-value").html("Setup")
            break;
        case "RUN":
            $("#game-state-value").removeClass("game-state-setup")
            $("#game-state-value").addClass("game-state-running")
            $("#game-state-value").html("Running")
            break;
        default:
            console.log("Unexpected Game State.")
            break;
    }
}

function displayStatus(isReady) {
    if (isReady) {
        $("#setup-status-value").removeClass("status-not-ready")
        $("#setup-status-value").addClass("status-ready")
        $("#setup-status-value").html("Ready To Start")
    } else {
        $("#setup-status-value").removeClass("status-ready")
        $("#setup-status-value").addClass("status-not-ready")
        $("#setup-status-value").html("Not Ready")
    }
}

function displayOfficers(list) {
    newContents = ""
    list.sort(function(a, b) {
        return b.Score - a.Score
    })
    for (plrId in list) {
        newContents += getPlayerRow(list[plrId])
    }
    $("#officer-elements").html(newContents)
}

function displaySpectators(list) {
    newContents = ""
    list.sort(function(a, b) {
        return b.Score - a.Score
    })
    for (plrId in list) {
        newContents += getPlayerRow(list[plrId])
    }
    $("#spectator-elements").html(newContents)
}

function getPlayerRow(player) {
    out  = "<div class=\"table-row\">"
    out += "<div class=\"table-cell user-name user-name-entry\">" + player.UserName + "</div>"
    out += "<div class=\"table-cell user-id user-id-entry\">" + player.UserId + "</div>"
    out += "<div class=\"table-cell score score-entry\">" + player.Score + "</div>"
    out += "<div class=\"table-cell is-online is-online-entry\">"
    if (player.IsOnline) {
        out += "Yes"
    } else {
        out += "No"
    }
    out += "</div></div>"

    return out
}

function disableStartGame() {
    $('#startGameButton').attr("disabled", true)
}

function enableStartGame() {
    $('#startGameButton').attr("disabled", false)
}

function displayConnectionError() {
    $('#status').html("Connection to server closed. Please try again later.")
}

function displayConnectionEstablished() {
    $('#status').html("Connection established!")
}

// Nasty legend alignment hack
function addToLegend(id) {
    $("#" + id).html("<div class=\"table-row\">"+
        "<div class=\"table-cell user-name cell-legend\">"+
            "User Name"+
        "</div>"+
        "<div class=\"table-cell user-id cell-legend\">"+
            "User Id"+
        "</div>"+
        "<div class=\"table-cell score cell-legend\">"+
            "Score"+
        "</div>"+
        "<div class=\"table-cell is-online cell-legend\">"+
            "Is Online"+
        "</div>"+
    "</div>")
}
