package main

import (
    "math"
    "strconv"
)

type PlayerState int

const (
    SPECTATOR PlayerState = iota
    OFFICER
    COMMANDER
    PROMOTION
    STANDBY
    REJECTED
)

// Holds player related data
type Player struct {
    id                 uint64
    userName           string
    state              PlayerState
    score              int
    isControllingEnemy bool
    controlledEnemyId  int64
    user               *User
}

// Sets the current use associated with this player
func (plr *Player) setUser(usr *User) {
    playerMap.plrC <- struct{}{}
    plr.user = usr
    playerMap.plrC <- struct{}{}
}

// Deassociate the current user if it equal the provided parameter
func (plr *Player) unsetUserIfEquals(usr *User) {
    playerMap.plrC <- struct{}{}
    if plr.user == usr {
        plr.user = nil
    }
    playerMap.plrC <- struct{}{}
}

// Sets the state of a player and sends a state update
func (plr *Player) setState(st PlayerState) {
    plr.state = st
    plr.sendStateUpdate()
}

// Sets the currently controlled enemy
func (plr *Player) setControlledEnemy(enemyId int64) {
    if plr.isControllingEnemy {
        return
    }

    if !sendTCPMsgToGameServer("CTRL:" + strconv.FormatInt(enemyId, 10)) {
        return
    }

    if !enemyMap.setControlled(enemyId, plr) {
        return
    }

    // TODO: notify Game Server

    plr.isControllingEnemy = true
    plr.controlledEnemyId = enemyId
    plr.sendControlledEnemyInfo()
}

// Sets the player as no longer controlling an enemy
func (plr *Player) unsetControlledEnemy() {
    plr.isControllingEnemy = false
    plr.controlledEnemyId = 0
    plr.sendControlledEnemyInfo()
}

// Sends the controlled enemy data to the phone client
func (plr *Player) sendControlledEnemyInfo() {
    // players with no active user don't need updating
    if plr.user == nil {
        return
    }

    msg := map[string]interface{}{
        "type": "ENM_CTRL",
        "data": map[string]interface{}{
            "isControlling": plr.isControllingEnemy,
            "controlledId":  plr.controlledEnemyId,
        },
    }

    plr.user.sendMsg(msg)
}

// Sends a user state update
func (plr *Player) sendStateUpdate() {
    // players with no active user don't need updating
    if plr.user == nil {
        return
    }

    msg := map[string]interface{}{
        "type": "USER_UPDATE",
        "data": map[string]interface{}{
            "state": plr.getStateString(),
        },
    }

    plr.user.sendMsg(msg)
    plr.sendControlledEnemyInfo()
}

// Sends a user state data update
func (plr *Player) sendDataUpdate(enemies map[int64]*Enemy, asteroids map[int]*Asteroid) {
    // players with no active user don't need updating
    if plr.user == nil {
        return
    }

    // TODO: add other objects
    msg := make(map[string]interface{})
    msg["type"] = "STATE_UPDATE"
    enemies_data := make([]map[string]interface{}, 0)
    // Add enemies to the message
    for id, enemy := range enemies {
        enemies_data = append(enemies_data, map[string]interface{}{
            "id": id,
            "x":  enemy.pos.x,
            "y":  enemy.pos.y,
            // TODO: Rotation is bugged
            "rot":      math.Atan2(enemy.forward.y, enemy.forward.x),
            "isHacked": enemy.isControlled,
        })
    }

    asteroids_data := make([]map[string]interface{}, 0)
    // Add asteroids to the message
    for id, ast := range asteroids {
        asteroids_data = append(asteroids_data, map[string]interface{}{
            "id": id,
            "x":  ast.pos.x,
            "y":  ast.pos.y,
        })
    }

    msg["data"] = map[string]interface{}{
        "asts": asteroids_data,
        "enms": enemies_data,
    }

    plr.user.sendMsg(msg)
}

// Send the relative coordinates to which an enemy should move
func (plr *Player) sendMoveToGameServer(data map[string]interface{}) {
    if !plr.isControllingEnemy {
        return
    }

    msg := "MV:"
    msg += strconv.FormatInt(plr.controlledEnemyId, 10) + ","
    msg += strconv.FormatFloat(data["x"].(float64), 'f', -1, 64) + ","
    msg += strconv.FormatFloat(data["y"].(float64), 'f', -1, 64)

    sendUDPMsgToGameServer(msg)
}

// Send attack command for the controlled enemy to the game server
func (plr *Player) sendAttackCommandToGameServer(enemyId int64) {
    if !plr.isControllingEnemy {
        return
    }

    msg := "ATT:"
    msg += strconv.FormatInt(plr.controlledEnemyId, 10) + ","
    msg += strconv.FormatInt(enemyId, 10)

    sendUDPMsgToGameServer(msg)
}

// Deals with state transition based on the answer to the promotion offer
func (plr *Player) processPromotionAnswer(accepted bool) {
    if accepted && plr.state == PROMOTION {
        plr.setState(OFFICER)
        playerMap.setOfficer(plr)
    } else {
        plr.setState(REJECTED)
    }
}

// Get a string representing the state, used as instruction for the phone
// web page transitions
func (plr *Player) getStateString() (out string) {
    switch plr.state {
    case SPECTATOR:
        out = "SPECTATOR"
    case OFFICER:
        out = "OFFICER"
    case COMMANDER:
        out = "COMMANDER"
    case PROMOTION:
        out = "PROMOTION"
    case REJECTED:
        fallthrough
    case STANDBY:
        out = "STANDBY"
    }

    return
}

// Get an intial player state based on the game state
func getNewPlayerState() PlayerState {
    // Means that a game isn't running and it hasnt ended
    if gameState.status == RUNNING && !gameState.canEnterNextState {
        return SPECTATOR
    } else {
        return STANDBY
    }
}
