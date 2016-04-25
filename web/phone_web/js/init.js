// Initialises the web page by first establishing a connection and then
// arranging the web page

$(document).ready(function() {
    initSocket(initScreen);
    window.onbeforeunload = function(event) {
        event.returnValue = "Leaving Crew App.";
    };
});

$(document).click(function(e) {
    if( /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent) ) {
        launchIntoFullscreen(document.documentElement);
    }

});

function launchIntoFullscreen(element) {
  if(element.requestFullscreen) {
    element.requestFullscreen();
  } else if(element.mozRequestFullScreen) {
    element.mozRequestFullScreen();
  } else if(element.webkitRequestFullscreen) {
    element.webkitRequestFullscreen();
  } else if(element.msRequestFullscreen) {
    element.msRequestFullscreen();
  }
}


// Starts initialisation of the web page
function initScreen() {
    if(typeof Cookies.get("user_id") === 'undefined') {
        initUnregistered()
    }
    else {
        // Display tutorial if not seen before
        seenTutorial = Cookies.get("seen_tutorial")
        if(seenTutorial == undefined || !seenTutorial) {
            changeScreen("tutorial", startSpectatorTutorialScreen,
                finaliseSpectatorTutorialScreen)
        } else {
            requestUserUpdate()
        }
    }
}

// Display the page version for unregistered users
function initUnregistered() {
    changeScreen("register", startRegisterScreen, finaliseRegisterScreen)
}

// Save user identification information as a cookie
function saveUserAndUpdate(user) {
    Cookies.set("user_id", user.id)
    initScreen()
}
