package main

import (
    "fmt"
)

// Holds enemy data
type Enemy struct {
    posX float64
    posY float64
    isControlled bool
}

// The collection of all enemies
type EnemyMap struct {
    m      map[uint64]*Enemy
    delC   chan uint64            // channel for requesting the deletion of an enemy
    setC   chan NewEnemy       // channel for requesting the updating of an enemy
    ctrlC  chan uint64         // channel for setttin an enemy as controlled
    resetC chan struct{}       // channel for clearing out the map
    copyC  chan map[uint64]*Enemy // channel for getting a copy of the map
}

// Wrapper of enemy data, sent on a channel
type NewEnemy struct {
    id    uint64
    enemy *Enemy
}

// Manages concurrent access to the enemy map data structure
func (enemies *EnemyMap) accessManager() {
    fmt.Println("Starting Enemy Map accessManager.")
    for {
        select {
        // deletion of an enemy
        case id := <-enemies.delC:
            delete(enemies.m, id)
        // setting of enemy values
        case toSet := <-enemies.setC:
            if enm, ok := enemies.m[toSet.id]; ok {
                enm.posX = toSet.enemy.posX
                enm.posY = toSet.enemy.posY
            } else {
                enemies.m[toSet.id] = toSet.enemy
            }
        // set an enemy as being controlled
        case id := <-enemies.ctrlC:
            if enm, ok := enemies.m[id]; ok && !enm.isControlled {
                enm.isControlled = true
                enemies.ctrlC <- id
            } else {
                enemies.ctrlC <- id+1 // a way to signal failure
            }
        // clears the map
        case <-enemies.resetC:
            enemies.m = make(map[uint64]*Enemy)
        // sending of a copy of the map
        case <-enemies.copyC:
            newCopy := make(map[uint64]*Enemy)
            for k, v := range enemies.m {
                enemyCopy := *v
                newCopy[k] = &enemyCopy
            }
            enemies.copyC <- newCopy
        }
    }
}

// Request an enemy data update
func (enemies *EnemyMap) set(id uint64, data *Enemy) {
    enemies.setC <- NewEnemy{id, data}
}

// Request an enemy deletion
func (enemies *EnemyMap) remove(id uint64) {
    enemies.delC <- id
}

// Sets an enemy as being controlled
// return value indicates success or failure
func (enemies *EnemyMap) setControlled(id uint64) bool {
    enemies.ctrlC <- id
    res := <-enemies.ctrlC
    return res == id // if it doesn't return the same id an error occured
}

// Request the resetting of the structure
func (enemies *EnemyMap) reset() {
    enemies.resetC <- struct{}{}
}

// Request a copy of the enemy map
func (enemies *EnemyMap) getCopy() map[uint64]*Enemy {
    enemies.copyC <- nil
    return <-enemies.copyC
}
