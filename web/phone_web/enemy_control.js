var controlledEnemyId = 0;
var isControllingEnemy = false;

// Action related to an enemy object
function actionOnEnemy(id) {
    console.log("enemy action ", id)
    if(!isControllingEnemy) {
        sendControlEnemyRequest(id)
    } else if (id != controlledEnemyId) {
        sendEnemyAttackRequest(id)
    }
}

// Action related to movement
function moveAction(x, y) {
    console.log("move action ", x, y)
    if(isControllingEnemy) {
        sendMoveEnemyRequest(x, y)
    }
}

// Update controll info based on data from server
function updateControllInfo(data) {
    controlledEnemyId = data.controlledId
    isControllingEnemy = data.isControlling
    if(isControllingEnemy) {
        // TODO: indicate that someting is being controlled
    } else {
        // TODO: indicate that nothing is being controlled
    }
}
