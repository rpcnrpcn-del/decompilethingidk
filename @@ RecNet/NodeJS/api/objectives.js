import {join} from 'path'
import { LevelMap, XPMap } from "../config.js";
import { ProfileJson } from "../playerData.js";
import { writeFile } from 'fs/promises';

/*
	public enum ObjectiveType
	{
		// Token: 0x04002C90 RID: 11408
		Default = -1,
		// Token: 0x04002C91 RID: 11409
		FirstSessionOfDay = 1,
		// Token: 0x04002C92 RID: 11410
		DailyObjective1 = 10,
		// Token: 0x04002C93 RID: 11411
		DailyObjective2,
		// Token: 0x04002C94 RID: 11412
		DailyObjective3,
		// Token: 0x04002C95 RID: 11413
		OOBE_OpenMenu = 20,
		// Token: 0x04002C96 RID: 11414
		OOBE_GoToLockerRoom,
		// Token: 0x04002C97 RID: 11415
		OOBE_GoToActivity,
		// Token: 0x04002C98 RID: 11416
		CharadesGames = 100,
		// Token: 0x04002C99 RID: 11417
		CharadesWinsPerformer,
		// Token: 0x04002C9A RID: 11418
		CharadesWinsGuesser,
		// Token: 0x04002C9B RID: 11419
		DiscGolfWins = 200,
		// Token: 0x04002C9C RID: 11420
		DiscGolfGames,
		// Token: 0x04002C9D RID: 11421
		DiscGolfHolesUnderPar,
		// Token: 0x04002C9E RID: 11422
		DodgeballWins = 300,
		// Token: 0x04002C9F RID: 11423
		DodgeballGames,
		// Token: 0x04002CA0 RID: 11424
		DodgeballHits,
		// Token: 0x04002CA1 RID: 11425
		PaddleballGames = 400,
		// Token: 0x04002CA2 RID: 11426
		PaddleballWins,
		// Token: 0x04002CA3 RID: 11427
		PaddleballScores,
		// Token: 0x04002CA4 RID: 11428
		PaintballAnyModeGames = 500,
		// Token: 0x04002CA5 RID: 11429
		PaintballAnyModeWins,
		// Token: 0x04002CA6 RID: 11430
		PaintballAnyModeHits,
		// Token: 0x04002CA7 RID: 11431
		PaintballCTFWins = 600,
		// Token: 0x04002CA8 RID: 11432
		PaintballCTFGames,
		// Token: 0x04002CA9 RID: 11433
		PaintballCTFHits,
		// Token: 0x04002CAA RID: 11434
		PaintballFlagCaptures,
		// Token: 0x04002CAB RID: 11435
		PaintballTeamBattleWins = 700,
		// Token: 0x04002CAC RID: 11436
		PaintballTeamBattleGames,
		// Token: 0x04002CAD RID: 11437
		PaintballTeamBattleHits,
		// Token: 0x04002CAE RID: 11438
		SoccerWins = 800,
		// Token: 0x04002CAF RID: 11439
		SoccerGames,
		// Token: 0x04002CB0 RID: 11440
		SoccerGoals
	}
*/

// json templates
export const T_ObjectiveComplete = {
    "deltaXp": 0,
    "currentLevel": 0,
    "xpRequiredToLevelUp": 0
}
export const T_CompleteObjective = {
    "objectiveType": "-1",
    "additonalXp": "0",
    "inParty": "false"
}

const rootDir = process.cwd()

const HardLevelLimit = 30

// functions
export async function CompleteObjective(PlayerId, ObjectiveType, additonalXp, inParty) {
    if (PlayerId == null || additonalXp == null || inParty == null) return;
    // get path
    const ProfilePath = join(rootDir, 'data', 'players', `${PlayerId}`, "Profile.json");
    const xpMapping = await XPMap()
    const levelMapping = await LevelMap()
    // get profile
    var profileJson = await ProfileJson(PlayerId)
    // more vars
    var NextLevel = parseInt(profileJson["Level"]) + 1
    var XpRequiredToLevelUp = 0
    var OldXP = parseInt(profileJson["XP"])
    var NewXP = parseInt(profileJson["XP"])
    var NewLevel = parseInt(profileJson["Level"])
    // logging
    /*console.log("Level:")
    console.log(profileJson["Level"])
    console.log("Next Level:")
    console.log(NextLevel)
    console.log("Level Map 1:")
    console.log(levelMapping)
    console.log("Level Map 2:")
    console.log(levelMapping[NextLevel.toString()])
    console.log("Level Map 3:")
    console.log(levelMapping[NextLevel.toString()]["requiredXp"])
    console.log("XP Map: %d", xpMapping[ObjectiveType])
    console.log("EX XP Map: %d", additonalXp)*/
    // stuff i dont understand
    NewXP += parseInt(xpMapping[ObjectiveType]) + parseInt(additonalXp)
    var deltaXp = NewXP - OldXP
    if (NewLevel < HardLevelLimit) {
        XpRequiredToLevelUp = parseInt(levelMapping[NextLevel]["requiredXp"]) - NewXP
        if (NewXP >= levelMapping[NextLevel]["requiredXp"]) {
            NewLevel += 1
            NewXP = 0
            XpRequiredToLevelUp = parseInt(levelMapping[NextLevel]["requiredXp"])
        }
    }
    profileJson["XpRequiredToLevelUp"] = XpRequiredToLevelUp
    // write xp n level stuff
    profileJson.Level = NewLevel
    profileJson.XP = NewXP
    await writeFile(ProfilePath, JSON.stringify(profileJson, null, 3))
    return {"xpRequiredToLevelUp": XpRequiredToLevelUp, "currentXp": NewXP, "currentLevel": NewLevel, "deltaXp": deltaXp}
}