import {readFile, writeFile} from "node:fs/promises"
import {existsSync} from "node:fs"
import path from 'path'

const rootDir = process.cwd()

const SettingsTemplate = [
    {
        "Key": "QualitySettings",
        "Value": "Fantastic"
    },
    {
        "Key": "VoiceChat",
        "Value": 0
    },
    {
        "Key": "ShowNames",
        "Value": 1
    },
    {
        "Key": "ShowRoomCenter",
        "Value": 0
    },
    {
        "Key": "ROTATION_INCREMENT",
        "Value": 0
    },
    {
        "Key": "MOTION_TELEPORT_ENABLED",
        "Value": 1
    },
    {
        "Key": "CONTINUOUS_ROTATION_MODE",
        "Value": 1
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

export async function CreateProfile(PlayerId, Name) {
    if (PlayerId == null || Name == null) return;
    const ProfilePath = rootDir + "\\data\\players\\" + PlayerId + ".json"
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

    await writeFile(ProfilePath, JSON.stringify(PlayerJson))
}
export async function GetProfile(PlayerId) {
    if (PlayerId == null) return;
    const ProfilePath = path.join(rootDir, 'data', 'players', `${PlayerId}.json`);
    if (!existsSync(ProfilePath)) return;
    // Read profile & return it.
    var profileData = await readFile(ProfilePath, 'utf8')
    var jsonProfile = JSON.parse(profileData)["Profile"]
    var textProfile = JSON.stringify(jsonProfile)
    return textProfile
}
export async function PlayerJSON(PlayerId) {
    if (PlayerId == null) return;
    const ProfilePath = path.join(rootDir, 'data', 'players', `${PlayerId}.json`);
    if (!existsSync(ProfilePath)) return;
    // Read profile & return it.
    var profileData = await readFile(ProfilePath, 'utf8')
    var jsonProfile = JSON.parse(profileData)
    return jsonProfile
}
export async function DoesProfileExist(PlayerId) {
    if (PlayerId == null) return;
    const ProfilePath = path.join(rootDir, 'data', 'players', `${PlayerId}.json`);
    // Does the profile already exist?
    if (existsSync(ProfilePath)) {
        return true;
    } else {
        return false;
    }
}