// The required hack progress for an enemy to become hacked
var REQ_HACK_PROGRESS = 100;

// The current hack progress
var hackProgress = 0;

// The ID of the enemy currently being hacked
var currentTargetID = 0;

// Whether the current hack is complete
var finishedHack = false;

// Indicates whether the user is holding
// the mouse down on an enemy
var enemyHeld = false;

// Increments the hacking progress for the
// current enemy. Also handles the case where
// the user clicks a new enemy half way through
// hacking a different one
function incrementHackProgress() {
    // If we haven't finished hacking the enemy
    // we continue with the hack
    if (!finishedHack) {
        hackProgress += 2

        // If the tractor beam hasn't been enabled yet
        // we enable it.
        if (!isTractorBeamEnabled()) {
            displayEnemyHacking(currentTargetID)
        }

        if (hackProgress >= REQ_HACK_PROGRESS) {
            sendControlEnemyRequest(currentTargetID)
            finishedHack = true
        }
    }
}

// Decrements the hacking progress and deals with the case
// where the hacking progress goes down to zero
function decrementHackProgress() {
    // Only decrement if the hack progress isn't zero
    if (hackProgress > 0) {
        hackProgress -= 2

        if (hackProgress <= 0) {
            resetHackProgress()
        }
    }
}

// Updates the hacking values depending
// on whether an enemy is held or not
function updateHacking() {
    if (enemyHeld) {
        incrementHackProgress()
    } else {
        decrementHackProgress()
    }
}

// Resets the hacking progress
function resetHackProgress() {
    hackProgress = 0
    finishedHack = false
    disableTractorBeam()
}

// Resets the global variables of this script
function resetHackingGame() {
    REQ_HACK_PROGRESS = 100
    resetHackProgress()
    resetHackTarget()
    enemyHeld = false
}

// Sets the hack target to the given id
// and resets hacking progress if the target
// has changed
function setHackTarget(id) {
    if (currentTargetID != id) {
        resetHackProgress()
        currentTargetID = id
    }
}

// Resets the hack target to the default value
function resetHackTarget() {
    currentTargetID = 0
}

// Sets the enemyHeld attribute to val
function setHeld(val) {
    enemyHeld = val
}
