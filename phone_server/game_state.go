package main

import (
    "fmt"
    "time"
)

type GameStateType int

const (
    SETUP GameStateType = iota
    RUNNING
    INIT // init should be used only when this application is ran
)

type GameState struct {
    status         GameStateType
    totalResources int
    updateStopC    chan struct{}
}

// Enters the pre-game setup state
// Clears out all of the relevant structures and
// sends out officer promotion offers
func (gs *GameState) enterSetupState() {
    // some insurance, might remove later
    if gs.status == SETUP {
        return
    }
    gs.status = SETUP
    // stop the periodic updates of game objects
    if gs.updateStopC != nil {
        gs.updateStopC <- struct{}{}
        gs.updateStopC = nil
    }
    // TODO: send last game stats where appropriate
    //clears data structures
    gs.totalResources = 0
    playerShip.reset()
    asteroidMap.reset()
    enemyMap.reset()
    playerMap.resetPlayers()
    // send invitations
    gs.inviteOfficers()
    // TODO: remove this call when admin console is implemented
    go gs.enterRunningState()
}

// Sends out invitations untill there are enough officers
// In the case there aren't enough players it retries every 5 seconds
// Rejects all offers that expire
func (gs *GameState) inviteOfficers() {
    fmt.Println("Setup: Starting to send invites.")
    for len(playerMap.mOfficers) < NUM_OFFICERS {
        var list []*Player
        for {
            list = playerMap.getSortedSpectators()
            if len(list) < (NUM_OFFICERS - len(playerMap.mOfficers)) {
                time.Sleep(5 * time.Second)
                fmt.Println("Setup: Not enough players to invite for officers.")
            } else {
                break
            }
        }
        i := 0
        for i < len(list) && ((NUM_OFFICERS - len(playerMap.mOfficers)) > 0) {
            for j := 0; j < (NUM_OFFICERS - len(playerMap.mOfficers)); j++ {
                if i < len(list) {
                    list[i].setState(PROMOTION)
                    i++
                }
            }
            time.Sleep(OFFER_VALIDITY_DURATION)
            for _, plr := range list {
                if plr.state == PROMOTION {
                    plr.setState(REJECTED)
                }
            }
        }
    }
    if len(playerMap.mOfficers) > NUM_OFFICERS {
        fmt.Printf("Setup: Too many officers added. There are %d instead of %d.\n", len(playerMap.mOfficers), NUM_OFFICERS)
    }
    fmt.Println("Setup: Finished sending invites.")
}

// Enters the game execution state
// Puts all spectators in the spectator game
// and starts the periodic updates of game data
func (gs *GameState) enterRunningState() {
    // some insurance, might remove later
    if gs.status == RUNNING {
        return
    }
    gs.status = RUNNING
    // start the spectator game for all spectators
    playerMap.startSpectators()
    // start the periodic game object updates
    gs.updateStopC = make(chan struct{})
    go updateTimer(gs.updateStopC)
}

// Triggers the sending of state data to mobile clients periodically
func updateTimer(stop chan struct{}) {
    ticker := time.NewTicker(DATA_UPDATE_INTERVAL)
    running := true
    for running {
        select {
        // trigger an update sequence
        case <-ticker.C:
            playerMap.updateC <- struct{}{}
        // stop this goroutine
        case <-stop:
            running = false
        }
    }
}
