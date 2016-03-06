package main

import (
        "fmt"
        "net/http"
        "net/url"
        //"golang.org/x/net/html"
        "log"
        "io/ioutil"
        "strings"
        "encoding/json"
        "database/sql"
        _ "github.com/go-sql-driver/mysql"

)

const (
    DB_HOST = "tcp(localhost:3306)"
    DB_NAME = "game_stats"
    DB_USER = /*"root"*/ "root"
    DB_PASS = /*""*/ "reverse"
)

type Scores struct{
    TeamName string `json:"team_name"`
    Score int `json:"score"`
}


func main() {
    fmt.Println("Starting Stats Server..")
    dsn := DB_USER + ":" + DB_PASS + "@" + DB_HOST + "/" + DB_NAME + "?charset=utf8"
    db, err := sql.Open("mysql", dsn);
    if err != nil {
        log.Fatal(err)  // Just for example purpose. You should use proper error handling instead of panic
    }
    defer db.Close()
    stmt, err := db.Prepare("INSERT game SET team_name=?,scores=?")
    if err != nil {
        panic(err.Error()) // proper error handling instead of panic in your app
    }
    defer stmt.Close()
    JSONString := ""
    //Handles game data which is sent periodiocally by the main game
    http.HandleFunc("/game_data", func(w http.ResponseWriter, r *http.Request) {
        byteArray, err1 := ioutil.ReadAll(r.Body);
        decodedString, err2 := url.QueryUnescape(string(byteArray));
        if err1 == nil {
                if err2 == nil {
                        JSONString = strings.TrimPrefix(decodedString,"JSON:=")
                        fmt.Println(JSONString)
                }       else{
            log.Fatal(err2)
                }
        }       else{
                log.Fatal(err1)
        }
    })
    http.Handle("/", http.FileServer(http.Dir("../web/stats_web")))
    http.HandleFunc("/json", func(w http.ResponseWriter, r *http.Request){
            fmt.Fprintf(w, "%q", JSONString)
    })
    //Handles data which is to be saved between games
    http.HandleFunc("/save_game_data", func(w http.ResponseWriter, r *http.Request) {
        var trimmedByteArray []byte
        byteArray, err1 := ioutil.ReadAll(r.Body);
        decodedString, err2 := url.QueryUnescape(string(byteArray));
        if err1 == nil {
                if err2 == nil {
                    JSONString = strings.TrimPrefix(decodedString,"JSON:=")
                    fmt.Println(JSONString)
                    trimmedByteArray = []byte(JSONString)
                }       else{
            log.Fatal(err2)
                }
        }       else{
                log.Fatal(err1)
        }
        var scores Scores
        err := json.Unmarshal(trimmedByteArray, &scores)
        res, err := stmt.Exec(scores.TeamName,scores.Score)
        if err != nil {
            panic(err.Error()) // proper error handling instead of panic in your app
            fmt.Println(res)
        }
    })
    log.Fatal(http.ListenAndServe(":8080", nil))
    // Prepare statement for reading data
}
