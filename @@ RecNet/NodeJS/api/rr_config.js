import { GetConfig, GetDefaultAv, GetAvatarItems } from "../config.js"

export async function DefaultAvatar() {
    var GottenData = await GetDefaultAv()
    return GottenData
}
export async function DummyAvatarItems() {
    var GottenData = await GetAvatarItems()
    return GottenData
}
export async function MainConfig() {
    var GottenData = await GetConfig()
    return GottenData
}

export async function MessageOfTheDay() {
    var conf = await GetConfig()
    var motd = conf["messageOfTheDay"]
    return motd
}
export async function Dummy_DailyObjectives() {
    var dailyObjectives = [
        [
            {
                "type": -1,
                "score": -1
            },
            {
                "type": -1,
                "score": -1
            },
            {
                "type": -1,
                "score": -1
            }
        ]
    ]
    return dailyObjectives
}
export async function Dummy_MatchmakingParams() {
    var matchmakingParams = {
        "PreferFullRoomsFrequency": 5,
        "PreferEmptyRoomsFrequency": 5
    }
    return matchmakingParams
}
export async function ConfigTable() {
    var configTable =
        [
            {
                "Key": "Gift.DropChance",
                "Value": "0.33"
            },
            {
                "Key": "Gift.XP",
                "Value": "100"
            }
        ]
    return configTable
}
export async function RRConfig() {
    var motd = await MessageOfTheDay()
    var dailyObjectives = await Dummy_DailyObjectives()
    var matchmakingParams = await Dummy_MatchmakingParams()
    var configTable = await ConfigTable()
    var finalResult = {
        "MessageOfTheDay": motd,
        "MatchmakingParams": matchmakingParams,
        "DailyObjectives": dailyObjectives,
        "ConfigTable": configTable
    }
    return finalResult
}