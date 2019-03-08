using System;
using System.ComponentModel;
using nexus.protocols.ble.scan;

namespace aclara_meters.Models
{
    public class DeviceListNotify : INotifyPropertyChanged
    {
        public int Id { get; set; }

        string _deviceName;
        public string deviceName
        {
            get { return _deviceName; }
            set
            {
                if (_deviceName != value)
                    _deviceName = value;
                OnPropertyChanged("deviceName");
            }
        }

        string _deviceMacAddress;
        public string deviceMacAddress
        {
            get { return _deviceMacAddress; }
            set
            {
                if (_deviceMacAddress != value)
                    _deviceMacAddress = value;
                OnPropertyChanged("deviceMacAddress");
            }
        }

        string _deviceBatteryIcon;
        public string deviceBatteryIcon
        {
            get { return _deviceBatteryIcon; }
            set
            {
                if (_deviceBatteryIcon != value)
                    _deviceBatteryIcon = value;
                OnPropertyChanged("deviceBatteryIcon");
            }
        }

        string _deviceBattery;
        public string deviceBattery
        {
            get { return _deviceBattery; }
            set
            {
                if (_deviceBattery != value)
                    _deviceBattery = value;
                OnPropertyChanged("deviceBattery");
            }
        }

        string _deviceRssiIcon;
        public string deviceRssiIcon
        {
            get { return _deviceRssiIcon; }
            set
            {
                if (_deviceRssiIcon != value)
                    _deviceRssiIcon = value;
                OnPropertyChanged("deviceRssiIcon");
            }
        }

        string _deviceRssi;
        public string deviceRssi
        {
            get { return _deviceRssi; }
            set
            {
                if (_deviceRssi != value)
                    _deviceRssi= value;
                OnPropertyChanged("deviceRssi");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
