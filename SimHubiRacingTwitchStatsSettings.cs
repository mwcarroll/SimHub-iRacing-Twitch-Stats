using System;

namespace SimHub.iRacing.Twitch.Stats
{
    public class SimHubIRacingTwitchStatsPluginSettings
    {
        public string Server { get; set; } = "localhost";

        public int Port { get; set; } = 1883;

        public string Login { get; set; } = "admin";

        public string Password { get; set; } = "admin";

        public string LastError { get { return _lastError ?? string.Empty; } set { _lastError = value; } }

        private string _lastError = string.Empty;
    }

    public class SimHubIRacingTwitchStatsPluginUserSettings
    {
        public Guid UserId { get; set; } = Guid.NewGuid();
    }
}