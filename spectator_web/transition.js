var currentScreen = ""

// Display the page as per the current user state
function updateScreen(userData) {
    switch (userData.state) {
        case "OFFLINE":
            transitionTo("joinCurrentScreen")
            break;
        case "SPECTATOR":
            transitionTo("spectatorScreen")
            break;
        case "CREW":
            transitionTo("crewScreen")
            break;
        case "PILOT":
            transitionTo("pilotScreen")
            break;
        default:
            console.log("Unexpected User State: "+userData.state)
    }
}

// Changes the visual elements based on the screen type
function transitionTo(screenId) {
    if (currentScreen != "") {
        document.getElementById(currentScreen).style.display = "none"
    }
    currentScreen = screenId
    document.getElementById(screenId).style.display = "inline"
}
