package main

import (
    "encoding/json"
    "fmt"
    "golang.org/x/net/websocket"
)

type Spectator struct {
    ws       *websocket.Conn
    userId   string
    username string
    state    string
}

//Listens for messages from the phone and handles them appropriately
func (spec *Spectator) handleSpectator() {
    receivedtext := make([]byte, 1024)
    for {
        n, err := spec.ws.Read(receivedtext)

        if err != nil {
            if err.Error() == "EOF" {
                fmt.Println("Connection Closed, EOF received")
            } else {
                fmt.Printf("Error: %s\n", err)
            }
            return
        }

        fmt.Println("Received: ", string(receivedtext[:n]))

        var msg interface{}

        if err := json.Unmarshal(receivedtext[:n], &msg); err != nil {
            fmt.Println(err)
        }
        spec.handleMessage(msg.(map[string]interface{}))

    }
}

//Multiplexes the received message based on its type
func (spec *Spectator) handleMessage(msg map[string]interface{}) {
    switch msg["type"].(string) {
    case "REG_USER":
        spec.registerNew(msg["data"].(string))
    default:
        fmt.Println("Received unexpected message of type: ", msg["type"])
    }
}

//Registers a new user and sends back user state data
func (spec *Spectator) registerNew(name string) {
    //register user
    //TODO: remove this dummy registering process
    spec.username = name
    spec.userId = name + "123"
    spec.state = "SPECTATOR"

    //reply with identification data and current user data
    msg := map[string]interface{}{
        "type": "SAVE_USER",
        "data": map[string]interface{}{
            "id": spec.userId,
            "userData": map[string]interface{}{
                "state": spec.state,
            },
        },
    }

    spec.sendMsg(msg)
}

//Deals with sending the message and error checking
func (spec *Spectator) sendMsg(msg map[string]interface{}) {
    toSend, err := json.Marshal(msg)
    if err != nil {
        fmt.Println("Error sendMsg(): Failed to marshal JSON: ", err)
    }

    fmt.Println("Sent: ", string(toSend))

    _, err = spec.ws.Write(toSend)
    if err != nil {
        fmt.Println("Error sendMsg(): Failed to send to client: ", err)
    }
}
