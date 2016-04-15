package main

import (
    "encoding/json"
    "fmt"
    "net"
    "strconv"
    "time"
)

type GameServerMessageType int

const (
    START_GAME GameServerMessageType = iota
    RESET_GAME
)

// Sets up the UDP connection structure
func initialiseGameServerUDPConnection() {
    fmt.Println("Initialising UDP Client.")
    serverAddr, err := net.ResolveUDPAddr("udp", *gameServerAddress+":"+
        GAME_SERVER_UDP_PORT)
    if err != nil {
        fmt.Println("UDP: Error resolving game server UDP address: " + err.Error())
        return
    }

    localAddr, err := net.ResolveUDPAddr("udp", "0.0.0.0"+":"+
        LOCAL_UDP_PORT)
    if err != nil {
        fmt.Println("UDP: Error resolving game server UDP address: " + err.Error())
        return
    }

    gameServerUDPConn, err = net.DialUDP("udp", localAddr, serverAddr)
    if err != nil {
        fmt.Println("UDP: Error establishing UDP connection to game server: " + err.Error())
    }
}

// Listens for new data
func gameServerUDPConnectionHandler() {
    fmt.Println("Starting UDP link handler.")
    for gameServerUDPConn == nil {
        initialiseGameServerUDPConnection()
        time.Sleep(2 * time.Second)
    }
    defer gameServerUDPConn.Close()
    receivedMsg := make([]byte, 51200) // allocate a 50KB buffer

    for {
        n, err := gameServerUDPConn.Read(receivedMsg)
        if err != nil {
            fmt.Println("UDP: Game Server Connection Error: ", err)
        } else {
            decodeGameServerMessage(receivedMsg[:n])
        }
    }
}

// Sets up the TCP connection structure and sends a greeting
func initialiseGameServerTCPConnection() {
    fmt.Println("Initialising TCP Connection.")
    serverAddr, err := net.ResolveTCPAddr("tcp", *gameServerAddress+":"+
        GAME_SERVER_TCP_PORT)
    if err != nil {
        fmt.Println("TCP: Error resolving game server address: " + err.Error())
        return
    }

    gameServerTCPConn, err = net.DialTCP("tcp", nil, serverAddr)
    if err != nil {
        fmt.Println("TCP: Error connecting to game server: " + err.Error())
    }
}

