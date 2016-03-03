package main

import (
    "fmt"
)

// Holds enemy data
type Enemy struct {
    posX float64
    posY float64
}

// The collection of all enemies
type EnemyMap struct {
    m      map[int64]*Enemy
    delC   chan int64            // channel for requesting the deletion of an enemy
    setC   chan NewEnemy       // channel for requesting the updating of an enemy
    resetC chan struct{}       // channele for clearing out the map
    copyC  chan map[int64]*Enemy // channel for getting a copy of the map
}

// Wrapper of enemy data, sent on a channel
type NewEnemy struct {
    id    int64
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
        // clears the map
        case <-enemies.resetC:
            enemies.m = make(map[int64]*Enemy)
        // sending of a copy of the map
        case <-enemies.copyC:
            newCopy := make(map[int64]*Enemy)
            for k, v := range enemies.m {
                enemyCopy := *v
                newCopy[k] = &enemyCopy
            }
            enemies.copyC <- newCopy
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

// Request the resetting of the structure
func (enemies *EnemyMap) reset() {
    enemies.resetC <- struct{}{}
}

// Request a copy of the enemy map
func (enemies *EnemyMap) getCopy() map[int64]*Enemy {
    enemies.copyC <- nil
    return <-enemies.copyC
}
