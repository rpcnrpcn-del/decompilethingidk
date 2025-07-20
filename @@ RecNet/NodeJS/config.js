import { readFile } from 'node:fs'
// i got lazy.
var configJson = {
    "debug": true,
    "version": "20170118_EA",
    "serverversion": "Build: January 18th 2017 | Server: 200720252314",
    "messageOfTheDay": "This is running on PingNet 200720252236. Yipee!",
    "DailyObjectives": null,
}
var defaultAvatar = null
var avatarItems = null

// Default Avatar
readFile("./config/DefaultAvatar.json", function(err, data) {
    if (err) {
        console.log("Unable to read Default Avatar JSON!")
        return;
    }
    defaultAvatar = JSON.parse(data)
    console.log("Got Default Avatar!")
})
// Avatar Items
readFile("./config/AvatarItems.json", function(err, data) {
    if (err) {
        console.log("Unable to read Avatar Items JSON!")
        return;
    }
    avatarItems = JSON.parse(data)
    console.log("Got Avatar Items!")
})

/*fs.readFile("./data/config.json", function(err, data) {
    if (err) throw err
    configJson = JSON.parse(data)
    console.log("\n\n")
    console.log("//////////////////// Config")
    console.log(configJson)
    console.log("////////////////////\n\n")
})*/

console.log("\n\n")
console.log("//////////////////// Config")
console.log(configJson)
console.log("////////////////////\n\n")

/*module.exports = {
    GetConfig : async function() {
        return configJson
    },
    GetDefaultAv: async function() {
        return defaultAvatar
    },
    GetAvatarItems: async function() {
        return avatarItems
    }
}*/

export async function GetConfig() {
    return configJson
}
export async function GetDefaultAv() {
    return defaultAvatar
}
export async function GetAvatarItems() {
    return avatarItems
}