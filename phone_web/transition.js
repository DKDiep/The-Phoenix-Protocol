var currentScreen = ""

// Display the page as per the current user state
function updateScreen(userData) {
    switch (userData.state) {
        case "SPECTATOR":
            transitionTo("spectator", function() {
              startGame();
            });
            break;
        case "GAME_LIMBO":
            transitionTo("game_limbo");
            break;
        case "CREW":
            transitionTo("crew", function () {
                updateAmmo(userData.ammo);
            });
            break;
        case "COMMANDER":
            transitionTo("commander");
            break;
        default:
            console.log("Unexpected User State: "+userData.state)
    }
}

// Changes the visual elements based on the screen type
function transitionTo(screen_name, callback) {
    $('#screen').fadeOut('2000', function (){
        $('#screen').load('/screens/'+screen_name+'.html');
        $('#screen').fadeIn('2000', callback)});
}
