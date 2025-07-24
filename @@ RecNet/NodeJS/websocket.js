const WebSocket = require('ws');
const internalBus = require('./events');

const ApiRelationship = require("./api/relationship")

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

    internalBus.on("msg", (data) => {
        ws.send(JSON.stringify(data))
    })
    internalBus.on("game-msg", (data) => {
        ws.send("[GAME-MESSAGE]:" + JSON.stringify(data))
    })
    ws.on('message', async function incoming(message) {
        if (message.includes("AppVersion")) { // lazy hack to determine if its from rec room or not
            // i have no idea what any of this is
            var WhatToSend = {
                "SessionId": SessionId,
            }
            if (!GottenProfile)
                GottenProfile = message

            console.log('received: %s', message);
            ws.send(JSON.stringify(WhatToSend));
        } else if (message.includes("[GAME-MESSAGE]:")) {
            var actualData = message.toString('utf-8').substring(0, "[GAME-MESSAGE]:".length)
            var parsedData = JSON.stringify(actualData)
        } else {
            /* Types of Push Notifications
                RelationshipChanged = 1
                MessageReceived = 2
                MessageDeleted = 3
            */
            /* Types of Messages
			    GameInvite = 0,
			    GameInviteDeclined = 1,
			    GameJoinFailed = 2,
			    PartyActivitySwitch = 3,
			    FriendInvite = 4,
			    VoteToKick = 5
            */
            var parsedData = JSON.parse(message) // there MUST be a PlayerId
            var playerId = parsedData["PlayerId"]
            var messageType = parsedData["Type"]
            // i have no idea if events are global or not
            if (playerId == GottenProfile["PlayerId"]) {
                switch (messageType) {
                    case "relationship":
                        var AllRelationships = await ApiRelationship.Router_GetRelationship(playerId)
                        var ResponseToSend = {"Id":1, "Msg":JSON.stringify(AllRelationships)}
                        ws.send(ResponseToSend)
                    break;
                }
            }
        }
    });
    ws.on('close', async function onclose() {
        // its a really quick and lazy implementation but it should hopefully work
        if (GottenProfile) {
            if (GottenProfile["PlayerId"]) {
                console.log("Trying to handle game quit from " + GottenProfile["PlayerId"])
                var fetchBody = JSON.stringify({"ws-user-id":GottenProfile["PlayerId"]})
                await fetch("http://localhost:" + APIHostPort + "/api/ws/handlequit", {method:"POST",headers:{'User-Agent':'rr-websocket'}, body:fetchBody})
            }
        }
        console.log("Client disconnected")
    })
});
