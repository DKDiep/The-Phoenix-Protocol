package main

import (
    "encoding/json"
    "fmt"
    "net"
    "time"
)

// Sets up the connection structure and sends a greeting
// Note: the pacnics in here will only trigger if it fails to send
// the UDP packet but they will NOT trigger if the packet is undelivered
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
// Note: the retry procedure worked when tested with netcat but it seems
// to not work when communicating with Unity, reason unknown
func gameServerConnectionHandler() {
    defer gameServerConn.Close()
    receivedMsg := make([]byte, 5120) // allocate a 5KB buffer

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
    case "ENM_UPD":
        setEnemies(msg["data"].([]interface{}))
    case "RMV_ENM":
        removeEnemies(msg["data"].([]interface{}))
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

// Updates the enemy data with the given values
func setEnemies(data []interface{}) {
    for _, d := range data {
        enemy := d.(map[string]interface{})
        enemyMap.set(int(enemy["id"].(float64)),
            &Enemy{
                posX: enemy["x"].(float64),
                posY: enemy["y"].(float64),
            })
    }
}

// Removes enemies from the local data structure
func removeEnemies(data []interface{}) {
    for _, id := range data {
        enemyMap.remove(int(id.(float64)))
    }
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
