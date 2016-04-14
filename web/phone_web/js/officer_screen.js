var MAX_AMMO = 20;
var AMMO_RECHARGE_RATE = 500;
var ammoRechargeTimer;
var currentAmmunition = 0;
var lastReceivedAmmoUpdate = 0;
var score = 0
var repairJobCount = 0
var upgradeJobCount = 0

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
    score = 0
    repairJobCount = 0
    upgradeJobCount = 0
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
        case "SCORE":
            score = data.data
        default:
            console.log("Officer received unknown update message: " + data.type)
    }
}

// Handles a list of job notifications received
// from the main game
function handleNotifications(data) {
    for (var i = 0; i < data.length; i++) {
        notification = data[i]
        if (notification.isUpgrade) {
            handleUpgradeNotification(notification)
        } else {
            handleRepairNotification(notification)
        }
    }
}

function handleUpgradeNotification(notification) {
    if (notification.toSet) {
        upgradeJobCount++
    } else {
        upgradeJobCount--
    }
}

function handleRepairNotification(notification) {
    if (notification.toSet) {
        repairJobCount++
    } else {
        repairJobCount--
    }
}

//Fills the next empty ammo segment
function rechargeAmmo() {
    if (currentAmmunition < MAX_AMMO) {
        $("#ammoSegment"+currentAmmunition).addClass("filled")
        currentAmmunition++;
    }
}
