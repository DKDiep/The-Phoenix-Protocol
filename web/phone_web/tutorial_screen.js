// Intialisation fucntion for the screen
function startSpectatorTutorialScreen() {
    // Do initialisation
}

// Finalise function for the screen
function finaliseSpectatorTutorialScreen() {
    // Do cleanup
}

// Leave tutorial and remember seeing it
function exitTutorialAndSaveCompletion() {
    Cookies.set("seen_tutorial", true)
    initScreen()
}
