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
    "XpRequiredToLevelUp": 100,
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
const MiscTemplate = {
    "ProfilePicture": "default"
}
const RelationshipTemplate = []

const PlayerTemplate = {
    "Profile": ProfileTemplate,
    "Avatar": AvatarTemplate,
    "Settings": SettingsTemplate,
    "Misc": MiscTemplate,
    "Relationship": RelationshipTemplate
}
// Profile Creation n stuff
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
    PlayerJson.Relationship = RelationshipTemplate

    await mkdir(ProfilePath)

    await writeFile(ProfilePath + "Profile.json", JSON.stringify(PlayerJson["Profile"], null, 3))
    await writeFile(ProfilePath + "Avatar.json", JSON.stringify(PlayerJson["Avatar"], null, 3))
    await writeFile(ProfilePath + "Settings.json", JSON.stringify(PlayerJson["Settings"], null, 3))
    await writeFile(ProfilePath + "Misc.json", JSON.stringify(PlayerJson["Misc"], null, 3))
    await writeFile(ProfilePath + "Relationship.json", JSON.stringify(PlayerJson["Relationship"], null, 3))
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
export async function ProfileJson(PlayerId) {
    if (PlayerId == null) return;
    const ProfilePath = path.join(rootDir, 'data', 'players', `${PlayerId}`, 'Profile.json');
    if (!existsSync(ProfilePath)) return;
    // Read settings & return it.
    var jsonData = await readFile(ProfilePath, 'utf8')
    jsonData = JSON.parse(jsonData)
    console.log(jsonData)
    return jsonData
}
// JSON Utils
export async function PlayerJSON(PlayerId) {
    if (PlayerId == null) return;
    const ProfilePath = path.join(rootDir, 'data', 'players', `${PlayerId}`);

    const ProfileJson = path.join(ProfilePath, 'Profile.json');
    const AvatarJson = path.join(ProfilePath, 'Avatar.json');
    const SettingsJson = path.join(ProfilePath, 'Settings.json');
    const MiscJson = path.join(ProfilePath, 'Misc.json');
    const RelationshipJson = path.join(ProfilePath, 'Relationship.json');

    if (!existsSync(ProfilePath)) return;
    // Read profile & return it.

    var profileData = await readFile(ProfileJson, 'utf8')
    var avatarData = await readFile(AvatarJson, 'utf8')
    var settingsData = await readFile(SettingsJson, 'utf8')
    var miscData = await readFile(MiscJson, 'utf8')
    var relationshipData = await readFile(RelationshipJson, 'utf8')

    profileData = JSON.parse(profileData)
    avatarData = JSON.parse(avatarData)
    settingsData = JSON.parse(settingsData)
    miscData = JSON.parse(miscData)
    relationshipData = JSON.parse(relationshipData)

    var jsonProfile = {"Profile":profileData,"Avatar":avatarData,"Settings":settingsData,"Misc":miscData,"Relationship":relationshipData}

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
export async function PlayerAvatar(PlayerId) {
    if (PlayerId == null) return;
    const ProfilePath = path.join(rootDir, 'data', 'players', `${PlayerId}`, 'Avatar.json');
    if (!existsSync(ProfilePath)) return;
    // Read settings & return it.
    var gottenData = await readFile(ProfilePath, 'utf8')
    gottenData = JSON.parse(gottenData)
    return gottenData
}
export async function PlayerRelationship(PlayerId) {
    if (PlayerId == null) return;
    const ProfilePath = path.join(rootDir, 'data', 'players', `${PlayerId}`, 'Relationship.json');
    if (!existsSync(ProfilePath)) return;
    // Read settings & return it.
    var gottenData = await readFile(ProfilePath, 'utf8')
    gottenData = JSON.parse(gottenData)
    return gottenData
}
export async function DoesProfileExist(PlayerId) {
    if (PlayerId == null) return;
    const ProfilePath = /*rootDir + "\\data\\players\\" + PlayerId + "\\"*/ path.join(rootDir, 'data', 'players', `${PlayerId}`);
    // Does the profile already exist?
    if (existsSync(ProfilePath)) {
        return true;
    } else {
        return false;
    }
}
// avatar
export async function SetAvatar(PlayerId, OutfitSelections, SkinColor, HairColor) { 
    if (PlayerId == null || OutfitSelections == null || SkinColor == null || HairColor == null) return;
    // get path
    const ProfilePath = path.join(rootDir, 'data', 'players', `${PlayerId}`, "Avatar.json");
    // get settings & set setting
    var Avatar = await PlayerAvatar(PlayerId)

    Avatar["OutfitSelections"] = OutfitSelections
    Avatar["SkinColor"] = SkinColor
    Avatar["HairColor"] = HairColor

    // write settings file
    await writeFile(ProfilePath, JSON.stringify(Avatar, null, 3))
    return true
}
// todo
export async function PlayerPresence(PlayerId) {
    
}
// Preferences
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
// pfp
export async function GetPFP(PlayerId) {
    if (PlayerId == null) return;
    // get path
    const ProfilePath = path.join(rootDir, 'data', 'players', `${PlayerId}`, "pfp.png");
    const DefaultPath = path.join(rootDir, "data", "pfp", "default.png")
    if (existsSync(ProfilePath)) {
        var readImage = await readFile(ProfilePath, 'binary')
        return readImage
    } else {
        var readImage = await readFile(DefaultPath, 'binary')
        return readImage
    }
}
export async function GetPFP_Path(PlayerId) {
    // get path
    const ProfilePath = path.join(rootDir, 'data', 'players', `${PlayerId}`, "pfp.png");
    const DefaultPath = path.join(rootDir, "data", "pfp", "default.png")
    // return path
    if (existsSync(ProfilePath)) {
        return ProfilePath
    } else {
        return DefaultPath
    }
}
export async function SetPFP(PlayerId, Image) {
    if (PlayerId == null || Image == null) return;
    // get path
    const ProfilePath = path.join(rootDir, 'data', 'players', `${PlayerId}`, "pfp.png");
    // write pfp file
    await writeFile(ProfilePath, Image.buffer)
    return true
}
// relationship
/*
			None = 0,
			FriendRequestSent = 1,
			FriendRequestReceived = 2,
			Friend = 3,
			BlockedLocal = 4,
			BlockedRemote = 5,
			BlockedMutual = 6
*/
export async function SetRelationship(PlayerId, OtherPlayer, RelationshipType) {
    if (PlayerId == null || OtherPlayer == null || RelationshipType == null) return false;
    // get path
    const ProfilePath = path.join(rootDir, 'data', 'players', `${PlayerId}`, "Relationship.json");
    // get relationship & set relationship
    var Relationship = await PlayerRelationship(PlayerId)

    var couldFindPlayer = false
    for (var i = 0; i < Relationship.length; i++) {
        if (Relationship[i]["PlayerID"] != OtherPlayer)
            continue;
        Relationship[i]["Relationship"] = RelationshipType
        couldFindPlayer = true
        break;
    }
    if (!couldFindPlayer)
        Relationship.push({"PlayerID":OtherPlayer,"RelationshipType":RelationshipType})

    // write settings file
    await writeFile(ProfilePath, JSON.stringify(Relationship, null, 3))
    return true
}
export async function RemoveRelationship(PlayerId, OtherPlayer) {
    if (PlayerId == null || OtherPlayer == null) return;
    // get path
    const ProfilePath = path.join(rootDir, 'data', 'players', `${PlayerId}`, "Relationship.json");
    // get settings & set setting
    var Relationship = await PlayerRelationship(PlayerId)

    for (var i = 0; i < Relationship.length; i++) {
        if (Relationship[i]["PlayerID"] != OtherPlayer)
            continue;
        Relationship[i] = null
        break;
    }

    // write settings file
    await writeFile(ProfilePath, JSON.stringify(Relationship, null, 3))
    return true
}
export async function GetRelationship(PlayerId) {
    if (PlayerId == null) return;
    // get relationship stuff
    var Relationship = await PlayerRelationship(PlayerId)
    return Relationship
}