var MAX_AMMO = 20;
var AMMO_RECHARGE_RATE = 500;
var ammoRechargeTimer;
var currentAmmunition = 0;
var lastReceivedAmmoUpdate = 0;
var notificationMap = {}
var currentNotifications = []

// Initialisation function for the screen
function startOfficerScreen() {
    // Do necessary initialisation
    updateAmmo()
}

// Finalisation function for the screen
function finaliseOfficerScreen() {
    // Do necessary cleanup and state reset
    currentAmmunition = 0;
    lastReceivedAmmoUpdate = 0;
    notificationMap = {}
    currentNotifications = []
}

//Sets the ammunition to the specified ammount and restarts the recharge
//timer.
function updateAmmo() {
    // To avoid concurrency issues
    ammo = lastReceivedAmmoUpdate
    clearInterval(ammoRechargeTimer)
    for (i = 0; i < ammo; i++) {
        $("#ammoSegment"+i).addClass("filled")
    }
    for (i = ammo; i < MAX_AMMO ; i++) {
        $("#ammoSegment"+i).removeClass("filled")
    }
    currentAmmunition = ammo
    ammoRechargeTimer = setInterval(rechargeAmmo, AMMO_RECHARGE_RATE)
}

// Deals with officer updates
// E.g. recieving job notifications
function updateOfficer(data) {
    switch(data.type) {
        case "NOTIFY":
            handleNotifications(data.data)
            break;
        case "AMMO":
            lastReceivedAmmoUpdate = data.data
            updateAmmo()
            break;
        default:
            console.log("Officer received unknown update message: " + data.type)
    }
}

// Handles a list of job notifications received
// from the main game
function handleNotifications(data) {
    for (i = 0; i < data.length; i++) {
        var notification = {type:data[i].type,isUpgrade:data[i].isUpgrade}

        // We want to add the notification
        if (data[i].toSet) {
            notificationMap[notification] = createMessageFromNotification(notification)
        } else {
            delete myObject[notification]
        }
    }

    updateCurrentNotifications()
}

function updateCurrentNotifications() {
    var newNotifications = []

    // Loop through the notifications
    for (var notification in notificationMap) {
        if (notificationMap.hasOwnProperty(notification)) {
            newNotifications.push(notificationMap[notification])
        }
    }

    currentNotifications = newNotifications
}

// Creates a message string from a notification object
function createMessageFromNotification(notification) {
    var message = ""

    if (notification.isUpgrade) {
        message = "The commander has requested "

        // Deal with "a" and "an" correctly
        if (/[AEIOU]/.test(notification.type.charAt(0))) {
            message += "an "
        } else {
            message += "a "
        }

        message += notification.type + " upgrade"
    } else {
        message = "The commander needs the " + notification.type + " repaired"
    }

    return message
}

//Fills the next empty ammo segment
function rechargeAmmo() {
    if (currentAmmunition < MAX_AMMO) {
        $("#ammoSegment"+currentAmmunition).addClass("filled")
        currentAmmunition++;
    }
}
