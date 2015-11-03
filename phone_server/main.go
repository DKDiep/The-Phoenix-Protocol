package main

import (
    //"fmt"
    "golang.org/x/net/websocket"
    "net/http"
)

const webDir string = "../spectator_web"

//Creates a spectator instance and adds it to the ecosystem
func webSocketHandler(webs *websocket.Conn) {
    spec := &Spectator{ws: webs}

    spec.handleSpectator()
}

func main() {
    http.Handle("/web_socket", websocket.Handler(webSocketHandler))
    http.Handle("/", http.FileServer(http.Dir(webDir)))
    err := http.ListenAndServe(":8080", nil)
    if err != nil {
        panic("Error: " + err.Error())
    }
}
