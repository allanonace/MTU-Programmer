// Copyright M. Griffie <nexus@nexussays.com>
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Acr.UserDialogs;
using aclara_meters.Helpers;
using aclara_meters.Models;
using Xamarin.Forms;
using System.Threading;
using nexus.protocols.ble.scan;
using System.Collections.ObjectModel;
using Plugin.Settings;
using System.Linq;
using MTUComm;

using ActionType = MTUComm.Action.ActionType;

namespace aclara_meters.view
{
    public partial class AclaraViewMainMenu
    {
        private bool autoConnect;
        private bool conectarDevice;

        private ActionType actionType;

        public DeviceItem last_item;

        private List<PageItem> MenuList { get; set; }
        private IUserDialogs dialogsSaved;
        private ObservableCollection<DeviceItem> employees;

        private int peripheralConnected = ble_library.BlePort.NO_CONNECTED;
        private Boolean peripheralManualDisconnection = false;
        private Thread printer;
        private int TimeOutSeconds = 5;


        private Command refresh_command;

        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        private bool GetDebugVar()
        {
            return false;
        }
        public AclaraViewMainMenu()
        {
            InitializeComponent();
        }


        public AclaraViewMainMenu(IUserDialogs dialogs)
        {
            InitializeComponent();
            PrintToConsole($"-------------------------------   AclaraViewMainMenu     , thread: { Thread.CurrentThread.ManagedThreadId}");
            Settings.IsConnectedBLE = false;
            NavigationPage.SetHasNavigationBar(this, false); //Turn off the Navigation bar
            TappedListeners();
            LoadPreUIGFX();

            if (Device.Idiom == TargetIdiom.Tablet)
            {
                LoadTabletUI();
            }
            else
            {
                LoadPhoneUI();
            }

            dialogsSaved = dialogs;
            LoadPostUIGFX();

            //Change username textview to Prefs. String
            if (FormsApp.credentialsService.UserName != null)
            {
                userName.Text = FormsApp.credentialsService.UserName; //"Kartik";
                CrossSettings.Current.AddOrUpdateValue("session_username", FormsApp.credentialsService.UserName);
            }

            LoadSideMenuElements();

            if (Device.Idiom == TargetIdiom.Phone)
            {
                background_scan_page.Opacity = 0;
                background_scan_page.FadeTo(1, 250);
            }

            if (Device.RuntimePlatform == Device.Android)
            {
                backmenu.Scale = 1.42;

            }

            InitRefreshCommand();

            #region New Scripting method is called

            //Device.BeginInvokeOnMainThread(() =>
            //{
            //    PrintToConsole("Se va a empezar el flujo");

            //    PrintToConsole("Se va a lanzar una Tarea. Task.Factory.StartNew(Init_Scripting_Method)");

            //    Task.Factory.StartNew(Interface_background_scan_page);

            //});

            //Task.Factory.StartNew(Interface_background_scan_page);

             Interface_background_scan_page();

            #endregion

            //BluetoothPeripheralDisconnect ( null, null );
        }

        private void InitRefreshCommand()
        {
            refresh_command = new Command(async () =>
            {
                PrintToConsole($"----------------------REFRESH command dispositivos encontrados : {FormsApp.ble_interface.GetBlePeripheralList().Count}");
                PrintToConsole($"-------------------------------        REFRESH command, thread: { Thread.CurrentThread.ManagedThreadId}");

                if (!GetAutoConnectStatus())
                {
                   
                    Esperando();
                   
                    if (printer.ThreadState == ThreadState.Suspended)
                    {
                        try
                        {
                            Console.WriteLine("---------------  printer resume");
                            //printer.Interrupt();
                            printer.Resume();
                        }
                        catch (Exception e11)
                        {
                            Console.WriteLine(e11.StackTrace);
                        }
                    }
                    //DeviceList.IsRefreshing = true;
                    employees = new ObservableCollection<DeviceItem>();

                    FormsApp.ble_interface.SetTimeOutSeconds(TimeOutSeconds);
                    await FormsApp.ble_interface.Scan();
                    TimeOutSeconds = 3; // los siguientes escaneos son de 5 sec

                    if (FormsApp.ble_interface.GetBlePeripheralList().Count>0)
                    {

                        //await ChangeListViewData();
                        ChangeListViewData();

                        //DeviceList.IsRefreshing = false;
                        if (employees.Count != 0)
                        {
                            DeviceList.ItemsSource = employees;
                        }
                        if (conectarDevice)
                        {
                            PairWithKnowDevice();
                        }
                    }
                    else
                    {
                        DeviceList.ItemsSource = null;
                        Application.Current.MainPage.DisplayAlert("Alert", "No device found, please, press the button to turn on the device and refresh", "Ok");
                        Terminado();
                    }

                }
            });

        }

