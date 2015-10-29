// Initialises the web page by first establishing a connection and then
// arranging the web page
function initialiseWebPage() {
    // in coms.js
    initSocket()
}

// Starts initialisation of the web page
function initScreen() {
    if(document.cookie == "") {
        initUnregistered()
    }
    else {
        requestUserUpdate()
    }
}

// Display the page version for unregistered users
function initUnregistered() {
    transitionTo("registerScreen")
}

// Save user identification information as a cookie
function saveUserAndUpdate(user) {
    document.cookie = "user_id="+user.id
    updateScreen(user.data)
}
