const socket = new WebSocket("wss://127.0.0.1:25565")
socket.addEventListener('open', event => {
    console.log("Opened WebSocket! (wss://127.0.0.1:25565)")
    socket.send("Open")
})
socket.addEventListener('message', event => {
    console.log("Received Data!\n" + event.data)
})