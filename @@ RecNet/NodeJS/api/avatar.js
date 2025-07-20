import {PlayerAvatar, SetAvatar} from "../playerData.js";

// Gifts
export async function CreateGift(Id, AvaterItemDesc, Xp) {
    console.log("Todo: Create Gift")
}
export async function GetGifts(Id, AvaterItemDesc, Xp) {
    console.log("Todo: Get Gifts")
}
// Items
export async function UnlockedItems(Player) {
    console.log("Todo: Get Unlocked Items")
}
// Avatar
export async function RouterSetAvatar(Player, OutfitSelections, SkinColor, HairColor) {
    var HasSet = await SetAvatar(Player, OutfitSelections, SkinColor, HairColor)
    return HasSet
}
export async function GetAvatar(Player) {
    if (Player == null) return;
    var playerData = await PlayerAvatar(Player)
    return playerData
}