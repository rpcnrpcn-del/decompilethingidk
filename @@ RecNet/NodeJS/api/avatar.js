import {PlayerJSON} from "../playerData.js";

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
export async function SetAvatar(Player, OutfitSelections, SkinColor, HairColor) {
    console.log("Todo: Set Avatar")
}
export async function GetAvatar(Player) {
    if (Player == null) return;
    var playerData = await PlayerJSON(Player)
    var playerAvatar = playerData["Avatar"]
    return playerAvatar
}