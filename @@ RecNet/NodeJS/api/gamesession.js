/*
What's expected:
    string Id
    string AppVersion
    string Activity
    bool Private
    int AvailableSpace
    bool GameInProgress
    List<ulong> PlayerIds
*/

export function CreateGameSession(Id, AppVersion, Activity, Private, AvailableSpace, GameInProgress, PlayerIds) {
    var newSession = {
        "Id": Id,
        "AppVersion": AppVersion,
        "Activity": Activity,
        "Private": Private,
        "AvailableSpace": AvailableSpace,
        "GameInProgress": GameInProgress,
        "PlayerIds": PlayerIds
    }
    return newSession
}
export async function GetAllGameSessions(ActiveServerSession, BuildVersion) {
    if (ActiveServerSession == null || BuildVersion == null) return;
    if (ActiveServerSession["Sessions"] == null) return;
    // retrieve game sessions
    var Sessions = []
    ActiveServerSession["Sessions"].forEach(element => {
        if (element != undefined) {
            if (element["AppVersion"] == BuildVersion) {
                Sessions.push(element)
            }
        }
    });
    // return sessions
    return Sessions
}