package main

import (
    //"fmt"
    "golang.org/x/net/websocket"
    "net/http"
)

const webDir string = "../phone_web"

//Main structure holding all the connections
var userList *UserList = &UserList{l: make([]*User, 0, 20)}

//Creates a user instance and adds it to the ecosystem
func webSocketHandler(webs *websocket.Conn) {
    //add user
    userList.Lock()
    usr := &User{ws: webs, listId: len(userList.l)}
    userList.l = append(userList.l, usr)
    userList.Unlock()

    usr.handleUser()

    //remove user when the connection is closed
    userList.Lock()
    i := usr.listId
    userList.l[i] = userList.l[len(userList.l)-1] // replace with last
    userList.l[i].listId = i                      // update listId of user
    userList.l = userList.l[:len(userList.l)-1]   // trim last
    userList.Unlock()
}

func main() {
    http.Handle("/web_socket", websocket.Handler(webSocketHandler))
    http.Handle("/", http.FileServer(http.Dir(webDir)))
    err := http.ListenAndServe(":8080", nil)
    if err != nil {
        panic("Error: " + err.Error())
    }
}
