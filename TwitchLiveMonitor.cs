using System.Collections.Generic;
using System.Threading.Tasks;
using TwitchLib.Api;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;

namespace SimHub.iRacing.Twitch.Stats
{
    public class TwitchLiveMonitor
    {
        private LiveStreamMonitorService _monitor;
        private TwitchAPI _api;
        
        private string _channelName;

        public bool StreamLive { get; private set; }

        public TwitchLiveMonitor(string channelName, string clientId, string accessToken)
        {
            Task.Run(() => ConfigLiveMonitorAsync(channelName, clientId, accessToken));
        }

        private async Task ConfigLiveMonitorAsync(string channelName, string clientId, string accessToken)
        {
            _channelName = channelName;
            
            _api = new TwitchAPI
            {
                Settings =
                {
                    ClientId = clientId,
                    AccessToken = accessToken
                }
            };

            _monitor = new LiveStreamMonitorService(_api, 1);

            _monitor.OnStreamOnline += Monitor_OnStreamOnline;
            _monitor.OnStreamOffline += Monitor_OnStreamOffline;

            List<string> lst = new List<string>{ _channelName };
            _monitor.SetChannelsByName(lst);      

            _monitor.Start(); //Keep at the end!

            await Task.Delay(-1);
        }

        private void Monitor_OnStreamOnline(object sender, OnStreamOnlineArgs e)
        {
            if (e.Channel.ToLower().Equals(_channelName.ToLower()))
            {
                StreamLive = true;
            }
        }
        private void Monitor_OnStreamOffline(object sender, OnStreamOfflineArgs e)
        {
            if (e.Channel.ToLower().Equals(_channelName.ToLower()))
            {
                StreamLive = false;
            }
        }
    }
}