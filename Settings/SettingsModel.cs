using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SimHub.iRacing.Twitch.Stats.Settings
{
    public class SimHubIRacingTwitchStatsPluginUiModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        private string _twitchChannelName;

        public string TwitchChannelName
        {
            get => _twitchChannelName;
            set
            {
                _twitchChannelName = value;
                
                OnPropertyChanged();
            }
        }
        
        private string _twitchClientId;

        public string TwitchClientId
        {
            get => _twitchClientId;
            set
            {
                _twitchClientId = value;
                
                OnPropertyChanged();
            }
        }
        
        private string _twitchAccessToken;

        public string TwitchAccessToken
        {
            get => _twitchAccessToken;
            set
            {
                _twitchAccessToken = value;
                
                OnPropertyChanged();
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}