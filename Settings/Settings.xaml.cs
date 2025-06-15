using System.Windows;
using System.Windows.Controls;

namespace SimHub.iRacing.Twitch.Stats.Settings
{
    public partial class SimHubIRacingTwitchStatsPluginUi : UserControl
    {
        private SimHubIRacingTwitchStatsPluginUiModel Model { get; }

        private SimHubIRacingTwitchStatsPlugin SimHubIRacingTwitchStatsPlugin { get; }
        
        public SimHubIRacingTwitchStatsPluginUi(SimHubIRacingTwitchStatsPlugin simHubIRacingTwitchStatsPlugin)
        {
            InitializeComponent();
            SimHubIRacingTwitchStatsPlugin = simHubIRacingTwitchStatsPlugin;

            Model = new SimHubIRacingTwitchStatsPluginUiModel
            {
                TwitchChannelName = simHubIRacingTwitchStatsPlugin.Settings.TwitchChannelName,
                TwitchClientId = simHubIRacingTwitchStatsPlugin.Settings.TwitchClientId,
                TwitchAccessToken = simHubIRacingTwitchStatsPlugin.Settings.TwitchAccessToken
            };

            DataContext = Model;
        }
        
        private void Apply_Settings(object sender, RoutedEventArgs e)
        {
            SimHubIRacingTwitchStatsPlugin.Settings.TwitchChannelName = Model.TwitchChannelName;
            SimHubIRacingTwitchStatsPlugin.Settings.TwitchClientId = Model.TwitchClientId;
            SimHubIRacingTwitchStatsPlugin.Settings.TwitchAccessToken = Model.TwitchAccessToken;
        }
    }
}