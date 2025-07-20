import { LevelMap, XPMap } from "../config.js";
import { GetProfile, PlayerJSON } from "../playerData.js";

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

const HardLevelLimit = 30

// functions
export async function CompleteObjective(PlayerId, ObjectiveType, additonalXp, inParty) {
    if (PlayerId == null || additonalXp == null || inParty == null) return;
    // get path
    const ProfilePath = path.join(rootDir, 'data', 'players', `${PlayerId}`, "Profile.json");
    const xpMapping = XPMap()
    const levelMapping = LevelMap()
    // get profile
    var ProfileJson = GetProfile(PlayerId)
    // more vars
    var NextLevel = ProfileJson["Level"]
    var NextLevel_Str = NextLevel.toString()
    var XpRequiredToLevelUp = 0
    var OldXP = PlayerJson["XP"]
    // stuff i dont understand
    ProfileJson["XP"] += xpMapping[ObjectiveType] + additonalXp
    if (ProfileJson["Level"] < HardLevelLimit) {
        XpRequiredToLevelUp = ProfileJson["XP"] - levelMapping[NextLevel_Str]["requiredXP"]
        if (ProfileJson["XP"] >= levelMapping[NextLevel_Str]["requiredXP"]) {
            ProfileJson["Level"] += 1
        }
    }
    var deltaXp = ProfileJson["XP"] - OldXP
    ProfileJson["XpRequiredToLevelUp"] = XpRequiredToLevelUp
    // write xp n level stuff
    await writeFile(ProfilePath, JSON.stringify(ProfileJson, null, 3))
    return {"XpRequiredToLevelUp": XpRequiredToLevelUp, "XP": ProfileJson["XP"], "currentLevel": ProfileJson["Level"], "deltaXp": deltaXp}
}