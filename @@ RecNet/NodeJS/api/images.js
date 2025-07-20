import { SetPFP, GetPFP, GetPFP_Path } from "../playerData.js";

// Profile Pictures
export async function ProfileImage_Set(PlayerId, Image) {
    var GottenData = await SetPFP(PlayerId, Image)
    return GottenData
}
export async function ProfileImage_Get(PlayerId) {
    var GottenData = await GetPFP_Path(PlayerId)
    return GottenData
}