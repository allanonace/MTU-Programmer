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
using Plugin.Settings;
using System.Linq;
using System.Threading;
using MTUComm;
using System.IO;
using nexus.protocols.ble.scan;
using System.Collections.ObjectModel;
using System.Web;

using Error = Xml.Error;

namespace aclara_meters.view
{
    public partial class AclaraViewScripting
    {
        private const bool DEBUG_MODE_ON = false;

        private List<ReadMTUItem> MTUDataListView { get; set; }
        private List<PageItem> MenuList { get; set; }
        private bool _userTapped;
        private IUserDialogs dialogsSaved;
        private Thread printer;
        private ObservableCollection<DeviceItem> employees;
        private String username;

        private string resultCallback;
        private string resultScriptName;

        private string resultDataXml;
        private bool autoConnect;

        private bool conectarDevice;

        public AclaraViewScripting()
        {
            InitializeComponent();
        }

        public AclaraViewScripting(string url, string callback, string script_name)
        {

            PrintToConsole($"-------------------------------       AclaraViewScripting, thread: { Thread.CurrentThread.ManagedThreadId}");
            InitializeComponent();

            //CrossSettings.Current.AddOrUpdateValue("session_dynamicpass", string.Empty);

            resultCallback = callback;
            resultScriptName = script_name;

            //FormsApp.credentialsService.SaveCredentials("test", "test");

            if (Device.Idiom == TargetIdiom.Tablet)
            {
                Task.Run(() =>
                {
                    Device.BeginInvokeOnMainThread(LoadTabletUI);
                });
            }
            else
            {
                Task.Run(() =>
                {
                    Device.BeginInvokeOnMainThread(LoadPhoneUI);
                });
            }

            NavigationPage.SetHasNavigationBar(this, false); //Turn off the Navigation bar

            ContentView_Scripting_label_read.Text = "";

            _userTapped = false;

            TappedListeners();

            //Change username textview to Prefs. String
            if (FormsApp.credentialsService.UserName != null)
            {
                username = FormsApp.credentialsService.UserName; //"Kartik";
            }

            ContentView_Scripting_battery_level.Source = CrossSettings.Current.GetValueOrDefault("battery_icon_topbar", "battery_toolbar_high_white");
            ContentView_Scripting_rssi_level.Source = CrossSettings.Current.GetValueOrDefault("rssi_icon_topbar", "rssi_toolbar_high_white");

            Task.Run(() =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    hamburger_icon.Scale = 0;
                    hamburger_icon.IsEnabled = false;
                    ContentView_Scripting_hamburger_icon.Scale = 0;
                    ContentView_Scripting_hamburger_icon.IsEnabled = false;
                });
            });


            resultDataXml = File.ReadAllText(url);

            ContentView_Scripting_textScript.Text = "Processing ...";
            ContentView_Scripting_fieldpath_script.Text = url;


            #region New Scripting method is called

            //Device.BeginInvokeOnMainThread(() =>
            //{
            //    PrintToConsole("Se va a empezar el flujo");

            //    PrintToConsole("Se va a lanzar una Tarea. Task.Factory.StartNew(Init_Scripting_Method)");

            //    Task.Factory.StartNew(Interface_ContentView_DeviceList);

            //});
            InitRefreshCommand();

            Interface_ContentView_DeviceList();
            #endregion

        }


        /*--------------------------------------------------*/
        /*          Device List Interface Contenview
        /---------------------------------------------------*/
        private void InitRefreshCommand()
        {
            DeviceList.RefreshCommand = new Command(async () =>
            {
                PrintToConsole($"----------------------REFRESH command dispositivos encontrados : {FormsApp.ble_interface.GetBlePeripheralList().Count}");
                PrintToConsole($"-------------------------------        REFRESH command, thread: { Thread.CurrentThread.ManagedThreadId}");

                if (!GetAutoConnectStatus())
                {

                    Esperando();

                    if (printer.ThreadState == System.Threading.ThreadState.Suspended)
                    {
                        try
                        {

                            printer.Resume();
                        }
                        catch (Exception e11)
                        {
                            Console.WriteLine(e11.StackTrace);
                        }
                    }
                    //DeviceList.IsRefreshing = true;
                    employees = new ObservableCollection<DeviceItem>();
                    PrintToConsole("comienza el Escaneo de dispositivos - Interface_background_scan_page");
                    //FormsApp.ble_interface.SetTimeOutSeconds(TimeOutSeconds);
                    //bucle infinito hasy a que encuentre al gun puck
                    while (FormsApp.ble_interface.GetBlePeripheralList().Count == 0)
                    {
                        await FormsApp.ble_interface.Scan();
                    }
                    //TimeOutSeconds = 3; // los siguientes escaneos son de 5 sec

                    if (FormsApp.ble_interface.GetBlePeripheralList().Count > 0)
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
                        else
                            ContentView_DeviceList.IsVisible = true;
                    }
                    //else
                    //{
                    //    DeviceList.ItemsSource = null;
                    //    Application.Current.MainPage.DisplayAlert("Alert", "No device found, please, press the button to turn on the device and refresh", "Ok");
                    //    Terminado();
                    //}
                    Terminado();
                }
            });

        }

        private void PairWithKnowDevice()
        {

            autoConnect = false;
            conectarDevice = false;
            #region Autoconnect to stored device 

            PrintToConsole($"-----------------------------------va a conectar con : {peripheral.Advertisement.DeviceName}");
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
                backdark_bg.IsVisible = false;
                indicator.IsVisible = true;
                txtBuscando.IsVisible = true;
                ContentView_DeviceList.IsEnabled = false;
                ContentView_DeviceList.IsVisible = true;
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
                txtBuscando.IsVisible = false;


                //DeviceList.IsEnabled = true;
                //fondo.Opacity = 1;
                //background_scan_page.Opacity = 1;
                //background_scan_page.IsEnabled = true;
                #endregion
            });
        }




        private void Interface_ContentView_DeviceList()
        {
            PrintToConsole($"-------------------------------    Interface_ContentView_DeviceList, thread: { Thread.CurrentThread.ManagedThreadId}");

            printer = new Thread(new ThreadStart(InvokeMethod));

            printer.Start();

            DeviceList.RefreshCommand.Execute(true);
              
        }

        private void InvokeMethod()
        {
            PrintToConsole("dentro del metodo - InvokeMethod");

            int timeout_connecting = 0;

            PrintToConsole("se va a ejecutar un bucle (WHILE TRUE) - InvokeMethod");
         
            while (true)
            {
                PrintToConsole("dentro del bucle (WHILE TRUE) - InvokeMethod");

                PrintToConsole("buscamos el estado de la conexion - InvokeMethod");

                int status = FormsApp.ble_interface.GetConnectionStatus();

                PrintToConsole("se obtiene el estado de la conexion - InvokeMethod");

                if (status != peripheralConnected)
                {

                    PrintToConsole("buscamos el estado de la conexion - InvokeMethod");

                    PrintToConsole("¿ES NO_CONNECTED? - InvokeMethod");

                    if (peripheralConnected == ble_library.BlePort.NO_CONNECTED)
                    {
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
                                            ContentView_DeviceList.IsEnabled = true;

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
                                            ContentView_DeviceList.IsEnabled = true;

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
                                            ContentView_DeviceList.IsEnabled = true;

                                            #endregion

                                        });
                                        PrintToConsole("Desactivar barra de progreso - InvokeMethod");
                                        Application.Current.MainPage.DisplayAlert("Alert", "Please, press the button to change PAIRING mode", "Ok");
                                        break;
                                }
                                DeviceList.IsEnabled = true;
                                fondo.Opacity = 1;
                                ContentView_DeviceList.Opacity = 1;
                                ContentView_DeviceList.IsEnabled = true;

                               

                            });
                            peripheralConnected = status;
                            peripheral = null;
                        }
                        else
                        {
                            PrintToConsole("Estas Conectado - InvokeMethod");

                            DeviceList.IsEnabled = true;

                            peripheralConnected = status;
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                fondo.Opacity = 1;
                                ContentView_DeviceList.Opacity = 1;
                                ContentView_DeviceList.IsEnabled = true;

                                //Console.WriteLine("CONNECTED");

                                ContentView_Scripting.IsVisible = true;
                                ContentView_DeviceList.IsVisible = false;


                                try
                                {
                                    Device.BeginInvokeOnMainThread(() =>
                                    {
										// Minor bug fix in case of missing battery data
                                        try
                                        {
                                            byte[] bateria = peripheral.Advertisement.ManufacturerSpecificData.ElementAt(0).Data.Skip(4).Take(1).ToArray();

                                            String icono_bateria = "battery_toolbar_high";

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

                                            if (peripheral.Rssi <= -90)
                                            {
                                                rssiIcono = "rssi_toolbar_empty";
                                            }
                                            else if (peripheral.Rssi <= -80 && peripheral.Rssi > -90)
                                            {
                                                rssiIcono = "rssi_toolbar_low";
                                            }
                                            else if (peripheral.Rssi <= -60 && peripheral.Rssi > -80)
                                            {
                                                rssiIcono = "rssi_toolbar_mid";
                                            }
                                            else // (blePeripherals[i].Rssi > -60) 
                                            {
                                                rssiIcono = "rssi_toolbar_high";
                                            }

                                            ContentView_Scripting_battery_level.Source = icono_bateria + "_white";
                                            ContentView_Scripting_rssi_level.Source = rssiIcono + "_white";



                                            /*
                                                                                   
                                            #region New Circular Progress bar Animations    


                                            DeviceList.IsRefreshing = false;
                                            backdark_bg.IsVisible = false;
                                            indicator.IsVisible = false;

                                            #endregion
                                                                                       
                                            */

                                         


                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(e.StackTrace);
                                        }


                                      

                                    });

                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.StackTrace);
                                }

                                PrintToConsole("Se va a ejecutar el Script - InvokeMethod");

                                try
                                {
                                    printer.Suspend();
                                }
                                catch (Exception e5)
                                {
                                    Console.WriteLine(e5.StackTrace);
                                }



                                //Connection Method
                                runScript();

                            });
                        }
                    }
                    else if (peripheralConnected == ble_library.BlePort.CONNECTED)
                    {
                        PrintToConsole("Nop, es CONNECTED - InvokeMethod");

                        DeviceList.IsEnabled = true;

                        peripheralConnected = status;
                        peripheral = null;
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            fondo.Opacity = 1;
                            ContentView_DeviceList.Opacity = 1;
                            ContentView_DeviceList.IsEnabled = true;

                            //Console.WriteLine("NOT CONNECTED");
                            ContentView_Scripting.IsVisible = false;
                            ContentView_DeviceList.IsVisible = true;
                          
                        });

                    }

                }

                PrintToConsole("¿Está en CONNECTING? - InvokeMethod");

                if (peripheralConnected == ble_library.BlePort.CONNECTING)
                {
                    PrintToConsole("Si, es CONNECTING - InvokeMethod");
                    timeout_connecting++;
                    if (timeout_connecting >= 2 * 10) // 10 seconds
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            PrintToConsole("Un Timeout que te llevas - InvokeMethod");
                            Application.Current.MainPage.DisplayAlert("Timeout", "Connection Timeout", "Ok");
                            DeviceList.IsEnabled = true;
                            fondo.Opacity = 1;
                            ContentView_DeviceList.Opacity = 1;
                            ContentView_DeviceList.IsEnabled = true;

                            autoConnect = false;

                            Device.BeginInvokeOnMainThread(() =>
                            {

                                #region Disable Circular Progress bar Animations when done

                                backdark_bg.IsVisible = false;
                                indicator.IsVisible = false;
                                ContentView_DeviceList.IsEnabled = true;

                                #endregion

                            });

                            try
                            {
                                printer.Suspend();
                            }
                            catch (Exception e5)
                            {
                                Console.WriteLine(e5.StackTrace);
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

        private void runScript()
        {
            Task.Run(async () =>
            {

                PrintToConsole("runScript Tarea Asincrona - InvokeMethod");
                await Task.Delay(150); 
                Device.BeginInvokeOnMainThread(() =>
                {
                    try
                    {

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            PrintToConsole("Mostrar barra de progreso - InvokeMethod");

                            backdark_bg.IsVisible = true;
                            indicator.IsVisible = true;
                            ContentView_DeviceList.IsEnabled = false;
                            ContentView_Scripting.IsEnabled = false;
                            _userTapped = true;
                            ContentView_Scripting_label_read.Text = "Executing Script ... ";
                        });


                        Device.BeginInvokeOnMainThread(() =>
                        {

                            Task.Factory.StartNew(scriptFunction);
                        });

                    }
                    catch (Exception e2)
                    {
                        Console.WriteLine(e2.StackTrace);

                    }

                });
            });

        }

        private bool GetAutoConnectStatus()
        {
            return autoConnect;
        }

        private void scriptFunction()
        {
            ScriptRunner runner = new ScriptRunner();

            //Define finish and error event handler
            runner.OnFinish     += OnFinish;
            runner.OnProgress   += OnProgress;
            runner.onStepFinish += onStepFinish;
            runner.OnError      += OnError;

            runner.ParseScriptAndRun ( FormsApp.config.GetBasePath(), FormsApp.ble_interface, resultDataXml, resultDataXml.Length );
        }

        //private async Task ChangeListViewData()
        private void ChangeListViewData()
        {
           // await Task.Factory.StartNew(() =>
           // {
                // wait until scan finish
              //  while (FormsApp.ble_interface.IsScanning())
              //  {
                    try
                    {
                        List<IBlePeripheral> blePeripherals;
                        blePeripherals = FormsApp.ble_interface.GetBlePeripheralList();


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
                                        if (employees[j].Peripheral.Advertisement.ManufacturerSpecificData.ElementAt(0).Data.Take(4).ToArray()
                                            .SequenceEqual(blePeripherals[i].Advertisement.ManufacturerSpecificData.ElementAt(0).Data.Take(4).ToArray()))
                                        {
                                            enc = true;
                                        }
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

                                      

                                        if (CrossSettings.Current.GetValueOrDefault("session_dynamicpass", string.Empty) != string.Empty &&
                                            bytes.Take(4).ToArray().SequenceEqual(byte_now) &&
                                            blePeripherals[i].Advertisement.DeviceName.Equals(CrossSettings.Current.GetValueOrDefault("session_peripheral", string.Empty)) &&
                                            !peripheralManualDisconnection &&
                                            peripheral == null)

                                        {
                                            if (!FormsApp.ble_interface.IsOpen())
                                            {
                                                try
                                                {

                                                    #region Autoconnect to stored device 

                                                    peripheral = blePeripherals[i];
                                                    peripheralConnected = ble_library.BlePort.NO_CONNECTED;
                                                    peripheralManualDisconnection = false;

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
                                                        ContentView_DeviceList.IsEnabled = true;

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
                                                ContentView_DeviceList.IsEnabled = true;

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
             //   }
            //});
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

                            // call your method to check for notifications here
                            FormsApp.ble_interface.Open(peripheral, true);
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
                int num = BitConverter.ToInt32(byte_aux, 0);
                s += num.ToString("0000");
            }
            catch (Exception )
            {
                s = BitConverter.ToString(id);
            }
            return s;
        }

        private void OnError ()
        {
            Task.Run(() =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    Error error = Errors.LastError;
                
                    ContentView_Scripting_textScript.Text = "Error code: " + error.Id + "\n" + error.Message;

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        backdark_bg.IsVisible = false;
                        indicator.IsVisible = false;
                        ContentView_Scripting.IsEnabled = true;
                        _userTapped = false;
                        ContentView_Scripting_label_read.Text = "Script Execution Error";
                    
                        //var a = HttpUtility.UrlEncode ( error.Message );
                        //var b = HttpUtility.UrlPathEncode ( error.Message );
                    
                        Device.OpenUri(new Uri(resultCallback +
                                            "?" +
                                            "status=error" +
                                            "&code= " + error.Id +
                                            "&message=" + HttpUtility.UrlEncode ( error.Message )));

                        FormsApp.ble_interface.Close();
                    });
                });
            });
        }

        private void onStepFinish(object sender, int step, MTUComm.Action.ActionFinishArgs e)
        {
            Console.WriteLine("HI FINISH RUNNER");
            //throw new NotImplementedException();
        }

        private void OnProgress ( object sender, MTUComm.Action.ActionProgressArgs e )
        {
            string mensaje = e.Message;

            Device.BeginInvokeOnMainThread(() =>
            {
                if ( ! string.IsNullOrEmpty ( mensaje ) )
                    ContentView_Scripting_label_read.Text = mensaje;
            });
        }

        private void OnFinish(object sender, MTUComm.Action.ActionFinishArgs e)
        {
            Task.Run(() =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    if ( e.FormLog == null )
                    {
                        ContentView_Scripting_textScript.Text = "";
                        Parameter[] allParams = e.Result.getParameters();
    
                        for (int k = 0; k < allParams.Length; k++)
                        {
                            String res = allParams[k].getLogDisplay() + ": " + allParams[k].Value;
                            String val = ContentView_Scripting_textScript.Text;
                            ContentView_Scripting_textScript.Text = val + res + "\r\n";
                        }
    
                        ActionResult[] allports = e.Result.getPorts();
    
                        for (int i = 0; i < allports.Length; i++)
                        {
                            ActionResult actionResult = allports[i];
                            Parameter[] portParams = actionResult.getParameters();
    
                            for (int j = 0; j < portParams.Length; j++)
                            {
                                String res = portParams[j].getLogDisplay() + ": " + portParams[j].Value;
                                String val = ContentView_Scripting_textScript.Text;
                                ContentView_Scripting_textScript.Text = val + res + "\r\n";
                            }
                        }
                    }
                    else
                        ContentView_Scripting_textScript.Text = e.FormLog.ToString ();

                    String xmlResultTocallback = string.Empty;
                    if ( e.FormLog == null )
                         xmlResultTocallback = ((MTUComm.Action)sender).GetResultXML(e.Result);
                    else xmlResultTocallback = ContentView_Scripting_textScript.Text;

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        backdark_bg.IsVisible = false;
                        indicator.IsVisible = false;
                        ContentView_Scripting.IsEnabled = true;
                        _userTapped = false;
                        ContentView_Scripting_label_read.Text = "Successful Script Execution";

                        Xamarin.Forms.Device.OpenUri(new Uri(resultCallback + "?" +
                                                             "status=success" +
                                                             "&output_filename="+resultScriptName +
                                                             "&output_data=" + System.Web.HttpUtility.UrlEncode(
                                                                Base64Encode( xmlResultTocallback ) ) ));
                        FormsApp.ble_interface.Close();
                        
                        System.Diagnostics.Process.GetCurrentProcess().Kill();
                    });
                });
            });
        }

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

        private void TappedListeners()
        {
            ContentView_Scripting_bg_read_mtu_button_img.Scale = 0;
        }

        private async void PickFilesCommandHandler()
        {
            string resultMsg = "Successful MTU read";


            Task.Delay(100).ContinueWith(t =>
            Device.BeginInvokeOnMainThread(() =>
            {
                ContentView_Scripting_label_read.Text = resultMsg;
                _userTapped = false;
                ContentView_Scripting_bg_read_mtu_button.NumberOfTapsRequired = 1;
                backdark_bg.IsVisible = false;
                indicator.IsVisible = false;
                ContentView_Scripting.IsEnabled = true;


            }));


        }

        private void LoadPhoneUI()
        {
            ContentView_Scripting.Margin = new Thickness(0, 0, 0, 0);
            ContentView_Scripting_hamburger_icon.IsVisible = true;

        }

        private void LoadTabletUI()
        {

            ContentView_Scripting.Opacity = 1;

            ContentView_Scripting_hamburger_icon.IsVisible = false;
            ContentView_Scripting.Margin = new Thickness(0, 0, 0, 0);



            ContentView_Scripting_aclara_logo.Scale = 1.2;
            ContentView_Scripting_aclara_logo.TranslationX = 42;
            ContentView_Scripting_hamburger_icon.TranslationX = 42;
        }

        private void ReplaceMeterCancelTapped(object sender, EventArgs e)
        {
            dialog_open_bg.IsVisible = false;
            turnoff_mtu_background.IsVisible = false;
        }

        private void ReplaceMeterOkTapped(object sender, EventArgs e)
        {
            dialog_replacemeter_one.IsVisible = false;
            dialog_open_bg.IsVisible = false;
            turnoff_mtu_background.IsVisible = false;
            ////Application.Current.MainPage.Navigation.PushAsync(new AclaraViewReplaceMTU(dialogsSaved), false);
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

            Task.Run(async () =>
            {
                await Task.Delay(2000); Device.BeginInvokeOnMainThread(() =>
                {
                    dialog_turnoff_two.IsVisible = false;
                    dialog_turnoff_three.IsVisible = true;
                });
            });
        }

        private void MeterCancelTapped(object sender, EventArgs e)
        {
            dialog_open_bg.IsVisible = false;
            dialog_meter_replace_one.IsVisible = false;
            turnoff_mtu_background.IsVisible = false;
        }

        private void MeterOkTapped(object sender, EventArgs e)
        {
            dialog_meter_replace_one.IsVisible = false;
            dialog_open_bg.IsVisible = false;
            turnoff_mtu_background.IsVisible = false;
            ////Application.Current.MainPage.Navigation.PushAsync(new AclaraViewReplaceMeter(dialogsSaved), false);
        }

        private async void LogoutAsync(object sender, EventArgs e)
        {
            Settings.IsLoggedIn = false;
            FormsApp.credentialsService.DeleteCredentials();
            FormsApp.peripheral = null;
            int contador = Navigation.NavigationStack.Count;
            while (contador > 0)
            {
                try
                {
                    await Navigation.PopAsync(false);
                }
                catch (Exception v)
                {
                    Console.WriteLine(v.StackTrace);
                }
                contador--;
            }

            try
            {
                await Navigation.PopToRootAsync(false);

            }
            catch (Exception v)
            {
                Console.WriteLine(v.StackTrace);
            }
        }

        private void OnItemSelected(Object sender, SelectedItemChangedEventArgs e)
        {
            ((ListView)sender).SelectedItem = null;
        }

        private IBlePeripheral peripheral = null;
        private int peripheralConnected = ble_library.BlePort.NO_CONNECTED;
        private Boolean peripheralManualDisconnection = false;

        // Event for Menu Item selection, here we are going to handle navigation based
        // on user selection in menu ListView
        private void OnMenuItemSelectedListDevices(object sender, ItemTappedEventArgs e)
        {
            PrintToConsole($"-------------------------------       OnMenuItemSelectedListDevices, thread: { Thread.CurrentThread.ManagedThreadId}");
            try
            {
                printer.Resume();
            }
            catch (Exception e5)
            {
                Console.WriteLine(e5.StackTrace);
            }
           

            var item = (DeviceItem)e.Item;
            //fondo.Opacity = 0;
            ContentView_DeviceList.Opacity = 0.5;
            ContentView_DeviceList.IsEnabled = false;

            Device.BeginInvokeOnMainThread(() =>
            {

                #region New Circular Progress bar Animations    

                DeviceList.IsRefreshing = false;
                backdark_bg.IsVisible = true;
                indicator.IsVisible = true;
                ContentView_DeviceList.IsEnabled = false;
                #endregion
            });

            bool reassociate = false;
            if (CrossSettings.Current.GetValueOrDefault("session_dynamicpass", string.Empty) != string.Empty &&
                FormsApp.credentialsService.UserName.Equals(CrossSettings.Current.GetValueOrDefault("session_username", string.Empty)) &&
                System.Convert.ToBase64String(item.Peripheral.Advertisement.ManufacturerSpecificData.ElementAt(0).Data.Take(4).ToArray()).Equals(CrossSettings.Current.GetValueOrDefault("session_peripheral_DeviceId", string.Empty)) &&
                item.Peripheral.Advertisement.DeviceName.Equals(CrossSettings.Current.GetValueOrDefault("session_peripheral", string.Empty)))
            {
                reassociate = true;
            }

            FormsApp.ble_interface.Open(item.Peripheral, reassociate);
            peripheral = item.Peripheral;

            Device.BeginInvokeOnMainThread(() =>
            {
                ContentView_Scripting_battery_level.Source = item.deviceBatteryIcon + "_white";
                ContentView_Scripting_rssi_level.Source = item.deviceRssiIcon + "_white";
            });


            /*
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

            */
        }


        public void PrintToConsole(string printConsole)
        {

            if(DEBUG_MODE_ON)
                Console.WriteLine("DEBUG_ACL: " + printConsole);
        }

    }
}
