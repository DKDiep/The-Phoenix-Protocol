var controlledEnemyId = 0;
var isControllingEnemy = false;

// Action related to an enemy object
function actionOnEnemy(target) {
    moveTarget.alpha = 0;
    if(!isControllingEnemy) {
        setHackTarget(target.spaceGameId)
        setHeld(true)
    } else if (target.spaceGameId != controlledEnemyId && !target.isHacked) {
        sendEnemyAttackRequest(target.spaceGameId)
    }
}

// Action related to movement
function moveAction(x, y) {
    if(isControllingEnemy) {
        moveTarget.alpha = moveTargetAlpha;
        moveTarget.position.x = (0.5*renderer.width) + (x*zoom*maxDim);
        moveTarget.position.y = (0.5*renderer.height) + ((-y)*zoom*maxDim);
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
    if(controlledEnemySprite == undefined) { return }
    controlledEnemySprite.isHacked = true
    controlledEnemySprite.touchTarget.interactive = false;
    if(!controlledEnemySprite.hasOverlay) {
        controlledEnemySprite.hasOverlay = true;
        var overlay = new PIXI.Sprite(loadedResources.controlled.texture);
        overlay.anchor.x = 0.5
        overlay.anchor.y = 0.5
        controlledEnemySprite.addChild(overlay)
    }
    // Enabling tractor beam as example of usage
    enableTractorBeam(controlledEnemySprite)
    displayTutorialPrompt(2);
    // Hack to make the hacking game finished
    finishedHack = true;
    hackProgress = REQ_HACK_PROGRESS;
    enemyControllUpdate = undefined
}

// Clear controll enemy visualisations and data
function clearDisplayingOfControlledEnemy() {
    controlledEnemySprite = undefined
    moveTarget.alpha = 0;
    // Disabling tractor beam as example of usage
    disableTractorBeam()
    enemyControllUpdate = undefined
    displayTutorialPrompt(0);
}
