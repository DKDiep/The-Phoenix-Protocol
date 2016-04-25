
$(document).ready(function() {
    if(window.location.hash.substr(1) == "auto")
    {
        Cookies.set("seen_tutorial", true)
        Cookies.set("user_id", 1)
    }
});

// Intialisation fucntion for the screen
function startRegisterScreen() {
    // Do initialisation
}

// Finalise function for the screen
function finaliseRegisterScreen() {
    // Do cleanup
}

// Send off username for user creation and disables input to prevent fuckups
function registerUser(elem) {
    $(elem).prop("disabled", "true")
    $("#usernameField").prop("disabled", "true")
    username = $("#usernameField").val()

    // in coms.js
    requestUserCreation(username)
}
