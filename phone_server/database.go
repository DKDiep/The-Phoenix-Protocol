package main

import (
    "database/sql"
    "fmt"
    "math/rand"
    "time"
    "strconv"
    _ "github.com/go-sql-driver/mysql"
)

// Struct extending the Database class
type GameDatabase struct {
    *sql.DB
    isConnected bool
}

// Sets a database object and check if it can connect
func ConnectToDatabase() *GameDatabase {
    // Set database parameters
    dsn := DATABASE_USER + ":" + DATABASE_PASS +
        "@" + DATABASE_HOST + "/" + DATABASE_NAME
    db, err := sql.Open("mysql", dsn)

    gameDB := &GameDatabase{db, false}

    // Establish database availability
    if err != nil {
        fmt.Println("Database: Error preparing parameters:", err)
        return gameDB
    }

    err = db.Ping()
    if err != nil {
        fmt.Println("Database: Error pinging server:", err)
        return gameDB
    }

    gameDB.isConnected = true
    return gameDB
}

// Registers a player into the database and give back his assigned id
func (gameDB *GameDatabase) registerPlayer(userName string) (id uint64) {
    res, err := gameDB.Exec(
        "INSERT INTO players (username) VALUES(?);", userName)

    if err != nil {
        fmt.Println("Database: Error registering player:", err.Error())
        rand.Seed(time.Now().UnixNano())
        id = uint64(rand.Uint32())
    } else {
        resId, _ := res.LastInsertId()
        id = uint64(resId)
    }

    return
}

// Gets a player's name based on his id
func (gameDB *GameDatabase) getPlayerName(playerId uint64) (userName string) {
    err := gameDB.QueryRow("SELECT username FROM players WHERE player_id=?;",
        playerId).Scan(&userName)

    if err != nil {
        fmt.Println("Database: Error retreiving player information:",
            err.Error())
        userName = strconv.FormatUint(playerId, 16)
    }

    return
}
