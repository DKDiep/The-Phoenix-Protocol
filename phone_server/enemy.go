package main

import (
    "fmt"
)

// Holds enemy data
type Enemy struct {
    pos Point
    forward Point
    isControlled bool
    controllingPlayer *Player
}

// Function of GeometricObject interface
func (enm *Enemy) GetPosObj() *Point {
    return &enm.pos
}

// The collection of all enemies
type EnemyMap struct {
    m      map[int64]*Enemy
    delC   chan int64            // used for requesting the deletion of an enemy
    setC   chan NewEnemy       // used for requesting the updating of an enemy
    ctrlC  chan ControllingPlayer // used for setttin an enemy as controlled
    resetC chan struct{}       // used for clearing out the map
    copyC  chan *EnmCopyExchange // used for getting a copy of the map
}

// Wrapper of enemy data, sent on a channel
type NewEnemy struct {
    id    int64
    enemy *Enemy
}

// Wrapper of id and player used to set controll of enemy
type ControllingPlayer struct {
    id int64
    player *Player
}

// Wrapper of send/receive data on copy channel
type EnmCopyExchange struct {
    plrShipData *PlayerShip
    copy map[int64]*Enemy
}

// Manages concurrent access to the enemy map data structure
func (enemies *EnemyMap) accessManager() {
    fmt.Println("Starting Enemy Map accessManager.")
    for {
        select {
        // deletion of an enemy
        case id := <-enemies.delC:
            enemies.removeAsync(id)
        // setting of enemy values
        case toSet := <-enemies.setC:
            enemies.setAsync(toSet.id, toSet.enemy)
        // set an enemy as being controlled
        case ctrlData := <-enemies.ctrlC:
            success := enemies.setControlledAsync(ctrlData.id, ctrlData.player)
            if success {
                enemies.ctrlC <- ctrlData
            } else {
                // a way to signal failure
                enemies.ctrlC <- ControllingPlayer{player: nil}
            }
        // clears the map
        case <-enemies.resetC:
            enemies.m = make(map[int64]*Enemy)
        // sending of a copy of the map
        case data := <-enemies.copyC:
            data.copy = enemies.getCopyAsync(data.plrShipData)
            enemies.copyC <- data
        }
    }
}

// Request an enemy data update
func (enemies *EnemyMap) set(id int64, data *Enemy) {
    enemies.setC <- NewEnemy{id, data}
}

// Request an enemy deletion
func (enemies *EnemyMap) remove(id int64) {
    enemies.delC <- id
}

// Sets an enemy as being controlled
// return value indicates success or failure
func (enemies *EnemyMap) setControlled(id int64, plr *Player) bool {
    enemies.ctrlC <- ControllingPlayer{id, plr}
    res := <-enemies.ctrlC
    return res.player != nil // if the player in answer is nil, error occured
}

// Request the resetting of the structure
func (enemies *EnemyMap) reset() {
    enemies.resetC <- struct{}{}
}

// Request a copy of the enemy map
func (enemies *EnemyMap) getCopy(plrShip *PlayerShip) map[int64]*Enemy {
    data := &EnmCopyExchange{plrShipData: plrShip}
    enemies.copyC <- data
    result := <-enemies.copyC
    return result.copy
}

// Request an asynchronous enemy data update
// NOTE: DO NOT USE
func (enemies *EnemyMap) setAsync(id int64, toSet *Enemy) {
    if enm, ok := enemies.m[id]; ok {
        enm.pos.x = toSet.pos.x
        enm.pos.y = toSet.pos.y
    } else {
        enemies.m[id] = toSet
    }
}

// Request an asynchronous enemy deletion
// NOTE: DO NOT USE
func (enemies *EnemyMap) removeAsync(id int64) {
    if enm, ok := enemies.m[id]; ok {
        if enm.isControlled {
            enm.controllingPlayer.unsetControlledEnemy()
        }
        delete(enemies.m, id)
    }
}

// Sets asynchronosly an enemy as being controlled
// return value indicates success or failure
// NOTE: DO NOT USE
func (enemies *EnemyMap) setControlledAsync(id int64, plr *Player) bool {
    if enm, ok := enemies.m[id]; ok && !enm.isControlled {
        enm.isControlled = true
        enm.controllingPlayer = plr
        return true
    } else {
        return false
    }
}

// Gets a copy of close enemies asynchronosly
// NOTE: DO NOT USE
func (enemies *EnemyMap) getCopyAsync(plrShip *PlayerShip) map[int64]*Enemy {
    newCopy := make(map[int64]*Enemy)
    for k, v := range enemies.m {
        if isCloseToShip(plrShip, v) {
            enemyCopy := *v
            newCopy[k] = &enemyCopy
        }
    }

    return newCopy
}
