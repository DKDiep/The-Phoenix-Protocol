$( document ).ready(function() {
    initSocket()
    addToLegend("officer-legend")
    addToLegend("spectator-legend")
});

function disableAdminPanel() {
    displayConnectionError()

    // disable all buttons
    $('.btn').each(function() {
        $(this).attr("disabled", true)
    });

    // show ERROR everywhere
    $("#game-state-value").css("color", "red");
    $("#game-state-value").html("ERROR")

    $("#setup-status-value").css("color", "red")
    $("#setup-status-value").html("ERROR")
}

function enableAdminPanel() {
    displayConnectionEstablished()
}

function setPlayerAction(btn, userId, role) {
    btn.disabled = true
    sendSetPlayerSignal(userId, role)
}

function displayState(state) {
    switch (state) {
        case "INV":
        case "STP":
            $("#game-state-value").css("color", "orange")
            $("#game-state-value").html("Setup")
            break;
        case "RUN":
            $("#game-state-value").css("color", "green")
            $("#game-state-value").html("Running")
            break;
            break;
        default:
            console.log("Unexpected Game State.")
            break;
    }
}

function displayStatus(state, isReady) {
    switch (state) {
        case "INV":
            $("#status-text").html("New Game Status:")
            if(isReady) {
                $("#setup-status-value").css("color", "green")
                $("#setup-status-value").html("Can Be Started")
            } else {
                $("#setup-status-value").css("color", "orange")
                $("#setup-status-value").html("Sending Invitations")
            }
            break;
        case "RUN":
            $("#status-text").html("Curent Game Status:")
            if(isReady) {
                $("#setup-status-value").css("color", "orange")
                $("#setup-status-value").html("Finished")
            } else {
                $("#setup-status-value").css("color", "green")
                $("#setup-status-value").html("In Progress")
            }
            break;
        case "STP":
            $("#status-text").html("Officer Invitations Status:")
            if(isReady) {
                $("#setup-status-value").css("color", "green")
                $("#setup-status-value").html("Can Be Sent")
            } else {
                $("#setup-status-value").css("color", "red")
                $("#setup-status-value").html("Can't Be Sent")
            }
            break;
    }
}

function displayOfficers(list) {
    newContents = ""
    list.sort(usernameCompare)
    list.sort(function(a, b) {
        return b.Score - a.Score
    })
    for (plrId in list) {
        newContents += getPlayerRow(list[plrId], roles.OFFICER)
    }
    $("#officer-elements").html(newContents)
}

function displaySpectators(list) {
    newContents = ""
    list.sort(usernameCompare)
    list.sort(function(a, b) {
        return b.Score - a.Score
    })
    for (plrId in list) {
        newContents += getPlayerRow(list[plrId], roles.SPECTATOR)
    }
    $("#spectator-elements").html(newContents)
}

function usernameCompare(a,b) {
  if (a.UserName < b.UserName)
    return -1;
  else if (a.UserName > b.UserName)
    return 1;
  else
    return 0;
}

function getPlayerRow(player, role) {
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
    out += "</div>"

    // action button
    switch (role) {
        case roles.OFFICER:
            out += "<button type=\"button\" class=\"btn btn-default table-cell user-action user-action-entry\" onclick=\"setPlayerAction(this," + player.UserId + ", " + roles.SPECTATOR + ")\">Set Spectator</button>"
            break;
        case roles.SPECTATOR:
            out += "<button type=\"button\" class=\"btn btn-default table-cell user-action user-action-entry\" onclick=\"setPlayerAction(this," + player.UserId + ", " + roles.OFFICER + ")\">Set Officer</button>"
            break;
        default:
            console.log("Unknown role to request to be set.")
            return "ERROR"
    }

    out += "</div>"

    return out
}

function disableNextStateButton() {
    $('#nextStateButton').attr("disabled", true)
}

function enableNextStateButton() {
    $('#nextStateButton').attr("disabled", false)
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
        "<div class=\"table-cell user-action cell-legend\">"+
            "Action"+
        "</div>"+
    "</div>")
}
