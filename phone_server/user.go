package main

import (
    "encoding/json"
    "fmt"
    "golang.org/x/net/websocket"
    //"math"
)

// TODO: remove this when movement simulation is no longer needed
var incry float64 = 10

// Holds user related data
type User struct {
    ws       *websocket.Conn
    listId   int
    userId   string
    username string
    state    string
}

// Listens for messages from the phone and handles them appropriately
// Returns when the connection is closed
func (usr *User) handleUser() {
    receivedtext := make([]byte, 1024)
    for {
        n, err := usr.ws.Read(receivedtext)

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
        usr.handleMessage(msg.(map[string]interface{}))
    }
}

// Multiplexes the received message based on its type
func (usr *User) handleMessage(msg map[string]interface{}) {
    switch msg["type"].(string) {
    case "REG_USER":
        usr.registerNew(msg["data"].(string))
    case "UPDATE_USER":
        usr.updateUser(msg["data"].(string))
    default:
        fmt.Println("Received unexpected message of type: ", msg["type"])
    }
}

// Registers a new user and sends back user state data
func (usr *User) registerNew(name string) {
    // register user
    // TODO: remove this dummy registering process
    usr.username = name
    usr.userId = name + "123"
    usr.state = "SPECTATOR"

    // reply with identification data and current user data
    msg := map[string]interface{}{
        "type": "SAVE_USER",
        "data": map[string]interface{}{
            "id": usr.userId,
            "userData": map[string]interface{}{
                "state": usr.state,
            },
        },
    }

    usr.sendMsg(msg)
}

// Retrieves the user data and sends it to the client
func (usr *User) updateUser(userId string) {
    // TODO: remove dummy data retrieval
    usr.username = userId[:len(userId)-3]
    usr.userId = userId
    usr.state = "SPECTATOR"
    usr.sendStateUpdate()
}

// Sends a user state update
func (usr *User) sendStateUpdate() {
    msg := map[string]interface{}{
        "type": "USER_UPDATE",
        "data": map[string]interface{}{
            "state": usr.state,
        },
    }

    usr.sendMsg(msg)
}

// Sends a user state data update
func (usr *User) sendDataUpdate(asteroids map[int]Asteroid) {
    // TODO: remove dummy data
    msg := make(map[string]interface{})
    msg["type"] = "STATE_UPDATE"
    //dataLen := len(asteroids)
    dataSegment := make([]map[string]interface{}, 0)
    for _, ast := range asteroids {
        dataSegment = append(dataSegment, map[string]interface{}{
            "type": "asteroid",
                "position": map[string]interface{}{
                    "x": float64(ast.posX)/5,
                    "y": float64(ast.posY)/5,
                },
        })
    }
    msg["data"] = dataSegment
    // incry += 0.1
    // msg := map[string]interface{}{
    //     "type": "STATE_UPDATE",
    //     "data": []map[string]interface{}{
    //         map[string]interface{}{
    //             "type": "ship",
    //             "position": map[string]interface{}{
    //                 "x": 10,
    //                 "y": math.Mod((incry + 14), 100),
    //             },
    //         },
    //         map[string]interface{}{
    //             "type": "debris",
    //             "position": map[string]interface{}{
    //                 "x": 20,
    //                 "y": math.Mod((incry + 53), 100),
    //             },
    //             "size": 10,
    //         },
    //         map[string]interface{}{
    //             "type": "asteroid",
    //             "position": map[string]interface{}{
    //                 "x": 32,
    //                 "y": math.Mod((incry + 15), 100),
    //             },
    //         },
    //     },
    // }

    usr.sendMsg(msg)
}

// Deals with sending the message and error checking
func (usr *User) sendMsg(msg map[string]interface{}) {
    toSend, err := json.Marshal(msg)
    if err != nil {
        fmt.Println("Error sendMsg(): Failed to marshal JSON: ", err)
    }

    //fmt.Println("Sent: ", string(toSend))

    _, err = usr.ws.Write(toSend)
    if err != nil {
        fmt.Println("Error sendMsg(): Failed to send to client: ", err)
    }
}
