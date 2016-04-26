package main

import (
    "flag"
    "fmt"
    "golang.org/x/net/websocket"
    "net"
    "net/http"
    "time"
)

const (
    DATABASE_HOST              string        = "tcp(localhost:3306)"
    DATABASE_NAME              string        = "game"
    DATABASE_USER              string        = "root"
    DATABASE_PASS              string        = "password"
    DATABASE_LOG_SCORES_PERIOD time.Duration = 200 * time.Millisecond

    ADMIN_WEB_DIR           string        = "../web/admin_web"
    ADMIN_PORT              string        = "52932"
    ADMIN_UPDATE_INTERVAL   time.Duration = 3 * time.Second
    USERS_WEB_DIR           string        = "../web/phone_web"
    USERS_PORT              string        = "80"
    LOCAL_UDP_PORT          string        = "46578"
    GAME_SERVER_UDP_PORT    string        = "2345"
    GAME_SERVER_TCP_PORT    string        = "2346"
    DATA_UPDATE_INTERVAL    time.Duration = 33 * time.Millisecond
    NUM_OFFICERS            int           = 1
    OFFER_VALIDITY_DURATION time.Duration = 20 * time.Second

    MAX_AMMO              float64 = 100
    ASTEROID_DRAW_RANGE_X float64 = 500
    ASTEROID_DRAW_RANGE_Y float64 = 500
    ASTEROID_DRAW_RANGE_Z float64 = 100
    ENEMY_DRAW_RANGE_X    float64 = 500
    ENEMY_DRAW_RANGE_Y    float64 = 500
    ENEMY_DRAW_RANGE_Z    float64 = 300
)

// Flag variables
var gameServerAddress = flag.String("ip", "localhost", "set the IP address of the Game Server")

// Structure dealing with the database operations
var gameDatabase *GameDatabase

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
    commander:    nil,
    mOfficers:    make(map[uint64]*Player),
    mSpec:        make(map[uint64]*Player),
    addC:         make(chan *Player),
    setRoleC:     make(chan SetPlr),
    plrC:         make(chan struct{}),
    resetC:       make(chan struct{}),
    startC:       make(chan struct{}),
    sortlC:       make(chan []*Player),
    listC:        make(chan struct{}),
    updateC:      make(chan struct{}),
    logScoresC:   make(chan struct{}),
    logOfficersC: make(chan struct{}),
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
    flag.Parse()
    gameDatabase = ConnectToDatabase()
    if gameDatabase.isConnected {
        fmt.Println("Database: Ready to query.")
    } else {
        fmt.Println("Database: Failed to reach database.")
    }
    defer gameDatabase.Close()

    // Reset officers in case server crashed and they werent cleared
    gameDatabase.resetOfficers()

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
    usersServerMux.Handle("/", http.FileServer(http.Dir(USERS_WEB_DIR)))

    // Server for the admin
    adminServerMux := http.NewServeMux()
    adminServerMux.Handle("/web_socket", websocket.Handler(adminWebSocketHandler))
    adminServerMux.Handle("/", http.FileServer(http.Dir(ADMIN_WEB_DIR)))

    go listenWrapper(usersServerMux, USERS_PORT)
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
