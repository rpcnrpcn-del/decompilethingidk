import {CreateProfile, DoesProfileExist, GetProfile} from "../playerData.js";

export async function GetOrCreate(Platform, PlatformPlayerId, Name) {
    if (Platform == null || PlatformPlayerId == null || Name == null) return 2;
    var doesExist = await DoesProfileExist(PlatformPlayerId);
    // create profile if we dont have one !!
    if (!doesExist) {
        console.log("Attempting to create profile for " + PlatformPlayerId);
        await CreateProfile(PlatformPlayerId, Name);
        return 1;
    } else {
        console.log("Attempting to download profile for " + PlatformPlayerId);
        // todo: download profile
        return 0;
    }
}
export async function DownloadProfile(PlayerId) {
    if (PlayerId == null) return 0;
    var gottenProfile = await GetProfile(PlayerId);
    return gottenProfile;
}