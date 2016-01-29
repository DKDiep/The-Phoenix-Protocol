package main

import (
    "fmt"
    "math"
)

// The collection of all users
type UserList struct {
    l       []*User
    addC    chan *User
    delC    chan *User
    updateC chan struct{}
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
    // TODO: calculate coordinates in client side space
    asteroidMap.copyC <- nil
    playerShip.getC <- nil
    asteroidData := <-asteroidMap.copyC
    playerShipData := <-playerShip.getC
    for _, asteroid := range asteroidData {
        asteroid.posX -= playerShipData.posX
        asteroid.posY -= playerShipData.posY
        newX := float64(asteroid.posX)*math.Cos((180+playerShipData.rot)*(math.Pi/180)) - float64(asteroid.posY)*math.Sin((180+playerShipData.rot)*(math.Pi/180))
        newY := float64(asteroid.posX)*math.Sin((180+playerShipData.rot)*(math.Pi/180)) + float64(asteroid.posY)*math.Cos((180+playerShipData.rot)*(math.Pi/180))
        //fmt.Println("X: ",asteroid.posX, " New X: ", newX, " Y: ", asteroid.posY, " New Y: ", newY)
        asteroid.posX = int(newX) + 50
        asteroid.posY = int(newY) + 50
    }
    for _, usr := range users.l {
        usr.sendDataUpdate(asteroidData)
    }
}
