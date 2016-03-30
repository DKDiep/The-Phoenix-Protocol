package main

import (
    "fmt"
    "math"
    "sort"
)

// The collection of all players, manages concurrent acces
type PlayerMap struct {
    mOfficers     map[uint64]*Player
    mSpec         map[uint64]*Player
    addC          chan *Player
    setOfficerC   chan *Player      // moves a player in the officer list
    setSpectatorC chan *Player      // moves a player in the officer list
    plrC          chan struct{}     // used for player specific actions
    resetC        chan struct{}     // prepares the maps for a new game
    startC        chan struct{}     // triggers state transitions for spectators
    sortlC        chan []*Player    // used for receiving a sorted list of spectators
    listC         chan []PlayerInfo // used to get 2 list of officers and spectators
    updateC       chan struct{}     // channel for triggering the broadcast of up to date data
}

// Interface specifying a type that has 3 coordinates
type GeometricObject interface {
    GetPosObj() *Point
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
        // move a player from the spectator map into the officer map
        case spectator := <-players.setSpectatorC:
            delete(players.mOfficers, spectator.id)
            players.mSpec[spectator.id] = spectator
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
        // request a sorted list of all players on standby
        case <-players.sortlC:
            list := make([]*Player, 0, 10)
            for _, v := range players.mSpec {
                if v.state == STANDBY {
                    list = append(list, v)
                }
            }
            sort.Sort(sort.Reverse(SortedPlayers(list)))
            players.sortlC <- list
        // gets unordered lists of the players
        case <-players.listC:
            players.listC <- getPlayerInfoList(players.mOfficers)
            players.listC <- getPlayerInfoList(players.mSpec)
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

// Wrapper used for placing a player in the spectator map
func (players *PlayerMap) setSpectator(plr *Player) {
    players.setSpectatorC <- plr
}

// Wrapper used for retrieving a user
func (players *PlayerMap) get(playerId uint64) *Player {
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

// Wrapper used for retrieving a list of officers and a list of spectators
func (players *PlayerMap) getPlayerLists() ([]PlayerInfo, []PlayerInfo) {
    players.listC <- nil
    officers := <-players.listC
    spectators := <-players.listC

    return officers, spectators
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
    for id, asteroid := range asteroidData {
        // Centre grid around player ship
        centerAroundShip(playerShipData, asteroid)
        // Project onto plane intersecting the ship front and right
        projectOnShipPlane(playerShipData, asteroid)
        // TODO: nasty way of removing far asteroids, needs improving
        if math.Abs(asteroid.pos.x) > 250 || math.Abs(asteroid.pos.y) > 250 {
            delete(asteroidData, id)
        }
    }

    // Transform enemy coordinates into phone screen space
    for _, enemy := range enemyData {
        // Centre grid around player ship
        centerAroundShip(playerShipData, enemy)
        // Project onto plane intersecting the ship's front and right
        projectOnShipPlane(playerShipData, enemy)
        // Set the enemy orientation to the projected one
        // projectEnemyDirections(playerShipData, enemy)
    }

    // Send updated data
    for _, plr := range players.mSpec {
        plr.sendDataUpdate(enemyData, asteroidData)
    }
}

// Translate so that player ship is point in the center of grid
func centerAroundShip(origin *PlayerShip, obj GeometricObject) {
    position := obj.GetPosObj()
    position.x -= origin.pos.x
    position.y -= origin.pos.y
    position.z -= origin.pos.z
}

// Project onto the plane of the ship
func projectOnShipPlane(origin *PlayerShip, obj GeometricObject) {
    position := obj.GetPosObj()
    newX := position.x*origin.right.x + position.y*origin.right.y + position.z*origin.right.z
    newY := position.x*origin.forward.x + position.y*origin.forward.y + position.z*origin.forward.z
    position.x = newX
    position.y = newY
    position.z = 0
}

// Rotate enemy directions so that they are correct in
// the space where the ship is facing upwards
// TODO: Doesn't work
func projectEnemyDirections(origin *PlayerShip, enm *Enemy) {
    // Project
    newX := enm.forward.x*origin.right.x + enm.forward.y*origin.right.y + enm.forward.z*origin.right.z
    newY := enm.forward.x*origin.forward.x + enm.forward.y*origin.forward.y + enm.forward.z*origin.forward.z

    mag := math.Sqrt(newX*newX + newY*newY)
    if(mag == 0) {
        mag = 1
    }
    enm.forward.x = newX/mag
    enm.forward.y = newY/mag

    // newX := enm.forward.x*origin.forward.y - enm.forward.y*origin.forward.x
    // newY := enm.forward.x*origin.forward.x + enm.forward.y*origin.forward.y
    //
    // enm.forward.x = newX
    // enm.forward.y = newY
    enm.forward.z = 0
}

// Get an unordered list of Player info from the provided map
func getPlayerInfoList(m map[uint64]*Player) []PlayerInfo {
    list := make([]PlayerInfo, 0, 10)
    for _, plr := range m {
        newInfo := PlayerInfo{IsOnline: false}
        newInfo.UserName = plr.userName
        newInfo.UserId = plr.id
        newInfo.Score = plr.score
        if plr.user != nil {
            newInfo.IsOnline = true
        }
        list = append(list, newInfo)
    }

    return list
}
