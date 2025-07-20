const HostPort = 7777
// path
const path = require("path")
// http
const http = require("http")
// express
const express = require('express')
const app = express()
// socket.io
const server = http.createServer(app)
const { Server } = require("socket.io")
const io = new Server(server)
// Log if we're successful
server.listen(HostPort, () => {
    console.log("Listening on port " + HostPort.toString() + "...")
})
// websocket
io.on("connection", (socket) => {
    console.log("A user connected.")
})