// Listens for new data and retries to connect in case of server outtage
func gameServerTCPConnectionHandler() {
    fmt.Println("Starting TCP link handler.")
    initialiseGameServerTCPConnection()
    // allocate a 1KB buffer
    // these messages should be way smaller anyway
    receivedMsg := make([]byte, 1024)
    wait := 2
    for {
        for gameServerTCPConn == nil {
            fmt.Println("TCP: Retrying to connect in", wait, "seconds.")
            time.Sleep(time.Duration(wait) * time.Second)
            initialiseGameServerTCPConnection()
        }

        fmt.Println("TCP: Connection Established.")
        defer gameServerTCPConn.Close()
        for gameServerTCPConn != nil {
            n, err := gameServerTCPConn.Read(receivedMsg)
            if err != nil {
                fmt.Println("TCP: Game Server Connection Error: ", err)
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
    case "NOTIFY_UPD":
        updateNotifications(msg["data"].([]interface{}))
    case "AMMO_UPD":
        updatePlayerAmmo(msg["data"].([]interface{}))
    case "SCORE_UPD":
        updatePlayerScore(msg["data"].([]interface{}))
    case "RMV_ENM":
        removeEnemies(msg["data"].([]interface{}))
    case "NEW_AST":
        addAsteroids(msg["data"].([]interface{}))
    case "RMV_AST":
        removeAsteroids(msg["data"].([]interface{}))
    case "GM_END":
        fmt.Println("Game Server: Received Game Over Signal.")
        gameState.canEnterNextState = true
    // TODO: handle more message types
    default:
        fmt.Println("Received unexpected message from Game Server of type: ", msg["type"])
    }
}

// Sends a message to the Gmae Server of the specified type
func sendSignalToGameServer(msgType GameServerMessageType) bool {
    switch msgType {
    case START_GAME:
        return sendStartGameSignalToGameServer()
    case RESET_GAME:
        return sendTCPMsgToGameServer("RESET")
    default:
        return false
    }
}

// Send a start game signal with all accepted officers
func sendStartGameSignalToGameServer() bool {
    officers, _, _ := playerMap.getPlayerLists()

    msg := "START:"

    for _, officer := range officers {
        msg += officer.UserName + "+"
        msg += strconv.FormatInt(officer.UserId, 10) + ","
    }
    msg = msg[:len(msg)-1]

    return sendTCPMsgToGameServer(msg)
}

// Send a string to the Game Server
// return value indicates if the message was sent succesfully
func sendTCPMsgToGameServer(msg string) (success bool) {
    success = true
    if gameServerTCPConn == nil {
        success = false
    } else {
        // All messages have added ";" as a delimiter in case the Game server
        // receives many at once
        _, err := gameServerTCPConn.Write([]byte(msg + ";"))
        if err != nil {
            fmt.Println("TCP: Error sending message: " + err.Error())
            success = false
        }
    }

    if !success {
        fmt.Println("TCP: Failed to send \"" + msg + "\".")
    }

    return
}

// Send a string to the Game Server
// return value indicates if the message was sent succesfully
func sendUDPMsgToGameServer(msg string) (success bool) {
    success = true
    if gameServerUDPConn == nil {
        success = false
    } else {
        // UDP msgs don't need a delimiter
        _, err := gameServerUDPConn.Write([]byte(msg))
        if err != nil {
            fmt.Println("UDP: Error sending message: " + err.Error())
            success = false
        }
    }

    if !success {
        fmt.Println("UDP: Failed to send \"" + msg + "\".")
    }

    return
}

// Updates the ship data with the received values
func updateShipData(data map[string]interface{}) {
    newShipData := &PlayerShip{
        pos: Point{
            x: data["x"].(float64),
            y: data["y"].(float64),
            z: data["z"].(float64),
        },
        forward: Point{
            x: data["fX"].(float64),
            y: data["fY"].(float64),
            z: data["fZ"].(float64),
        },
        right: Point{
            x: data["rX"].(float64),
            y: data["rY"].(float64),
            z: data["rZ"].(float64),
        },
        up: Point{
            x: data["uX"].(float64),
            y: data["uY"].(float64),
            z: data["uZ"].(float64),
        },
    }
    playerShip.setShipData(newShipData)
}

// Updates the enemy data with the given values
// TODO: optimise this to do only a single channel send per set of enemies
func setEnemies(data []interface{}) {
    for _, d := range data {
        enemy := d.(map[string]interface{})
        enemyMap.set(int64(enemy["id"].(float64)),
            &Enemy{
                pos: Point{
                    x: enemy["x"].(float64),
                    y: enemy["y"].(float64),
                    z: enemy["z"].(float64),
                },
                forward: Point{
                    x: enemy["fX"].(float64),
                    y: enemy["fY"].(float64),
                    z: enemy["fZ"].(float64),
                },
                isControlled:      false,
                controllingPlayer: nil,
            })
    }
}

// Removes enemies from the local data structure
// TODO: optimise this to do only a single channel send per set of enemies
func removeEnemies(data []interface{}) {
    for _, id := range data {
        enemyMap.remove(int64(id.(float64)))
    }
}

// Adds asteroids to the local data structure
// TODO: optimise this to do only a single channel send per set of asteroids
func addAsteroids(data []interface{}) {
    for _, d := range data {
        asteroid := d.(map[string]interface{})
        asteroidMap.add(int(asteroid["id"].(float64)),
            &Asteroid{
                pos: Point{
                    x: asteroid["x"].(float64),
                    y: asteroid["y"].(float64),
                    z: asteroid["z"].(float64),
                },
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

// Updates the notifications based on received data
func updateNotifications(data []interface{}) {
    updateData := make([]map[string]interface{}, 0, 5)
    for _, d := range data {
        notification := d.(map[string]interface{})
        comp := stringToComponentType(notification["type"].(string))
        isUpgrade := notification["isUpgrade"].(bool)
        toSet := notification["toSet"].(bool)
        notificationMap.setNotification(comp, isUpgrade, toSet)
        updateData = append(updateData, constructNotificationJSON(comp, isUpgrade, toSet))
    }

    playerMap.sendNotificationUpdateToOfficers(updateData)
}

// Set ammo for players
func updatePlayerAmmo(data []interface{}) {
    for _, d := range data {
        plrAmmo := d.(map[string]interface{})
        plr := playerMap.get(uint64(plrAmmo["id"].(float64)))
        if plr != nil {
            plr.setAmmo(plrAmmo["ammo"].(float64))
        } else {
            fmt.Println("Error: Player is nil when trying to update ammo.")
        }
    }
}

// Set score for players
func updatePlayerScore(data []interface{}) {
    for _, d := range data {
        plrScore := d.(map[string]interface{})
        plr := playerMap.get(uint64(plrScore["id"].(float64)))
        if plr != nil {
            plr.setScore(uint64(plrScore["score"].(float64)))
        } else {
            fmt.Println("Error: Player is nil when trying to update score.")
        }
    }
}
