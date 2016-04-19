package main

import (
    "fmt"
    "math"
    "sort"
)

// The collection of all players, manages concurrent acces
type PlayerMap struct {
    commander    *Player
    mOfficers    map[uint64]*Player
    mSpec        map[uint64]*Player
    addC         chan *Player
    setRoleC     chan SetPlr    // moves a player in the officer list
    plrC         chan struct{}  // used for player specific actions
    resetC       chan struct{}  // prepares the maps for a new game
    startC       chan struct{}  // triggers state transitions for spectators
    sortlC       chan []*Player // used for receiving a sorted list of spectators
    listC        chan struct{}  // used to get 2 list of officers and spectators
    updateC      chan struct{}  // channel for triggering the broadcast of up to date data
    logScoresC   chan struct{}  // chan for logging scores
    logOfficersC chan struct{}  // chan for setting officers in the database
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

// Wrapped of data sent on the set role channel
type SetPlr struct {
    role PlayerState
    plr  *Player
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
        case data := <-players.setRoleC:
            players.setPlayerRoleAsync(data.plr, data.role)
        // blocks the manager, used for user specific actions
        case <-players.plrC:
            <-players.plrC
        // resets the players
        case <-players.resetC:
            players.resetPlayersAsync()
            players.resetC <- struct{}{}
        // starts the spectator game for all spectators
        case <-players.startC:
            players.startPlayersAsync()
        // request a sorted list of all players on standby
        case <-players.sortlC:
            players.sortlC <- players.getSortedOnlineSpectatorsAsync()
        // gets unordered lists of the players
        case <-players.listC:
            <-players.listC
        // request to update all users
        case <-players.updateC:
            players.updateSpectatorData()
        // log player scores to the database
        case <-players.logScoresC:
            players.logScoresAsync()
        case <-players.logOfficersC:
            players.logOfficersAsync()
        }
    }
}

// Wrapper used for user addition
func (players *PlayerMap) add(plr *Player) {
    players.addC <- plr
}

// Wrapper used for placing a player in the respective role map
func (players *PlayerMap) setPlayerRole(player *Player, setRole PlayerState) {
    players.setRoleC <- SetPlr{plr: player, role: setRole}
}

// Wrapper used for retrieving a user
func (players *PlayerMap) get(playerId uint64) *Player {
    players.plrC <- struct{}{}
    var plr *Player = nil
    if players.commander != nil && players.commander.id == playerId {
        plr = players.commander
    }
    if plr == nil {
        plr = players.mOfficers[playerId]
    }
    if plr == nil {
        plr = players.mSpec[playerId]
    }
    players.plrC <- struct{}{}
    return plr
}

// Wrapper used for retrieving a list of players sorted by score
func (players *PlayerMap) getSortedOnlineSpectators() []*Player {
    players.sortlC <- nil
    return <-players.sortlC
}

// Wrapper used for retrieving a list of officers and a list of spectators
func (players *PlayerMap) getPlayerLists() ([]PlayerInfo, []PlayerInfo,
    PlayerInfo) {
    players.listC <- struct{}{}
    officers := getPlayerInfoListAsync(players.mOfficers)
    spectators := getPlayerInfoListAsync(players.mSpec)
    commander := constructPlayerInfo(players.commander)
    players.listC <- struct{}{}

    return officers, spectators, commander
}

// Wrapper used for resetting the players at the start of a new game
func (players *PlayerMap) resetPlayers() {
    players.resetC <- struct{}{}
    <-players.resetC // wait for confirmation the the action is finished
}

// Start the spectator game for all spectators
func (players *PlayerMap) startPlayers() {
    players.startC <- struct{}{}
}

// Send any necessary up to date data about the Game State to the players
func (players *PlayerMap) updatePlayerStates() {
    players.updateC <- struct{}{}
}

// Logs the player scores in the database
func (players *PlayerMap) logScores() {
    players.logScoresC <- struct{}{}
}

// Logs the player scores in the database
func (players *PlayerMap) logOfficers() {
    players.logOfficersC <- struct{}{}
}

// Sets the player in the respective role map
// NOTE: DO NOT USE
func (players *PlayerMap) setPlayerRoleAsync(plr *Player, role PlayerState) {
    // Failsafe
    if plr == nil {
        return
    }
    switch role {
    case OFFICER:
        delete(players.mSpec, plr.id)
        if players.commander == plr {
            players.commander = nil
        }
        players.mOfficers[plr.id] = plr
    case SPECTATOR:
        delete(players.mOfficers, plr.id)
        if players.commander == plr {
            players.commander = nil
        }
        players.mSpec[plr.id] = plr
    case COMMANDER:
        if players.commander != nil {
            players.setPlayerRoleAsync(players.commander, SPECTATOR)
        }
        players.commander = plr
        delete(players.mOfficers, plr.id)
        delete(players.mSpec, plr.id)
    }
    plr.setState(role)
}

