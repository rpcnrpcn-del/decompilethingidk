import { GetConfig } from '../config.js'

export async function VersionCheck(GameVersion) {
    var RequiredVersion = await GetConfig()["version"]
    if (GameVersion != RequiredVersion) {
        return false
    }
    return true
}