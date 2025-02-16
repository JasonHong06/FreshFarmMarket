using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace FreshFarmMarket.Pages
{
    public class SessionController : ControllerBase
    {
        private static readonly ConcurrentDictionary<string, string> ActiveSessions = new();

        public static bool IsSessionActive(string userId, string sessionId)
        {
            return ActiveSessions.TryGetValue(userId, out var activeSession) && (activeSession ?? "") == sessionId;
        }


        public static void SetActiveSession(string userId, string sessionId)
        {
            ActiveSessions[userId] = sessionId;
        }

        public static void RemoveSession(string userId)
        {
            ActiveSessions.TryRemove(userId, out _);
        }
    }
}
