// The required hack progress for an enemy to become hacked
var REQ_HACK_PROGRESS = 100

// The current hack progress
var hackProgress = 0;

// The ID of the enemy currently being hacked
var currentTargetID = null;

// Whether the current hack is complete
var finishedHack = false;

// Increments the hacking progress for the
// current enemy. Also handles the case where
// the user clicks a new enemy half way through
// hacking a different one
function incrementHackProgress(id) {
    if (id != currentTargetID) {
        swapTarget(id)
    } else if (!finishedHack) {
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
    if (hackProgress > 0) {
        hackProgress -= 2

        if (hackProgress <= 0) {
            resetHackProgress()
        }
    }
}

function swapTarget(id) {
    currentTargetID = id
    resetHackProgress()
}

function resetHackProgress() {
    hackProgress = 0
    finishedHack = false
    disableTractorBeam()
}

