package main

import (
    "fmt"
)

// Holds asteroid data
type Asteroid struct {
    pos              Point
    heightBasedAlpha float64
}

// Function of GeometricObject interface
func (ast *Asteroid) GetPosObj() *Point {
    return &ast.pos
}

// The collection of all asteroids
type AsteroidMap struct {
    m      map[int]*Asteroid
    delC   chan int              // channel for requesting the deletion of an asteroid
    addC   chan NewAst           // channel for requesting the addition of an asteroid
    resetC chan struct{}         // channele for clearing out the map
    copyC  chan *AstCopyExchange // channel for getting a copy of the map
}

// Wrapper of asteroid data, sent on a channel
type NewAst struct {
    id  int
    ast *Asteroid
}

// Wrapper of send/receive data on copy channel
type AstCopyExchange struct {
    plrShipData *PlayerShip
    copy        map[int]*Asteroid
}

// Manages concurrent access to the asteroid map data structure
func (asteroids *AsteroidMap) accessManager() {
    fmt.Println("Starting Asteroid Map accessManager.")
    for {
        asteroids.handleAccess()
    }
}

// Handle a single access request
func (asteroids *AsteroidMap) handleAccess() {
    defer func() {
        if r := recover(); r != nil {
            fmt.Println("Asteroid Map: Runtime panic:", r)
        }
    }()

    select {
    // deletion of asteroid
    case id := <-asteroids.delC:
        delete(asteroids.m, id)
    // addition of asteroid
    case new := <-asteroids.addC:
        asteroids.m[new.id] = new.ast
    // clears the map
    case <-asteroids.resetC:
        asteroids.m = make(map[int]*Asteroid)
    // sending a copy of the map
    case data := <-asteroids.copyC:
        data.copy = asteroids.getCopyAsync(data.plrShipData)
        asteroids.copyC <- data
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

// Request a full reset of the data structure
func (asteroids *AsteroidMap) reset() {
    asteroids.resetC <- struct{}{}
}

// Request a copy of close asteroids
func (asteroids *AsteroidMap) getCopy(plrShip *PlayerShip) map[int]*Asteroid {
    data := &AstCopyExchange{plrShipData: plrShip}
    asteroids.copyC <- data
    result := <-asteroids.copyC
    return result.copy
}

// Gets a copy of close asteroids asynchronosly
// NOTE: DO NOT USE
func (asteroids *AsteroidMap) getCopyAsync(plrShip *PlayerShip) map[int]*Asteroid {
    newCopy := make(map[int]*Asteroid)
    for k, v := range asteroids.m {
        if asteroidIsCloseToDraw(plrShip, v) {
            astCopy := *v
            newCopy[k] = &astCopy
        }
    }

    return newCopy
}
