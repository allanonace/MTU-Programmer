using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MvvmHelpers;
using Xamarin.Forms;

namespace aclara.ViewModels
{
    public class RandomObject
    {
        public string Icon { get; set;  }
        public string Title { get; set;  }

        /*    Date/Time   */
        public string DateTime { get; set; }
        public string DateTimeIsVisible { get; set; }

        /*    User   */
        public string User { get; set; }
        public string UserIsVisible { get; set; }

        /*    MTU Status   */
        public string MTUStatus { get; set; }
        public string MTUStatusIsVisible { get; set; }

        /*    MTU Ser No   */
        public string MTUSerNo { get; set; }
        public string MTUSerNoIsVisible { get; set; }

        /*   1 Way Tx Freq  */
        public string OneWayTxFreq { get; set; }
        public string OneWayTxFreqIsVisible { get; set; }

        /*   2 Way Tx Freq  */
        public string TwoWayTxFreq { get; set; }
        public string TwoWayTxFreqIsVisible { get; set; }

        /*   2 Way Rx Freq  */
        public string TwoWayRxFreq { get; set; }
        public string TwoWayRxFreqIsVisible { get; set; }

        /*    Interface Tamp   */
        public string InterfaceTamp { get; set; }
        public string InterfaceTampIsVisible { get; set; }

        /*    LastGasp   */
        public string LastGasp { get; set; }
        public string LastGaspIsVisible { get; set; }

        /*    Insf. Mem   */
        public string InsfMem { get; set; }
        public string InsfMemIsVisible { get; set; }

        /*    Cut 1 Wire Tamp   */
        public string CutOneWireTamp { get; set; }
        public string CutOneWireTampIsVisible { get; set; }

        /*    Coil Error   */
        public string ErrorCoil { get; set; }
        public string ErrorCoilIsVisible { get; set; }


    }

    public class CustomSampleViewModel : BaseViewModel
    {
        private int _loadingCount = 1;

        private ICommand _selectedItemCommand;
        private ICommand _reloadItemCommand;
        private ICommand _setRandomSelectedItemCommand;
        private ICommand _leftPanelSelectCommand;

        public ICommand SelectedItemCommand => _selectedItemCommand ?? (_selectedItemCommand = new Command((selectedItem) => Select((RandomObject)selectedItem)));
        public ICommand ReloadItemCommand => _reloadItemCommand ?? (_reloadItemCommand = new Command(async () => await LoadData(true)));
        public ICommand SetRandomSelectedItemCommand => _setRandomSelectedItemCommand ?? (_setRandomSelectedItemCommand = new Command(SetRandomSelectedItem));
        public ICommand LeftPanelSelectCommand => _leftPanelSelectCommand ?? (_leftPanelSelectCommand = new Command((value) => LeftPanelSelect((string)value)));

        public ObservableRangeCollection<RandomObject> Items { get; } = new ObservableRangeCollection<RandomObject>();

        public string SelectedValue { get; set; } = "No selection";
        public object SelectedItem { get; set; }

