package main

import (
    "bufio"
    "fmt"
    "os"
)

// Accepts predefined string patterns to affect internal state
func stdinHandler() {
    scanner := bufio.NewScanner(os.Stdin)
    for scanner.Scan() {
        input := scanner.Text()
        switch input {
        case "unblock admin":
            unblockAdmin()
        default:
            fmt.Println("Unrecognised input pattern \"" + input + "\".")
        }
    }
}
