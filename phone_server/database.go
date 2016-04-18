package main

import (
    "database/sql"
    "fmt"
    _ "github.com/go-sql-driver/mysql"
    "math/rand"
    "strconv"
    "time"
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
func (gameDB *GameDatabase) getPlayerData(playerId uint64) (userName string, score uint64) {
    err := gameDB.QueryRow("SELECT username, score FROM players WHERE player_id=?;",
        playerId).Scan(&userName, &score)

    if err != nil {
        fmt.Println("Database: Error retreiving player information:",
            err.Error())
        userName = strconv.FormatUint(playerId, 16)
        score = 1337
    }

    return
}

// Logs the players scores, not threadsafe, the maps should not be
// changed while this is executing
func (gameDB *GameDatabase) logPlayerScores(spectators map[uint64]*Player,
    officers map[uint64]*Player) {
    // Prepare statement
    stmt, err := gameDB.Prepare(
        "UPDATE players SET score=? WHERE player_id=?;")

    if err != nil {
        fmt.Println("Database: Error preparing score logging statement:",
            err.Error())
        return
    }
    defer stmt.Close()

    // Send updates for all players
    for id, plr := range spectators {
        stmt.Exec(plr.score, id)
    }
    for id, plr := range officers {
        stmt.Exec(plr.score, id)
    }
}

// Set officers in the database
func (gameDB *GameDatabase) setOfficers(officers map[uint64]*Player) {
    // Prepare statement
    stmt, err := gameDB.Prepare(
        "UPDATE players SET in_main_game=true, score=0 WHERE player_id=?;")

    if err != nil {
        fmt.Println("Database: Error preparing set officer statement:",
            err.Error())
        return
    }
    defer stmt.Close()

    // Set players as officers in database
    for id, _ := range officers {
        stmt.Exec(id)
    }
}

// Unset officers in the database
func (gameDB *GameDatabase) resetOfficers() {
    // Prepare statement
    _, err := gameDB.Exec(
        "UPDATE players SET in_main_game=false, score=0 WHERE in_main_game=true;")

    if err != nil {
        fmt.Println("Database: Error unsetting officers:",
            err.Error())
        return
    }
}
