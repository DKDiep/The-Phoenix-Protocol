package main

import (
    "fmt"
)

type ComponentType int

const (
    SHIELDS ComponentType = iota
    TURRETS
    ENGINES
    HULL
    NONE
)

// "constant" array of the component types
var IntercatableComponents = [...]ComponentType{SHIELDS, TURRETS, ENGINES, HULL}

// Describes a notification
type Notification struct {
    needsUpgrade bool
    needsRepair  bool
}

// Collection of all existing notifications
type NotificationMap struct {
    m      map[ComponentType]*Notification
    setC   chan struct{} // channel for setting a notification
    getC   chan struct{}
    resetC chan struct{} // channele for resetting the map
}

// Creates a new notification map
func newNotificationMap() map[ComponentType]*Notification {
    m := make(map[ComponentType]*Notification)

    for _, comp := range IntercatableComponents {
        m[comp] = &Notification{false, false}
    }

    return m
}

// Manages concurrent access to the notification map data structure
func (notifications *NotificationMap) accessManager() {
    fmt.Println("Starting Notification Map accessManager.")
    for {
        select {
        // set a notification
        case <-notifications.setC:
            <-notifications.setC
        // get a notification
        case <-notifications.getC:
            <-notifications.getC
        // reset the map
        case <-notifications.resetC:
            notifications.m = newNotificationMap()
        }
    }
}

// Sets a notification
func (notifications *NotificationMap) setNotification(comp ComponentType,
    isUpgrade bool, toSet bool) {

    notifications.setC <- struct{}{}
    notifications.setNotificationAsync(comp, isUpgrade, toSet)
    notifications.setC <- struct{}{}
}

// Gets the notfications related to a component
func (notifications *NotificationMap) getNotifications(comp ComponentType) Notification {
    notifications.getC <- struct{}{}
    copy := notifications.getNotificationsAsync(comp)
    notifications.getC <- struct{}{}

    return copy
}

// Resets the data structure
func (notifications *NotificationMap) reset() {
    notifications.resetC <- struct{}{}
}

// Sets a notification asynchronously
// NOTE: DO NOT USE
func (notifications *NotificationMap) setNotificationAsync(comp ComponentType,
    isUpgrade bool, toSet bool) {

    if isUpgrade {
        notifications.m[comp].needsUpgrade = toSet
    } else {
        notifications.m[comp].needsRepair = toSet
    }
}

// Gets the notfications related to a component asynchronously
// NOTE: DO NOT USE
func (notifications *NotificationMap) getNotificationsAsync(comp ComponentType) Notification {
    copy := *notifications.m[comp]
    return copy
}

// Construct the JSON content for sending notifications related to a component
// to a phone client
func constructNotificationsForComponent(comp ComponentType,
    notification Notification) []map[string]interface{} {

    msgs := make([]map[string]interface{}, 0, 2)
    if notification.needsUpgrade {
        msgs = append(msgs, constructNotificationJSON(comp, true, true))
    }
    if notification.needsRepair {
        msgs = append(msgs, constructNotificationJSON(comp, false, true))
    }

    return msgs
}

// Construct the JSON content for sending notifications related to a component
// to a phone client
func constructNotificationJSON(comp ComponentType, isUpgrade bool,
    toSet bool) map[string]interface{} {

    msg := make(map[string]interface{})
    msg["type"] = componentTypeToString(comp)
    msg["isUpgrade"] = isUpgrade
    msg["toSet"] = toSet

    return msg
}

// A simple mapping of types from JSON data to internal Types
func stringToComponentType(s string) ComponentType {
    switch s {
    case "SHIELDS":
        return SHIELDS
    case "TURRETS":
        return TURRETS
    case "ENGINES":
        return ENGINES
    case "HULL":
        return HULL
    default:
        return NONE
    }
}

// A mapping of components to strings send in JSON msgs
func componentTypeToString(comp ComponentType) string {
    switch comp {
    case SHIELDS:
        return "SHIELDS"
    case TURRETS:
        return "TURRETS"
    case ENGINES:
        return "ENGINES"
    case HULL:
        return "HULL"
    default:
        return ""
    }
}
