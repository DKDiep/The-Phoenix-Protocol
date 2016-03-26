package main

import (
    "database/sql"
    "fmt"
    _ "github.com/go-sql-driver/mysql"
)

type GameDatabase struct {
    *sql.DB
}

func ConnectToDatabase() *GameDatabase {
    // Establish database availability
    dsn := DATABASE_USER + ":" + DATABASE_PASS +
        "@" + DATABASE_HOST + "/" + DATABASE_NAME
    db, err := sql.Open("mysql", dsn)

    if err != nil {
        fmt.Println("Database: Error preparing parameters:", err)
        return nil
    }

    err = db.Ping()
    if err != nil {
        fmt.Println("Database: Error pinging server:", err)
        db.Close()
        return nil
    }

    return &GameDatabase{db}
}
