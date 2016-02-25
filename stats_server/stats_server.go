package main

import (
	"fmt"
	"net/http"
	"net/url"
	"golang.org/x/net/html"
	"log"
	"io/ioutil"
	"strings"
)

func main() {
	JSONString := ""
	http.HandleFunc("/bar", func(w http.ResponseWriter, r *http.Request) {
		fmt.Fprintf(w, "Hello, %q", html.EscapeString(r.URL.Path))
		// fmt.Printf("Cello, %q", html.EscapeString(r.URL.Path))

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

		// hah, err := ioutil.ReadAll(r.Body);
		// if err != nil {
		// 	fmt.Fprintf(w, "%s", err)
		// }
		// fmt.Fprintf(w,"%s",hah)
	})
	http.Handle("/", http.FileServer(http.Dir("../web/stats_web")))
	http.HandleFunc("/json", func(w http.ResponseWriter, r *http.Request){
		fmt.Fprintf(w, "%q", JSONString)
	})
	log.Fatal(http.ListenAndServe(":8080", nil))
}
