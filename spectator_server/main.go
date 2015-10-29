package main

import (
    "fmt"
    "golang.org/x/net/websocket"
    "net/http"
)

const webDir string = "../spectator_web"

func echoHandler(ws *websocket.Conn) {
    for {
        receivedtext := make([]byte, 255)

        n, err := ws.Read(receivedtext)

        if err != nil {
            if err.Error() == "EOF" {
                fmt.Println("Connection Closed, EOF received")
            } else {
                fmt.Printf("Error: %s\n", err)
            }
            return
        }

        s := string(receivedtext[:n])
        fmt.Printf("Received: %d bytes: %s\n", n, s)
        ws.Write([]byte(receivedtext[:n]))
    }
}

func main() {
    http.Handle("/echo", websocket.Handler(echoHandler))
    http.Handle("/", http.FileServer(http.Dir(webDir)))
    err := http.ListenAndServe(":8080", nil)
    if err != nil {
        panic("Error: " + err.Error())
    }
}
