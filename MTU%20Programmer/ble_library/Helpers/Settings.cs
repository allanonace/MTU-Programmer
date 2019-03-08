/*
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using Xamarin.Forms;

namespace aclara_meters.Helpers
{
    /// <summary>
    /// This is the Settings static class that can be used in your Core solution or in any
    /// of your client applications. All settings are laid out the same exact way with getters
    /// and setters. 
    /// </summary>
   
    public static class Settings
    {
        private static ISettings AppSettings
        {
            get
            {
                return CrossSettings.Current;
            }
        }

        #region Setting Constants

        private const string SettingsKey = "settings_key";
        private static readonly string SettingsDefault = string.Empty;

        private const string IsLoggedInTokenKey = "isloggedid_key";
        private static readonly bool IsLoggedInTokenDefault = false;

        private const string UserName = "username";
        private static readonly string UserNameDefault = string.Empty;

        private const string deviceInfo = "deviceInfo";
        private static readonly string deviceInfoDefault = string.Empty;

        private const string IsConnectedToDevice = "IsConnectedToDevice";
        private static readonly bool IsConnectedToDeviceDefault = false;

        private const string ChangesDoneAddMTU = "ChangesDoneAddMTU";
        private static readonly bool ChangesDoneAddMTUDefault = false;

        #endregion


        public static string SavedUserName
        {
            get { return AppSettings.GetValueOrDefault(UserName, UserNameDefault); }
            set { AppSettings.AddOrUpdateValue(UserName, value); }
        }

        public static string SavedDeviceInfo
        {
            get { return AppSettings.GetValueOrDefault(deviceInfo, deviceInfoDefault); }
            set { AppSettings.AddOrUpdateValue(deviceInfo, value); }
        }

        public static string GeneralSettings
        {
            get
            {
                return AppSettings.GetValueOrDefault(SettingsKey, SettingsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(SettingsKey, value);
            }
        }

        public static bool IsLoggedIn
        {
            get { return AppSettings.GetValueOrDefault(IsLoggedInTokenKey, IsLoggedInTokenDefault); }
            set { AppSettings.AddOrUpdateValue(IsLoggedInTokenKey, value); }
        }

        public static bool IsConnectedBLE
        {
            get { return AppSettings.GetValueOrDefault(IsConnectedToDevice, IsConnectedToDeviceDefault); }
            set { AppSettings.AddOrUpdateValue(IsConnectedToDevice, value); }
        }


        public static bool IsChangesDoneAddMTU
        {
            get { return AppSettings.GetValueOrDefault(ChangesDoneAddMTU, ChangesDoneAddMTUDefault); }
            set { AppSettings.AddOrUpdateValue(ChangesDoneAddMTU, value); }
        }

    }
}
*/