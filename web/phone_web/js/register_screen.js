
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
    username = $("#usernameField").val()

    // If the team name is invalid we show an error message
    // and return
    if (username == "") {
        $('#usernameError').css("color", "red")
        $('#usernameError').html("Please enter a valid user name!<br/><br/>")
        $("#usernameField").val("")
        return
    } else {
        $("#usernameError").html("")
        $("#usernameField").val("")
        $(elem).prop("disabled", "true")
        $("#usernameField").prop("disabled", "true")
    }

    // in coms.js
    requestUserCreation(username)
}