        public async Task LoadData(bool isReloading = false)
        {
            IsBusy = true;

            if (isReloading)
                _loadingCount++;

            try
            {
                String myDate = DateTime.Now.ToString();



                var items = new List<RandomObject>
                {
                    new RandomObject{ 
                        Icon= "logs_file_button",
                        Title= "Read MTU",

                        DateTime= myDate,
                        DateTimeIsVisible= "true",

                        User= "iker",
                        UserIsVisible= "true",

                        MTUStatus= "On",
                        MTUStatusIsVisible= "true",

                        MTUSerNo= "080017715",
                        MTUSerNoIsVisible= "true",

                        OneWayTxFreq= "459.0875",
                        OneWayTxFreqIsVisible= "true",

                        TwoWayTxFreq= "467.0875",
                        TwoWayTxFreqIsVisible= "true",

                        TwoWayRxFreq= "467.0875",
                        TwoWayRxFreqIsVisible= "true",

                        InterfaceTamp= "Enabled",
                        InterfaceTampIsVisible= "true",

                        LastGasp= "Enabled",
                        LastGaspIsVisible= "true",

                        InsfMem= "Enabled",
                        InsfMemIsVisible= "true",

                        CutOneWireTamp= "Triggered",
                        CutOneWireTampIsVisible= "true",

                        ErrorCoil = "",
                        ErrorCoilIsVisible = "false"


                    },


                    new RandomObject{
						Icon= "logs_file_button",
                        Title= "Read MTU",

                        DateTime= myDate,
                        DateTimeIsVisible= "true",

                        User= "iker",
                        UserIsVisible= "true",

                        MTUStatus= "On",
                        MTUStatusIsVisible= "true",

                        MTUSerNo= "080017715",
                        MTUSerNoIsVisible= "true",

                        OneWayTxFreq= "459.0875",
                        OneWayTxFreqIsVisible= "true",

                        TwoWayTxFreq= "467.0875",
                        TwoWayTxFreqIsVisible= "true",

                        TwoWayRxFreq= "467.0875",
                        TwoWayRxFreqIsVisible= "true",

                        InterfaceTamp= "Enabled",
                        InterfaceTampIsVisible= "true",

                        LastGasp= "Enabled",
                        LastGaspIsVisible= "true",

                        InsfMem= "Enabled",
                        InsfMemIsVisible= "true",

                        CutOneWireTamp= "Triggered",
                        CutOneWireTampIsVisible= "true",


                        ErrorCoil = "",
                        ErrorCoilIsVisible = "false"
                    },


                    new RandomObject{
                        Icon= "error",
                        Title= "NO connection to the Coil",

                        DateTime= "",
                        DateTimeIsVisible= "false",

                        User= "",
                        UserIsVisible= "false",

                        MTUStatus= "",
                        MTUStatusIsVisible= "false",

                        MTUSerNo= "",
                        MTUSerNoIsVisible= "false",

                        OneWayTxFreq= "",
                        OneWayTxFreqIsVisible= "false",

                        TwoWayTxFreq= "",
                        TwoWayTxFreqIsVisible= "false",

                        TwoWayRxFreq= "",
                        TwoWayRxFreqIsVisible= "false",

                        InterfaceTamp= "",
                        InterfaceTampIsVisible= "false",

                        LastGasp= "",
                        LastGaspIsVisible= "false",

                        InsfMem= "",
                        InsfMemIsVisible= "false",

                        CutOneWireTamp= "",
                        CutOneWireTampIsVisible= "false",
                            
                        ErrorCoil = "Port1: ERROR - Check Meter",
                        ErrorCoilIsVisible = "true"
                   
                    }

                };

                if (isReloading)
                    await Task.Delay(5000);

                Items.ReplaceRange(items);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task LoadNullData(bool isReloading = false)
        {
            IsBusy = true;

            if (isReloading)
                _loadingCount++;

            if (isReloading)
                await Task.Delay(5000);

            Items.ReplaceRange(new List<RandomObject>());

            IsBusy = false;
        }


        private void Select(RandomObject selectedItem)
        {
            SelectedValue = $"Selected item { Items.IndexOf(selectedItem)}: {selectedItem.Icon} => {selectedItem.Title}";

            OnPropertyChanged(nameof(SelectedValue));
        }


        private void SetRandomSelectedItem()
        {
            var randomItem = Items.OrderBy(x => Guid.NewGuid()).FirstOrDefault();

            SelectedItem = new RandomObject
            {
                Icon = randomItem.Icon,

                Title = randomItem.Title,

                DateTime = randomItem.DateTime,
                DateTimeIsVisible = randomItem.DateTimeIsVisible,

                User = randomItem.User,
                UserIsVisible = randomItem.UserIsVisible,

                MTUStatus = randomItem.MTUStatus,
                MTUStatusIsVisible = randomItem.MTUStatusIsVisible,

                MTUSerNo = randomItem.MTUSerNo,
                MTUSerNoIsVisible = randomItem.MTUSerNoIsVisible,

                OneWayTxFreq = randomItem.OneWayTxFreq,
                OneWayTxFreqIsVisible = randomItem.OneWayTxFreqIsVisible,

                TwoWayTxFreq = randomItem.TwoWayTxFreq,
                TwoWayTxFreqIsVisible = randomItem.TwoWayTxFreqIsVisible,

                TwoWayRxFreq = randomItem.TwoWayRxFreq,
                TwoWayRxFreqIsVisible = randomItem.TwoWayRxFreqIsVisible,

                InterfaceTamp = randomItem.InterfaceTamp,
                InterfaceTampIsVisible = randomItem.InterfaceTampIsVisible,

                LastGasp = randomItem.LastGasp,
                LastGaspIsVisible = randomItem.LastGaspIsVisible,

                InsfMem = randomItem.InsfMem,
                InsfMemIsVisible = randomItem.InsfMemIsVisible,

                CutOneWireTamp = randomItem.CutOneWireTamp,
                CutOneWireTampIsVisible = randomItem.CutOneWireTampIsVisible,

                ErrorCoil =  randomItem.ErrorCoil,
                ErrorCoilIsVisible = randomItem.ErrorCoilIsVisible
                   

            };

            OnPropertyChanged(nameof(SelectedItem));
        }


        private void LeftPanelSelect(string value)
        {
            SelectedValue = $"SelectedLeftItem {value}";

            OnPropertyChanged(nameof(SelectedValue));
        }
    }
}
