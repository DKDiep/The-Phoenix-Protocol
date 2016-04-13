package main

import (
    "fmt"
    "math"
)

type Point struct {
    x float64
    y float64
    z float64
}

// Player ship data holding structure
type PlayerShip struct {
    pos     Point
    forward Point
    right   Point
    up      Point
}

// Function of GeometricObject interface
func (plrShp *PlayerShip) GetPosObj() *Point {
    return &plrShp.pos
}

// Wrapper around the player ship object handling concurrency
type PlayerShipController struct {
    data   *PlayerShip
    setC   chan *PlayerShip // channel for requesting the updating of ship data
    getC   chan *PlayerShip // channel for getting a copy of the ship data
    resetC chan struct{}
}

// Manages concurrent access to the player ship data structure
func (plrShip *PlayerShipController) accessManager() {
    fmt.Println("Starting Player Ship accessManager.")
    for {
        select {
        // setting of the ship data
        case new := <-plrShip.setC:
            plrShip.data = new
        // sending a copy of the ship data
        case <-plrShip.getC:
            toSend := *plrShip.data
            plrShip.getC <- &toSend
        // clears out the ship data
        case <-plrShip.resetC:
            plrShip.data = &PlayerShip{}
        }
    }
}

// Set the ship data to new values
func (plrShip *PlayerShipController) setShipData(data *PlayerShip) {
    plrShip.setC <- data
}

// Request a copy of the ship data
func (plrShip *PlayerShipController) getShipData() *PlayerShip {
    plrShip.getC <- nil
    return <-plrShip.getC
}

// Request the reset of the data structure
func (plrShip *PlayerShipController) reset() {
    plrShip.resetC <- struct{}{}
}

// Function to check if an enemy is within the range in which we draw
func enemyIsCloseToDraw(plrShip *PlayerShip, obj GeometricObject) bool {
    if distanceAlongDirection(plrShip, obj, plrShip.forward) > ENEMY_DRAW_RANGE_X ||
        distanceAlongDirection(plrShip, obj, plrShip.right) > ENEMY_DRAW_RANGE_Y ||
        distanceAlongDirection(plrShip, obj, plrShip.up) > ENEMY_DRAW_RANGE_Z {

        return false
    } else {
        return true
    }
}

// Function to check if an object is within the range in which we draw
func asteroidIsCloseToDraw(plrShip *PlayerShip, obj GeometricObject) bool {
    if distanceAlongDirection(plrShip, obj, plrShip.forward) > ASTEROID_DRAW_RANGE_X ||
        distanceAlongDirection(plrShip, obj, plrShip.right) > ASTEROID_DRAW_RANGE_Y ||
        distanceAlongDirection(plrShip, obj, plrShip.up) > ASTEROID_DRAW_RANGE_Z {

        return false
    } else {
        return true
    }
}

// Get the difference of two vectors along a direction
// Direction must be a unit vector
func distanceAlongDirection(objA GeometricObject, objB GeometricObject,
    direction Point) float64 {

    posA := objA.GetPosObj()
    posB := objB.GetPosObj()

    lenA := posA.x*direction.x + posA.y*direction.y + posA.z*direction.z
    lenB := posB.x*direction.x + posB.y*direction.y + posB.z*direction.z

    return math.Abs(lenA - lenB)
}
