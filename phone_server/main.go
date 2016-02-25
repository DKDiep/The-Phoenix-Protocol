package main

import (
    "fmt"
    "golang.org/x/net/websocket"
    "net"
    "net/http"
    "time"
)

const WEB_DIR string = "../web/phone_web"
const GAME_SERVER_ADDRESS string = "192.168.56.1:2345"
const DATA_UPDATE_INTERVAL time.Duration = 33 * time.Millisecond

// Structure dealing with the Game Server Connection
var gameServerConn *net.UDPConn

// Holds the player ship data and modification channels
var playerShip *PlayerShipController = &PlayerShipController{
    data: &PlayerShip{},
    setC: make(chan *PlayerShip),
    getC: make(chan *PlayerShip),
}

// Holds the player data and modification channels
var playerMap *PlayerMap = &PlayerMap{
    m:       make(map[string]*Player),
    addC:    make(chan NewPlr),
    plrC:    make(chan struct{}),
    updateC: make(chan struct{}),
}

// Main structure holding all enemy data
var enemyMap *EnemyMap = &EnemyMap{
    m:     make(map[int]*Enemy),
    delC:  make(chan int),
    setC:  make(chan NewEnemy),
    copyC: make(chan map[int]*Enemy),
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

    usr.handleUser()

    // remove user when the connection is closed
    // deassociate this user with its respective player
    if usr.player != nil {
        usr.player.unsetUserIfEquals(usr)
    }
}

// Starts all necessary subroutines
func main() {
    initialiseGameServerConnection()
    go gameServerConnectionHandler()
    go playerMap.accessManager()
    go playerShip.accessManager()
    go asteroidMap.accessManager()
    go enemyMap.accessManager()
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
        playerMap.updateC <- struct{}{}
    }
}
