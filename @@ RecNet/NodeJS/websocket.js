const WebSocket = require('ws');
const HostPort = 7777
const wss = new WebSocket.Server({ port: HostPort, path: '/api/notification/v2' });

var SessionIds = [] /* it would be cool if i could do {"PlayerId", "SessionId"} */
// ^^ useless

console.log("Listening on port " + HostPort + "...")

wss.on('connection', function connection(ws) {
    console.log('Client connected');

    const SessionId = `${Math.floor(Math.random()*1000)}${Math.floor(Math.random()*1000)}${Math.floor(Math.random()*1000)}`

    ws.on('message', function incoming(message) {
        var WhatToSend = {
            "SessionId": SessionId,
        }
        console.log('received: %s', message);
        ws.send(JSON.stringify(WhatToSend));
    });
    ws.on('close', () => {
        console.log("Client disconnected")
    })
});
