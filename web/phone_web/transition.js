var finalisePrevious = undefined;
var currState = "NONE"

// Display the page as per the current user state
function updateScreen(userData) {
    switch (userData.state) {
        case "SPECTATOR":
            changeScreen("spectator", startSpectatorScreen, finaliseSpectatorScreen)
            break;
        case "PROMOTION":
            changeScreen("promotion", startPromotionScreen, finalisePromotionScreen)
            break;
        case "OFFICER":
            changeScreen("officer", startOfficerScreen, finaliseOfficerScreen)
            lastReceivedAmmoUpdate = userData.ammo
            break;
        case "COMMANDER":
            changeScreen("commander", startCommanderScreen, finaliseCommanderScreen)
            break;
        case "STANDBY":
            changeScreen("standby", startStandbyScreen, finaliseStandbyScreen)
            break;
        default:
            console.log("Unexpected User State: " + userData.state)
            return;
    }
    // Set current state
    currState = userData.state
}

// Update objects according to current state
function updateObjects(data) {
    switch (currState) {
        case "SPECTATOR":
            updateSprites(data);
            break;
        default:
            console.log("Received object updates in unhandled state: " + currState)
    }
}

// Change the current screen and handle init and cleanup
function changeScreen(screenName, startFun, endFun) {
    // Do any necessary cleanup from the previos state
    if (finalisePrevious != undefined) {
        finalisePrevious()
    }
    // Chnage html contents and call init callback
    transitionTo(screenName, function () {
        startFun();
    });
    // Set next cleanup function
    finalisePrevious = endFun
}

// Changes the visual elements based on the screen type
function transitionTo(screenName, callback) {
    $('#screen').fadeOut('2000', function (){
        $('#screen').load('/screens/' + screenName + '.html');
        $('#screen').fadeIn('2000', callback)});
}
