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
const { RemovePreference, SetPreference } = require('./playerData')

var activeSession = {
    "Presence": {}
}

/*app.use(bodyParser.json({extended:true}))
app.use(bodyParser.urlencoded({extended:true}))*/
app.use(express.json({ type: ['application/json', 'text/json'] }))

app.listen(28960, () => {
    console.log("Listening on port 28960...")
})
// Test
app.post('/api/test', upload.none(), async (req, res) => {
    res.send(req.body)
})
// Analytics
app.post('/api/analytics/v1/session/event', async (req, res) => {
    console.log("Sending Event...")
    var returnVal = await apiAnalytics.SessionEvent(req.fields["SessionId"], req.fields["Category"], req.fields["Action"])
    res.send(returnVal)
})
// Version
app.get('/api/versioncheck/v1', async (req, res) => {
    console.log("Checking Version...")
    var returnVal = await apiVersion.VersionCheck(req.headers["X-Rec-Room-Version"])
    if (returnVal == 1) {
        res.sendStatus(200)
    } else {
        res.sendStatus(403)
    }
})
// Player
app.post("/api/players/v1/getorcreate", bodyParser.urlencoded({extended:true}), async (req, res) => {
    console.log("Getting/Creating Player...")
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
    console.log("Getting Player...")
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
    console.log("Getting Avatar...")
    var PlayerId = req.headers["x-rec-room-profile"]
    var GottenAvatar = await apiAvatar.GetAvatar(PlayerId)
    if (GottenAvatar != null)
        res.send(GottenAvatar)
    else
        res.status(500)
})
app.post("/api/avatar/v2/set", async (req, res) => {
    console.log("Setting Avatar...")
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
    console.log("Getting Preferences.")
    var PlayerId = req.headers["x-rec-room-profile"]
    var Settings = await apiPlayers.DownloadPreferences(PlayerId)
    if (Settings == null || Settings == 0) {
        res.send(404)
    } else {
        res.send(Settings)
    }
})
// placeholders
app.post("/api/settings/v2/set", upload.none(), async (req, res) => {
    console.log("Setting Preference...")
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
    console.log("Removing Preference...")
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
app.post("/api/players/v2/objective", upload.none(), async (req, res) => {
    console.log("Completing Objective...")
    // vars
    var PlayerId = req.headers["x-rec-room-profile"]
    var objectiveType = req.body["objectiveType"]
    var additionalXp = req.body["additionalXp"]
    var inParty = req.body["inParty"]
    // bleh
    res.send("placeholder, implement later.")
})
// Presence
app.get("/api/presence/v1/list", async (req, res) => {
    var presences = []
    req.body.forEach(element => {
        presences.push(activeSession["Presence"][element])
    });
    res.send(presences)
})
app.get("/api/presence/v1/:profileId", async (req, res) => {
    var ProfileId = req.params["profileId"]
    res.send(activeSession["Presence"][ProfileId])
})
app.post("/api/presence/v2", upload.none(), async (req, res) => {
    var PlayerId = req.body["PlayerId"]
    var GameSessionId = req.body["GameSessionId"]
    var AppVersion = req.body["AppVersion"]
    var LastUpdateTime = req.body["LastUpdateTime"]
    var Activity = req.body["Activity"]
    var Private = req.body["Private"]
    var AvailableSpace = req.body["AvailableSpace"]
    var GameInProgress = req.body["GameInProgress"]
    activeSession["Presence"][PlayerId] = {
        "PlayerId": PlayerId,
        "GameSessionId": GameSessionId,
        "AppVersion": AppVersion,
        "LastUpdateTime": LastUpdateTime,
        "Activity": Activity,
        "Private": Private,
        "AvailableSpace": AvailableSpace,
        "GameInProgress": GameInProgress
    }
    res.send("OK")
})