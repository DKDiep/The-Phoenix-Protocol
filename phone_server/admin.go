package main

import (
    "encoding/json"
    "fmt"
    "golang.org/x/net/websocket"
)

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
        handleReceivedData(adminWebSocket)
        adminWebSocket = nil
        fmt.Println("Admin: Admin client disconnected.")
    }
}

// Listens for messages from the admin panel
// Returns when the connection is closed
func handleReceivedData(ws *websocket.Conn) {
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
        gameState.enterRunningState()
        sendSignalToGameServer(START_GAME)
    default:
        fmt.Println("Admin: Received unexpected message of type: ", msg["type"])
    }
}
