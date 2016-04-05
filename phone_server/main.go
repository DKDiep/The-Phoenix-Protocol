package main

import (
    "fmt"
    "golang.org/x/net/websocket"
    "net"
    "net/http"
    "time"
)

const (
    ADMIN_WEB_DIR           string        = "../web/admin_web"
    ADMIN_PORT              string        = "52932"
    ADMIN_UPDATE_INTERVAL   time.Duration = 3 * time.Second
    USER_WEB_DIR            string        = "../web/phone_web"
    USER_PORT               string        = "8080"
    LOCAL_UDP_PORT          string        = "46578"
    GAME_SERVER_ADDRESS     string        = "192.168.56.1"
    GAME_SERVER_UDP_PORT    string        = "2345"
    GAME_SERVER_TCP_PORT    string        = "2346"
    DATA_UPDATE_INTERVAL    time.Duration = 33 * time.Millisecond
    NUM_OFFICERS            int           = 1
    OFFER_VALIDITY_DURATION time.Duration = 20 * time.Second

    PROJECTION_RANGE float64 = 500
)

// Structures dealing with the Game Server Connections
var gameServerUDPConn *net.UDPConn
var gameServerTCPConn *net.TCPConn

// Holds and handles game state related information
var gameState *GameState = &GameState{
    status:            SETUP,
    updateStopC:       nil,
    canEnterNextState: true,
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
    mOfficers: make(map[uint64]*Player),
    mSpec:     make(map[uint64]*Player),
    addC:      make(chan *Player),
    setRoleC:  make(chan SetPlr),
    plrC:      make(chan struct{}),
    resetC:    make(chan struct{}),
    startC:    make(chan struct{}),
    sortlC:    make(chan []*Player),
    listC:     make(chan []PlayerInfo),
    updateC:   make(chan struct{}),
}

// Holds the notifications per component
var notificationMap *NotificationMap = &NotificationMap{
    m:      newNotificationMap(),
    setC:   make(chan struct{}),
    getC:   make(chan struct{}),
    resetC: make(chan struct{}),
}

// Main structure holding all enemy data
var enemyMap *EnemyMap = &EnemyMap{
    m:      make(map[int64]*Enemy),
    delC:   make(chan int64),
    setC:   make(chan NewEnemy),
    ctrlC:  make(chan ControllingPlayer),
    resetC: make(chan struct{}),
    copyC:  make(chan *EnmCopyExchange),
}

// Main structure holding all asteroid data
var asteroidMap *AsteroidMap = &AsteroidMap{
    m:      make(map[int]*Asteroid),
    delC:   make(chan int),
    addC:   make(chan NewAst),
    resetC: make(chan struct{}),
    copyC:  make(chan *AstCopyExchange),
}

// Starts all necessary subroutines
func main() {
    go stdinHandler()
    go gameServerTCPConnectionHandler()
    go gameServerUDPConnectionHandler()
    go playerMap.accessManager()
    go playerShip.accessManager()
    go asteroidMap.accessManager()
    go enemyMap.accessManager()
    go notificationMap.accessManager()

    // Server for the users
    usersServerMux := http.NewServeMux()
    usersServerMux.Handle("/web_socket", websocket.Handler(userWebSocketHandler))
    usersServerMux.Handle("/", http.FileServer(http.Dir(USER_WEB_DIR)))

    // Server for the admin
    adminServerMux := http.NewServeMux()
    adminServerMux.Handle("/web_socket", websocket.Handler(adminWebSocketHandler))
    adminServerMux.Handle("/", http.FileServer(http.Dir(ADMIN_WEB_DIR)))

    go listenWrapper(usersServerMux, USER_PORT)
    listenWrapper(adminServerMux, ADMIN_PORT)
}

// A wrapper used to check for errors even when spawned as a goroutine
func listenWrapper(srv *http.ServeMux, port string) {
    fmt.Println("Starting Web Server on port " + port + ".")
    err := http.ListenAndServe(":"+port, srv)
    if err != nil {
        panic("Error starting web server on port " + port + " : " + err.Error())
    }
}
