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
export async function GetAllGameSessions(ServerSessions, BuildVersion) {
    if (ServerSessions == null) return "No Game Sessions?";
    // retrieve game sessions
    var Sessions = []
    ServerSessions.forEach(element => {
        if (element != undefined) {
           Sessions.push(element)
        }
    });
    // return sessions
    return Sessions
}