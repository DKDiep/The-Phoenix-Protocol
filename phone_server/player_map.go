package main

import (
    "fmt"
    "math"
    "sort"
)

// The collection of all players, manages concurrent acces
type PlayerMap struct {
    mOfficers   map[string]*Player
    mSpec       map[string]*Player
    addC        chan *Player
    setOfficerC chan *Player   // moves a player in the officer list
    plrC        chan struct{}  // used for player specific actions
    resetC      chan struct{}  // prepares the maps for a new game
    startC      chan struct{}  // triggers state transitions for spectators
    sortlC      chan []*Player // used for receiving a sorted list of spectators
    updateC     chan struct{}  // channel for triggering the broadcast of up to date data
}

// A wrapper around data needed for user addition
type NewPlr struct {
    id  string
    plr *Player
}

// Struct and functions used to sort a list of Players
type SortedPlayers []*Player

func (a SortedPlayers) Len() int {
    return len(a)
}

func (a SortedPlayers) Swap(i, j int) {
    a[i], a[j] = a[j], a[i]
}

func (a SortedPlayers) Less(i, j int) bool {
    return a[i].score < a[j].score
}

// Manages concurrent access to the player map data structure
func (players *PlayerMap) accessManager() {
    fmt.Println("Starting Player Map accessManager.")
    for {
        select {
        // addition of player
        case new := <-players.addC:
            players.mSpec[new.id] = new
        // move a player from the spectator map into the officer map
        case officer := <-players.setOfficerC:
            delete(players.mSpec, officer.id)
            players.mOfficers[officer.id] = officer
        // blocks the manager, used for user specific actions
        case <-players.plrC:
            <-players.plrC
        // resets the players
        case <-players.resetC:
            for k, v := range players.mOfficers {
                players.mSpec[k] = v
                v.score = 0
                delete(players.mOfficers, k)
            }
            for _, v := range players.mSpec {
                v.setState(STANDBY)
            }
            players.resetC <- struct{}{}
        // starts the spectator game for all spectators
        case <-players.startC:
            for _, v := range players.mSpec {
                v.setState(SPECTATOR)
            }
        // request a sorted list of all players
        case <-players.sortlC:
            list := make([]*Player, 0)
            for _, v := range players.mSpec {
                if v.state == STANDBY {
                    list = append(list, v)
                }
            }
            sort.Sort(sort.Reverse(SortedPlayers(list)))
            players.sortlC <- list
        // request to update all users
        case <-players.updateC:
            players.updateData()
        }
    }
}

// Wrapper used for user addition
func (players *PlayerMap) add(plr *Player) {
    players.addC <- plr
}

// Wrapper used for placing a player in the officer map
func (players *PlayerMap) setOfficer(plr *Player) {
    players.setOfficerC <- plr
}

// Wrapper used for retrieving a user
func (players *PlayerMap) get(playerId string) *Player {
    players.plrC <- struct{}{}
    plr := players.mSpec[playerId]
    if plr == nil {
        plr = players.mOfficers[playerId]
    }
    players.plrC <- struct{}{}
    return plr
}

// Wrapper used for retrieving a list of players sorted by score
func (players *PlayerMap) getSortedSpectators() []*Player {
    players.sortlC <- nil
    return <-players.sortlC
}

// Wrapper used for resetting the players at the start of a new game
func (players *PlayerMap) resetPlayers() {
    players.resetC <- struct{}{}
    <-players.resetC // wait for confirmation the the action is finished
}

func (players *PlayerMap) startSpectators() {
    players.startC <- struct{}{}
}

// Sends a state data update to all spectators
func (players *PlayerMap) updateData() {
    playerShipData := playerShip.getShipData()
    enemyData := enemyMap.getCopy()
    asteroidData := asteroidMap.getCopy()

    // Transform asteroid coordinates into phone screen space
    for _, asteroid := range asteroidData {
        // Centre grid around player ship
        asteroid.posX -= playerShipData.posX
        asteroid.posY -= playerShipData.posY
        // Rotate grid so ship is pointing north on the screen
        newX := asteroid.posX*math.Cos((playerShipData.rot)*(math.Pi/180)) - asteroid.posY*math.Sin((playerShipData.rot)*(math.Pi/180))
        newY := asteroid.posX*math.Sin((playerShipData.rot)*(math.Pi/180)) + asteroid.posY*math.Cos((playerShipData.rot)*(math.Pi/180))
        // Translate grid so ship is in the centre
        asteroid.posX = newX + 50
        asteroid.posY = -(newY) + 50 // flip Y to match rendering orientation
    }

    // Transform enemy coordinates into phone screen space
    for _, enemy := range enemyData {
        // Centre grid around player ship
        enemy.posX -= playerShipData.posX
        enemy.posY -= playerShipData.posY
        // Rotate grid so ship is pointing north on the screen
        newX := enemy.posX*math.Cos((playerShipData.rot)*(math.Pi/180)) - enemy.posY*math.Sin((playerShipData.rot)*(math.Pi/180))
        newY := enemy.posX*math.Sin((playerShipData.rot)*(math.Pi/180)) + enemy.posY*math.Cos((playerShipData.rot)*(math.Pi/180))
        // Translate grid so ship is in the centre
        enemy.posX = newX + 50
        enemy.posY = -(newY) + 50 // flip Y to match rendering orientation
    }

    // Send updated data
    for _, plr := range players.mSpec {
        plr.sendDataUpdate(enemyData, asteroidData)
    }
}
