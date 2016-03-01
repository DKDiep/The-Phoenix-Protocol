function disableAdminPanel() {
    displayConnectionError()
    $('#startGameButton').attr("disabled", true)
}

function enableAdminPanel() {
    displayConnectionEstablished()
    $('#startGameButton').attr("disabled", false)
}

function displayConnectionError() {
    $('#status').html("Connection to server closed. Please try again later.")
}

function displayConnectionEstablished() {
    $('#status').html("Connection established!")
}
