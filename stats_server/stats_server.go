package main

import (
	"fmt"
	"net/http"
	"net/url"
	"log"
	"io/ioutil"
	"strings"
)

func main() {
	fmt.Println("Starting Stats Server..")
	JSONString := ""
	http.HandleFunc("/game_data", func(w http.ResponseWriter, r *http.Request) {
		byteArray, err1 := ioutil.ReadAll(r.Body);
		decodedString, err2 := url.QueryUnescape(string(byteArray));
		if err1 == nil {
			if err2 == nil {
				JSONString = strings.TrimPrefix(decodedString,"JSON:=")
				fmt.Println(decodedString)
			}	else{
		    log.Fatal(err2)
			}
		}	else{
			log.Fatal(err1)
		}
	})
	http.Handle("/", http.FileServer(http.Dir("../web/stats_web")))
	http.HandleFunc("/json", func(w http.ResponseWriter, r *http.Request){
		fmt.Fprintf(w, "%q", JSONString)
	})
	log.Fatal(http.ListenAndServe(":8080", nil))
}
