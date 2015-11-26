package main

import(
    "net"
    "fmt"
    "time"
    "encoding/json"
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
            fmt.Println("Game Server Connection Error: ",err)
            fmt.Println("Retrying in",wait,"seconds.")
            time.Sleep(time.Duration(wait) * time.Second)
            fmt.Println("Retrying.")
            gameServerConn.Write([]byte("INIT\n"))
        } else {
            go decodeGameServerMessage(receivedMsg[:n])
        }
    }
}

// Deals with the message payload based on its type
func decodeGameServerMessage(rawData []byte ) {
    var msg map[string]interface{}
    if err := json.Unmarshal(rawData, &msg); err != nil {
        fmt.Println(err)
    }

    switch msg["type"].(string) {
    case "ADD_AST":
        addAsteroids(msg["data"].([]interface{}))
    case "RMV_AST":
        removeAsteroids(msg["data"].([]interface{}))
    // TODO: handle more message types
    default:
        fmt.Println("Received unexpected message from Game Server of type: ", msg["type"])
    }
}

// Adds asteroids to the local data structure
func addAsteroids(data []interface{}) {
    for _, d := range data {
        asteroid := d.(map[string]interface{})
        asteroidMap.add(int(asteroid["id"].(float64)),
                        &Asteroid{
                            posX: int(asteroid["x"].(float64)),
                            posY: int(asteroid["y"].(float64)),
                        })
    }
}

// Removes asteroids from the local data structure
func removeAsteroids(data []interface{}) {
    for _, id := range data {
        asteroidMap.remove(int(id.(float64)))
    }
}
