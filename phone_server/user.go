package main

import (
    "encoding/json"
    "fmt"
    "golang.org/x/net/websocket"
)

// Holds user related data
// A user represents a connection and deals with message passing
type User struct {
    ws     *websocket.Conn
    player *Player
}

// Creates a user instance and adds it to the ecosystem
func userWebSocketHandler(webs *websocket.Conn) {
    usr := &User{ws: webs}

    usr.handleUser()

    // remove user when the connection is closed
    // deassociate this user with its respective player
    if usr.player != nil {
        usr.player.unsetUserIfEquals(usr)
    }
}

// Listens for messages from the phone and handles them appropriately
// Returns when the connection is closed
func (usr *User) handleUser() {
    receivedtext := make([]byte, 1024)
    for {
        n, err := usr.ws.Read(receivedtext)

        // stop listening for activity if an error occurs
        if err != nil {
            if err.Error() == "EOF" {
                fmt.Println("Connection Closed, EOF received.")
            } else {
                fmt.Printf("Error: %s\n", err)
            }
            return
        }

        //TODO: remove when no longer needed
        fmt.Println("Received: ", string(receivedtext[:n]))

        // decode JSON
        var msg interface{}
        if err := json.Unmarshal(receivedtext[:n], &msg); err != nil {
            fmt.Println(err)
            continue
        }

        // close the connection from this side if we encounter a problem
        succes := usr.handleMessage(msg.(map[string]interface{}))
        if !succes {
            return
        }
    }
}

// Multiplexes the decoding of the received message based on its type
func (usr *User) handleMessage(msg map[string]interface{}) bool {
    // Prevents people from fucking up things by opening multiple tabs
    if usr.player != nil && usr.player.user != usr {
        return false
    }

    switch msg["type"].(string) {
    case "REG_USER":
        usr.registerNew(msg["data"].(string))
    case "UPDATE_USER":
        usr.updateUser(uint64(msg["data"].(float64)))
    case "PROM":
        usr.player.inviteAnswerAction(msg["data"].(bool))
    case "ENM_CTRL":
        usr.player.setControlledEnemy(int64(msg["data"].(float64)))
    case "ENM_MV":
        usr.player.sendMoveCommandToGameServer(msg["data"].(map[string]interface{}))
    case "ENM_ATT":
        usr.player.sendAttackCommandToGameServer(int64(msg["data"].(float64)))
    default:
        fmt.Println("Received unexpected message of type: ", msg["type"])
    }

    return true
}

// Registers a new user and sends back user state data
func (usr *User) registerNew(name string) {
    // register user
    playerId := gameDatabase.registerPlayer(name)

    // reply with identification data and current user data
    msg := map[string]interface{}{
        "type": "SAVE_USER",
        "data": map[string]interface{}{
            "id": playerId,
        },
    }

    usr.sendMsg(msg)
}

// Retrieves the user data and sends it to the client
func (usr *User) updateUser(playerId uint64) {
    // check if the user has already joined the game
    plr := playerMap.get(playerId)
    // if so associate this user with the player
    if plr != nil {
        usr.player = plr
        plr.setUser(usr)
        // otherwise assign a new player to the user
    } else {
        name := gameDatabase.getPlayerName(playerId)
        newPlr := NewPlayer(playerId, name, usr)
        usr.player = newPlr
        playerMap.add(newPlr)
    }

    // inform the user of the current player state
    usr.player.sendStateUpdate()
    usr.player.sendCurrentData()
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
        // close connection
        usr.player.unsetUserIfEquals(usr)
        usr.ws.Close()
    }
}
