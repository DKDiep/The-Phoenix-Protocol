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
    data *PlayerShip
    setC chan *PlayerShip
    getC chan *PlayerShip
}

// Manages concurrent access to the player ship data structure
func (plrShip *PlayerShipController) accessManager() {
    fmt.Println("Starting Player Ship accessManager.")
    for {
        select {
        case new := <-plrShip.setC:
            plrShip.data = new
        case <-plrShip.getC:
            toSend := *plrShip.data
            plrShip.getC <- &toSend
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
