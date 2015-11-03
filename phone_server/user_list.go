package main

import (
    "sync"
)

type UserList struct {
    sync.RWMutex
    l []*User
}
