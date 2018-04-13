using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Schoggifabrik.Data
{
    public static class SessionHttpContexExtensions
    {
        private const string SessionKey = "SessionData";

        public static SessionData GetSessionData(this HttpContext context)
        {
            string value = context.Session.GetString(SessionKey);
            return value == null ? new SessionData() : JsonConvert.DeserializeObject<SessionData>(value);
        }

        public static void SetSessionData(this HttpContext context, SessionData data) =>
            context.Session.SetString(SessionKey, JsonConvert.SerializeObject(data));
    }
}
