import {readFile, writeFile, mkdir} from "node:fs/promises"
import {existsSync} from "node:fs"
import path from 'path'
import { GetConfig, GetDefaultAv, GetAvatarItems } from "./config.js"

const rootDir = process.cwd()

const SettingsTemplate = [
    {
        "Key": "QualitySettings",
        "Value": "3"
    },
    {
        "Key": "VoiceChat",
        "Value": "0"
    },
    {
        "Key": "ShowNames",
        "Value": "1"
    },
    {
        "Key": "ShowRoomCenter",
        "Value": "1"
    },
    {
        "Key": "ROTATION_INCREMENT",
        "Value": "0"
    },
    {
        "Key": "MOTION_TELEPORT_ENABLED",
        "Value": "1"
    },
    {
        "Key": "CONTINUOUS_ROTATION_MODE",
        "Value": "1"
    },
    {
        "Key": "MOD_BLOCKED_TIME",
        "Value": "0"
    },
    {
        "Key": "MOD_BLOCKED_DURATION",
        "Value": "0"
    }
]
const ProfileTemplate = {
    "Id": -1,
    "Username": "Username",
    "DisplayName": "DisplayName",
    "XP": 0,
    "Level": 1,
    "Reputation": 0,
    "Verified": true,
    "Developer": false
}
const AvatarTemplate = {
    "OutfitSelections": "",
    "SkinColor": "",
    "HairColor": ""
}
const GiftTemplate = {
    "Id": -1,
    "AvatarItemDesc": "",
    "Xp": -1
}
const PlayerTemplate = {
    "Profile": ProfileTemplate,
    "Avatar": AvatarTemplate,
    "Settings": SettingsTemplate
}
const MiscTemplate = {
    "ProfilePicture": "default"
}

export async function CreateProfile(PlayerId, Name) {
    if (PlayerId == null || Name == null) return;
    const ProfilePath = rootDir + "\\data\\players\\" + PlayerId + "\\"
    var profileTemplate = PlayerTemplate
    // Does the profile already exist?
    if (existsSync(ProfilePath)) {
        console.log("Player ID " + PlayerId + " already exists!")
        return;
    }
    // Create Profile :)
    console.log("Creating Profile for Player " + PlayerId + "...")
    var PlayerJson = JSON.parse(JSON.stringify(profileTemplate))

    PlayerJson.Profile.Id = PlayerId
    PlayerJson.Profile.Username = Name
    PlayerJson.Profile.DisplayName = Name
    PlayerJson.Avatar = await GetDefaultAv();
    PlayerJson.Misc = MiscTemplate

    mkdir(ProfilePath)

    await writeFile(ProfilePath + "Profile.json", JSON.stringify(PlayerJson["Profile"], null, 3))
    await writeFile(ProfilePath + "Avatar.json", JSON.stringify(PlayerJson["Avatar"], null, 3))
    await writeFile(ProfilePath + "Settings.json", JSON.stringify(PlayerJson["Settings"], null, 3))
    await writeFile(ProfilePath + "Misc.json", JSON.stringify(PlayerJson["Misc"], null, 3))
}
export async function GetProfile(PlayerId) {
    if (PlayerId == null) return;
    const ProfilePath = path.join(rootDir, 'data', 'players', `${PlayerId}`, 'Profile.json');
    if (!existsSync(ProfilePath)) return;
    // Read profile & return it.
    var profileData = await readFile(ProfilePath, 'utf8')
    var jsonProfile = JSON.parse(profileData)
    var textProfile = JSON.stringify(jsonProfile)
    return textProfile
}
export async function PlayerJSON(PlayerId) {
    if (PlayerId == null) return;
    const ProfilePath = path.join(rootDir, 'data', 'players', `${PlayerId}`);

    const ProfileJson = path.join(ProfilePath, 'Profile.json');
    const AvatarJson = path.join(ProfilePath, 'Avatar.json');
    const SettingsJson = path.join(ProfilePath, 'Settings.json');
    const MiscJson = path.join(ProfilePath, 'Misc.json');

    if (!existsSync(ProfilePath)) return;
    // Read profile & return it.

    var profileData = await readFile(ProfileJson, 'utf8')
    var avatarData = await readFile(AvatarJson, 'utf8')
    var settingsData = await readFile(SettingsJson, 'utf8')
    var miscData = await readFile(MiscJson, 'utf8')

    profileData = JSON.parse(profileData)
    avatarData = JSON.parse(avatarData)
    settingsData = JSON.parse(settingsData)
    miscData = JSON.parse(miscData)

    var jsonProfile = {"Profile":profileData,"Avatar":avatarData,"Settings":settingsData,"Misc":miscData}

    return jsonProfile
}
export async function PlayerSettings(PlayerId) {
    if (PlayerId == null) return;
    const ProfilePath = path.join(rootDir, 'data', 'players', `${PlayerId}`, 'Settings.json');
    if (!existsSync(ProfilePath)) return;
    // Read settings & return it.
    var settingsData = await readFile(ProfilePath, 'utf8')
    settingsData = JSON.parse(settingsData)
    return settingsData
}
export async function DoesProfileExist(PlayerId) {
    if (PlayerId == null) return;
    const ProfilePath = path.join(rootDir, 'data', 'players', `${PlayerId}`, 'Profile.json');
    // Does the profile already exist?
    if (existsSync(ProfilePath)) {
        return true;
    } else {
        return false;
    }
}
export async function PlayerPresence(PlayerId) {
    
}
export async function RemovePreference(PlayerId, Key) {
    if (PlayerId == null || Key == null || Value == null) return;
    // get path
    const ProfilePath = path.join(rootDir, 'data', 'players', `${PlayerId}`, "Settings.json");
    // get settings & set setting
    var Settings = await PlayerSettings(PlayerId)

    for (var i = 0; i < Settings.length; i++) {
        if (Settings[i]["Key"] != Key)
            continue;
        Settings[i]["Value"] = null
        break;
    }

    // write settings file
    await writeFile(ProfilePath, JSON.stringify(Settings, null, 3))
    return true
}
export async function SetPreference(PlayerId, Key, Value) {
    if (PlayerId == null || Key == null || Value == null) return;
    // get path
    const ProfilePath = path.join(rootDir, 'data', 'players', `${PlayerId}`, "Settings.json");
    // get settings & set setting
    var Settings = await PlayerSettings(PlayerId)

    var couldFindSetting = false
    for (var i = 0; i < Settings.length; i++) {
        if (Settings[i]["Key"] != Key)
            continue;
        Settings[i]["Value"] = Value
        couldFindSetting = true
        break;
    }
    if (!couldFindSetting)
        Settings.push({"Key":Key,"Value":Value})

    // write settings file
    await writeFile(ProfilePath, JSON.stringify(Settings, null, 3))
    return true
}