module.exports = {
    SessionEvent: async function(SessionId, Category, Action) {
        if (SessionId == null || Category == null || Action == null) return ""
        return "Received!"
    }
}