package main

import (
    "fmt"
    "math"
)

// The collection of all players, manages concurrent acces
type PlayerMap struct {
    m       map[string]*Player
    addC    chan NewPlr
    plrC    chan struct{} // used for player specific actions
    updateC chan struct{} // channel for triggering the broadcast of up to date data
}

// A wrapper around data needed for user addition
type NewPlr struct {
    id  string
    plr *Player
}

// Manages concurrent access to the player map data structure
func (players *PlayerMap) accessManager() {
    fmt.Println("Starting Player Map accessManager.")
    for {
        select {
        // addition of player
        case new := <-players.addC:
            players.m[new.id] = new.plr
        // blocks the manager, used for user specific actions
        case <-players.plrC:
            <-players.plrC
        // request to update all users
        case <-players.updateC:
            players.updateData()
        }
    }
}

// Wrapper used for user addition
func (players *PlayerMap) add(id string, plr *Player) {
    players.addC <- NewPlr{id, plr}
}

// Wrapper used for retrieving a user
func (players *PlayerMap) get(playerId string) *Player {
    players.plrC <- struct{}{}
    plr := players.m[playerId]
    players.plrC <- struct{}{}
    return plr
}

// Sends a state data update to all players
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
    for _, plr := range players.m {
        plr.sendDataUpdate(enemyData, asteroidData)
    }
}