// Wrapper used for retrieving a list of players sorted by score
// NOTE: DO NOT USE
func (players *PlayerMap) getSortedOnlineSpectatorsAsync() []*Player {
    list := make([]*Player, 0, 10)
    for _, v := range players.mSpec {
        if v.state == SPECTATOR && v.user != nil {
            list = append(list, v)
        }
    }
    sort.Sort(sort.Reverse(SortedPlayers(list)))

    return list
}

// Wrapper used for resetting the players at the start of a new game
// NOTE: DO NOT USE
func (players *PlayerMap) resetPlayersAsync() {
    gameDatabase.resetOfficers()
    // Reset officers
    for k, v := range players.mOfficers {
        players.mSpec[k] = v
        v.score = 0
        delete(players.mOfficers, k)
    }
    // Reset commander
    if players.commander != nil {
        players.mSpec[players.commander.id] = players.commander
        players.commander = nil
    }
    // Reset all spectator states, puts them on standby as well
    for _, v := range players.mSpec {
        v.setState(SPECTATOR)
    }
}

// Start the spectator game for all spectators
// NOTE: DO NOT USE
func (players *PlayerMap) startPlayersAsync() {
    // if players.commander != nil {
    //     players.commander.setState(COMMANDER)
    // }
    // for _, v := range players.mOfficers {
    //     v.setState(OFFICER)
    // }
    for _, v := range players.mSpec {
        v.setState(SPECTATOR)
    }
}

// Log the player scores in the database asynchronously
// NOTE: DO NOT USE
func (players *PlayerMap) logScoresAsync() {
    gameDatabase.logPlayerScores(players.mSpec, players.mOfficers)
}

// Log which players are officers
// NOTE: DO NOT USE
func (players *PlayerMap) logOfficersAsync() {
    // Set player score locally to match database reset
    for _, plr := range players.mOfficers {
        plr.score = 0
    }
    gameDatabase.setOfficers(players.mOfficers)
}

// Get an unordered list of Player info from the provided map
// NOTE: DO NOT USE
func getPlayerInfoListAsync(m map[uint64]*Player) []PlayerInfo {
    list := make([]PlayerInfo, 0, 10)
    for _, plr := range m {
        newInfo := constructPlayerInfo(plr)
        list = append(list, newInfo)
    }

    return list
}

// COnstructs a PlayerInfo instance from a player
func constructPlayerInfo(plr *Player) PlayerInfo {
    newInfo := PlayerInfo{IsOnline: false, UserId: -1}
    if plr == nil {
        return newInfo
    }

    newInfo.UserName = plr.userName
    newInfo.UserId = int64(plr.id)
    newInfo.Score = plr.score
    if plr.user != nil {
        newInfo.IsOnline = true
    }

    return newInfo
}

// Sends a single notification to all officers
func (players *PlayerMap) sendNotificationUpdateToOfficers(updateData []map[string]interface{}) {
    players.plrC <- struct{}{}
    for _, plr := range players.mOfficers {
        plr.sendNotifications(updateData)
    }
    players.plrC <- struct{}{}
}

// Sends a state data update to all spectators
func (players *PlayerMap) updateSpectatorData() {
    playerShipData := playerShip.getShipData()
    enemyData := enemyMap.getCopy(playerShipData)
    asteroidData := asteroidMap.getCopy(playerShipData)

    // Transform enemy coordinates into phone screen space
    for _, enemy := range enemyData {
        // Centre grid around player ship
        centerAroundShip(playerShipData, enemy)
        // Project onto plane intersecting the ship's front and right
        projectOnShipPlane(playerShipData, enemy)
        // Set the enemy orientation to the projected one
        projectEnemyDirections(playerShipData, enemy)
    }

    // Transform asteroid coordinates into phone screen space
    for _, asteroid := range asteroidData {
        asteroid.heightBasedAlpha = calculateAsteroidAlpha(playerShipData, asteroid)
        // Centre grid around player ship
        centerAroundShip(playerShipData, asteroid)
        // Project onto plane intersecting the ship front and right
        projectOnShipPlane(playerShipData, asteroid)
    }

    // Send updated data
    for _, plr := range players.mSpec {
        plr.sendSpectatorDataUpdate(enemyData, asteroidData)
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
    newX := enm.right.x*origin.right.x + enm.right.y*origin.right.y + enm.right.z*origin.right.z
    newY := enm.right.x*origin.forward.x + enm.right.y*origin.forward.y + enm.right.z*origin.forward.z

    mag := math.Sqrt(newX*newX + newY*newY)
    if mag == 0 {
        mag = 1
    }
    enm.right.x = newX / mag
    enm.right.y = newY / mag
    enm.right.z = 0
}

// Calculate the asteroid alpha in range 0 - 1 based on max allowed
// height difference
func calculateAsteroidAlpha(plrShip *PlayerShip, ast GeometricObject) float64 {
    return (1 - (distanceAlongDirection(plrShip, ast, plrShip.up) / ASTEROID_DRAW_RANGE_Z))
}
