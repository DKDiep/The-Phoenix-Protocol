package main

import (
    "fmt"
)

// Holds asteroid data
type Asteroid struct {
    posX float64
    posY float64
}

// The collection of all asteroids
type AsteroidMap struct {
    m     map[int]*Asteroid
    delC  chan int // channel for requesting the deletion of an asteroid
    addC  chan NewAst // channel for requesting the addition of an asteroid
    copyC chan map[int]*Asteroid // channel for getting a copy of the map
}

// Wrapper of asteroid data, sent on a channel
type NewAst struct {
    id  int
    ast *Asteroid
}

// Manages concurrent access to the asteroid map data structure
func (asteroids *AsteroidMap) accessManager() {
    fmt.Println("Starting Asteroid Map accessManager.")
    for {
        select {
        // deletion of asteroid
        case id := <-asteroids.delC:
            delete(asteroids.m, id)
        // addition of asteroid
        case new := <-asteroids.addC:
            asteroids.m[new.id] = new.ast
        // sending a copy of the map
        case <-asteroids.copyC:
            newCopy := make(map[int]*Asteroid)
            for k, v := range asteroids.m {
                astCopy := *v
                newCopy[k] = &astCopy
            }
            asteroids.copyC <- newCopy
        }
    }
}

// Request an asteroid addition
func (asteroids *AsteroidMap) add(id int, data *Asteroid) {
    asteroids.addC <- NewAst{id, data}
}

// Request an asteroid deletion
func (asteroids *AsteroidMap) remove(id int) {
    asteroids.delC <- id
}

// Request a copy of the asteroid map
// TODO: get a copy of  the asteroids that are whithin a certain distance
func (asteroids *AsteroidMap) getCopy() map[int]*Asteroid{
    asteroids.copyC <- nil
    return <-asteroids.copyC
}
