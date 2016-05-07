package main

import (
    "encoding/json"
    "fmt"
    "golang.org/x/net/websocket"
    // "strconv"
    "runtime/debug"
    "time"
)

// Encodes the update message to the Admin console
type AdminUpdateMessage struct {
    State string
    // is the game ready to start
    Ready      bool
    Officers   []PlayerInfo
    Spectators []PlayerInfo
    Commander  PlayerInfo
}

// Wrapper of player data that is to be send to the admin
type PlayerInfo struct {
    UserName string
    UserId   int64
    Score    uint64
    IsOnline bool
}

// The web socket of the currently connected admin
var adminWebSocket *websocket.Conn = nil

// Used through stdin instructions to allow for a new admin instance
// Closes the current admin session and clears the global var
func unblockAdmin() {
    if adminWebSocket != nil {
        if err := adminWebSocket.Close(); err != nil {
            fmt.Println("Error force closing admin websocket: " + err.Error())
        }
        adminWebSocket = nil
    }
    fmt.Println("Admin: reset admin connection.")
}

// Returns if there is already an existing admin connection
func adminWebSocketHandler(webs *websocket.Conn) {
    if adminWebSocket == nil {
        fmt.Println("Admin: Admin client connected.")
        adminWebSocket = webs

        // start update timer and send firts update
        stopChan := make(chan struct{})
        go adminUpdateTimer(stopChan)
        updateAdmin()

        // block here while connection is active
        handleReceivedData(adminWebSocket)

        // stop update timer
        stopChan <- struct{}{}

        adminWebSocket = nil
        fmt.Println("Admin: Admin client disconnected.")
    }
}

// Listens for messages from the admin panel
// Returns when the connection is closed
func handleReceivedData(ws *websocket.Conn) {
    // Recover from fuckups
    defer func() {
        if r := recover(); r != nil {
            fmt.Println("Admin: Runtime panic:", r)
            debug.PrintStack()
        }
    }()

    receivedtext := make([]byte, 1024)
    for {
        n, err := ws.Read(receivedtext)

        // stop listening for activity if an error occurs
        if err != nil {
            if err.Error() == "EOF" {
                fmt.Println("Admin: Connection Closed, EOF received.")
            } else {
                fmt.Printf("Admin: Error: %s\n", err)
            }
            return
        }

        // TODO: remove when no longer needed
        // fmt.Println("Received: ", string(receivedtext[:n]))

        // decode JSON
        var msg interface{}
        if err := json.Unmarshal(receivedtext[:n], &msg); err != nil {
            fmt.Println(err)
            continue
        }

        handleAdminMessage(msg.(map[string]interface{}))
    }
}

// Multiplexes the decoding of the received message based on its type
func handleAdminMessage(msg map[string]interface{}) {
    switch msg["type"].(string) {
    case "GM_STRT":
        fmt.Println("Admin: Received Start Game signal.")
        data := msg["data"].(map[string]interface{})
        gameState.startGame(data["name"].(string), data["stats"].(string))
    case "GM_STP":
        fmt.Println("Admin: Received Enter Setup signal.")
        gameState.enterSetupState()
    case "GM_INV":
        fmt.Println("Admin: Received Send Invitations signal.")
        gameState.enterInvitationState()
    case "SET_OFFIC":
        gameState.processAdminSetSignal(uint64(msg["data"].(float64)), OFFICER)
    case "SET_SPEC":
        gameState.processAdminSetSignal(uint64(msg["data"].(float64)), SPECTATOR)
    case "SET_COMND":
        gameState.processAdminSetSignal(uint64(msg["data"].(float64)), COMMANDER)
    default:
        fmt.Println("Admin: Received unexpected message of type: ", msg["type"])
    }
    updateAdmin()
}

// Goroutine for updating admin data
func adminUpdateTimer(stop chan struct{}) {
    ticker := time.NewTicker(ADMIN_UPDATE_INTERVAL)
    running := true
    for running {
        select {
        // trigger an update sequence
        case <-ticker.C:
            updateAdmin()
        // stop this goroutine
        case <-stop:
            running = false
        }
    }
}

// Sends and update to the admin consolel
func updateAdmin() {
    if adminWebSocket == nil {
        return
    }

    msg := AdminUpdateMessage{}

    // game state information
    switch gameState.status {
    case RUNNING:
        msg.State = "RUN"
    case SETUP:
        msg.State = "STP"
    case INVITAION:
        msg.State = "INV"
    }

    msg.Ready = gameState.canEnterNextState

    officers, spectators, commander := playerMap.getPlayerLists()

    msg.Officers = officers
    msg.Spectators = spectators
    msg.Commander = commander

    sendMsgToAdmin(msg)
}

// Deals with sending the message and error checking
func sendMsgToAdmin(msg AdminUpdateMessage) {
    if adminWebSocket == nil {
        return
    }

    toSend, err := json.Marshal(msg)
    if err != nil {
        fmt.Println("Admin: Error sendMsg(): Failed to marshal JSON: ", err)
        return
    }

    _, err = adminWebSocket.Write(toSend)
    if err != nil {
        fmt.Println("Admin: Error sendMsg(): Failed to send to client: ", err)
    }
}
