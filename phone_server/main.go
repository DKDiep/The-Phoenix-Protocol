package main

import (
    "fmt"
    "golang.org/x/net/websocket"
    "net"
    "net/http"
    "time"
)

const WEB_DIR string = "../phone_web"
const GAME_SERVER_ADDRESS string = "192.168.56.1:2345"
const DATA_UPDATE_INTERVAL time.Duration = 33 * time.Millisecond

// Structure dealing with the Game Server Connection
var gameServerConn *net.UDPConn

var playerShip *PlayerShipController = &PlayerShipController{
    data: &PlayerShip{},
    setC: make(chan *PlayerShip),
    getC: make(chan *PlayerShip),
}

// Main structure holding all the user
var userList *UserList = &UserList{
    l:       make([]*User, 0, 20),
    delC:    make(chan *User),
    addC:    make(chan *User),
    updateC: make(chan struct{}),
}

// Main structure holding all asteroid data
var asteroidMap *AsteroidMap = &AsteroidMap{
    m:     make(map[int]*Asteroid),
    delC:  make(chan int),
    addC:  make(chan NewAst),
    copyC: make(chan map[int]*Asteroid),
}

// Creates a user instance and adds it to the ecosystem
func webSocketHandler(webs *websocket.Conn) {
    usr := &User{ws: webs}
    //add user
    userList.add(usr)

    usr.handleUser()

    //remove user when the connection is closed
    userList.remove(usr)
}

// Starts all necessary subroutines
func main() {
    initialiseGameServerConnection()
    go gameServerConnectionHandler()
    go playerShip.accessManager()
    go userList.accessManager()
    go asteroidMap.accessManager()
    // TODO: Run this timer only when a game session is running
    go updateTimer()
    http.Handle("/web_socket", websocket.Handler(webSocketHandler))
    http.Handle("/", http.FileServer(http.Dir(WEB_DIR)))
    fmt.Println("Starting Web Server.")
    err := http.ListenAndServe(":8080", nil)
    if err != nil {
        panic("Error starting web server: " + err.Error())
    }
}

// Triggers the sending of state data to mobile clients periodically
func updateTimer() {
    ticker := time.NewTicker(DATA_UPDATE_INTERVAL)
    for {
        <-ticker.C
        userList.updateC <- struct{}{}
    }
}
