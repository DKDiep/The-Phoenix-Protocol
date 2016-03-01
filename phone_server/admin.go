package main

import (
    "fmt"
    "encoding/json"
    "golang.org/x/net/websocket"
)

func adminWebSocketHandler(webs *websocket.Conn) {
    handleReceivedData(webs)
}

// Listens for messages from the phone and handles them appropriately
// Returns when the connection is closed
func handleReceivedData(ws *websocket.Conn) {
    receivedtext := make([]byte, 1024)
    for {
        n, err := ws.Read(receivedtext)

        // stop listening for activity if an error occurs
        if err != nil {
            if err.Error() == "EOF" {
                fmt.Println("Connection Closed, EOF received.")
            } else {
                fmt.Printf("Error: %s\n", err)
            }
            return
        }

        // TODO: remove when no longer needed
        // fmt.Println("Received: ", string(receivedtext[:n]))

        // decode JSON
        var msg interface{}
        if err := json.Unmarshal(receivedtext[:n], &msg); err != nil {
            fmt.Println(err)
        }

        handleAdminMessage(msg.(map[string]interface{}))
    }
}

// Multiplexes the decoding of the received message based on its type
func handleAdminMessage(msg map[string]interface{}) {
    switch msg["type"].(string) {
    case "GM_STRT":
        fmt.Println("Received a start game signal form admin.")
    default:
        fmt.Println("Received unexpected message of type: ", msg["type"])
    }
}
