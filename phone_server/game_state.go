package main

import (
    "fmt"
    "strconv"
    "time"
)

type GameStateType int

const (
    SETUP GameStateType = iota
    RUNNING
    INVITAION
)

type GameState struct {
    status            GameStateType
    canEnterNextState bool
    updateStopC       chan struct{}
}

// Enters the pre-game setup state
// Clears out all of the relevant structures and
// sends out officer promotion offers
func (gs *GameState) enterSetupState() {
    // some insurance, might remove later
    if gs.status == SETUP {
        return
    }

    // if we can't notify the Game Server do nothing
    if !sendSignalToGameServer(RESET_GAME, "") {
        return
    }

    gs.status = SETUP
    gs.canEnterNextState = false

    // stop the periodic updates of game objects
    if gs.updateStopC != nil {
        gs.updateStopC <- struct{}{}
        gs.updateStopC = nil
    }
    // TODO: send last game stats where appropriate
    // clears data structures
    playerShip.reset()
    asteroidMap.reset()
    enemyMap.reset()
    notificationMap.reset()
    playerMap.resetPlayers()
    gs.canEnterNextState = true
}

func (gs *GameState) enterInvitationState() {
    // some insurance, might remove later
    if gs.status == INVITAION {
        return
    }
    gs.status = INVITAION
    gs.canEnterNextState = false

    // send invitations
    go gs.inviteOfficers()
}

// Sends out invitations untill there are enough officers
// In the case there aren't enough players it retries every 5 seconds
// Rejects all offers that expire
func (gs *GameState) inviteOfficers() {
    fmt.Println("Setup: Starting to send invites.")
    for len(playerMap.mOfficers) < NUM_OFFICERS {
        numNeededOfficers := NUM_OFFICERS - len(playerMap.mOfficers)
        var list []*Player
        for {
            list = playerMap.getSortedOnlineSpectators()
            if len(list) < numNeededOfficers {
                time.Sleep(3 * time.Second)
                fmt.Println("Setup: Not enough players to invite for officers.")
            } else {
                break
            }
        }
        // Send invites
        answersC := make(chan bool, numNeededOfficers)
        for i := 0; i < len(list) && i < numNeededOfficers; i++ {
            go list[i].inviteAsOfficer(answersC)
        }
        // Wait for answers
        for i := 0; i < len(list) && i < numNeededOfficers; i++ {
            <-answersC
        }
    }
    if len(playerMap.mOfficers) > NUM_OFFICERS {
        fmt.Printf("Setup: Too many officers added. There are %d instead of %d.\n", len(playerMap.mOfficers), NUM_OFFICERS)
    }
    fmt.Println("Setup: Finished sending invites.")

    gs.canEnterNextState = true
}

// Enters the game execution state
// Puts all spectators in the spectator game
// and starts the periodic updates of game data
func (gs *GameState) startGame(teamName string) {
    if gs.status == RUNNING {
        return
    }

    // if we can't notify the Game Server do nothing
    if !sendSignalToGameServer(START_GAME, teamName) {
        return
    }

    gs.status = RUNNING
    gs.canEnterNextState = false

    // Log which officers are in the game in the DB
    playerMap.logOfficers()
    // start the spectator game for all spectators
    playerMap.startPlayers()
    // start the periodic game object updates
    gs.updateStopC = make(chan struct{})
    go doPeriodicUpdates(gs.updateStopC)
}

// Handles the setting of a player into a spectator or officer
func (gs *GameState) processAdminSetSignal(id uint64, state PlayerState) {
    plr := playerMap.get(id)
    if plr == nil {
        fmt.Println("Admin: Invalid playerId: " + strconv.FormatUint(id, 10))
        return
    }

    playerMap.setPlayerRole(plr, state)
}

// Runs all necessary periodic updates until stopped
func doPeriodicUpdates(stop chan struct{}) {
    stateDataUpdC := make(chan struct{})
    dbLogScoresC := make(chan struct{})

    // Start periodic updates
    go periodicUpdate(playerMap.updatePlayerStates, DATA_UPDATE_INTERVAL,
        stateDataUpdC)
    go periodicUpdate(playerMap.logScores, DATABASE_LOG_SCORES_PERIOD,
        dbLogScoresC)

    // Wait for stop signal
    <-stop

    // stop all periodic updates
    stateDataUpdC <- struct{}{}
    dbLogScoresC <- struct{}{}
}

// Call an update function periodically until something is sent
// on the stop chan
func periodicUpdate(updateFunc func(), period time.Duration,
    stop chan struct{}) {
    ticker := time.NewTicker(period)
    running := true
    for running {
        select {
        // trigger an update sequence
        case <-ticker.C:
            updateFunc()
        // stop this goroutine
        case <-stop:
            running = false
        }
    }
}
