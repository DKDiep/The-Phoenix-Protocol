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

// Function to check if an object is withing the range in which we project
func isCloseToShip(plrShip *PlayerShip, obj GeometricObject) bool {
    position := obj.GetPosObj()
    if math.Abs(position.x-plrShip.pos.x) > PROJECTION_RANGE ||
        math.Abs(position.y-plrShip.pos.y) > PROJECTION_RANGE ||
        math.Abs(position.z-plrShip.pos.z) > PROJECTION_RANGE {

        return false
    } else {
        return true
    }
}
