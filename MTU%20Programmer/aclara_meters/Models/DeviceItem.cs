using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nexus.protocols.ble.scan;

namespace aclara_meters.Models
{
    public class DeviceItem
    {
        public string deviceName { get; set; }
        public string deviceMacAddress { get; set; }
        public string deviceBattery { get; set; }
        public string deviceRssi { get; set; }
        public string deviceBatteryIcon { get; set; }
        public string deviceRssiIcon { get; set; }
        public IBlePeripheral Peripheral { get; internal set; }
    }
}

