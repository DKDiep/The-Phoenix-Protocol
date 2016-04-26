package main

import (
	"fmt"
	"net/http"
	"net/url"
	"database/sql"
	"encoding/json"
	_ "github.com/go-sql-driver/mysql"
	"io/ioutil"
	"log"
	"strings"
)

const (
	DB_HOST = "tcp(localhost:3306)"
	DB_NAME = "game"
	DB_USER = "root"
	DB_PASS = "password"
)

type Scores struct {
	TeamName string `json:"team_name"`
	Score    int    `json:"score"`
}

type Player struct {
    Name   string    `json:"name"`
    Score  int       `json:"score"`
}

func main() {
	fmt.Println("Starting Stats Server..")
	dsn := DB_USER + ":" + DB_PASS + "@" + DB_HOST + "/" + DB_NAME + "?charset=utf8"

	db, err := sql.Open("mysql", dsn)
    defer db.Close()
	if err != nil {
		log.Fatal(err)
	}


    // Prepare query for inserting into game
	saveGameStmt, err := db.Prepare("INSERT game SET team_name=?, scores=?")
    defer saveGameStmt.Close()
	if err != nil {
		panic(err.Error())
	}

    // Prepare query for getting spectator
    spectatorStmt, err := db.Prepare("SELECT username, score FROM players WHERE player_id > 1 AND in_main_game = 0 ORDER BY score DESC LIMIT 10")
    defer spectatorStmt.Close()
    if err != nil {
        panic(err.Error())
    }

	mainGameJson := ""

	//Handles game data which is sent periodiocally by the main game
	http.HandleFunc("/game_data", func(w http.ResponseWriter, r *http.Request) {
		byteArray, err1 := ioutil.ReadAll(r.Body)
		decodedString, err2 := url.QueryUnescape(string(byteArray))
		if err1 == nil {
			if err2 == nil {
				mainGameJson = strings.TrimPrefix(decodedString, "JSON:=")
				fmt.Println(mainGameJson)
			} else {
				log.Fatal(err2)
			}
		} else {
			log.Fatal(err1)
		}
	})

	http.Handle("/", http.FileServer(http.Dir("../web/stats_web")))


	http.HandleFunc("/json", func(w http.ResponseWriter, r *http.Request) {
		fmt.Fprintf(w, "%q", mainGameJson)
	})


    http.HandleFunc("/get_spectator_data", func(w http.ResponseWriter, r *http.Request) {
        var (
        	name string
        	score int
        )
        var players []Player;
        rows, err := spectatorStmt.Query()
        if err != nil {
            log.Fatal(err)
        }
        defer rows.Close()
        for rows.Next() {
            err := rows.Scan(&name, &score)
        	if err != nil {
        		log.Fatal(err)
        	}
            players = append(players, Player{
                Name:   name,
                Score:  score,
            })
        }
        if err = rows.Err(); err != nil {
            log.Fatal(err)
        }
        spectatorGameJson, _ := json.Marshal(players)
        fmt.Fprintf(w, "%s", "{\"spectators\": " + string(spectatorGameJson) + "}")
    })


	//Handles data which is to be saved between games
	http.HandleFunc("/save_game_data", func(w http.ResponseWriter, r *http.Request) {
		var trimmedByteArray []byte
		byteArray, err1 := ioutil.ReadAll(r.Body)
		decodedString, err2 := url.QueryUnescape(string(byteArray))
		if err1 == nil {
			if err2 == nil {
				mainGameJson = strings.TrimPrefix(decodedString, "JSON:=")
				fmt.Println(mainGameJson)
				trimmedByteArray = []byte(mainGameJson)
			} else {
				log.Fatal(err2)
			}
		} else {
			log.Fatal(err1)
		}
		var scores Scores
		err := json.Unmarshal(trimmedByteArray, &scores)
		_, err = saveGameStmt.Exec(scores.TeamName, scores.Score)
		if err != nil {
			panic(err.Error())
		}
	})

	log.Fatal(http.ListenAndServe(":8081", nil))
}
