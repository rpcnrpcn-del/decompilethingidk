import {join} from 'path'
import { LevelMap, XPMap } from "../config.js";
import { ProfileJson } from "../playerData.js";
import { writeFile } from 'fs/promises';

// json templates
export const T_ObjectiveComplete = {
    "deltaXp": 0,
    "currentLevel": 0,
    "xpRequiredToLevelUp": 0
}
export const T_CompleteObjective = {
    "objectiveType": "-1",
    "additonalXp": "0",
    "inParty": "false"
}

const rootDir = process.cwd()

const HardLevelLimit = 30

// functions
export async function CompleteObjective(PlayerId, ObjectiveType, additonalXp, inParty) {
    if (PlayerId == null || additonalXp == null || inParty == null) return;
    // get path
    const ProfilePath = join(rootDir, 'data', 'players', `${PlayerId}`, "Profile.json");
    const xpMapping = await XPMap()
    const levelMapping = await LevelMap()
    // get profile
    var profileJson = await ProfileJson(PlayerId)
    // more vars
    var NextLevel = profileJson["Level"] + 1
    var XpRequiredToLevelUp = 0
    var OldXP = profileJson["XP"]
    // logging
    console.log("Level:")
    console.log(profileJson["Level"])
    console.log("Next Level:")
    console.log(NextLevel)
    console.log("Level Map 1:")
    console.log(levelMapping)
    console.log("Level Map 2:")
    console.log(levelMapping[NextLevel.toString()])
    console.log("Level Map 3:")
    console.log(levelMapping[NextLevel.toString()]["requiredXp"])
    console.log("XP Map: %d", xpMapping[ObjectiveType])
    console.log("EX XP Map: %d", additonalXp)
    // stuff i dont understand
    profileJson["XP"] += xpMapping[ObjectiveType] + additonalXp
    if (profileJson["Level"] < HardLevelLimit) {
        XpRequiredToLevelUp = profileJson["XP"] - levelMapping[NextLevel]["requiredXp"]
        if (profileJson["XP"] >= levelMapping[NextLevel]["requiredXp"]) {
            profileJson["Level"] += 1
        }
    }
    var deltaXp = profileJson["XP"] - OldXP
    profileJson["XpRequiredToLevelUp"] = XpRequiredToLevelUp
    // write xp n level stuff
    await writeFile(ProfilePath, JSON.stringify(profileJson, null, 3))
    return {"xpRequiredToLevelUp": XpRequiredToLevelUp, "currentXp": profileJson["XP"], "currentLevel": profileJson["Level"], "deltaXp": deltaXp}
}