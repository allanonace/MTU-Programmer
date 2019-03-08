using aclara_meters.Helpers;
using aclara_meters.view;
using System.Collections.Specialized;
using Acr.UserDialogs;
using ble_library;
using MTUComm;
using nexus.protocols.ble;
using Plugin.DeviceInfo;
using Plugin.Multilingual;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using nexus.protocols.ble.scan;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace aclara_meters
{
    public partial class FormsApp : Application
    {
        #region Initial FTP - Default Config data

        string host = "159.89.29.176";
        string username = "aclara";
        string password = "aclara1234";
        string pathRemoteFile = "/home/aclara";

        #endregion

        #region Constants

        private const bool   DEBUG_MODE_ON = false;

        private const string SO_ANDROID = "Android";
        private const string SO_IOS     = "iOS";
        private const string SO_UNKNOWN = "Unknown";
        private const string XML_EXT    = ".xml";
        public static bool ScriptingMode = false;

        private string[] filesToCheck =
        {
            "Alarm",
            "DemandConf",
            "Global",
            "Interface",
            "Meter",
            "Mtu",
            "Error"
        };

        #endregion

        #region Attributes

        public string appVersion_str;
        public string deviceId;
        
        public static ICredentialsService credentialsService { get; private set; }
        public static BleSerial ble_interface;
        public static Logger loggger;
        public static Configuration config;
        public static IBlePeripheral peripheral;

        private IBluetoothLowEnergyAdapter adapter;
        private List<string> listaDatos;
        private IUserDialogs dialogs;
        private string appVersion;

        #endregion

        #region Properties

        public static string AppName
        {
            get { return "Aclara MTU Programmer"; }
        }

        #endregion

        #region Initialization

        public FormsApp ()
        {
            InitializeComponent ();

           
        }

        public FormsApp(IBluetoothLowEnergyAdapter adapter, List<string> listaDatos, IUserDialogs dialogs, string appVersion)
        {
            InitializeComponent();

            this.adapter = adapter;
            this.listaDatos = listaDatos;
            this.dialogs = dialogs;
            this.appVersion = appVersion;

            if (Device.RuntimePlatform == Device.Android)
            {
                Task.Run(async () => { await PermisosLocationAsync(); });
                CallToInitApp(adapter, listaDatos, dialogs, appVersion);
            }
            else
                Task.Factory.StartNew(ThreadProcedure);

        }

 

        #region iPad & iPhone devices have a different behaviour when initializating the app, this sems to fix it

        private void ThreadProcedure()
        {
            CallToInitApp(adapter, listaDatos, dialogs, appVersion);
        }


        private void CallToInitApp(IBluetoothLowEnergyAdapter adapter, List<string> listaDatos, IUserDialogs dialogs, string appVersion)
        {
            appVersion_str = appVersion;

            deviceId = CrossDeviceInfo.Current.Id;

            // Profiles manager
            credentialsService = new CredentialsService();

            // Initializes Bluetooth
            ble_interface = new BleSerial(adapter);

            string data = string.Empty;

            if (listaDatos.Count != 0 ||
                 listaDatos != null)
                for (int i = 0; i < listaDatos.Count; i++)
                    data = data + listaDatos[i] + "\r\n";

            string base64CertificateString = "";

            try
            {
                base64CertificateString = listaDatos[2].Replace("cert_file: ", "");
                byte[] bytes = Convert.FromBase64String(base64CertificateString);
                X509Certificate2 x509certificate = new X509Certificate2(bytes);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }

            AppResources.Culture = CrossMultilingual.Current.DeviceCultureInfo;

            // Force to not download server XML files
            if ( DEBUG_MODE_ON )
                this.LoadXmlsAndCreateContainer ( dialogs, data );
            else
            {
                // Downloads, if necesary, and loads configuration from XML files
                if ( this.HasDeviceAllXmls () )
                     this.LoadXmlsAndCreateContainer ( dialogs, data );
                else this.DownloadXmlsIfNecessary ( dialogs, data );
            }
        }

        #endregion


        #endregion
        private async Task PermisosLocationAsync()
        {
            try
            {
                var statusLocation = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location);
                var statusStorage = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);

                if (statusLocation != PermissionStatus.Granted)
                {
                    await CrossPermissions.Current.RequestPermissionsAsync(Permission.Location);
                }

                if (statusStorage != PermissionStatus.Granted)
                {
                    await CrossPermissions.Current.RequestPermissionsAsync(Permission.Storage);
                }
            }
            catch
            {
                this.MainPage = new NavigationPage(new ErrorInitView("There is a problem with permissions"));
            }

        }

        #region Configuration XMLs

        private bool HasDeviceAllXmls ()
        {
            string path = Mobile.GetPath ();

            // Directory could exist but is empty
            if ( string.IsNullOrEmpty ( path ) )
                return false;

            // Directory exists and is not empty
            string[] filesLocal = Directory.GetFiles ( path );

            //if ( ! filesLocal.Any () )
            //    return false;
            
            int count = 0;

            foreach ( string fileNeeded in filesToCheck )
            {
                foreach ( string filePath in filesLocal )
                {
                    string compareStr = fileNeeded + XML_EXT;
                    compareStr = compareStr.Replace ( path, "" ).Replace("/", "");

                    string fileStr = filePath.ToString ();
                    fileStr = fileStr.Replace ( path, "" ).Replace("/","");

                    if ( fileStr.Equals ( compareStr ) )
                    {
                        count++;
                        break;
                    }

                }
            }

            if(count == filesToCheck.Length)
                return true;

            return false;

            /*

            foreach ( string filePath in filesLocal )
            {
                foreach ( string fileNeeded in filesToCheck )
                {
                    string compareStr = fileNeeded + XML_EXT;
                    compareStr = compareStr.Replace ( path, "" );

                    string fileStr = filePath.ToString ();
                    fileStr = fileStr.Replace ( path, "" );
                    
                    if ( fileStr.Equals ( compareStr ) &&
                         ++count >= filesToCheck.Length )
                        return true;
                }
            }

            return false;

            */
        }

        private void DownloadXmlsIfNecessary (
            IUserDialogs dialogs,
            string data )
        {
            // Checks network channels
            if (Mobile.IsNetAvailable())
            {
                // Donwloads all configuracion XML files
                if (this.DownloadXmls())
                {
                    this.LoadXmlsAndCreateContainer(dialogs, data);
                }
                else
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        this.MainPage = new NavigationPage(new ErrorInitView("Error Downloading files"));
                    });
                }
            }
            else 
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    this.MainPage = new NavigationPage(new ErrorInitView());
                });

            }
        }

        private bool DownloadXmls ()
        {
            try
            {
                using (SftpClient sftp = new SftpClient(host, 22, username, password))
                {
                    try
                    {

                        sftp.Connect();


                        /*--------------------------------------------------*/
                        // List all posible files in the documents directory 
                        // Check if file's lastwritetime is the lastest 
                        /*--------------------------------------------------*/
                        List<SftpFile> ftp_array_files = new List<SftpFile>();

                        // Remote FTP File directory
                        var ftp_files = sftp.ListDirectory(pathRemoteFile);
                        foreach (var file in ftp_files)
                        {

                            if (file.Name.Contains(".xml"))
                            {
                                ftp_array_files.Add(file);
                            }

                        }

                        string path = Mobile.GetPath ();

                        foreach ( var file in ftp_array_files )
                        {
                            string remoteFileName = file.Name;

                            using (Stream file1 = File.OpenWrite(Path.Combine( path, remoteFileName)))
                            {
                                sftp.DownloadFile(Path.Combine(pathRemoteFile, remoteFileName), file1);
                            }
                        }

                        sftp.Disconnect ();

                        return true;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("An exception has been caught " + e.ToString());
                    }

                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An exception has been caught " + e.ToString());
            }

            return false;
        }

        private void LoadXmlsAndCreateContainer ( IUserDialogs dialogs, string data )
        {
            // Load configuration from XML files
            this.LoadXmls ();

            #region Scripting Mode Detection 

            //Task.Run(async () =>
            //{
                //await Task.Delay(1100); Xamarin.Forms.Device.BeginInvokeOnMainThread(async () =>
                //{

                    #region Min Date Check

                    try
                    {
                        string datenow = DateTime.Now.ToString("MM/dd/yyyy");
                        string mindate = FormsApp.config.global.MinDate;

                  

                        if (DateTime.ParseExact(datenow, "MM/dd/yyyy", null) < DateTime.ParseExact(mindate, "MM/dd/yyyy", null))
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                MainPage = new NavigationPage(new ErrorInitView("System is not ahead or equals to Minimum expected Date!"));
                            });
                        }


                    }
                    catch (Exception)
                    {

                    }

                    #endregion

                    // Load pages container ( ContentPage )
                    if ( ! ScriptingMode )
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            MainPage = new NavigationPage(new AclaraViewLogin(dialogs, data));
                        });
            //    });
            //});

            #endregion
        }

        private void LoadXmls ()
        {
            config  = Configuration.GetInstance ();
            loggger = new Logger ( config );

            switch ( Device.RuntimePlatform )
            {
                case Device.Android:
                    config.setPlatform   ( SO_ANDROID );
                    config.setAppName    ( AppName    );
                    config.setVersion    ( appVersion_str);
                    config.setDeviceUUID ( deviceId   );
                    break;
                case Device.iOS:
                    config.setPlatform   ( SO_IOS     );
                    config.setAppName    ( AppName    );
                    config.setVersion    ( appVersion_str);
                    config.setDeviceUUID ( deviceId   );
                    break;
                default:
                    config.setPlatform   ( SO_UNKNOWN );
                    break;
            }

            Configuration.SetInstance ( config );
        }

        #endregion

        #region Base64

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        #endregion



        public void HandleUrl ( Uri url , IBluetoothLowEnergyAdapter adapter)
        {
            try
            {
                ScriptingMode = true; 
                ble_interface.Close();

                #region WE HAVE TO DISABLE THE BLUETOOTH ANTENNA, IN ORDER TO DISCONNECT FROM PREVIOUS CONNECTION, IF WE WENT FROM INTERACTIVE TO SCRIPTING MODE

                adapter.DisableAdapter();
                adapter.EnableAdapter(); //Android shows a window to allow bluetooth

                #endregion

            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }

            if ( url != null )
            {
                string path = Mobile.GetPath ();
                NameValueCollection query = HttpUtility.ParseQueryString ( url.Query );

                var script_name = query.Get ( "script_name" );
                var script_data = query.Get ( "script_data" );
                var callback    = query.Get ( "callback"    );

                if ( script_name != null )
                    path = Path.Combine ( path, script_name.ToString () );

                if ( script_data != null )
                    File.WriteAllText ( path, Base64Decode ( script_data ) );

                if ( callback != null ) { /* ... */ }

            
                Task.Run(async () =>
                {
                    await Task.Delay(1000); Xamarin.Forms.Device.BeginInvokeOnMainThread ( async () =>
                    {
                        //Settings.IsLoggedIn = false;
                        //credentialsService.DeleteCredentials ();

                        MainPage = new NavigationPage(new AclaraViewScripting ( path, callback, script_name ) );
                        await MainPage.Navigation.PopToRootAsync ( true );
                    });
                });
            }
        }

        #region OnEvent

        protected override void OnStart()
        {
            // https://appcenter.ms/users/ma.jimenez/apps/Aclara-MTU-Testing-App
            //AppCenter.Start("ios=cb622ad5-e2ad-469d-b1cd-7461f140b2dc;" + "android=53abfbd5-4a3f-4eb2-9dea-c9f7810394be", typeof(Analytics), typeof(Crashes), typeof(Distribute) );
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        #endregion


 
    }
}
