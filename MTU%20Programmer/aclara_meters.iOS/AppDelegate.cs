//using Microsoft.Intune.MAM;
using Acr.UserDialogs;
using Foundation;
using nexus.protocols.ble;
using System;
using System.Collections.Generic;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace aclara_meters.iOS
{

    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        private FormsApp appSave;
        private string identity = "";

        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and make window visible
        // NOTE: You have 17 seconds to return from this method or iOS will terminate application
        public override bool FinishedLaunching (
            UIApplication app,
            NSDictionary  options )
        {
            global::Xamarin.Forms.Forms.Init();
            //Distribute.DontCheckForUpdatesInDebug();

            /*
            IntuneMAMPolicyManager value = IntuneMAMPolicyManager.Instance;
            NSDictionary dictionary =  value.DiagnosticInformation;

            NSString[] keys = new NSString[]{new NSString("AppConfig")};

            NSDictionary key= dictionary.GetDictionaryOfValuesFromKeys(keys);

            var ftp_username = new NSObject();
            var ftp_pathremotefile = new NSObject();
            var cert_file = new NSObject();
            var ftp_password = new NSObject();
            var ftp_host = new NSObject();
        
            for (int i = 0, keyCount = (int)key.Count; i < keyCount; i++)
            {
                NSObject fields_values = key.ElementAt(i).Value;

                ftp_username = fields_values.ValueForKey(new NSString("ftp_username"));
                ftp_pathremotefile = fields_values.ValueForKey(new NSString("ftp_pathremotefile"));
                cert_file = fields_values.ValueForKey(new NSString("cert_file"));
                ftp_password = fields_values.ValueForKey(new NSString("ftp_password"));
                ftp_host = fields_values.ValueForKey(new NSString("ftp_host"));

                Console.WriteLine("ftp_username: {0}, ftp_pathremotefile: {1}, cert_file: {2}, ftp_password: {3}, ftp_host: {4}", 
                                  ftp_username, ftp_pathremotefile, cert_file, ftp_password, ftp_host );
            }
     
            List <string> listaDatos = new List<string>();

            try{
                listaDatos.Add("ftp_username: " + ftp_username );
            }catch (Exception c1){}

            try{
                listaDatos.Add("ftp_pathremotefile: " + ftp_pathremotefile);
            }catch (Exception c1){}

            try{
                listaDatos.Add("cert_file: " + cert_file);
            }catch (Exception c1) { }

            try{
                listaDatos.Add("ftp_password: " + ftp_password);
            }catch (Exception c1) { }

            try{
                listaDatos.Add("ftp_host: " + ftp_host);
            }catch (Exception c1) { }
            */

            List<string> listaDatos = new List<string>();

            var AppVersion = NSBundle.MainBundle.InfoDictionary[ "CFBundleVersion" ];

            IBluetoothLowEnergyAdapter bluetoothLowEnergyAdapter = BluetoothLowEnergyAdapter.ObtainDefaultAdapter();
            IUserDialogs userDialogs = UserDialogs.Instance;
            NSString appversion = (Foundation.NSString) AppVersion.Description;

            /*
            appSave = new FormsApp (
                bluetoothLowEnergyAdapter,
                userDialogs,
                listaDatos,
                appversion);
              */


            appSave = new FormsApp(
                bluetoothLowEnergyAdapter, listaDatos, userDialogs, appversion);



            //appSave = new FormsApp(bluetoothLowEnergyAdapter);

            base.LoadApplication ( appSave );

            return base.FinishedLaunching ( app, options );
        }

        public override bool OpenUrl (
            UIApplication app,
            NSUrl         url,
            NSDictionary  options )
        {
            appSave.HandleUrl ( ( Uri )url, BluetoothLowEnergyAdapter.ObtainDefaultAdapter() );
            return true;
        }
    }
}
