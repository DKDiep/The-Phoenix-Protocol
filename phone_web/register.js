// Send off username for user creation and disables input to prevent fuckups
function registerUser(elem) {
    $(elem).prop("disabled", "true")
    $("#usernameField").prop("disabled", "true")
    username = $("#usernameField").val()

    // in coms.js
    requestUserCreation(username)
}
