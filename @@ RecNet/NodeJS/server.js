// Packages
const express = require('express')
const bodyParser = require('body-parser')
const multer = require('multer')
const upload = multer()
const app = express()
const api = express.Router()
const cors = require('cors')
// APIs
const apiAnalytics = require('./api/analytics')
const apiVersion = require('./api/version')
const apiPlayers = require("./api/players")
const apiAvatar = require("./api/avatar")
const apiConfig = require("./api/rr_config")
const apiImage = require("./api/images")
const apiRelationship = require('./api/relationship')
const { RemovePreference, SetPreference } = require('./playerData')
const { CompleteObjective } = require('./api/objectives')

const HostPort = 28960

var activeSession = {
    "Presence": []
}

/*app.use(bodyParser.json({extended:true}))
app.use(bodyParser.urlencoded({extended:true}))*/
app.use(express.json({ type: ['application/json', 'text/json'] }))

app.listen(HostPort, () => {
    //console.log(`Listening on port ${HostPort}...`)
})
// Friending
async function AddFriend(PlayerId, OtherPlayer) {
    //console.log("Adding Friend...")
    // vars
    var RelationshipType = "3"
    // add friend
    var GottenData = await apiRelationship.Router_SetRelationship(PlayerId, OtherPlayer, RelationshipType)
    return GottenData
}
// Test
app.post('/api/test', upload.none(), async (req, res) => {
    res.send(req.body)
})
// Analytics
app.post('/api/analytics/v1/session/event', async (req, res) => {
    //console.log("Sending Event...")
    //var returnVal = await apiAnalytics.SessionEvent(req.fields["SessionId"], req.fields["Category"], req.fields["Action"])
    res.send("Received!")
})
// Version
app.get('/api/versioncheck/v1', async (req, res) => {
    //console.log("Checking Version...")
    var returnVal = await apiVersion.VersionCheck(req.headers["X-Rec-Room-Version"])
    if (returnVal == 1) {
        res.sendStatus(200)
    } else {
        res.sendStatus(403)
    }
})
// Player
app.post("/api/players/v1/getorcreate", bodyParser.urlencoded({extended:true}), async (req, res) => {
    //console.log("Getting/Creating Player...")
    // Fields
    var Platform = req.body["Platform"]
    var PlatformId = req.body["PlatformId"]
    var Name = req.body["Name"]
    // Funny :)
    var didCreate = await apiPlayers.GetOrCreate(Platform, PlatformId, Name)
    // Debug Response :)))))))
    if (didCreate == 2) {
        res.send("Invalid Data!")
        return
    }
    if (didCreate == 1) {
        var GottenProfile = await apiPlayers.DownloadProfile(PlatformId)
        // error :(
        if (GottenProfile == 0) {
            res.send("")
            return
        }
        // we got it!! yipee :D
        if (GottenProfile != 0) {
            res.send(GottenProfile)
            return
        }
        return;
    }
    if (didCreate == 0) {
        var GottenProfile = await apiPlayers.DownloadProfile(PlatformId)
        // error :(
        if (GottenProfile == 0 || GottenProfile == null) {
            res.status(500)
            return
        }
        // we got it!! yipee :D
        if (GottenProfile != 0) {
            res.send(GottenProfile)
            return
        }
        return;
    }
    res.send("Error.")
})
app.get("/api/players/v1/:PlayerId", async (req, res) => {
    //console.log("Getting Player...")
    var PlayerId = req.params["PlayerId"]
    var GottenProfile = await apiPlayers.DownloadProfile(PlayerId)
    // error :(
    if (GottenProfile == 0 || GottenProfile == null) {
        res.status(500)
        return
    }
    // we got it!! yipee :D
    if (GottenProfile != 0) {
        res.send(GottenProfile)
        return
    }
})
// Avatar
app.get("/api/avatar/v2", async (req, res) => {
    //console.log("Getting Avatar...")
    var PlayerId = req.headers["x-rec-room-profile"]
    var GottenAvatar = await apiAvatar.GetAvatar(PlayerId)
    if (GottenAvatar != null)
        res.send(GottenAvatar)
    else
        res.status(500)
})
app.post("/api/avatar/v2/set", async (req, res) => {
    //console.log("Setting Avatar...")
    // vars
    var PlayerId = req.headers["x-rec-room-profile"]
    var OutfitSelections = req.body["OutfitSelections"]
    var SkinColor = req.body["SkinColor"]
    var HairColor = req.body["HairColor"]
    // set avatar
    var HasSetAvatar = await apiAvatar.RouterSetAvatar(PlayerId, OutfitSelections, SkinColor, HairColor)
    if (HasSetAvatar == null || HasSetAvatar == false) {
        res.status(500)
    } else {
        res.send("done")
    }
})
app.get("/api/avatar/v3/items", async (req, res) => {
    var unlockedItems = await apiConfig.DummyAvatarItems()
    res.send(unlockedItems)
})
app.get("/api/avatar/v2/gifts", async (req, res) => {
    var dummyGifts = []
    res.send(dummyGifts)
})
// Config
app.get("/api/config/v2", async (req, res) => {
    var rrConfig = await apiConfig.RRConfig()
    res.send(rrConfig)
})
app.get("/api/settings/v2/", upload.none(), async (req, res) => {
    //console.log("Getting Preferences.")
    var PlayerId = req.headers["x-rec-room-profile"]
    var Settings = await apiPlayers.DownloadPreferences(PlayerId)
    if (Settings == null || Settings == 0) {
        res.send(404)
    } else {
        res.send(Settings)
    }
})
// Relationship
app.get("/api/relationships/v2/blockplayer", async (req, res) => {
    //console.log("Blocking Player...")
    // vars
    var PlayerId = req.headers["x-rec-room-profile"]
    var OtherPlayer = req.query["id"]
    // add friend
    var GottenData = await apiRelationship.Router_SetRelationship(PlayerId, OtherPlayer, "4")
    if (GottenData == null || GottenData == false) {
        res.send(500)
    } else {
        res.send("done")
    }
})
app.get("/api/relationships/v2/unblockplayer", async (req, res) => {
    //console.log("Unblocking Player...")
    // vars
    var PlayerId = req.headers["x-rec-room-profile"]
    var OtherPlayer = req.query["id"]
    // add friend
    var GottenData = await apiRelationship.Router_SetRelationship(PlayerId, OtherPlayer, "0")
    if (GottenData == null || GottenData == false) {
        res.send(500)
    } else {
        res.send("done")
    }
})
app.get("/api/relationships/v2/sendfriendrequest", async (req, res) => {
    //console.log("Sending Friend Request...")
    // vars
    var PlayerId = req.headers["x-rec-room-profile"]
    var OtherPlayer = req.query["id"]
    // add friend
    var GottenData = await apiRelationship.Router_SetRelationship(PlayerId, OtherPlayer, "1")
    if (GottenData == null || GottenData == false) {
        res.send(500)
    } else {
        res.send("done")
    }
})
app.get("/api/relationships/v2/acceptfriendrequest", async (req, res) => {
    //console.log("Accepting Friend Request...")
    // vars
    var PlayerId = req.headers["x-rec-room-profile"]
    var OtherPlayer = req.query["id"]
    // add friend
    var GottenData = AddFriend(PlayerId, OtherPlayer)
    if (GottenData == null || GottenData == false) {
        res.send(500)
    } else {
        res.send("done")
    }
})
app.get("/api/relationships/v2/removefriend", async (req, res) => {
    //console.log("Removing Friend...")
    // vars
    var PlayerId = req.headers["x-rec-room-profile"]
    var OtherPlayer = req.query["id"]
    // add friend
    var GottenData = await apiRelationship.Router_RemoveRelationship(PlayerId, OtherPlayer)
    if (GottenData == null || GottenData == false) {
        res.send(500)
    } else {
        res.send("done")
    }
})
app.get("/api/relationships/v2/addfriend", async (req, res) => {
    // vars
    var PlayerId = req.headers["x-rec-room-profile"]
    var OtherPlayer = req.query["id"]
    // add friend
    var GottenData = AddFriend(PlayerId, OtherPlayer)
    if (GottenData == null || GottenData == false) {
        res.send(500)
    } else {
        res.send("done")
    }

    /*//console.log("Adding Friend...")
    // vars
    var PlayerId = req.headers["x-rec-room-profile"]
    var OtherPlayer = req.params["PlayerID"]
    var RelationshipType = "3"
    // add friend
    var GottenData = await apiRelationship.Router_SetRelationship(PlayerId, OtherPlayer, RelationshipType)
    if (GottenData == null || GottenData == false) {
        res.send(500)
    } else {
        res.send("done")
    }*/
})
app.get("/api/relationships/v2/get", async (req, res) => {
    //console.log("Getting Relationships...")
    // vars
    var PlayerId = req.headers["x-rec-room-profile"]
    // get relationship status of my ex-wife
    var GottenData = await apiRelationship.Router_GetRelationship(PlayerId)
    if (GottenData == null || GottenData == false) {
        res.send(500)
    } else {
        res.send(GottenData)
    }
})
// messages
app.get("/api/messages/v2/get", async (req, res) => {
    res.send([]) // we have no messages to send at the moment.
})
// placeholders
app.post("/api/settings/v2/set", upload.none(), async (req, res) => {
    //console.log("Setting Preference...")
    // vars
    var PlayerId = req.headers["x-rec-room-profile"]
    var DidWeSucceed = await SetPreference(PlayerId, req.body["Key"], req.body["Value"])
    // error handling idfk
    if (DidWeSucceed == null || DidWeSucceed == false) {
        res.status(500)
    }
    res.send("done")
})
app.post("/api/settings/v2/remove", upload.none(), async (req, res) => {
    //console.log("Removing Preference...")
    // vars
    var PlayerId = req.headers["x-rec-room-profile"]
    var DidWeSucceed = await RemovePreference(PlayerId, req.body["Key"])
    // error handling idfk
    if (DidWeSucceed == null || DidWeSucceed == false) {
        res.status(500)
    }
    res.send("done")
})
// Objectives
app.post("/api/players/v2/objective", bodyParser.urlencoded({extended:true}), async (req, res) => {
    //console.log("Completing Objective...")
    //console.log(req.body)
    // vars
    var PlayerId = req.headers["x-rec-room-profile"]
    var objectiveType = req.body["objectiveType"]
    var additionalXp = req.body["additionalXp"]
    var inParty = req.body["inParty"]
    // bleh
    var GottenData = await CompleteObjective(PlayerId, objectiveType, additionalXp, inParty)
    res.send(GottenData)
})
// Images
app.post("/api/images/v2/profile", upload.single('image'), async (req, res) => {
    //console.log("Setting Profile Picture...")
    // vars
    var PlayerId = req.headers["x-rec-room-profile"]
    var ProfileImage = req.file
    // set pfp
    var DidSet = await apiImage.ProfileImage_Set(PlayerId, ProfileImage)
    if (DidSet) {
        res.send("done")
    } else {
        res.status(500)
    }
})
app.get("/api/images/v1/profile/:PlayerId", async (req, res) => {
    // vars
    var PlayerId = req.params["PlayerId"]
    // get pfp
    var ProfileImage = await apiImage.ProfileImage_Get(PlayerId)
    res.sendFile(ProfileImage)
})
// Presence
app.post("/api/presence/v1/list", async (req, res) => {
    var presences = []
    //console.log("Body")
    //console.log(req.body)
    //console.log("Active Session")
    //console.log(activeSession["Presence"])

    /*req.body.forEach(element => {
        activeSession["Presence"].forEach(session => {
            if (session["PlayerId"] == element) {
                presences.push(activeSession["Presence"][element])
            }
        })
    });*/

    req.body.forEach(element => {
        for (var i = 0; i < activeSession["Presence"].length; i++) {
            if (activeSession["Presence"].PlayerId != element) {
                continue;
            } else {
                presences.push(activeSession["Presence"][i])
            }
        }
    })

    //console.log("Got:")
    //console.log(presences)
    res.send(presences)
})
app.get("/api/presence/v1/:profileId", async (req, res) => {
    var ProfileId = req.params["profileId"]

    var presence = null
    activeSession["Presence"].forEach(session => {
        if (session["PlayerId"] == ProfileId) {
            presence = session
        }
    })

    res.send(presence)
})
app.post("/api/presence/v2", upload.none(), async (req, res) => {
    //console.log("Setting Presence...")
    var PlayerId = req.body["PlayerId"]
    var GameSessionId = req.body["GameSessionId"]
    var AppVersion = req.body["AppVersion"]
    var LastUpdateTime = req.body["LastUpdateTime"]
    var Activity = req.body["Activity"]
    var Private = req.body["Private"]
    var AvailableSpace = req.body["AvailableSpace"]
    var GameInProgress = req.body["GameInProgress"]
    var NewPresence = {
        "PlayerId": PlayerId,
        "GameSessionId": GameSessionId,
        "AppVersion": AppVersion,
        "LastUpdateTime": LastUpdateTime,
        "Activity": Activity,
        "Private": Private,
        "AvailableSpace": AvailableSpace,
        "GameInProgress": GameInProgress
    }

    var weHavePresence = false
    for (var i = 0; i < activeSession["Presence"].length; i++) {
        if (activeSession["Presence"][i].PlayerId != PlayerId) {
            continue;
        } else {
            activeSession["Presence"][i] = NewPresence
            weHavePresence = true
        }
    }
    if (!weHavePresence)
        activeSession["Presence"].push({NewPresence})

    /*activeSession["Presence"].push({
        "PlayerId": PlayerId,
        "GameSessionId": GameSessionId,
        "AppVersion": AppVersion,
        "LastUpdateTime": LastUpdateTime,
        "Activity": Activity,
        "Private": Private,
        "AvailableSpace": AvailableSpace,
        "GameInProgress": GameInProgress
    })*/
    res.send("OK")
})