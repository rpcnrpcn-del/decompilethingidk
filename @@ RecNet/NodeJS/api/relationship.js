import { SetRelationship, RemoveRelationship, GetRelationship } from "../playerData.js";

export async function Router_SetRelationship(PlayerId, OtherPlayer, RelationshipType) {
    var GottenData = await SetRelationship(PlayerId, OtherPlayer, RelationshipType)
    return GottenData
}
export async function Router_RemoveRelationship(PlayerId, OtherPlayer) {
    var GottenData = await RemoveRelationship(PlayerId, OtherPlayer)
    return GottenData
}
export async function Router_GetRelationship(PlayerId) {
    var GottenData = await GetRelationship(PlayerId)
    return GottenData
}