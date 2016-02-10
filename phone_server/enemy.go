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
    m     map[int]*Enemy
    delC  chan int            // channel for requesting the deletion of an enemy
    setC  chan NewEnemy       // channel for requesting the updating of an enemy
    copyC chan map[int]*Enemy // channel for getting a copy of the map
}

// Wrapper of enemy data, sent on a channel
type NewEnemy struct {
    id    int
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
            enemies.m[toSet.id] = toSet.enemy
        // sending of a copy of the map
        case <-enemies.copyC:
            newCopy := make(map[int]*Enemy)
            for k, v := range enemies.m {
                enemyCopy := *v
                newCopy[k] = &enemyCopy
            }
            enemies.copyC <- newCopy
        }
    }
}

// Request an enemy data update
func (enemies *EnemyMap) set(id int, data *Enemy) {
    enemies.setC <- NewEnemy{id, data}
}

// Request an enemy deletion
func (enemies *EnemyMap) remove(id int) {
    enemies.delC <- id
}

// Request a copy of the enemy map
func (enemies *EnemyMap) getCopy() map[int]*Enemy {
    enemies.copyC <- nil
    return <-enemies.copyC
}
