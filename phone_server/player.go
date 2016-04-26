package main

import (
    "math"
    "strconv"
    "sync"
    "time"
)

type PlayerState int

const (
    SPECTATOR PlayerState = iota
    OFFICER
    COMMANDER
    PROMOTION
    REJECTED
)

// Holds player related data
type Player struct {
    id                 uint64
    userName           string
    state              PlayerState
    score              uint64
    ammo               float64
    isControllingEnemy bool
    controlledEnemyId  int64
    targetId          int64
    user               *User
    inviteAnswerAction func(bool)
}

// Initialises a player
func NewPlayer(playerId uint64, name string, scr uint64, usr *User) *Player {
    return &Player{
        id:                 playerId,
        userName:           name,
        state:              SPECTATOR,
        score:              scr,
        ammo:               MAX_AMMO,
        isControllingEnemy: false,
        controlledEnemyId:  0,
        user:               usr,
        inviteAnswerAction: func(bool) {},
    }
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

    if !sendTCPMsgToGameServer("CTRL:" + strconv.FormatInt(enemyId, 10) + "+" +
        strconv.FormatUint(plr.id, 10) + "+" + plr.userName) {
        return
    }

    if !enemyMap.setControlled(enemyId, plr) {
        return
    }

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

// Sets the ammo
func (plr *Player) setAmmo(amnt float64) {
    plr.ammo = amnt
    plr.sendCurrentAmmo()
}

// Sets the score
func (plr *Player) setScore(score uint64) {
    plr.score = score
    plr.sendCurrentScore()
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
}

// Sends the necessary data for a player to join the game
func (plr *Player) sendCurrentData() {
    switch plr.state {
    case SPECTATOR:
        plr.sendControlledEnemyInfo()
    case OFFICER:
        plr.sendActiveNotifications()
        plr.sendCurrentAmmo()
        plr.sendCurrentScore()
    }
}

// Sends the current value of the ammo
func (plr *Player) sendCurrentAmmo() {
    if plr.user == nil {
        return
    }

    msg := make(map[string]interface{})
    msg["type"] = "AMMO"
    msg["data"] = plr.ammo

    plr.sendStateDataUpdate(msg)
}

func (plr *Player) sendCurrentScore() {
    if plr.user == nil {
        return
    }

    msg := make(map[string]interface{})
    msg["type"] = "SCORE"
    msg["data"] = plr.score

    plr.sendStateDataUpdate(msg)
}

// Sends all active notifications
func (plr *Player) sendActiveNotifications() {
    if plr.user == nil {
        return
    }

    toSend := make([]map[string]interface{}, 0)

    for _, comp := range IntercatableComponents {
        notifStatus := notificationMap.getNotifications(comp)
        notifJSON := constructNotificationsForComponent(comp, notifStatus)
        toSend = append(toSend, notifJSON...)
    }

    plr.sendNotifications(toSend)
}

// Sends a notifcation message based on data
func (plr *Player) sendNotifications(data []map[string]interface{}) {
    if plr.user == nil {
        return
    }

    stateData := make(map[string]interface{})
    stateData["type"] = "NOTIFY"
    stateData["data"] = data

    plr.sendStateDataUpdate(stateData)
}

// Sends a user state data update
func (plr *Player) sendSpectatorDataUpdate(enemies map[int64]*Enemy,
    asteroids map[int]*Asteroid) {
    // players with no active user don't need updating
    if plr.user == nil {
        return
    }

    // TODO: add other objects
    enemies_data := make([]map[string]interface{}, 0)
    // Add enemies to the message
    for id, enemy := range enemies {
        name := ""
        tarId := id
        if enemy.isControlled {
            name = enemy.controllingPlayer.userName
            tarId = enemy.controllingPlayer.targetId
        }
        enemies_data = append(enemies_data, map[string]interface{}{
            "id": id,
            "x":  enemy.pos.x,
            "y":  enemy.pos.y,
            "rot":      math.Atan2(enemy.forward.y, enemy.forward.x) - math.Pi/2,
            "isHacked": enemy.isControlled,
            "name": name,
            "targetId": tarId,
        })
    }

    asteroids_data := make([]map[string]interface{}, 0)
    // Add asteroids to the message
    for id, ast := range asteroids {
        asteroids_data = append(asteroids_data, map[string]interface{}{
            "id":    id,
            "x":     ast.pos.x,
            "y":     ast.pos.y,
            "alpha": ast.heightBasedAlpha,
        })
    }

    spriteData := map[string]interface{}{
        "asts": asteroids_data,
        "enms": enemies_data,
    }

    data := make(map[string]interface{})
    data["type"] = "OBJ"
    data["data"] = spriteData

    plr.sendStateDataUpdate(data)
}

// Sends a state update with the provided data
func (plr *Player) sendStateDataUpdate(data map[string]interface{}) {
    if plr.user == nil {
        return
    }

    msg := make(map[string]interface{})
    msg["type"] = "STATE_UPDATE"
    msg["data"] = data

    plr.user.sendMsg(msg)
}

// Send the relative coordinates to which an enemy should move
func (plr *Player) sendMoveCommandToGameServer(data map[string]interface{}) {
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

    plr.targetId = enemyId

    msg := "ATT:"
    msg += strconv.FormatInt(plr.controlledEnemyId, 10) + ","
    msg += strconv.FormatInt(enemyId, 10)

    sendUDPMsgToGameServer(msg)
}

// Send invitation and set up answer function
// On timeout answer as if the player refected the offer
func (plr *Player) inviteAsOfficer(answerC chan bool) {
    // Prepare action that can be triggered only once
    action := sync.Once{}

    // Function that communicates the answer to the inviting funtion
    // and sets the player appropriately
    plr.inviteAnswerAction = func(answer bool) {
        // Execute only once ever
        action.Do(func() {
            answerC <- answer
            plr.processPromotionAnswer(answer)
        })
    }

    // Send invitation
    plr.setState(PROMOTION)

    // If no answer is received consider it a rejection
    time.Sleep(OFFER_VALIDITY_DURATION)
    // Attempt action, it is triggered if no answer was received
    plr.inviteAnswerAction(false)
}

// Deals with state transition based on the answer to the promotion offer
func (plr *Player) processPromotionAnswer(isAccepted bool) {
    if isAccepted {
        playerMap.setPlayerRole(plr, OFFICER)
    } else {
        plr.setState(REJECTED)
    }
}

// Get a string representing the state, used as instruction for the phone
// web page transitions
func (plr *Player) getStateString() (out string) {
    switch plr.state {
    case REJECTED:
        fallthrough
    case SPECTATOR:
        if gameState.status == RUNNING && !gameState.canEnterNextState {
            out = "SPECTATOR"
        } else {
            out = "STANDBY"
        }
    case OFFICER:
        out = "OFFICER"
    case COMMANDER:
        out = "COMMANDER"
    case PROMOTION:
        out = "PROMOTION"
    }

    return
}
