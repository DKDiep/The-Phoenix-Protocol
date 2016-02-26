package main

import (
    "encoding/json"
    "fmt"
    "net"
    "time"
)

// Sets up the UDP connection structure
func initialiseGameServerUDPConnection() {
    serverAddr, err := net.ResolveUDPAddr("udp", GAME_SERVER_ADDRESS + ":" +
                                                 GAME_SERVER_UDP_PORT)
    if err != nil {
        fmt.Println("Error resolving game server UDP address: " + err.Error())
        return
    }

    gameServerUDPConn, err = net.DialUDP("udp", nil, serverAddr)
    if err != nil {
        fmt.Println("Error establishing UDP connection to game server: " + err.Error())
    }
}

// Listens for new data
func gameServerUDPConnectionHandler() {
    defer gameServerUDPConn.Close()
    receivedMsg := make([]byte, 51200) // allocate a 50KB buffer

    for {
        n, err := gameServerUDPConn.Read(receivedMsg)
        if err != nil {
            fmt.Println("Game Server Connection Error: ", err)
        } else {
            decodeGameServerMessage(receivedMsg[:n])
        }
    }
}

// Sets up the TCP connection structure and sends a greeting
func initialiseGameServerTCPConnection() {
    serverAddr, err := net.ResolveTCPAddr("tcp", GAME_SERVER_ADDRESS + ":" +
                                                 GAME_SERVER_TCP_PORT)
    if err != nil {
        fmt.Println("Error resolving game server address: " + err.Error())
        return
    }

    gameServerTCPConn, err = net.DialTCP("tcp", nil, serverAddr)
    if err != nil {
        fmt.Println("Error connecting to game server: " + err.Error())
        return
    }

    _, err = gameServerTCPConn.Write([]byte("INIT\n"))
    if err != nil {
        fmt.Println("Error connecting to game server: " + err.Error())
    }
}

// Listens for new data and retries to connect in case of server outtage
func gameServerTCPConnectionHandler() {
    // allocate a 1KB buffer
    // these messages should be way smaller anyway
    receivedMsg := make([]byte, 1024)
    wait := 5
    for {
        for gameServerTCPConn == nil {
            fmt.Println("Retrying to connect in", wait, "seconds.")
            time.Sleep(time.Duration(wait) * time.Second)
            initialiseGameServerTCPConnection()
        }

        defer gameServerTCPConn.Close()
        for gameServerTCPConn != nil {
            n, err := gameServerTCPConn.Read(receivedMsg)
            if err != nil {
                fmt.Println("Game Server Connection Error: ", err)
                gameServerTCPConn.Close()
                gameServerTCPConn = nil
            } else {
                decodeGameServerMessage(receivedMsg[:n])
            }
        }
    }
}

// Deals with the message payload based on its type
func decodeGameServerMessage(rawData []byte) {
    var msg map[string]interface{}
    if err := json.Unmarshal(rawData, &msg); err != nil {
        fmt.Println(err)
        return
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
    case "GM_END":
        gameState.enterSetupState()
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
// TODO: optimise this to do only a single channel send per set of enemies
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
// TODO: optimise this to do only a single channel send per set of enemies
func removeEnemies(data []interface{}) {
    for _, id := range data {
        enemyMap.remove(int(id.(float64)))
    }
}

// Adds asteroids to the local data structure
// TODO: optimise this to do only a single channel send per set of asteroids
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
// TODO: optimise this to do only a single channel send per set of asteroids
func removeAsteroids(data []interface{}) {
    for _, id := range data {
        asteroidMap.remove(int(id.(float64)))
    }
}
