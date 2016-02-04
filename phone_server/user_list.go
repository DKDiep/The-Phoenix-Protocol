package main

import (
    "fmt"
    "math"
)

// The collection of all users
type UserList struct {
    l       []*User
    addC    chan *User // channel for requesting addition of a user
    delC    chan *User // channel for requeston deletion of a user
    updateC chan struct{} // channel for triggering the broadcast of up to date data
}

// Manages concurrent access to the user list data structure
func (users *UserList) accessManager() {
    fmt.Println("Starting User accessManager.")
    for {
        select {
        // request to add
        case usr := <-users.addC:
            usr.listId = len(userList.l)
            users.l = append(userList.l, usr)
        // request to delete
        case usr := <-users.delC:
            i := usr.listId
            userList.l[i] = userList.l[len(userList.l)-1] // replace with last
            userList.l[i].listId = i                      // update listId of user
            userList.l = userList.l[:len(userList.l)-1]   // trim last
        // request to update all users
        case <-users.updateC:
            users.updateData()
        }
    }
}

// Request a user addition
func (users *UserList) add(usr *User) {
    users.addC <- usr
}

// Request a user deletion
func (users *UserList) remove(usr *User) {
    users.delC <- usr
}

// Sends a state data update to all users
func (users *UserList) updateData() {
    playerShipData := playerShip.getShipData()
    enemyData := enemyMap.getCopy()
    asteroidData := asteroidMap.getCopy()

    // Transform asteroid coordinates into phone screen space
    for _, asteroid := range asteroidData {
        // Centre grid around player ship
        asteroid.posX -= playerShipData.posX
        asteroid.posY -= playerShipData.posY
        // Rotate grid so ship is pointing north on the screen
        newX := asteroid.posX*math.Cos((playerShipData.rot)*(math.Pi/180)) - asteroid.posY*math.Sin((playerShipData.rot)*(math.Pi/180))
        newY := asteroid.posX*math.Sin((playerShipData.rot)*(math.Pi/180)) + asteroid.posY*math.Cos((playerShipData.rot)*(math.Pi/180))
        // Translate grid so ship is in the centre
        asteroid.posX = newX + 50
        asteroid.posY = -(newY) + 50 // flip Y to match rendering orientation
    }

    // Transform enemy coordinates into phone screen space
    for _, enemy := range enemyData {
        // Centre grid around player ship
        enemy.posX -= playerShipData.posX
        enemy.posY -= playerShipData.posY
        // Rotate grid so ship is pointing north on the screen
        newX := enemy.posX*math.Cos((playerShipData.rot)*(math.Pi/180)) - enemy.posY*math.Sin((playerShipData.rot)*(math.Pi/180))
        newY := enemy.posX*math.Sin((playerShipData.rot)*(math.Pi/180)) + enemy.posY*math.Cos((playerShipData.rot)*(math.Pi/180))
        // Translate grid so ship is in the centre
        enemy.posX = newX + 50
        enemy.posY = -(newY) + 50 // flip Y to match rendering orientation
    }

    // Send updated data
    for _, usr := range users.l {
        usr.sendDataUpdate(enemyData, asteroidData)
    }
}
