package main

import (
    "fmt"
    "golang.org/x/net/websocket"
    "net"
    "net/http"
    "time"
)

const WEB_DIR string = "../web/phone_web"
const GAME_SERVER_ADDRESS string = "localhost"
const GAME_SERVER_UDP_PORT string = "2345"
const GAME_SERVER_TCP_PORT string = "2346"
const DATA_UPDATE_INTERVAL time.Duration = 33 * time.Millisecond
const NUM_OFFICERS int = 3
const OFFER_VALIDITY_DURATION time.Duration = 20 * time.Second

// Structures dealing with the Game Server Connections
var gameServerUDPConn *net.UDPConn
var gameServerTCPConn *net.TCPConn

// Holds and handles game state related information
var gameState *GameState = &GameState{
    status:      INIT,
    updateStopC: nil,
}

// Holds the player ship data and modification channels
var playerShip *PlayerShipController = &PlayerShipController{
    data:   &PlayerShip{},
    setC:   make(chan *PlayerShip),
    getC:   make(chan *PlayerShip),
    resetC: make(chan struct{}),
}

// Holds the player data and modification channels
var playerMap *PlayerMap = &PlayerMap{
    mOfficers:   make(map[string]*Player),
    mSpec:       make(map[string]*Player),
    addC:        make(chan *Player),
    setOfficerC: make(chan *Player),
    plrC:        make(chan struct{}),
    resetC:      make(chan struct{}),
    startC:      make(chan struct{}),
    sortlC:      make(chan []*Player),
    updateC:     make(chan struct{}),
}

// Main structure holding all enemy data
var enemyMap *EnemyMap = &EnemyMap{
    m:      make(map[int]*Enemy),
    delC:   make(chan int),
    setC:   make(chan NewEnemy),
    resetC: make(chan struct{}),
    copyC:  make(chan map[int]*Enemy),
}

// Main structure holding all asteroid data
var asteroidMap *AsteroidMap = &AsteroidMap{
    m:      make(map[int]*Asteroid),
    delC:   make(chan int),
    addC:   make(chan NewAst),
    resetC: make(chan struct{}),
    copyC:  make(chan map[int]*Asteroid),
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
    initialiseGameServerTCPConnection()
    go gameServerTCPConnectionHandler()
    initialiseGameServerUDPConnection()
    go gameServerUDPConnectionHandler()
    go playerMap.accessManager()
    go playerShip.accessManager()
    go asteroidMap.accessManager()
    go enemyMap.accessManager()
    http.Handle("/web_socket", websocket.Handler(webSocketHandler))
    http.Handle("/", http.FileServer(http.Dir(WEB_DIR)))
    fmt.Println("Starting Web Server.")
    // TODO: remove when admin console is implemented
    go gameState.enterSetupState()
    err := http.ListenAndServe(":8080", nil)
    if err != nil {
        panic("Error starting web server: " + err.Error())
    }
}
