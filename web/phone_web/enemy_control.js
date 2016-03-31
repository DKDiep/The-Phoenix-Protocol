var controlledEnemyId = 0;
var isControllingEnemy = false;

// Action related to an enemy object
function actionOnEnemy(target) {
    console.log("enemy action ", target.spaceGameId)
    if(!isControllingEnemy) {
        setHackTarget(target.spaceGameId)
        setHeld(true)
    } else if (id != controlledEnemyId) {
        sendEnemyAttackRequest(target.spaceGameId)
    }
}

// Action related to movement
function moveAction(x, y) {
    console.log("move action ", x, y)
    if(isControllingEnemy) {
        sendMoveEnemyRequest(x, y)
    } else {
        decrementHackProgress()
    }
}

// Update controll info based on data from server
function updateControllInfo(data) {
    controlledEnemyId = data.controlledId
    isControllingEnemy = data.isControlling
    if(isControllingEnemy) {
        enemyControllUpdate = displayNewControlledEnemy
    } else {
        enemyControllUpdate = clearDisplayingOfControlledEnemy
    }
}

function displayEnemyHacking(id) {
    targetEnemySprite = findEnemyWithID(id)
    enableTractorBeam(targetEnemySprite)
}

// Highligth controlled enemy and deals with other state changes
function displayNewControlledEnemy() {
    controlledEnemySprite = findControlledEnemy()
    controlledEnemySprite.isHacked = true
    controlledEnemySprite.texture = loadedResources.controlled_enm.texture
    // Enabling tractor beam as example of usage
    enableTractorBeam(controlledEnemySprite)
}

// Clear controll enemy visualisations and data
function clearDisplayingOfControlledEnemy() {
    controlledEnemySprite = undefined
    // Disabling tractor beam as example of usage
    disableTractorBeam()
}
