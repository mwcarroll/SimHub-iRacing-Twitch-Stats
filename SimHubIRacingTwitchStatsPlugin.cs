using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using GameReaderCommon;
using GongSolutions.Wpf.DragDrop.Utilities;
using IRacingReader;
using iRacingSDK;
using SimHub.iRacing.Twitch.Stats.Properties;
using SimHub.iRacing.Twitch.Stats.Settings;
using SimHub.Plugins;

namespace SimHub.iRacing.Twitch.Stats
{
    [PluginAuthor("mwcarroll")]
    [PluginDescription("iRacing Twitch Stats Plugin")]
    [PluginName("iRacing Twitch Stats")]
    // ReSharper disable once ClassNeverInstantiated.Global
    public class SimHubIRacingTwitchStatsPlugin : IDataPlugin, IWPFSettingsV2
    {
        public SimHubIRacingTwitchStatsPluginSettings Settings;

        /// <summary>
        /// Instance of the current plugin manager
        /// </summary>
        public PluginManager PluginManager { get; set; }

        /// <summary>
        /// Gets the left menu icon. Icon must be 24x24 and compatible with black and white display.
        /// </summary>
        public ImageSource PictureIcon => this.ToIcon(Resources.SH_iRacing_Twitch_Stats_MenuIcon);

        /// <summary>
        /// Gets a short plugin title to show in left menu. Return null if you want to use the title as defined in PluginName attribute.
        /// </summary>
        public string LeftMenuTitle => "iRacing Twitch Stats";

        private TwitchLiveMonitor _twitchLiveMonitor;

        private bool _twitchInitalized;
        private bool _iRacingInitalized;

        private int _playerCarIndex;

        private long _startingIRating;
        private long _startLicenseLevel;
        private long _startLicenseSubLevel;
        
        private long _currentIRating;
        private long _currentLicenseLevel;
        private long _currentLicenseSubLevel;
        
        private long _deltaIRating;
        private long _deltaLicenseLevel;
        private long _deltaLicenseSubLevel;
        private long _deltaSafetyRating;

        /// <summary>
        /// Called one time per game data update, contains all normalized game data,
        /// raw data are intentionally "hidden" under a generic object type (A plugin SHOULD NOT USE IT)
        ///
        /// This method is on the critical path, it must execute as fast as possible and avoid throwing any error
        ///
        /// </summary>
        /// <param name="pluginManager"></param>
        /// <param name="data">Current game data, including current and previous data frame.</param>
        public void DataUpdate(PluginManager pluginManager, ref GameData data)
        {
            if (_twitchLiveMonitor == null)
            {
                if (
                    !string.IsNullOrEmpty(Settings.TwitchChannelName.Trim()) &&
                    !string.IsNullOrEmpty(Settings.TwitchClientId.Trim()) &&
                    !string.IsNullOrEmpty(Settings.TwitchAccessToken.Trim())
                )
                {
                    _twitchLiveMonitor = new TwitchLiveMonitor(Settings.TwitchChannelName, Settings.TwitchClientId,
                        Settings.TwitchAccessToken);
                }
                else return;
            }
            
            if(!_twitchLiveMonitor.StreamLive) return;
            
            if (!_twitchInitalized && _twitchLiveMonitor.StreamLive)
            {
                _twitchInitalized = true;
            }
            else
            {
                _twitchInitalized = false;
                return;
            }
            
            if (!data.GameRunning || data.GameName.ToLower() != "iracing")
            {
                _iRacingInitalized = false;
                return;
            }

            if (!(data.NewData.GetRawDataObject() is DataSampleEx irData)) return;
            
            _playerCarIndex = TelemetryInt(irData, "PlayerCarIdx");
            SessionData._DriverInfo._Drivers myData = irData.SessionData.DriverInfo.CompetingDrivers[_playerCarIndex];
            
            // mark sensors as available
            if (!_iRacingInitalized)
            {
                _iRacingInitalized = true;
                
                _startingIRating = myData.IRating;
                _startLicenseLevel = myData.LicLevel;
                _startLicenseSubLevel = myData.LicSubLevel;

                _deltaIRating = 0;
                _deltaLicenseLevel = 0;
                _deltaLicenseSubLevel = 0;
                _deltaSafetyRating = 0;

                File.WriteAllText(@"%appdata%\SimHub-iRacing-Twitch-Stats\current-stream-irating-delta.txt", _deltaIRating.ToString());
                File.WriteAllText(@"%appdata%\SimHub-iRacing-Twitch-Stats\current-stream-safety-rating-delta.txt", _deltaSafetyRating.ToString());
            }

            bool iRatingChanged = false;
            bool licenseChanged = false;
            bool licenseSubChanged = false;

            if (myData.IRating != _currentIRating)
            {
                iRatingChanged = true;
                
                _currentIRating = myData.IRating;
                _deltaIRating = _startingIRating - _currentIRating;
            }

            if (myData.LicLevel != _startLicenseLevel)
            {
                licenseChanged = true;
                
                _currentLicenseLevel = myData.LicLevel;
                _deltaLicenseLevel = _startLicenseLevel - _currentLicenseLevel;
            }

            if (myData.LicSubLevel != _startLicenseSubLevel)
            {
                licenseSubChanged = true;
                
                _currentLicenseSubLevel = myData.LicSubLevel;
                _deltaLicenseSubLevel = _startLicenseSubLevel - _currentLicenseSubLevel;
            }
            
            // TODO: Figure out SR gain/loss calculation,
            // taking into account license promotions and demotions
            if (licenseChanged || licenseSubChanged)
            {
                // _deltaSafetyRating = ...some calculation
            }
            
            if(iRatingChanged) File.WriteAllText(@"%appdata%\SimHub-iRacing-Twitch-Stats\current-stream-irating-delta.txt", _deltaIRating.ToString());
            if(licenseChanged || licenseSubChanged) File.WriteAllText(@"%appdata%\SimHub-iRacing-Twitch-Stats\current-stream-safety-rating-delta.txt", _deltaSafetyRating.ToString());
        }

        /// <summary>
        /// Called at plugin manager stop, close/dispose anything needed here !
        /// Plugins are rebuilt at game change
        /// </summary>
        /// <param name="pluginManager"></param>
        public void End(PluginManager pluginManager)
        {
            // Save settings
            this.SaveCommonSettings("GeneralSettings", Settings);
        }

        private Control _wpfSettingsControl;

        /// <summary>
        /// Returns the settings control, return null if no settings control is required
        /// </summary>
        /// <param name="pluginManager"></param>
        /// <returns></returns>
        public Control GetWPFSettingsControl(PluginManager pluginManager)
        {
            return _wpfSettingsControl ?? (_wpfSettingsControl = new SimHubIRacingTwitchStatsPluginUi(this));
        }

        /// <summary>
        /// Called once after plugins startup
        /// Plugins are rebuilt at game change
        /// </summary>
        /// <param name="pluginManager"></param>
        public void Init(PluginManager pluginManager)
        {
            Logging.Current.Info("Starting SimHub.iRacing.Twitch.Stats");

            // Load settings
            Settings = this.ReadCommonSettings("GeneralSettings", () => new SimHubIRacingTwitchStatsPluginSettings());
        }
        
        private static int TelemetryInt(DataSampleEx irData, string name)
        {
            irData.Telemetry.TryGetValue(name, out var obj);
            return Convert.ToInt32(obj);
        }
        
    }
}