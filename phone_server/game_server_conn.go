package main

import (
    "encoding/json"
    "fmt"
    "net"
    "time"
)

// Sets up the connection structure and sends a greeting
func initialiseGameServerConnection() {
    serverAddr, err := net.ResolveUDPAddr("udp", GAME_SERVER_ADDRESS)
    if err != nil {
        panic("Error resolving game server address: " + err.Error())
    }

    gameServerConn, err = net.DialUDP("udp", nil, serverAddr)
    if err != nil {
        panic("Error connecting to game server: " + err.Error())
    }

    _, err = gameServerConn.Write([]byte("INIT\n"))
    if err != nil {
        panic("Error connecting to game server: " + err.Error())
    }
}

// Listens for new data and retries to connect in case of server outtage
func gameServerConnectionHandler() {
    defer gameServerConn.Close()
    receivedMsg := make([]byte, 5120)

    for {
        n, err := gameServerConn.Read(receivedMsg)
        if err != nil {
            wait := 5
            fmt.Println("Game Server Connection Error: ", err)
            fmt.Println("Retrying in", wait, "seconds.")
            time.Sleep(time.Duration(wait) * time.Second)
            fmt.Println("Retrying.")
            gameServerConn.Write([]byte("INIT\n"))
        } else {
            toDecode := make([]byte, n)
            copy(toDecode, receivedMsg[:n])
            go decodeGameServerMessage(toDecode)
        }
    }
}

// Deals with the message payload based on its type
func decodeGameServerMessage(rawData []byte) {
    var msg map[string]interface{}
    if err := json.Unmarshal(rawData, &msg); err != nil {
        fmt.Println(err)
    }

    switch msg["type"].(string) {
    case "SHP_UPD":
        updateShipData(msg["data"].(map[string]interface{}))
    case "NEW_AST":
        addAsteroids(msg["data"].([]interface{}))
    case "RMV_AST":
        removeAsteroids(msg["data"].([]interface{}))
    // TODO: handle more message types
    default:
        fmt.Println("Received unexpected message from Game Server of type: ", msg["type"])
    }
}

// Updates the ship data with the received values
func updateShipData(data map[string]interface{}) {
    newShipData := &PlayerShip{
        posX: data["x"].(float64),
        posY: data["y"].(float64),
        rot:  data["rot"].(float64),
    }
    playerShip.setShipData(newShipData)
}

// Adds asteroids to the local data structure
func addAsteroids(data []interface{}) {
    for _, d := range data {
        asteroid := d.(map[string]interface{})
        asteroidMap.add(int(asteroid["id"].(float64)),
            &Asteroid{
                posX: asteroid["x"].(float64),
                posY: asteroid["y"].(float64),
            })
    }
}

// Removes asteroids from the local data structure
func removeAsteroids(data []interface{}) {
    for _, id := range data {
        asteroidMap.remove(int(id.(float64)))
    }
}
