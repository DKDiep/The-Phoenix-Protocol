package main

import (
    "fmt"
)

// Player ship data holding structure
type PlayerShip struct {
    posX float64
    posY float64
    rot  float64
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
