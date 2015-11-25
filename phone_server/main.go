package main

import (
    //"fmt"
    "golang.org/x/net/websocket"
    "net/http"
    "time"
)

const WEB_DIR string = "../phone_web"
const DATA_UPDATE_INTERVAL time.Duration = 100 * time.Millisecond

// Main structure holding all the user
var userList *UserList = &UserList{
    l:      make([]*User, 0, 20),
    delC:   make(chan *User),
    addC:   make(chan *User),
    update: make(chan struct{}),
}

// Creates a user instance and adds it to the ecosystem
func webSocketHandler(webs *websocket.Conn) {
    usr := &User{ws: webs}
    //add user
    userList.add(usr)

    usr.handleUser()

    //remove user when the connection is closed
    userList.remove(usr)
}

// Starts all necessary subroutines
func main() {
    go userList.accessManager()
    // TODO: Run this timer only when a game session is running
    go updateTimer()
    http.Handle("/web_socket", websocket.Handler(webSocketHandler))
    http.Handle("/", http.FileServer(http.Dir(WEB_DIR)))
    err := http.ListenAndServe(":8080", nil)
    if err != nil {
        panic("Error: " + err.Error())
    }
}

// Triggers the sending of state data to mobile clients periodically
func updateTimer() {
    ticker := time.NewTicker(DATA_UPDATE_INTERVAL)
    for {
        <-ticker.C
        userList.update <- struct{}{}
    }
}
