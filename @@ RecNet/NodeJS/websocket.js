const WebSocket = require('ws');

const HostPort = 7777
const APIHostPort = 28960

const wss = new WebSocket.Server({ port: HostPort, path: '/api/notification/v2' });

var SessionIds = [] /* it would be cool if i could do {"PlayerId", "SessionId"} */
// ^^ useless

console.log("Listening on port " + HostPort + "...")

wss.on('connection', function connection(ws) {
    console.log('Client connected');

    var GottenProfile = undefined
    const SessionId = `${Math.floor(Math.random()*1000)}${Math.floor(Math.random()*1000)}${Math.floor(Math.random()*1000)}`

    ws.on('message', async function incoming(message) {
        // i have no idea what any of this is
        var WhatToSend = {
            "SessionId": SessionId,
        }
        if (!GottenProfile)
            GottenProfile = message
        
        console.log('received: %s', message);
        ws.send(JSON.stringify(WhatToSend));
    });
    ws.on('close', async function onclose() {
        // its a really quick and lazy implementation but it should hopefully work
        if (GottenProfile) {
            if (GottenProfile["PlayerId"]) {
                var fetchBody = JSON.stringify({"ws-user-id":GottenProfile["PlayerId"]})
                await fetch("http://localhost:" + APIHostPort + "/api/ws/handlequit", {body:fetchBody})
            }
        }
        console.log("Client disconnected")
    })
});
