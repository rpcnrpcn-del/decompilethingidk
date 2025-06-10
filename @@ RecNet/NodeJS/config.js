const fs = require('node:fs')
// i got lazy.
const configJson = {
    "debug": true,
    "version": "20170125_EA",
    "messageOfTheDay": "Balls.",
    "DailyObjectives": null
}

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

module.exports = {
    GetConfig : async function() {
        return configJson
    }
}