// Send off username for user creation and disables input to prevent fuckups
function registerUser(elem) {
    elem.disabled = true
    username = document.getElementById("usernameField").value
    //document.getElementById("usernameField").value = ""
    document.getElementById("usernameField").disabled = true

    // in coms.js
    requestUserCreation(username)
}
