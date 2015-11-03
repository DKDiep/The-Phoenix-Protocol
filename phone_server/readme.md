The directory for the spectator server

Run setup.sh to create a simlink from your $GOPATH/src directory to
this directory. Needed for "go" commands to work properly.

At the moment this only works as a web socket echo server.

Currently this server is also serving the web page files.
This is the only way I got the web sockets to work.
I plan to keep it like this for now as it is easier to develop both
the server and the web front end.
--Stoil
