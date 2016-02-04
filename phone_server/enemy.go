package main

import (
    "fmt"
)

type Enemy struct {
    posX float64
    posY float64
}

type EnemyMap struct {
    m     map[int]*Enemy
    delC  chan int
    setC  chan NewEnemy
    copyC chan map[int]*Enemy
}

type NewEnemy struct {
    id  int
    enemy *Enemy
}

// Manages concurrent access to the enemy map data structure
func (enemies *EnemyMap) accessManager() {
    fmt.Println("Starting Enemy accessManager.")
    for {
        select {
        case id := <-enemies.delC:
            delete(enemies.m, id)
        case toSet := <-enemies.setC:
            enemies.m[toSet.id] = toSet.enemy
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

func (enemies *EnemyMap) set(id int, data *Enemy) {
    enemies.setC <- NewEnemy{id, data}
}

func (enemies *EnemyMap) remove(id int) {
    enemies.delC <- id
}
