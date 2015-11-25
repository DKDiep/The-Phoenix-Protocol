package main

// The collection of all users
type UserList struct {
    l      []*User
    addC   chan *User
    delC   chan *User
    update chan struct{}
}

// Manages concurrent access to the user list data structure
func (users *UserList) accessManager() {
    for {
        select {
        // request to add
        case usr := <-users.addC:
            usr.listId = len(userList.l)
            users.l = append(userList.l, usr)
        // request to delete
        case usr := <-users.delC:
            i := usr.listId
            userList.l[i] = userList.l[len(userList.l)-1] // replace with last
            userList.l[i].listId = i                      // update listId of user
            userList.l = userList.l[:len(userList.l)-1]   // trim last
        // request to update all users
        case <-users.update:
            users.updateData()
        }
    }
}

// Request a user addition
func (users *UserList) add(usr *User) {
    users.addC <- usr
}

// Request a user deletion
func (users *UserList) remove(usr *User) {
    users.delC <- usr
}

// Sends a state data update to all users
func (users *UserList) updateData() {
    // TODO: calculate coordinates in client side space
    for _, usr := range users.l {
        usr.sendDataUpdate()
    }
}
