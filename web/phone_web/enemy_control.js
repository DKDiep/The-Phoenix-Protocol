var controlledEnemyId = 0;
var isControllingEnemy = false;

function actionOnEnemy(id) {
    console.log("enemy action ", id)
    if(!isControllingEnemy) {
        sendControlEnemyRequest(id)
    } else if (id != controlledEnemyId) {
        sendEnemyAttackRequest(id)
    }
}

function moveAction(x, y) {
    console.log("move action ", x, y)
    if(isControllingEnemy) {
        sendMoveEnemyRequest(x, y)
    }
}
