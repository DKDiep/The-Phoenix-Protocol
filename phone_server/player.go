package main

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
    id       string
    userName string
    state    PlayerState
    score    int
    user     *User
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

// Sends a user state data update
func (plr *Player) sendDataUpdate(enemies map[int]*Enemy, asteroids map[int]*Asteroid) {
    // players with no active user don't need updating
    if plr.user == nil {
        return
    }

    // TODO: add other objects
    msg := make(map[string]interface{})
    msg["type"] = "STATE_UPDATE"
    dataSegment := make([]map[string]interface{}, 0)

    // Add enemies to the message
    for _, enemy := range enemies {
        dataSegment = append(dataSegment, map[string]interface{}{
            "type": "ship",
            "position": map[string]interface{}{
                "x": enemy.posX,
                "y": enemy.posY,
            },
        })
    }

    // Add asteroids to the message
    for _, ast := range asteroids {
        dataSegment = append(dataSegment, map[string]interface{}{
            "type": "asteroid",
            "position": map[string]interface{}{
                "x": ast.posX,
                "y": ast.posY,
            },
        })
    }

    msg["data"] = dataSegment
    // incry += 0.1
    // msg := map[string]interface{}{
    //     "type": "STATE_UPDATE",
    //     "data": []map[string]interface{}{
    //         map[string]interface{}{
    //             "type": "ship",
    //             "position": map[string]interface{}{
    //                 "x": 10,
    //                 "y": math.Mod((incry + 14), 100),
    //             },
    //         },
    //         map[string]interface{}{
    //             "type": "debris",
    //             "position": map[string]interface{}{
    //                 "x": 20,
    //                 "y": math.Mod((incry + 53), 100),
    //             },
    //             "size": 10,
    //         },
    //         map[string]interface{}{
    //             "type": "asteroid",
    //             "position": map[string]interface{}{
    //                 "x": 32,
    //                 "y": math.Mod((incry + 15), 100),
    //             },
    //         },
    //     },
    // }

    plr.user.sendMsg(msg)
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
    if gameState.status == RUNNING {
        return SPECTATOR
    } else {
        return STANDBY
    }
}
