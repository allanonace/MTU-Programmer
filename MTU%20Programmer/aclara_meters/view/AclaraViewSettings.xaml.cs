// Copyright M. Griffie <nexus@nexussays.com>
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Acr.UserDialogs;
using aclara_meters.Helpers;
using aclara_meters.Models;
using Xamarin.Forms;
using aclara.ViewModels;
using Plugin.Settings;
using MTUComm;
using Xamarin.Essentials;

using ActionType = MTUComm.Action.ActionType;

namespace aclara_meters.view
{
    public partial class AclaraViewSettings
    {
        private const string TEXT_COPYR   = "Copyright © 2018 Aclara Technologies LLC.";
        private const string TEXT_SUPPORT = "System tech Support: 1-866-205-5058";
        private const string TEXT_VERSION = "Application Version: 1.36.3";
        private const string TEXT_LICENSE = "Licensed to: ";

        private ActionType actionType;
        private IUserDialogs dialogsSaved;
        private TabLogViewModel _viewModelread;
        private List<PageItem> MenuList { get; set; }

        public AclaraViewSettings()
        {
            InitializeComponent();
        }

        private void LoadMTUView()
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

        readonly bool notConnected;

        public AclaraViewSettings(bool notConnected)
        {
            this.notConnected = notConnected;
            InitializeComponent();
            //Settings.IsNotConnectedInSettings = true;
            if (Device.Idiom == TargetIdiom.Tablet)
            {
                Task.Run(() =>
                {
                    Device.BeginInvokeOnMainThread(LoadTabletUIConnected);
                });
            }
            else
            {
                Task.Run(() =>
                {
                    Device.BeginInvokeOnMainThread(LoadPhoneUIConnected);
                });
            }

            NavigationPage.SetHasNavigationBar(this, false); //Turn off the Navigation bar
            LoadMTUView();
            navigationDrawerList.IsEnabled = false;
            navigationDrawerList.Opacity = 0.65;

            //Change username textview to Prefs. String
            if (FormsApp.credentialsService.UserName != null)
            {
                userName.Text = FormsApp.credentialsService.UserName;
            }

            // portrait
            Task.Run(async () =>
            {
                await Task.Delay(100); 
                Device.BeginInvokeOnMainThread(() =>
                {
                    _viewModelread = new TabLogViewModel();
                    BindingContext = _viewModelread;
                    Task.WaitAll(_viewModelread.LoadData());
                });
           });

            ButtonListeners();
            InitLayout(1); 
            Task.Run(() =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {

                    backdark_bg.IsVisible = false;
                    indicator.IsVisible = false;
                    battery_level.Opacity = 0;
                    rssi_level.Opacity = 0;
                });
            });
        }


        public AclaraViewSettings(IUserDialogs dialogs)
        {
            InitializeComponent();
            if (Device.Idiom == TargetIdiom.Tablet)
            {
                Task.Run(() =>
                {
                    Device.BeginInvokeOnMainThread(LoadTabletUIConnected);
                });
            }
            else
            {
                Task.Run(() =>
                {
                    Device.BeginInvokeOnMainThread(LoadPhoneUIConnected);
                });
            }

            dialogsSaved = dialogs;
            LoadMTUView();
            NavigationPage.SetHasNavigationBar(this, false); //Turn off the Navigation bar

            //Change username textview to Prefs. String
            if (FormsApp.credentialsService.UserName != null)
            {
                userName.Text = FormsApp.credentialsService.UserName;
            }

            ButtonListeners();
            InitLayout(1);

            // portrait
            Task.Run(async () =>
            {
                await Task.Delay(100); 
                Device.BeginInvokeOnMainThread(() =>
                {
                    _viewModelread = new TabLogViewModel();
                    BindingContext = _viewModelread;
                    Task.WaitAll(_viewModelread.LoadData());
                });
            });

            Task.Run(() =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    backdark_bg.IsVisible = false;
                    indicator.IsVisible = false;
                });
            });


            battery_level.Source = CrossSettings.Current.GetValueOrDefault("battery_icon_topbar", "battery_toolbar_high_white");
            rssi_level.Source = CrossSettings.Current.GetValueOrDefault("rssi_icon_topbar", "rssi_toolbar_high_white");

        }

        private void LoadPhoneUIConnected()
        {
            background_scan_page.Margin = new Thickness(0, 0, 0, 0);
            close_menu_icon.Opacity = 1;
            hamburger_icon.IsVisible = true;
            sync_block.Scale = 0.9;
            logs_block.Scale = 1;
            about_block.Scale = 1;
            tablet_user_view.TranslationY = 0;
            tablet_user_view.Scale = 1;
            logo_tablet_aclara.Opacity = 1;
        }

        private void LoadTabletUIConnected()
        {
            ContentNav.IsVisible = true;
            background_scan_page.Opacity = 1;
            close_menu_icon.Opacity = 0;
            hamburger_icon.IsVisible = true;
            background_scan_page.Margin = new Thickness(310, 0, 0, 0);
            sync_block.Scale = 1;
            logs_block.Scale = 1;
            about_block.Scale = 1;
            tablet_user_view.TranslationY = -22;
            tablet_user_view.Scale = 1.2;
            logo_tablet_aclara.Opacity = 0;
            lowerbar.TranslationX = 310;
            lowerbar.Margin = new Thickness(0, 0, 310, 0);
            shadoweffect.IsVisible = true;
            aclara_logo.Scale = 1.2;
            aclara_logo.TranslationX = 42;
            aclara_logo.TranslationX = 42;

            shadoweffect.Source = "shadow_effect_tablet";
        }

        private void DoBasicRead()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Task.Factory.StartNew(BasicReadThread);
            });
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

        private void TurnOffMTUOKTapped(object sender, EventArgs e)
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


        private async void LogoutTapped(object sender, EventArgs e)
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

            /*
            Settings.IsLoggedIn = false;
            FormsApp.credentialsService.DeleteCredentials();

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
            catch (Exception v1)
            {
                Console.WriteLine(v1.StackTrace);
            }
            */

        }

        private void ReturnToMainView(object sender, EventArgs e)
        {
            Navigation.PopToRootAsync(false);
        }

        private void OnItemSelected(Object sender, SelectedItemChangedEventArgs e)
        {
            ((ListView)sender).SelectedItem = null;
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
                 // MRA   la pagina de settings tiene como actiontype ReadMtu
                 //   if (this.actionType != page)
                 //   {
                        this.actionType = page;
                        NavigationController(page);
                 //   }
                   
                   

                }
                catch (Exception w2)
                {
                    Console.WriteLine(w2.StackTrace);
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

                    #region Read Mtu Controller

                    background_scan_page.Opacity = 1;

                    background_scan_page.IsEnabled = true;

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

                case ActionType.AddMtu:

                    #region Add Mtu Controller

                    background_scan_page.Opacity = 1;

                    background_scan_page.IsEnabled = true;


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

                    background_scan_page.IsEnabled = true;

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

                    background_scan_page.IsEnabled = true;

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

                    background_scan_page.IsEnabled = true;

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

                    background_scan_page.IsEnabled = true;

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

                    background_scan_page.IsEnabled = true;

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

                    background_scan_page.IsEnabled = true;

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

                    background_scan_page.IsEnabled = true;

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


        void BasicReadThread()
        {
            MTUComm.Action basicRead = new MTUComm.Action(
               config: FormsApp.config,
               serial: FormsApp.ble_interface,
               type: MTUComm.Action.ActionType.BasicRead,
               user: FormsApp.credentialsService.UserName);

            basicRead.OnFinish += ((s, e) =>
            {
                Task.Delay(100).ContinueWith(t =>
                    Device.BeginInvokeOnMainThread(() =>
                    {

                        Application.Current.MainPage.Navigation.PushAsync(new AclaraViewAddMTU(dialogsSaved,  this.actionType), false);

                        #region New Circular Progress bar Animations    

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

                backdark_bg.IsVisible = true;
                indicator.IsVisible = true;
                background_scan_page.IsEnabled = false;

                #endregion

            });

            basicRead.Run();

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


        private void OnMenuCaseReplaceMeter()
        {
            background_scan_page.Opacity = 1;
            background_scan_page.IsEnabled = true;

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
                dialog_meter_replace_one.IsVisible = true;
                background_scan_page.Opacity = 1;

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
            }));
        }

        private void OnMenuCaseReplaceMTU()
        {
            background_scan_page.Opacity = 1;
            background_scan_page.IsEnabled = true;

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
                dialog_replacemeter_one.IsVisible = true;
                background_scan_page.Opacity = 1;

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
            }));
        }

        private void OnMenuCaseTurnOff()
        {
            background_scan_page.Opacity = 1;
            background_scan_page.IsEnabled = true;

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
                dialog_turnoff_one.IsVisible = true;
                dialog_turnoff_two.IsVisible = false;
                dialog_turnoff_three.IsVisible = false;
                dialog_replacemeter_one.IsVisible = false;
                background_scan_page.Opacity = 1;

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
            }));
        }

        private void OnMenuCaseAddMTU()
        {
            background_scan_page.Opacity = 1;
            background_scan_page.IsEnabled = true;

            if (Device.Idiom == TargetIdiom.Phone)
            {
                ContentNav.TranslateTo(-310, 0, 175, Easing.SinOut);
                shadoweffect.TranslateTo(-310, 0, 175, Easing.SinOut);
            }

            Task.Delay(200).ContinueWith(t =>
            Device.BeginInvokeOnMainThread(() =>
            {
                navigationDrawerList.SelectedItem = null;
                Application.Current.MainPage.Navigation.PushAsync(new AclaraViewAddMTU(dialogsSaved, ActionType.AddMtu), false);
                background_scan_page.Opacity = 1;

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
            }));
        }

        private void OnMenuCaseReadMTU()
        {
            background_scan_page.Opacity = 1;
            background_scan_page.IsEnabled = true;

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
            }));

        }

        private void ForceSyncButtonTapped(object sender, EventArgs e)
        {
            force_sync.IsEnabled = false;
            backdark_bg.IsVisible = true;
            indicator.IsVisible = true;

            if (FormsApp.config.global.UploadPrompt)
            {
                #region Show Upload prompt

                GenericUtilsClass.UploadFilesTaskSettings();

                #endregion
            }

            Task.Delay(100).ContinueWith(t =>
            Device.BeginInvokeOnMainThread(() =>
            {
                String myDate = DateTime.Now.ToString();
                date_sync.Text = myDate;
                updated_files.Text = "1456";
                pending_files.Text = "23";
                force_sync.IsEnabled = true;
                backdark_bg.IsVisible = false;
                indicator.IsVisible = false;
            }));

                                 
        }

        private void ButtonListeners()
        {
            about_button_pressed.Tapped += AboutButtonTapped;
            logs_button_pressed.Tapped += LogsButtonTapped;
            sync_button_pressed.Tapped += SyncButtonTapped;
            force_sync.Clicked += ForceSyncButtonTapped;
            back_button.Tapped += ReturnToMainView;
            logout_button.Tapped += LogoutTapped;
            turnoffmtu_ok.Tapped += TurnOffMTUOKTapped;
            turnoffmtu_no.Tapped += TurnOffMTUNoTapped;
            turnoffmtu_ok_close.Tapped += TurnOffMTUCloseTapped;
            replacemeter_ok.Tapped += ReplaceMtuOkTapped;
            replacemeter_cancel.Tapped += ReplaceMtuCancelTapped;
            meter_ok.Tapped += MeterOkTapped;
            meter_cancel.Tapped += MeterCancelTapped;


            logoff_no.Tapped += LogOffNoTapped;
            logoff_ok.Tapped += LogOffOkTapped;


            dialog_AddMTUAddMeter_ok.Tapped += dialog_AddMTUAddMeter_okTapped;
            dialog_AddMTUAddMeter_cancel.Tapped += dialog_AddMTUAddMeter_cancelTapped;

            dialog_AddMTUReplaceMeter_ok.Tapped += dialog_AddMTUReplaceMeter_okTapped;
            dialog_AddMTUReplaceMeter_cancel.Tapped += dialog_AddMTUReplaceMeter_cancelTapped;

            dialog_ReplaceMTUReplaceMeter_ok.Tapped += dialog_ReplaceMTUReplaceMeter_okTapped;
            dialog_ReplaceMTUReplaceMeter_cancel.Tapped += dialog_ReplaceMTUReplaceMeter_cancelTapped;


            dialog_AddMTU_ok.Tapped += dialog_AddMTU_okTapped;
            dialog_AddMTU_cancel.Tapped += dialog_AddMTU_cancelTapped;

        }


        private void TapToHome_Tabletmode(object sender, EventArgs e)
        {
            Navigation.PopToRootAsync(false);
            //int contador = Navigation.NavigationStack.Count;

            //while (contador > 2)
            //{
            //    try
            //    {
            //        Navigation.PopAsync(false);
            //    }
            //    catch (Exception v)
            //    {
            //        Console.WriteLine(v.StackTrace);
            //    }
            //    contador--;
            //}


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

            Settings.IsLoggedIn = false;
            FormsApp.credentialsService.DeleteCredentials();
            FormsApp.peripheral = null;
            FormsApp.ble_interface.Close();
            background_scan_page.IsEnabled = true;

            Application.Current.MainPage = new NavigationPage(new AclaraViewLogin(dialogsSaved));
            //Navigation.PopToRootAsync(false);

        }

        private void LogOffNoTapped(object sender, EventArgs e)
        {
            dialog_logoff.IsVisible = false;
            dialog_open_bg.IsVisible = false;
            turnoff_mtu_background.IsVisible = false;
        }



        private void AboutButtonTapped(object sender, EventArgs e)
        {
            InitLayout(1);
        }

        private void LogsButtonTapped(object sender, EventArgs e)
        {
            InitLayout(2);
        }

        private void SyncButtonTapped(object sender, EventArgs e)
        {
            InitLayout(3);
        }

        private void InitLayout(int valor)
        {
            #region Customer name

            customers_copyr  .Text = TEXT_COPYR;
            customers_support.Text = TEXT_SUPPORT;
            customers_version.Text = TEXT_VERSION;
            /*
            #if __IOS__
            customers_version.Text = TEXT_VERSION + NSBundle.MainBundle
                                     .ObjectForInfoDictionary ( "CFBundleShortVersionString" ).ToString ();
            #elif __ANDROID__
            customers_version.Text = TEXT_VERSION + NSBundle.MainBundle
                                     .ObjectForInfoDictionary ( "CFBundleShortVersionString" ).ToString ();
            #endif
            */
            customers_name   .Text = TEXT_LICENSE + FormsApp.config.global.CustomerName;

            #endregion


            switch (valor)
            {
                case 1:
                    about_block.Opacity = 0;
                    logs_block.Opacity = 0;
                    sync_block.Opacity = 0;

                    about_block.IsVisible = true; logs_block.IsVisible = false; sync_block.IsVisible = false;
                    about_block.IsEnabled = true; logs_block.IsEnabled = false; sync_block.IsEnabled = false;
                    about_button_text.Opacity = 1; about_button.Opacity = 1;
                    logs_button_text.Opacity = 0.5; logs_button.Opacity = 0.5;
                    sync_button_text.Opacity = 0.5; sync_button.Opacity = 0.5;
                    title_text.Text = "About";

                    about_block.FadeTo(1, 200);

                    break;

                case 2:

                    about_block.Opacity = 0;
                    logs_block.Opacity = 0;
                    sync_block.Opacity = 0;


                    about_block.IsVisible = false; logs_block.IsVisible = true; sync_block.IsVisible = false;
                    about_block.IsEnabled = false; logs_block.IsEnabled = true; sync_block.IsEnabled = false;
                    about_button_text.Opacity = 0.5; about_button.Opacity = 0.5;
                    logs_button_text.Opacity = 1; logs_button.Opacity = 1;
                    sync_button_text.Opacity = 0.5; sync_button.Opacity = 0.5;
                    title_text.Text = "Activity Logs";

                    logs_block.FadeTo(1, 200);

                    break;

                case 3:

                    about_block.Opacity = 0;
                    logs_block.Opacity = 0;
                    sync_block.Opacity = 0;

                    about_block.IsVisible = false; logs_block.IsVisible = false; sync_block.IsVisible = true;
                    about_block.IsEnabled = false; logs_block.IsEnabled = false; sync_block.IsEnabled = true;
                    about_button_text.Opacity = 0.5; about_button.Opacity = 0.5;
                    logs_button_text.Opacity = 0.5; logs_button.Opacity = 0.5;
                    sync_button_text.Opacity = 1; sync_button.Opacity = 1;
                    title_text.Text = "File Syncronization";

                    sync_block.FadeTo(1, 200);

                    break;
            }
        }
    }
}