        private void PairWithKnowDevice()
        {
           
            autoConnect = false;
            conectarDevice = false;
            #region Autoconnect to stored device 

            PrintToConsole($"-----------------------------------va a conectar con : {FormsApp.peripheral.Advertisement.DeviceName}");
            //Task.Factory.StartNew(NewOpenConnectionWithDevice);
            NewOpenConnectionWithDevice();
            #endregion

        }
        private void Esperando()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                #region New Circular Progress bar Animations    
                DeviceList.IsRefreshing = false;
                backdark_bg.IsVisible = true;
                indicator.IsVisible = true;
                background_scan_page.IsEnabled = true;
                #endregion
            });
        }

        private void Terminado()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                #region Disable Circular Progress bar Animations when done

                backdark_bg.IsVisible = false;
                indicator.IsVisible = false;
                background_scan_page.IsEnabled = true;

                //DeviceList.IsEnabled = true;
                //fondo.Opacity = 1;
                //background_scan_page.Opacity = 1;
                //background_scan_page.IsEnabled = true;
                #endregion
            });
        }
        /*--------------------------------------------------*/
        /*          Device List Interface Contenview
        /---------------------------------------------------*/

        private bool GetAutoConnectStatus()
        {
            return autoConnect;
        }

       
        private void Interface_background_scan_page()
        {
            PrintToConsole($"-------------------------------    Interface_background_scan_page, thread: { Thread.CurrentThread.ManagedThreadId}");

            printer = new Thread(new ThreadStart(InvokeMethod));

            printer.Start();
            //employees = new ObservableCollection<DeviceItem>();

            DeviceList.RefreshCommand = refresh_command;

            FirstRefreshSearchPucs();
               
        }



        public void FirstRefreshSearchPucs()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                refresh_command.Execute(true);
            });
        }

        private void LoadSideMenuElements()
        {
            // Creating our pages for menu navigation
            // Here you can define title for item, 
            // icon on the left side, and page that you want to open after selection

            MenuList = new List<PageItem>();

            // Adding menu items to MenuList

            MenuList.Add(new PageItem() { Title = "Read MTU", Icon = "readmtu_icon.png", TargetType = ActionType.ReadMtu }); 

            if (FormsApp.config.global.ShowTurnOff)
                MenuList.Add(new PageItem() { Title = "Turn Off MTU", Icon = "turnoff_icon.png", TargetType = ActionType.TurnOffMtu });

            if (FormsApp.config.global.ShowAddMTU)
                MenuList.Add(new PageItem() { Title = "Add MTU", Icon = "addMTU.png", TargetType = ActionType.AddMtu });

            if (FormsApp.config.global.ShowReplaceMTU)
                MenuList.Add(new PageItem() { Title = "Replace MTU", Icon = "replaceMTU2.png", TargetType = ActionType.ReplaceMTU });

            if (FormsApp.config.global.ShowReplaceMeter)
                MenuList.Add(new PageItem() { Title = "Replace Meter", Icon = "replaceMeter.png", TargetType = ActionType.ReplaceMeter });

            if (FormsApp.config.global.ShowAddMTUMeter)
                MenuList.Add(new PageItem() { Title = "Add MTU / Add Meter", Icon = "addMTUaddmeter.png", TargetType = ActionType.AddMtuAddMeter });

            if (FormsApp.config.global.ShowAddMTUReplaceMeter)
                MenuList.Add(new PageItem() { Title = "Add MTU / Rep. Meter", Icon = "addMTUrepmeter.png", TargetType = ActionType.AddMtuReplaceMeter });

            if (FormsApp.config.global.ShowReplaceMTUMeter)
                MenuList.Add(new PageItem() { Title = "Rep.MTU / Rep. Meter", Icon = "repMTUrepmeter.png", TargetType = ActionType.ReplaceMtuReplaceMeter });

            if (FormsApp.config.global.ShowInstallConfirmation)
                MenuList.Add(new PageItem() { Title = "Install Confirmation", Icon = "installConfirm.png", TargetType = ActionType.MtuInstallationConfirmation });


            // ListView needs to be at least  elements for UI Purposes, even empty ones
            while (MenuList.Count < 9)
                MenuList.Add(new PageItem() { Title = "", Icon = "" });

            // Setting our list to be ItemSource for ListView in MainPage.xaml
            navigationDrawerList.ItemsSource = MenuList;

        }

        private void OnSwiped(object sender, SwipedEventArgs e)
        {
            if (Device.Idiom == TargetIdiom.Tablet)
                return;

            switch (e.Direction)
            {
                case SwipeDirection.Left:


                    fondo.Opacity = 1;
                    ContentNav.TranslateTo(-310, 0, 175, Easing.SinOut);
                    shadoweffect.TranslateTo(-310, 0, 175, Easing.SinOut);
                    background_scan_page.Opacity = 1;
                    background_scan_page_detail.Opacity = 1;

                    Task.Delay(200).ContinueWith(t =>
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        ContentNav.Opacity = 0;
                        shadoweffect.IsVisible = false;
                        ContentNav.IsVisible = false;
                      //  background_scan_page.IsEnabled = true;
                      //  background_scan_page_detail.IsEnabled = true;
                    }));

                    break;
                case SwipeDirection.Right:
                    fondo.Opacity = 0;
                    ContentNav.IsVisible = true;
                    shadoweffect.IsVisible = true;
                    background_scan_page.Opacity = 0.5;
                    background_scan_page_detail.Opacity = 0.5;
                    ContentNav.Opacity = 1;
                    ContentNav.TranslateTo(0, 0, 175, Easing.SinIn);
                    shadoweffect.TranslateTo(0, 0, 175, Easing.SinIn);
                   //background_scan_page.IsEnabled = true;

                   // background_scan_page_detail.IsEnabled = true;
                    break;

            }
        }

        private void LoadPreUIGFX()
        {
            shadoweffect.IsVisible = false;
            background_scan_page_detail.IsVisible = true;
            background_scan_page_detail.IsVisible = false;
        }

        private void LoadPostUIGFX()
        {
            background_scan_page_detail.IsVisible = true;
            background_scan_page_detail.IsVisible = false;
            background_scan_page.IsVisible = true;
            navigationDrawerList.IsEnabled = true;
            navigationDrawerList.Opacity = 0.65;

            if (Device.Idiom == TargetIdiom.Tablet)
            {
                ContentNav.Opacity = 1;
                ContentNav.IsVisible = true;
            }
            else
            {
                ContentNav.Opacity = 0;
                ContentNav.IsVisible = false;
            }



            background_scan_page.Opacity = 1;
            background_scan_page_detail.Opacity = 1;
        }

        private void LoadPhoneUI()
        {
            background_scan_page.Margin = new Thickness(0, 0, 0, 0);
            background_scan_page_detail.Margin = new Thickness(0, 0, 0, 0);
            close_menu_icon.Opacity = 1;
            hamburger_icon.IsVisible = true;
            hamburger_icon_detail.IsVisible = true;
            aclara_detail_logo.IsVisible = true;
            aclara_logo.IsVisible = true;
            tablet_user_view.TranslationY = 0;
            tablet_user_view.Scale = 1;
            aclara_logo.IsVisible = true;
            logo_tablet_aclara.Opacity = 0;
            aclara_detail_logo.IsVisible = true;
            tablet_user_view.TranslationY = -22;
            tablet_user_view.Scale = 1.2;
            ContentNav.TranslationX = -310;
            shadoweffect.TranslationX = -310;
            ContentNav.IsVisible = true;
            shadoweffect.IsVisible = true;
            ContentNav.IsVisible = false;
            shadoweffect.IsVisible = false;
        }

        private void LoadTabletUI()
        {
            ContentNav.IsVisible = true;
            background_scan_page.Opacity = 1;
            background_scan_page_detail.Opacity = 1;
            close_menu_icon.Opacity = 0;
            hamburger_icon.IsVisible = false;
            hamburger_icon_detail.IsVisible = false;
            background_scan_page.Margin = new Thickness(310, 0, 0, 0);
            background_scan_page_detail.Margin = new Thickness(310, 0, 0, 0);
            aclara_logo.IsVisible = true;
            logo_tablet_aclara.Opacity = 0;
            aclara_detail_logo.IsVisible = true;
            tablet_user_view.TranslationY = -22;
            tablet_user_view.Scale = 1.2;
            shadoweffect.IsVisible = true;
            aclara_logo.Scale = 1.2;
            aclara_detail_logo.Scale = 1.2;
            aclara_detail_logo.TranslationX = 42;
            aclara_logo.TranslationX = 42;

            shadoweffect.Source = "shadow_effect_tablet";

        }

        private void TappedListeners()
        {
            turnoffmtu_ok.Tapped += TurnOffMTUOkTapped;
            turnoffmtu_no.Tapped += TurnOffMTUNoTapped;
            turnoffmtu_ok_close.Tapped += TurnOffMTUCloseTapped;
            replacemeter_ok.Tapped += ReplaceMtuOkTapped;
            replacemeter_cancel.Tapped += ReplaceMtuCancelTapped;
            meter_ok.Tapped += MeterOkTapped;
            meter_cancel.Tapped += MeterCancelTapped;

            dialog_AddMTUAddMeter_ok.Tapped += dialog_AddMTUAddMeter_okTapped;
            dialog_AddMTUAddMeter_cancel.Tapped += dialog_AddMTUAddMeter_cancelTapped;

            dialog_AddMTUReplaceMeter_ok.Tapped += dialog_AddMTUReplaceMeter_okTapped;
            dialog_AddMTUReplaceMeter_cancel.Tapped += dialog_AddMTUReplaceMeter_cancelTapped;

            dialog_ReplaceMTUReplaceMeter_ok.Tapped += dialog_ReplaceMTUReplaceMeter_okTapped;
            dialog_ReplaceMTUReplaceMeter_cancel.Tapped += dialog_ReplaceMTUReplaceMeter_cancelTapped;


            dialog_AddMTU_ok.Tapped += dialog_AddMTU_okTapped;
            dialog_AddMTU_cancel.Tapped += dialog_AddMTU_cancelTapped;



            disconnectDevice.Tapped += BluetoothPeripheralDisconnect;
            back_button.Tapped += SideMenuOpen;
            back_button_menu.Tapped += SideMenuClose;
            logout_button.Tapped += LogoutTapped;
            back_button_detail.Tapped += SideMenuOpen;
            settings_button.Tapped += OpenSettingsTapped;

            logoff_no.Tapped += LogOffNoTapped;
            logoff_ok.Tapped += LogOffOkTapped;


            if (Device.Idiom == TargetIdiom.Tablet)
            {
                hamburger_icon_home.IsVisible = true;
                hamburger_icon_home_detail.IsVisible = true;

                hamburger_icon_home.Opacity = 0;
                hamburger_icon_home_detail.Opacity = 0;
            }

            refresh_signal.Tapped += refreshBleData;



        }

        private void refreshBleData(object sender, EventArgs e)
        {
            DeviceList.RefreshCommand.Execute(true);
        }


        /***
         * 
         *  //Dynamic battery detection when connected
         * 
         * 
            try
            {
                battery = FormsApp.ble_interface.GetBatteryLevel();

                if(battery != null)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        
                        if(battery[0] < 101 && battery[0] > 1 )
                        {
                            batteryLevel.Text = battery[0].ToString() + " %";

                            if (battery[0] > 75)
                            {

                                imageBattery.Source = "battery_toolbar_high";
                                battery_level.Source = "battery_toolbar_high_white";
                                battery_level_detail.Source = "battery_toolbar_high_white";
                            }

                            if (battery[0] > 45 && battery[0] < 75)
                            {

                                imageBattery.Source = "battery_toolbar_mid";
                                battery_level.Source = "battery_toolbar_mid_white";
                                battery_level_detail.Source = "battery_toolbar_mid_white";
                            }

                            if (battery[0] > 15 && battery[0] < 45)
                            {

                                imageBattery.Source = "battery_toolbar_low";
                                battery_level.Source = "battery_toolbar_low_white";
                                battery_level_detail.Source = "battery_toolbar_low_white";
                            }

                            if (battery[0] < 15)
                            {

                                imageBattery.Source = "battery_toolbar_empty";
                                battery_level.Source = "battery_toolbar_empty_white";
                                battery_level_detail.Source = "battery_toolbar_empty_white";
                            }

                        }
                    });
                }
            }catch (Exception e5){
                
            }
         *
         ***/


        private void InvokeMethod()
        {
            //PrintToConsole("dentro del metodo - InvokeMethod");

            int timeout_connecting = 0;

            //PrintToConsole("se va a ejecutar un bucle (WHILE TRUE) - InvokeMethod");

            while (true)
            {

                PrintToConsole($"---------------------------------Invoke method while ----dispositivos encontados : {FormsApp.ble_interface.GetBlePeripheralList().Count}");
                PrintToConsole($"---------------------------------Invoke method while ---- Thread: {Thread.CurrentThread.ManagedThreadId}");
              
                int status = FormsApp.ble_interface.GetConnectionStatus();

                PrintToConsole("se obtiene el estado de la conexion - InvokeMethod");

                if (status != peripheralConnected)
                {

                    PrintToConsole($"---------------------------------Invoke method ----estado : {status} , Perifericoconnected: {peripheralConnected}");
                    PrintToConsole($"---------------------------------Invoke method ---- Thread: {Thread.CurrentThread.ManagedThreadId}");
                    
                    //PrintToConsole("¿ES NO_CONNECTED? - InvokeMethod");

                    if (peripheralConnected == ble_library.BlePort.NO_CONNECTED)
                    {
                        PrintToConsole("    NO_CONNECTED - InvokeMethod");
                        peripheralConnected = status;
                        timeout_connecting = 0;
                    }
                    else if (peripheralConnected == ble_library.BlePort.CONNECTING)
                    {
                        PrintToConsole("Nop, es CONNECTING - InvokeMethod");

                        if (status == ble_library.BlePort.NO_CONNECTED)
                        {
                            PrintToConsole("Se va a ejecutar algo en la UI - InvokeMethod");

                            Device.BeginInvokeOnMainThread(() =>
                            {
                                PrintToConsole("Se va a detectar el estado de la conexion - InvokeMethod");

                                switch (FormsApp.ble_interface.GetConnectionError())
                                {
                                    case ble_library.BlePort.NO_ERROR:
                                        PrintToConsole("Estado conexion: NO_ERROR - InvokeMethod");
                                        break;
                                    case ble_library.BlePort.CONECTION_ERRROR:
                                        PrintToConsole("Estado conexion: CONECTION_ERRROR - InvokeMethod");

                                        Device.BeginInvokeOnMainThread(() =>
                                        {
                                            #region New Circular Progress bar Animations    

                                            DeviceList.IsRefreshing = false;
                                            backdark_bg.IsVisible = false;
                                            indicator.IsVisible = false;
                                            background_scan_page.IsEnabled = true;

                                            #endregion
                                        });

                                        PrintToConsole("Desactivar barra de progreso - InvokeMethod");

                                        Application.Current.MainPage.DisplayAlert("Alert", "Connection error. Please, retry", "Ok");
                                        break;
                                    case ble_library.BlePort.DYNAMIC_KEY_ERROR:
                                        PrintToConsole("Estado conexion: DYNAMIC_KEY_ERROR - InvokeMethod");

                                        Device.BeginInvokeOnMainThread(() =>
                                        {
                                            #region New Circular Progress bar Animations    

                                            DeviceList.IsRefreshing = false;
                                            backdark_bg.IsVisible = false;
                                            indicator.IsVisible = false;
                                            background_scan_page.IsEnabled = true;

                                            #endregion
                                        });

                                        PrintToConsole("Desactivar barra de progreso - InvokeMethod");
                                        Application.Current.MainPage.DisplayAlert("Alert", "Please, press the button to change PAIRING mode", "Ok");
                                        break;
                                    case ble_library.BlePort.NO_DYNAMIC_KEY_ERROR:
                                        PrintToConsole("Estado conexion: NO_DYNAMIC_KEY_ERROR - InvokeMethod");

                                        Device.BeginInvokeOnMainThread(() =>
                                        {
                                            #region New Circular Progress bar Animations    

                                            DeviceList.IsRefreshing = false;
                                            backdark_bg.IsVisible = false;
                                            indicator.IsVisible = false;
                                            background_scan_page.IsEnabled = true;

                                            #endregion

                                        });
                                        PrintToConsole("Desactivar barra de progreso - InvokeMethod");
                                        Application.Current.MainPage.DisplayAlert("Alert", "Please, press the button to change PAIRING mode", "Ok");
                                        break;
                                }
                                DeviceList.IsEnabled = true;
                                fondo.Opacity = 1;
                                background_scan_page.Opacity = 1;
                                background_scan_page.IsEnabled = true;

                            });
                            peripheralConnected = status;
                            FormsApp.peripheral = null;
                        }
                        else // status == ble_library.BlePort.CONNECTED
                        {
                            PrintToConsole("Estas Conectado - InvokeMethod");

                            DeviceList.IsEnabled = true;
                           
                            peripheralConnected = status;
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                fondo.Opacity = 1;
                                background_scan_page.Opacity = 1;
                                background_scan_page.IsEnabled = true;

                                IsConnectedUIChange(true);
                            });
                        }
                    }
                    else if (peripheralConnected == ble_library.BlePort.CONNECTED)
                    {
                        PrintToConsole("Nop, es CONNECTED - InvokeMethod");

                        DeviceList.IsEnabled = true;
                       
                        peripheralConnected = status;
                        FormsApp.peripheral = null;
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            fondo.Opacity = 1;
                            background_scan_page.Opacity = 1;
                            background_scan_page.IsEnabled = true;
                            //desconectar disp
                            IsConnectedUIChange(false);
                        });
                    }
                }

                PrintToConsole("¿Está en CONNECTING? - InvokeMethod");
                if (peripheralConnected == ble_library.BlePort.CONNECTING)
                {
                    PrintToConsole("Si, es CONNECTING - InvokeMethod");
                    timeout_connecting++;
                    if (timeout_connecting >= 2 * 12) // 10 seconds
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            PrintToConsole("Un Timeout que te llevas - InvokeMethod");
                            Application.Current.MainPage.DisplayAlert("Timeout", "Connection Timeout", "Ok");

                            employees = new ObservableCollection<DeviceItem>();
                            DeviceList.ItemsSource = employees;

                            DeviceList.IsEnabled = true;
                            fondo.Opacity = 1;
                            background_scan_page.Opacity = 1;
                            background_scan_page.IsEnabled = true;

                            autoConnect = false;

                            Device.BeginInvokeOnMainThread(() =>
                            {

                                #region Disable Circular Progress bar Animations when done

                                backdark_bg.IsVisible = false;
                                indicator.IsVisible = false;
                                background_scan_page.IsEnabled = true;

                                #endregion

                            });

                            try
                            {
                                Console.WriteLine("---------------  printer suspend");
                                printer.Suspend();
                            }
                            catch (Exception e5)
                            {
                                Console.WriteLine($"------ {e5.StackTrace}, {e5.Message}");
                            }


                        });
                        peripheralConnected = ble_library.BlePort.NO_CONNECTED;
                        timeout_connecting = 0;

                        PrintToConsole("Cerrar Conexion - InvokeMethod");

                        FormsApp.ble_interface.Close();
                    }
                }
                else
                {
                    PrintToConsole("Nop, no es CONNECTING - InvokeMethod");
                }

                PrintToConsole("Esperamos 300 ms - InvokeMethod");
                Thread.Sleep(300); // 0.5 Second

                PrintToConsole("¿Se va a realizar reconexion? - InvokeMethod");

            }

        }

        private void IsConnectedUIChange(bool v)
        {
            PrintToConsole($"---------------------------------IsConnectedUIChange param: {v} ---- Thread: {Thread.CurrentThread.ManagedThreadId}");
            if (v)
            {
                try
                {
                    // TODO: la siguente linea siempre da error xq peripheral es null
                    deviceID.Text = FormsApp.peripheral.Advertisement.DeviceName;
                    macAddress.Text = DecodeId(FormsApp.peripheral.Advertisement.ManufacturerSpecificData.ElementAt(0).Data.Take(4).ToArray());

                    //imageBattery.Source = "battery_toolbar_high";
                    // imageRssi.Source = "rssi_toolbar_high";
                    // batteryLevel.Text = "100%";
                    // rssiLevel.Text = peripheral.Rssi.ToString() + " dBm";

                    byte[] battery_ui = FormsApp.peripheral.Advertisement.ManufacturerSpecificData.ElementAt(0).Data.Skip(4).Take(1).ToArray();

                    if (battery_ui[0] < 101 && battery_ui[0] > 1)
                    {
                        batteryLevel.Text = battery_ui[0].ToString() + " %";

                        if (battery_ui[0] >= 75)
                        {
                            imageBattery.Source = "battery_toolbar_high";
                           
                            battery_level_detail.Source = "battery_toolbar_high_white";
                        }
                        else if (battery_ui[0] >= 45 && battery_ui[0] < 75)
                        {
                            imageBattery.Source = "battery_toolbar_mid";
                 
                            battery_level_detail.Source = "battery_toolbar_mid_white";
                        }
                        else if (battery_ui[0] >= 15 && battery_ui[0] < 45)
                        {
                            imageBattery.Source = "battery_toolbar_low";

                            battery_level_detail.Source = "battery_toolbar_low_white";
                        }
                        else // battery_ui[0] < 15
                        {
                            imageBattery.Source = "battery_toolbar_empty";
                           
                            battery_level_detail.Source = "battery_toolbar_empty_white";
                        }
                    }

                    /*** RSSI ICONS UPDATE ***/
                    if (FormsApp.peripheral.Rssi <= -90)
                    {
                        imageRssi.Source = "rssi_toolbar_empty";
                       
                        rssi_level_detail.Source = "rssi_toolbar_empty_white";
                    }
                    else if (FormsApp.peripheral.Rssi <= -80 && FormsApp.peripheral.Rssi > -90)
                    {
                        imageRssi.Source = "rssi_toolbar_low";
                       
                        rssi_level_detail.Source = "rssi_toolbar_low_white";
                    }
                    else if (FormsApp.peripheral.Rssi <= -60 && FormsApp.peripheral.Rssi > -80)
                    {
                        imageRssi.Source = "rssi_toolbar_mid";

                        rssi_level_detail.Source = "rssi_toolbar_mid_white";
                    }
                    else // (peripheral.Rssi > -60) 
                    {
                        imageRssi.Source = "rssi_toolbar_high";
                       
                        rssi_level_detail.Source = "rssi_toolbar_high_white";
                    }

                    //Save Battery & Rssi info for the next windows
                    CrossSettings.Current.AddOrUpdateValue("battery_icon_topbar", battery_level_detail.Source.ToString().Substring(6));
                    CrossSettings.Current.AddOrUpdateValue("rssi_icon_topbar", rssi_level_detail.Source.ToString().Substring(6));

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }

                background_scan_page_detail.IsVisible = true;
                block_ble_disconnect.Opacity = 0;
                block_ble_disconnect.FadeTo(1, 250);
                block_ble.Opacity = 0;
                block_ble.FadeTo(1, 250);
                background_scan_page.IsVisible = false;
                navigationDrawerList.IsEnabled = true;
                navigationDrawerList.Opacity = 1;

                #region Disable Circular Progress bar Animations when done

                backdark_bg.IsVisible = false;
                indicator.IsVisible = false;

                #endregion



            }
            else
            {
                background_scan_page_detail.IsVisible = false;
                navigationDrawerList.Opacity = 0.65;
                navigationDrawerList.IsEnabled = true;
                background_scan_page.IsVisible = true;
                refresh_command.Execute(true);
                Navigation.PopToRootAsync();


            }
        }

        private string DecodeId(byte[] id)
        {
            string s;
            try
            {
                s = System.Text.Encoding.ASCII.GetString(id.Take(2).ToArray());
                byte[] byte_aux = new byte[4];
                byte_aux[0] = id[3];
                byte_aux[1] = id[2];
                byte_aux[2] = 0;
                byte_aux[3] = 0;
                int num= BitConverter.ToInt32(byte_aux, 0);
                s += num.ToString("0000");
            }
            catch (Exception)
            {
                s = BitConverter.ToString(id);
            }
            return s;
        }

        //private async Task ChangeListViewData()
        private  void ChangeListViewData()
        {
            //await Task.Factory.StartNew(() =>
            // {
            // wait until scan finish
            PrintToConsole($"-------------------------------    ChangeListViewData, thread: {Thread.CurrentThread.ManagedThreadId}");
              //  while (FormsApp.ble_interface.IsScanning())
              //  {
                    try
                    {
                       // Console.WriteLine($"------------------------------- ChangeListViewData while IsScanning, thread: {Thread.CurrentThread.ManagedThreadId}");
                        List<IBlePeripheral> blePeripherals;
                        blePeripherals = FormsApp.ble_interface.GetBlePeripheralList();

                        // YOU CAN RETURN THE PASS BY GETTING THE STRING AND CONVERTING IT TO BYTE ARRAY TO AUTO-PAIR
                        byte[] bytes = System.Convert.FromBase64String(CrossSettings.Current.GetValueOrDefault("session_peripheral_DeviceId", string.Empty));

                        byte[] byte_now = new byte[] { };

                        int sizeList = blePeripherals.Count;

                        for (int i = 0; i < sizeList; i++)
                        {
                            try
                            {
                                if (blePeripherals[i] != null)
                                {
                                    byte_now = blePeripherals[i].Advertisement.ManufacturerSpecificData.ElementAt(0).Data.Take(4).ToArray();

                                    bool enc = false;
                                    int sizeListTemp = employees.Count;

                                    for (int j = 0; j < sizeListTemp; j++)
                                    {
                                        if (employees[j].Peripheral.Advertisement.ManufacturerSpecificData.ElementAt(0).Data.Take(4).ToArray().SequenceEqual
                                        (blePeripherals[i].Advertisement.ManufacturerSpecificData.ElementAt(0).Data.Take(4).ToArray()))
                                            enc = true;
                                    }

                                    string icono_bateria;

                                    byte[] bateria;

                                    if (!enc)
                                    {
                                        bateria = blePeripherals[i].Advertisement.ManufacturerSpecificData.ElementAt(0).Data.Skip(4).Take(1).ToArray();

                                        icono_bateria = "battery_toolbar_high";

                                        if (bateria[0] >= 75)
                                        {
                                            icono_bateria = "battery_toolbar_high";
                                        }
                                        else if (bateria[0] >= 45 && bateria[0] < 75)
                                        {
                                            icono_bateria = "battery_toolbar_mid";
                                        }
                                        else if (bateria[0] >= 15 && bateria[0] < 45)
                                        {
                                            icono_bateria = "battery_toolbar_low";
                                        }
                                        else // bateria[0] < 15
                                        {
                                            icono_bateria = "battery_toolbar_empty";
                                        }

                                        string rssiIcono = "rssi_toolbar_high";

                                        /*** RSSI ICONS UPDATE ***/

                                        if (blePeripherals[i].Rssi <= -90)
                                        {
                                            rssiIcono = "rssi_toolbar_empty";
                                        }
                                        else if (blePeripherals[i].Rssi <= -80 && blePeripherals[i].Rssi > -90)
                                        {
                                            rssiIcono = "rssi_toolbar_low";
                                        }
                                        else if (blePeripherals[i].Rssi <= -60 && blePeripherals[i].Rssi > -80)
                                        {
                                            rssiIcono = "rssi_toolbar_mid";
                                        }
                                        else // (blePeripherals[i].Rssi > -60) 
                                        {
                                            rssiIcono = "rssi_toolbar_high";
                                        }

                                        DeviceItem device = new DeviceItem
                                        {
                                            deviceMacAddress = DecodeId(byte_now),
                                            deviceName = blePeripherals[i].Advertisement.DeviceName,
                                            deviceBattery = bateria[0].ToString() + "%",
                                            deviceRssi = blePeripherals[i].Rssi.ToString() + " dBm",
                                            deviceBatteryIcon = icono_bateria,
                                            deviceRssiIcon = rssiIcono,
                                            Peripheral = blePeripherals[i]
                                        };

                                        employees.Add(device);

                                        //VERIFY IF PREVIOUSLY BOUNDED DEVICES WITH THE RIGHT USERNAME
                                        if (CrossSettings.Current.GetValueOrDefault("session_dynamicpass", string.Empty) != string.Empty &&
                                            FormsApp.credentialsService.UserName.Equals(CrossSettings.Current.GetValueOrDefault("session_username", string.Empty)) &&
                                            bytes.Take(4).ToArray().SequenceEqual(byte_now) &&
                                            blePeripherals[i].Advertisement.DeviceName.Equals(CrossSettings.Current.GetValueOrDefault("session_peripheral", string.Empty)) &&
                                            !peripheralManualDisconnection &&
                                            FormsApp.peripheral == null)
                                        {
                                            if (!FormsApp.ble_interface.IsOpen())
                                            {
                                                try
                                                {
                                                    FormsApp.peripheral = blePeripherals[i];
                                                    peripheralConnected = ble_library.BlePort.NO_CONNECTED;
                                                    peripheralManualDisconnection = false;


                                                    #region Autoconnect to stored device 

                                                    conectarDevice = true;

                                                    autoConnect = true;

                                                    #endregion



                                                }
                                                catch (Exception e)
                                                {
                                                    Console.WriteLine(e.StackTrace);
                                                }

                                            }
                                            else
                                            {

                                                if (autoConnect)
                                                {

                                                    Device.BeginInvokeOnMainThread(() =>
                                                    {
                                                        #region Disable Circular Progress bar Animations when done

                                                        backdark_bg.IsVisible = false;
                                                        indicator.IsVisible = false;
                                                        background_scan_page.IsEnabled = true;

                                                        #endregion
                                                    });

                                                }

                                            }


                                        }

                                        else
                                        {

                                            // if (autoConnect)
                                            //  {

                                            Device.BeginInvokeOnMainThread(() =>
                                            {
                                                #region Disable Circular Progress bar Animations when done

                                                DeviceList.IsRefreshing = false;
                                                backdark_bg.IsVisible = false;
                                                indicator.IsVisible = false;
                                                background_scan_page.IsEnabled = true;

                                                #endregion

                                            });

                                            //  }

                                        }
                                    }
                                }
                            }
                            catch (Exception er)
                            {
                                Console.WriteLine(er.StackTrace); //2018-09-21 13:08:25.918 aclara_meters.iOS[505:190980] System.NullReferenceException: Object reference not set to an instance of an object
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                //}
           // });
        }


        #region We want to connect to the device if there is not scanning running

        private void NewOpenConnectionWithDevice()
        {

            PrintToConsole("Se va a entrar en un bucle mientras esté Escaneando bluetooth - NewOpenConnectionWithDevice");

            while (FormsApp.ble_interface.IsScanning())
            {
                PrintToConsole("A esperar 100 ms mientras escanea... - NewOpenConnectionWithDevice");
                Thread.Sleep(100);
            }

            PrintToConsole("Se va a ejecutar algo en el UI - NewOpenConnectionWithDevice");

            Device.BeginInvokeOnMainThread(() =>
            {
                var seconds = TimeSpan.FromSeconds(1); // Don't execute it asap!

                Device.StartTimer(seconds, () =>
                {
                    PrintToConsole("Cada 1 segundo, se ejectua lo siguinete en el UI - NewOpenConnectionWithDevice");
                    Device.BeginInvokeOnMainThread(() =>
                    {

                        PrintToConsole("¿Esta la conexion abierta ? - NewOpenConnectionWithDevice");


                        if (!FormsApp.ble_interface.IsOpen())
                        {
                            PrintToConsole("¿Esta escaneando perifericos ? - NewOpenConnectionWithDevice");
                            while (FormsApp.ble_interface.IsScanning())
                            {
                                PrintToConsole("A esperar 100 ms en bucle - NewOpenConnectionWithDevice");
                                Thread.Sleep(100);
                            }
                            /* MRA
                            while(FormsApp.ble_interface.GetConnectionStatus() != ble_library.BlePort.CONNECTING)
                            {

                            }*/
                            // call your method to check for notifications here
                            FormsApp.ble_interface.Open(FormsApp.peripheral, true);
                        }
                        else
                        {
                            PrintToConsole("NOPE, no lo esta - NewOpenConnectionWithDevice");
                        }
                    });

                    return false;
                });
            });
        }

        #endregion

        private void DoBasicRead()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Task.Factory.StartNew(BasicReadThread);
            });
        }

        protected override void OnAppearing()
        {
            //DeviceList.RefreshCommand.Execute ( true );
        }


        private void LogOffOkTapped(object sender, EventArgs e)
        {
            if (FormsApp.config.global.UploadPrompt)
            {
                #region Show Upload prompt

                GenericUtilsClass.UploadFilesTask();

                #endregion
            }

            dialog_logoff.IsVisible = false;
            dialog_open_bg.IsVisible = false;
            turnoff_mtu_background.IsVisible = false;

            printer.Abort(); //.Suspend();

            Settings.IsLoggedIn = false;
            FormsApp.credentialsService.DeleteCredentials();
            FormsApp.peripheral = null;
            FormsApp.ble_interface.Close();

            background_scan_page.IsEnabled = true;
            background_scan_page_detail.IsEnabled = true;
            
            Application.Current.MainPage = new NavigationPage(new AclaraViewLogin(dialogsSaved));
            //Navigation.PopAsync();

        }

        private void LogOffNoTapped(object sender, EventArgs e)
        {
            dialog_logoff.IsVisible = false;
            dialog_open_bg.IsVisible = false;
            turnoff_mtu_background.IsVisible = false;
        }

        private void ReplaceMtuCancelTapped(object sender, EventArgs e)
        {
            dialog_open_bg.IsVisible = false;
            turnoff_mtu_background.IsVisible = false;
        }

        private void ReplaceMtuOkTapped(object sender, EventArgs e)
        {
            dialog_replacemeter_one.IsVisible = false;
            dialog_open_bg.IsVisible = false;
            turnoff_mtu_background.IsVisible = false;

            DoBasicRead();

        }




        private void TurnOffMTUCloseTapped(object sender, EventArgs e)
        {
            dialog_open_bg.IsVisible = false;
            turnoff_mtu_background.IsVisible = false;
        }

        private void TurnOffMTUNoTapped(object sender, EventArgs e)
        {
            dialog_open_bg.IsVisible = false;
            turnoff_mtu_background.IsVisible = false;
        }

        private void TurnOffMTUOkTapped(object sender, EventArgs e)
        {
            dialog_turnoff_one.IsVisible = false;
            dialog_turnoff_two.IsVisible = true;

            Task.Factory.StartNew(TurnOffMethod);
        }

        private void TurnOffMethod()
        {

            MTUComm.Action turnOffAction = new MTUComm.Action(
                config: FormsApp.config,
                serial: FormsApp.ble_interface,
                type: MTUComm.Action.ActionType.TurnOffMtu,
                user: FormsApp.credentialsService.UserName);

            turnOffAction.OnFinish += ((s, args) =>
            {
                ActionResult actionResult = args.Result;

                Task.Delay(2000).ContinueWith(t =>
                   Device.BeginInvokeOnMainThread(() =>
                   {
                       this.dialog_turnoff_text.Text = "MTU turned off Successfully";

                       dialog_turnoff_two.IsVisible = false;
                       dialog_turnoff_three.IsVisible = true;
                   }));
            });

            turnOffAction.OnError += (() =>
            {
                Task.Delay(2000).ContinueWith(t =>
                   Device.BeginInvokeOnMainThread(() =>
                   {
                       this.dialog_turnoff_text.Text = "MTU turned off Unsuccessfully";

                       dialog_turnoff_two.IsVisible = false;
                       dialog_turnoff_three.IsVisible = true;
                   }));
            });

            turnOffAction.Run();
        }

        void MeterCancelTapped(object sender, EventArgs e)
        {
            dialog_open_bg.IsVisible = false;
            dialog_meter_replace_one.IsVisible = false;
            turnoff_mtu_background.IsVisible = false;
        }

        void MeterOkTapped(object sender, EventArgs e)
        {
            dialog_meter_replace_one.IsVisible = false;
            dialog_open_bg.IsVisible = false;
            turnoff_mtu_background.IsVisible = false;

            DoBasicRead();


        }

        void dialog_AddMTUAddMeter_cancelTapped(object sender, EventArgs e)
        {
            dialog_open_bg.IsVisible = false;
            dialog_AddMTUAddMeter.IsVisible = false;
            turnoff_mtu_background.IsVisible = false;
        }

        void dialog_AddMTUAddMeter_okTapped(object sender, EventArgs e)
        {
            dialog_AddMTUAddMeter.IsVisible = false;
            dialog_open_bg.IsVisible = false;
            turnoff_mtu_background.IsVisible = false;

            DoBasicRead();


        }

        void dialog_AddMTUReplaceMeter_cancelTapped(object sender, EventArgs e)
        {
            dialog_open_bg.IsVisible = false;
            dialog_AddMTUReplaceMeter.IsVisible = false;
            turnoff_mtu_background.IsVisible = false;
        }

        void dialog_AddMTUReplaceMeter_okTapped(object sender, EventArgs e)
        {
            dialog_AddMTUReplaceMeter.IsVisible = false;
            dialog_open_bg.IsVisible = false;
            turnoff_mtu_background.IsVisible = false;

            DoBasicRead();

        }

        void dialog_ReplaceMTUReplaceMeter_cancelTapped(object sender, EventArgs e)
        {
            dialog_open_bg.IsVisible = false;
            dialog_ReplaceMTUReplaceMeter.IsVisible = false;
            turnoff_mtu_background.IsVisible = false;
        }

        void dialog_ReplaceMTUReplaceMeter_okTapped(object sender, EventArgs e)
        {
            dialog_ReplaceMTUReplaceMeter.IsVisible = false;
            dialog_open_bg.IsVisible = false;
            turnoff_mtu_background.IsVisible = false;

            DoBasicRead();


        }

        void dialog_AddMTU_cancelTapped(object sender, EventArgs e)
        {
            dialog_open_bg.IsVisible = false;
            dialog_AddMTU.IsVisible = false;
            turnoff_mtu_background.IsVisible = false;
        }

        void dialog_AddMTU_okTapped(object sender, EventArgs e)
        {
            dialog_AddMTU.IsVisible = false;
            dialog_open_bg.IsVisible = false;
            turnoff_mtu_background.IsVisible = false;

            DoBasicRead();

        }

        void BasicReadThread()
        {
            MTUComm.Action basicRead = new MTUComm.Action(
               config: FormsApp.config,
               serial: FormsApp.ble_interface,
               type: MTUComm.Action.ActionType.BasicRead,
               user: FormsApp.credentialsService.UserName);

            /*
            basicRead.OnFinish += ((s, args) =>
            { });
            */

            basicRead.OnFinish += ((s, e) =>
            {
                Task.Delay(100).ContinueWith(t =>
                    Device.BeginInvokeOnMainThread(() =>
                    {

                        Application.Current.MainPage.Navigation.PushAsync(new AclaraViewAddMTU(dialogsSaved,  this.actionType), false);

                        #region New Circular Progress bar Animations    

                        DeviceList.IsRefreshing = false;
                        backdark_bg.IsVisible = false;
                        indicator.IsVisible = false;
                        background_scan_page.IsEnabled = true;

                        #endregion

                    })
                );
            });

            basicRead.OnError += (() =>
            {
                Task.Delay(100).ContinueWith(t =>
                    Device.BeginInvokeOnMainThread(() =>
                    {

                        Device.BeginInvokeOnMainThread(() =>
                        {

                            #region New Circular Progress bar Animations    

                            DeviceList.IsRefreshing = false;
                            backdark_bg.IsVisible = false;
                            indicator.IsVisible = false;
                            background_scan_page.IsEnabled = true;

                            #endregion

                            Application.Current.MainPage.DisplayAlert("Alert", "Cannot read device, try again", "Ok");

                        });

                    })
                );
            });

            Device.BeginInvokeOnMainThread(() =>
            {
                #region New Circular Progress bar Animations    

                DeviceList.IsRefreshing = false;
                backdark_bg.IsVisible = true;
                indicator.IsVisible = true;
                background_scan_page.IsEnabled = true;

                #endregion

            });

            basicRead.Run();

           

        }


        private void BluetoothPeripheralDisconnect(object sender, EventArgs e)
        {

            FormsApp.ble_interface.Close();

            peripheralManualDisconnection = true;

            CrossSettings.Current.AddOrUpdateValue("session_dynamicpass", string.Empty);

        }

        private void LogoutTapped(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                dialog_turnoff_one.IsVisible = false;
                dialog_open_bg.IsVisible = true;
                dialog_meter_replace_one.IsVisible = false;
                dialog_turnoff_two.IsVisible = false;
                dialog_turnoff_three.IsVisible = false;
                dialog_replacemeter_one.IsVisible = false;
                dialog_logoff.IsVisible = true;
                dialog_open_bg.IsVisible = true;
                turnoff_mtu_background.IsVisible = true;
            });

        }

        public void externalReconnect(Boolean reassociate)
        {
            try
            {
                FormsApp.ble_interface.Open(FormsApp.peripheral, reassociate);
            }
            catch (Exception e5)
            {
                Console.WriteLine(e5.StackTrace);
            }
           
        }

        // Event for Menu Item selection, here we are going to handle navigation based
        // on user selection in menu ListView
        private void OnMenuItemSelectedListDevices(object sender, ItemTappedEventArgs e)
        {
            var item = (DeviceItem)e.Item;
            //fondo.Opacity = 0;
            //background_scan_page.Opacity = 0.5;
            background_scan_page.IsEnabled = false;

            #region New Circular Progress bar Animations    

            DeviceList.IsRefreshing = false;
            backdark_bg.IsVisible = true;
            indicator.IsVisible = true;

            #endregion

            bool reassociate = false;

            if (CrossSettings.Current.GetValueOrDefault("session_dynamicpass", string.Empty) != string.Empty &&
                FormsApp.credentialsService.UserName.Equals(CrossSettings.Current.GetValueOrDefault("session_username", string.Empty)) &&
                System.Convert.ToBase64String(item.Peripheral.Advertisement.ManufacturerSpecificData.ElementAt(0).Data.Take(4).ToArray()).Equals(CrossSettings.Current.GetValueOrDefault("session_peripheral_DeviceId", string.Empty)) &&
                item.Peripheral.Advertisement.DeviceName.Equals(CrossSettings.Current.GetValueOrDefault("session_peripheral", string.Empty)))
            {
                reassociate = true;
            }

            last_item = item;

            try
            {

                FormsApp.peripheral = item.Peripheral;

                externalReconnect(reassociate);

                Device.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        deviceID.Text = item.deviceName;
                        macAddress.Text = item.deviceMacAddress;
                        imageBattery.Source = item.deviceBatteryIcon;
                        imageRssi.Source = item.deviceRssiIcon;
                        batteryLevel.Text = item.deviceBattery;
                        rssiLevel.Text = item.deviceRssi;
                    }
                    catch (Exception e4)
                    {
                        Console.WriteLine(e4.StackTrace);
                    }
                });

            }
            catch (Exception e22)
            {
                Console.WriteLine(e22.StackTrace);
            }

        }

        // Event for Menu Item selection, here we are going to handle navigation based
        // on user selection in menu ListView
        private void OnMenuItemSelected(object sender, ItemTappedEventArgs e)
        {
            if (!FormsApp.ble_interface.IsOpen())
            {
                // don't do anything if we just de-selected the row.
                if (e.Item == null) return;
                // Deselect the item.
                if (sender is ListView lv) lv.SelectedItem = null;
            }

            if (FormsApp.ble_interface.IsOpen())
            {
                navigationDrawerList.SelectedItem = null;
                try
                {
                    var item = (PageItem)e.Item;
                    ActionType page = item.TargetType;
                    ((ListView)sender).SelectedItem = null;

                    this.actionType = page;

                    NavigationController(page);
                }
                catch (Exception w1)
                {
                    Console.WriteLine(w1.StackTrace);
                }
            }
            else
            {
                Application.Current.MainPage.DisplayAlert("Alert", "Connect to a device and retry", "Ok");
            }
        }

        private void NavigationController(ActionType page)
        {
            switch (page)
            {
                case ActionType.ReadMtu:

                    #region New Circular Progress bar Animations    

                    DeviceList.IsRefreshing = false;
                    backdark_bg.IsVisible = true;
                    indicator.IsVisible = true;

                    #endregion

                    #region Read Mtu Controller

                    background_scan_page.Opacity = 1;
                    background_scan_page_detail.Opacity = 1;

                    background_scan_page.IsEnabled = true;
                    background_scan_page_detail.IsEnabled = true;

                    if (Device.Idiom == TargetIdiom.Phone)
                    {
                        ContentNav.TranslateTo(-310, 0, 175, Easing.SinOut);
                        shadoweffect.TranslateTo(-310, 0, 175, Easing.SinOut);
                    }

                    Task.Delay(200).ContinueWith(t =>

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            navigationDrawerList.SelectedItem = null;

                            Application.Current.MainPage.Navigation.PushAsync(new AclaraViewReadMTU(dialogsSaved), false);

                            background_scan_page.Opacity = 1;
                            background_scan_page_detail.Opacity = 1;

                            if (Device.Idiom == TargetIdiom.Tablet)
                            {
                                ContentNav.Opacity = 1;
                                ContentNav.IsVisible = true;
                            }
                            else
                            {
                                ContentNav.Opacity = 0;
                                ContentNav.IsVisible = false;
                            }
                            shadoweffect.IsVisible &= Device.Idiom != TargetIdiom.Phone; // if (Device.Idiom == TargetIdiom.Phone) shadoweffect.IsVisible = false;

                            #region New Circular Progress bar Animations    

                            DeviceList.IsRefreshing = false;
                            backdark_bg.IsVisible = false;
                            indicator.IsVisible = false;

                            #endregion

                        })
                    );

                    #endregion

                    break;

                case ActionType.AddMtu:

                    #region Add Mtu Controller

                    background_scan_page.Opacity = 1;
                    background_scan_page_detail.Opacity = 1;
                    background_scan_page.IsEnabled = true;
                    background_scan_page_detail.IsEnabled = true;

                    if (Device.Idiom == TargetIdiom.Phone)
                    {
                        ContentNav.TranslateTo(-310, 0, 175, Easing.SinOut);
                        shadoweffect.TranslateTo(-310, 0, 175, Easing.SinOut);
                    }

                    Task.Delay(200).ContinueWith(t =>

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            dialog_open_bg.IsVisible = true;
                            turnoff_mtu_background.IsVisible = true;
                            dialog_turnoff_one.IsVisible = false;
                            dialog_turnoff_two.IsVisible = false;
                            dialog_turnoff_three.IsVisible = false;
                            dialog_replacemeter_one.IsVisible = false;
                            dialog_meter_replace_one.IsVisible = false;

                            dialog_AddMTUAddMeter.IsVisible = false;
                            dialog_AddMTUReplaceMeter.IsVisible = false;
                            dialog_ReplaceMTUReplaceMeter.IsVisible = false;

                            #region Check ActionVerify

                            if (FormsApp.config.global.ActionVerify)
                                dialog_AddMTU.IsVisible = true;
                            else
                                CallLoadViewAddMtu();

                            #endregion

                            background_scan_page.Opacity = 1;
                            background_scan_page_detail.Opacity = 1;

                            if (Device.Idiom == TargetIdiom.Tablet)
                            {
                                ContentNav.Opacity = 1;
                                ContentNav.IsVisible = true;
                            }
                            else
                            {
                                ContentNav.Opacity = 0;
                                ContentNav.IsVisible = false;
                            }

                            shadoweffect.IsVisible &= Device.Idiom != TargetIdiom.Phone;
                        })
                    );

                    #endregion

                    break;

                case ActionType.TurnOffMtu:

                    #region Turn Off Controller

                    background_scan_page.Opacity = 1;
                    background_scan_page_detail.Opacity = 1;

                    background_scan_page.IsEnabled = true;
                    background_scan_page_detail.IsEnabled = true;

                    if (Device.Idiom == TargetIdiom.Phone)
                    {
                        ContentNav.TranslateTo(-310, 0, 175, Easing.SinOut);
                        shadoweffect.TranslateTo(-310, 0, 175, Easing.SinOut);
                    }

                    Task.Delay(200).ContinueWith(t =>

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            dialog_open_bg.IsVisible = true;
                            turnoff_mtu_background.IsVisible = true;
                            dialog_meter_replace_one.IsVisible = false;

                            #region Check ActionVerify

                            if (FormsApp.config.global.ActionVerify)
                                dialog_turnoff_one.IsVisible = true;
                            else
                                CallLoadViewTurnOff();

                            #endregion

                            dialog_turnoff_two.IsVisible = false;
                            dialog_turnoff_three.IsVisible = false;
                            dialog_replacemeter_one.IsVisible = false;

                            background_scan_page.Opacity = 1;
                            background_scan_page_detail.Opacity = 1;

                            if (Device.Idiom == TargetIdiom.Tablet)
                            {
                                ContentNav.Opacity = 1;
                                ContentNav.IsVisible = true;
                            }
                            else
                            {
                                ContentNav.Opacity = 0;
                                ContentNav.IsVisible = false;
                            }

                            shadoweffect.IsVisible &= Device.Idiom != TargetIdiom.Phone;
                        })
                    );

                    #endregion

                    break;

                case ActionType.MtuInstallationConfirmation:

                    #region Install Confirm Controller

                    background_scan_page.Opacity = 1;
                    background_scan_page_detail.Opacity = 1;

                    background_scan_page.IsEnabled = true;
                    background_scan_page_detail.IsEnabled = true;

                    if (Device.Idiom == TargetIdiom.Phone)
                    {
                        ContentNav.TranslateTo(-310, 0, 175, Easing.SinOut);
                        shadoweffect.TranslateTo(-310, 0, 175, Easing.SinOut);
                    }

                    Task.Delay(200).ContinueWith(t =>

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            navigationDrawerList.SelectedItem = null;

                            Application.Current.MainPage.Navigation.PushAsync(new AclaraViewInstallConfirmation(dialogsSaved), false);

                            background_scan_page.Opacity = 1;
                            background_scan_page_detail.Opacity = 1;

                            if (Device.Idiom == TargetIdiom.Tablet)
                            {
                                ContentNav.Opacity = 1;
                                ContentNav.IsVisible = true;
                            }
                            else
                            {
                                ContentNav.Opacity = 0;
                                ContentNav.IsVisible = false;
                            }
                            shadoweffect.IsVisible &= Device.Idiom != TargetIdiom.Phone;
                        })
                    );

                    #endregion

                    break;

                case ActionType.ReplaceMTU:

                    #region Replace Mtu Controller

                    background_scan_page.Opacity = 1;
                    background_scan_page_detail.Opacity = 1;

                    background_scan_page.IsEnabled = true;
                    background_scan_page_detail.IsEnabled = true;

                    if (Device.Idiom == TargetIdiom.Phone)
                    {
                        ContentNav.TranslateTo(-310, 0, 175, Easing.SinOut);
                        shadoweffect.TranslateTo(-310, 0, 175, Easing.SinOut);
                    }

                    Task.Delay(200).ContinueWith(t =>

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            dialog_open_bg.IsVisible = true;
                            turnoff_mtu_background.IsVisible = true;
                            dialog_meter_replace_one.IsVisible = false;
                            dialog_turnoff_one.IsVisible = false;
                            dialog_turnoff_two.IsVisible = false;
                            dialog_turnoff_three.IsVisible = false;

                            #region Check ActionVerify

                            if (FormsApp.config.global.ActionVerify)
                                dialog_replacemeter_one.IsVisible = true;
                            else
                                CallLoadViewReplaceMtu();

                            #endregion

                            background_scan_page.Opacity = 1;
                            background_scan_page_detail.Opacity = 1;

                            if (Device.Idiom == TargetIdiom.Tablet)
                            {
                                ContentNav.Opacity = 1;
                                ContentNav.IsVisible = true;
                            }
                            else
                            {
                                ContentNav.Opacity = 0;
                                ContentNav.IsVisible = false;
                            }

                            shadoweffect.IsVisible &= Device.Idiom != TargetIdiom.Phone; //if (Device.Idiom == TargetIdiom.Phone) shadoweffect.IsVisible = false;
                        })
                    );

                    #endregion

                    break;

                case ActionType.ReplaceMeter:

                    #region Replace Meter Controller

                    background_scan_page.Opacity = 1;
                    background_scan_page_detail.Opacity = 1;

                    background_scan_page.IsEnabled = true;
                    background_scan_page_detail.IsEnabled = true;

                    if (Device.Idiom == TargetIdiom.Phone)
                    {
                        ContentNav.TranslateTo(-310, 0, 175, Easing.SinOut);
                        shadoweffect.TranslateTo(-310, 0, 175, Easing.SinOut);
                    }
                    Task.Delay(200).ContinueWith(t =>

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            dialog_open_bg.IsVisible = true;
                            turnoff_mtu_background.IsVisible = true;
                            dialog_turnoff_one.IsVisible = false;
                            dialog_turnoff_two.IsVisible = false;
                            dialog_turnoff_three.IsVisible = false;
                            dialog_replacemeter_one.IsVisible = false;


                            #region Check ActionVerify

                            if (FormsApp.config.global.ActionVerify)
                                dialog_meter_replace_one.IsVisible = true;
                            else
                                CallLoadViewReplaceMeter();

                            #endregion

                            background_scan_page.Opacity = 1;
                            background_scan_page_detail.Opacity = 1;

                            if (Device.Idiom == TargetIdiom.Tablet)
                            {
                                ContentNav.Opacity = 1;
                                ContentNav.IsVisible = true;
                            }
                            else
                            {
                                ContentNav.Opacity = 0;
                                ContentNav.IsVisible = false;
                            }
                            shadoweffect.IsVisible &= Device.Idiom != TargetIdiom.Phone; // if (Device.Idiom == TargetIdiom.Phone) shadoweffect.IsVisible = false;
                        })
                    );

                    #endregion

                    break;

                case ActionType.AddMtuAddMeter:

                    #region Add Mtu | Add Meter Controller

                    background_scan_page.Opacity = 1;
                    background_scan_page_detail.Opacity = 1;

                    background_scan_page.IsEnabled = true;
                    background_scan_page_detail.IsEnabled = true;

                    if (Device.Idiom == TargetIdiom.Phone)
                    {
                        ContentNav.TranslateTo(-310, 0, 175, Easing.SinOut);
                        shadoweffect.TranslateTo(-310, 0, 175, Easing.SinOut);
                    }

                    Task.Delay(200).ContinueWith(t =>

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            dialog_open_bg.IsVisible = true;
                            turnoff_mtu_background.IsVisible = true;
                            dialog_turnoff_one.IsVisible = false;
                            dialog_turnoff_two.IsVisible = false;
                            dialog_turnoff_three.IsVisible = false;
                            dialog_replacemeter_one.IsVisible = false;
                            dialog_meter_replace_one.IsVisible = false;

                            #region Check ActionVerify

                            if (FormsApp.config.global.ActionVerify)
                                dialog_AddMTUAddMeter.IsVisible = true;
                            else
                                CallLoadViewAddMTUAddMeter();

                            #endregion

                            background_scan_page.Opacity = 1;
                            background_scan_page_detail.Opacity = 1;

                            if (Device.Idiom == TargetIdiom.Tablet)
                            {
                                ContentNav.Opacity = 1;
                                ContentNav.IsVisible = true;
                            }
                            else
                            {
                                ContentNav.Opacity = 0;
                                ContentNav.IsVisible = false;
                            }
                            shadoweffect.IsVisible &= Device.Idiom != TargetIdiom.Phone; // if (Device.Idiom == TargetIdiom.Phone) shadoweffect.IsVisible = false;
                        })
                    );

                    #endregion

                    break;

                case ActionType.AddMtuReplaceMeter:

                    #region Add Mtu | Replace Meter Controller

                    background_scan_page.Opacity = 1;
                    background_scan_page_detail.Opacity = 1;

                    background_scan_page.IsEnabled = true;
                    background_scan_page_detail.IsEnabled = true;

                    if (Device.Idiom == TargetIdiom.Phone)
                    {
                        ContentNav.TranslateTo(-310, 0, 175, Easing.SinOut);
                        shadoweffect.TranslateTo(-310, 0, 175, Easing.SinOut);
                    }

                    Task.Delay(200).ContinueWith(t =>

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            dialog_open_bg.IsVisible = true;
                            turnoff_mtu_background.IsVisible = true;
                            dialog_turnoff_one.IsVisible = false;
                            dialog_turnoff_two.IsVisible = false;
                            dialog_turnoff_three.IsVisible = false;
                            dialog_replacemeter_one.IsVisible = false;
                            dialog_meter_replace_one.IsVisible = false;
                            dialog_AddMTUAddMeter.IsVisible = false;

                            #region Check ActionVerify

                            if (FormsApp.config.global.ActionVerify)
                                dialog_AddMTUReplaceMeter.IsVisible = true;
                            else
                                CallLoadViewAddMTUReplaceMeter();

                            #endregion

                            background_scan_page.Opacity = 1;
                            background_scan_page_detail.Opacity = 1;

                            if (Device.Idiom == TargetIdiom.Tablet)
                            {
                                ContentNav.Opacity = 1;
                                ContentNav.IsVisible = true;
                            }
                            else
                            {
                                ContentNav.Opacity = 0;
                                ContentNav.IsVisible = false;
                            }
                            shadoweffect.IsVisible &= Device.Idiom != TargetIdiom.Phone; // if (Device.Idiom == TargetIdiom.Phone) shadoweffect.IsVisible = false;
                        })
                    );

                    #endregion

                    break;

                case ActionType.ReplaceMtuReplaceMeter:

                    #region Replace Mtu | Replace Meter Controller

                    background_scan_page.Opacity = 1;
                    background_scan_page_detail.Opacity = 1;

                    background_scan_page.IsEnabled = true;
                    background_scan_page_detail.IsEnabled = true;

                    if (Device.Idiom == TargetIdiom.Phone)
                    {
                        ContentNav.TranslateTo(-310, 0, 175, Easing.SinOut);
                        shadoweffect.TranslateTo(-310, 0, 175, Easing.SinOut);
                    }

                    Task.Delay(200).ContinueWith(t =>

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            dialog_open_bg.IsVisible = true;
                            turnoff_mtu_background.IsVisible = true;
                            dialog_turnoff_one.IsVisible = false;
                            dialog_turnoff_two.IsVisible = false;
                            dialog_turnoff_three.IsVisible = false;
                            dialog_replacemeter_one.IsVisible = false;
                            dialog_meter_replace_one.IsVisible = false;
                            dialog_AddMTUAddMeter.IsVisible = false;
                            dialog_AddMTUReplaceMeter.IsVisible = false;

                            #region Check ActionVerify

                            if (FormsApp.config.global.ActionVerify)
                                dialog_ReplaceMTUReplaceMeter.IsVisible = true;
                            else
                                CallLoadViewReplaceMTUReplaceMeter();

                            #endregion


                            background_scan_page.Opacity = 1;
                            background_scan_page_detail.Opacity = 1;

                            if (Device.Idiom == TargetIdiom.Tablet)
                            {
                                ContentNav.Opacity = 1;
                                ContentNav.IsVisible = true;
                            }
                            else
                            {
                                ContentNav.Opacity = 0;
                                ContentNav.IsVisible = false;
                            }
                            shadoweffect.IsVisible &= Device.Idiom != TargetIdiom.Phone; // if (Device.Idiom == TargetIdiom.Phone) shadoweffect.IsVisible = false;
                        })
                    );

                    #endregion

                    break;

            }
        }

        private void CallLoadViewReplaceMTUReplaceMeter()
        {
            dialog_ReplaceMTUReplaceMeter.IsVisible = false;
            dialog_open_bg.IsVisible = false;
            turnoff_mtu_background.IsVisible = false;

            DoBasicRead();

        }

        private void CallLoadViewAddMTUReplaceMeter()
        {
            dialog_AddMTUReplaceMeter.IsVisible = false;
            dialog_open_bg.IsVisible = false;
            turnoff_mtu_background.IsVisible = false;

            DoBasicRead();
        }

        private void CallLoadViewAddMTUAddMeter()
        {

            dialog_AddMTUAddMeter.IsVisible = false;
            dialog_open_bg.IsVisible = false;
            turnoff_mtu_background.IsVisible = false;

            DoBasicRead();

        }

        private void CallLoadViewReplaceMeter()
        {
            dialog_meter_replace_one.IsVisible = false;
            dialog_open_bg.IsVisible = false;
            turnoff_mtu_background.IsVisible = false;

            DoBasicRead();
        }

        private void CallLoadViewReplaceMtu()
        {
            dialog_replacemeter_one.IsVisible = false;
            dialog_open_bg.IsVisible = false;
            turnoff_mtu_background.IsVisible = false;


            DoBasicRead();

        }

        private void CallLoadViewTurnOff()
        {
            dialog_turnoff_one.IsVisible = false;
            dialog_turnoff_two.IsVisible = true;

            Task.Factory.StartNew(TurnOffMethod);
        }

        private void CallLoadViewAddMtu()
        {
            dialog_AddMTU.IsVisible = false;
            dialog_open_bg.IsVisible = false;
            turnoff_mtu_background.IsVisible = false;

            DoBasicRead();
        }



        private void OpenSettingsTapped(object sender, EventArgs e)
        {
            //printer.Suspend();
            background_scan_page.Opacity = 1;
            background_scan_page_detail.Opacity = 1;
            background_scan_page.IsEnabled = true;
            background_scan_page_detail.IsEnabled = true;

            if (Device.Idiom == TargetIdiom.Phone)
            {
                ContentNav.TranslateTo(-310, 0, 175, Easing.SinOut);
                shadoweffect.TranslateTo(-310, 0, 175, Easing.SinOut);
            }

           
            Task.Delay(200).ContinueWith(t =>
            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        #region New Circular Progress bar Animations    

                        DeviceList.IsRefreshing = false;
                        backdark_bg.IsVisible = true;
                        indicator.IsVisible = true;
                        background_scan_page.IsEnabled = false;

                        #endregion

                    });

                    if (FormsApp.ble_interface.IsOpen())
                    {
                        Application.Current.MainPage.Navigation.PushAsync(new AclaraViewSettings(dialogsSaved), false);
                        if (Device.Idiom == TargetIdiom.Tablet)
                        {
                            ContentNav.Opacity = 1;
                            ContentNav.IsVisible = true;
                        }
                        else
                        {
                            ContentNav.Opacity = 0;
                            ContentNav.IsVisible = false;
                        }
                        background_scan_page.Opacity = 1;
                        background_scan_page_detail.Opacity = 1;

                        shadoweffect.IsVisible &= Device.Idiom != TargetIdiom.Phone; //   if (Device.Idiom == TargetIdiom.Phone) shadoweffect.IsVisible = false;

                        Device.BeginInvokeOnMainThread(() =>
                        {

                            #region New Circular Progress bar Animations    

                            DeviceList.IsRefreshing = false;
                            backdark_bg.IsVisible = false;
                            indicator.IsVisible = false;
                            background_scan_page.IsEnabled = true;

                            #endregion


                        });

                        return;
                    }
                    else
                    {
                        Application.Current.MainPage.Navigation.PushAsync(new AclaraViewSettings(true), false);

                        if (Device.Idiom == TargetIdiom.Tablet)
                        {
                            ContentNav.Opacity = 1;
                            ContentNav.IsVisible = true;
                        }
                        else
                        {
                            ContentNav.Opacity = 0;
                            ContentNav.IsVisible = false;
                        }

                        background_scan_page.Opacity = 1;
                        background_scan_page_detail.Opacity = 1;

                        shadoweffect.IsVisible &= Device.Idiom != TargetIdiom.Phone; // if (Device.Idiom == TargetIdiom.Phone) shadoweffect.IsVisible = false; 

                        Device.BeginInvokeOnMainThread(() =>
                        {

                            #region New Circular Progress bar Animations    

                            DeviceList.IsRefreshing = false;
                            backdark_bg.IsVisible = false;
                            indicator.IsVisible = false;
                            background_scan_page.IsEnabled = true;

                            #endregion


                        });

                    }
                }
                catch (Exception i2)
                {
                    Console.WriteLine(i2.StackTrace);
                }
            }));
        }

        private void SideMenuOpen(object sender, EventArgs e)
        {
            fondo.Opacity = 0;
            ContentNav.IsVisible = true;
            shadoweffect.IsVisible = true;
            background_scan_page.Opacity = 0.5;
            background_scan_page_detail.Opacity = 0.5;
            ContentNav.Opacity = 1;
            ContentNav.TranslateTo(0, 0, 175, Easing.SinIn);
            shadoweffect.TranslateTo(0, 0, 175, Easing.SinIn);
            background_scan_page.IsEnabled = false;
            background_scan_page_detail.IsEnabled = false;
        }

        private void SideMenuClose(object sender, EventArgs e)
        {
            fondo.Opacity = 1;
            ContentNav.TranslateTo(-310, 0, 175, Easing.SinOut);
            shadoweffect.TranslateTo(-310, 0, 175, Easing.SinOut);
            background_scan_page.Opacity = 1;
            background_scan_page_detail.Opacity = 1;

            Task.Delay(200).ContinueWith(t =>
            Device.BeginInvokeOnMainThread(() =>
            {
                ContentNav.Opacity = 0;
                shadoweffect.IsVisible = false;
                ContentNav.IsVisible = false;
                background_scan_page.IsEnabled = true;
                background_scan_page_detail.IsEnabled = true;
            }));
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // todo: this is a hack - hopefully Xamarin adds the ability to name a Pushed Page.
            //MainMenu.IsSegmentShowing = false;
            bool value = FormsApp.ble_interface.IsOpen();
            value &= Navigation.NavigationStack.Count >= 3; //  if(Navigation.NavigationStack.Count < 3) Settings.IsLoggedIn = false;
        }


        public void PrintToConsole(string printConsole)
        {

            if (GetDebugVar())
            {
                Console.WriteLine("DEBUG_ACL: " + printConsole);
            }
        }


    }
}
