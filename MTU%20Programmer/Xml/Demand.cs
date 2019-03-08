using System;
using System.Xml.Serialization;

namespace Xml
{
    public class Demand
    {
        [XmlAttribute("MTUType")]
        public int MTUType { get; set; }

        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlElement("BlockTime")]
        public byte BlockTime { get; set; }

        [XmlElement("IntervalTime")]
        public byte IntervalTime { get; set; }

        [XmlElement("AutoClear")]
        public byte AutoClear { get; set; }

        [XmlElement("ConfigReportInterval")]
        public byte ConfigReportInterval { get; set; }

        [XmlElement("ConfigReportItems")]
        public string ConfigReportItemsSerialize { get; set; }

        [XmlIgnore]
        public byte[] ConfigReportItems
        {
            get
            {
                byte[] configReportItems = new byte[70];

                /* Initialize to 255 */
                for (int i = 0; i < configReportItems.Length; i++)
                {
                    configReportItems[i] = 255;
                }

                string[] items = ConfigReportItemsSerialize.Split('-');

                /* Fill with read values */
                for (int i = 0; i < items.Length; i++)
                {
                    configReportItems[i] = Convert.ToByte(items[i]);
                }

                return configReportItems;
            }
        }

        [XmlElement("MtuNumLowPriorityMsg")]
        public byte MtuNumLowPriorityMsg { get; set; }

        [XmlElement("MtuPrimaryWindowInterval")]
        public int MtuPrimaryWindowInterval { get; set; }

        [XmlElement("MtuPrimaryWindowIntervalB")]
        public int MtuPrimaryWindowIntervalB { get; set; }

        [XmlElement("MtuPrimaryWindowOffset")]
        public byte MtuPrimaryWindowOffset { get; set; }

        [XmlElement("MtuWindowAStart")]
        public byte MtuWindowAStart { get; set; }

        [XmlElement("MtuWindowBStart")]
        public byte MtuWindowBStart { get; set; }

        [XmlElement("ReadRqst01Item")]
        public string ReadRqst01Item { get; set; }

        [XmlElement("ReadRqst02Item")]
        public string ReadRqst02Item { get; set; }

        [XmlElement("ReadRqst03Item")]
        public string ReadRqst03Item { get; set; }

        [XmlElement("ReadRqst04Item")]
        public string ReadRqst04Item { get; set; }

        [XmlElement("ReadRqst05Item")]
        public string ReadRqst05Item { get; set; }

        [XmlElement("ReadRqst06Item")]
        public string ReadRqst06Item { get; set; }

        [XmlElement("ReadRqst07Item")]
        public string ReadRqst07Item { get; set; }

        [XmlElement("ReadRqst08Item")]
        public string ReadRqst08Item { get; set; }

        [XmlIgnore]
        public byte[] ReadRqstItem
        {
            get
            {
                byte[] readRqstItem = new byte[80];

                /* Initialize to 255 */
                for (int i = 0; i < readRqstItem.Length; i++)
                {
                    readRqstItem[i] = 255;
                }

                /* ReadRqst01Item */
                readRqstItem[0] = (byte)(Int32.Parse(ReadRqst01Item) & 0xff);
                readRqstItem[1] = (byte)((Int32.Parse(ReadRqst01Item) >> 8) & 0xff);

                /* ReadRqst02Item */
                readRqstItem[10] = (byte)(Int32.Parse(ReadRqst02Item) & 0xff);
                readRqstItem[11] = (byte)((Int32.Parse(ReadRqst02Item) >> 8) & 0xff);

                /* ReadRqst03Item */
                readRqstItem[20] = (byte)(Int32.Parse(ReadRqst03Item) & 0xff);
                readRqstItem[21] = (byte)((Int32.Parse(ReadRqst03Item) >> 8) & 0xff);

                /* ReadRqst04Item */
                readRqstItem[30] = (byte)(Int32.Parse(ReadRqst04Item) & 0xff);
                readRqstItem[31] = (byte)((Int32.Parse(ReadRqst04Item) >> 8) & 0xff);

                /* ReadRqst05Item */
                readRqstItem[40] = (byte)(Int32.Parse(ReadRqst05Item) & 0xff);
                readRqstItem[41] = (byte)((Int32.Parse(ReadRqst05Item) >> 8) & 0xff);

                /* ReadRqst06Item */
                readRqstItem[50] = (byte)(Int32.Parse(ReadRqst06Item) & 0xff);
                readRqstItem[51] = (byte)((Int32.Parse(ReadRqst06Item) >> 8) & 0xff);

                /* ReadRqst07Item */
                readRqstItem[60] = (byte)(Int32.Parse(ReadRqst07Item) & 0xff);
                readRqstItem[61] = (byte)((Int32.Parse(ReadRqst07Item) >> 8) & 0xff);

                /* ReadRqst08Item */
                readRqstItem[70] = (byte)(Int32.Parse(ReadRqst08Item) & 0xff);
                readRqstItem[71] = (byte)((Int32.Parse(ReadRqst08Item) >> 8) & 0xff);

                return readRqstItem;
            }
        }

        [XmlElement("TrendMode")]
        public string TrendModeSerialize { get; set; }

        [XmlIgnore]
        public byte TrendMode
        {
            get
            {
                byte trendMode = 0;

                if (TrendModeSerialize.ToLower().Equals("enable"))
                {
                    trendMode = 1;
                }
                return trendMode;
            }
        }

        [XmlElement("TrendModeReadInterval")]
        public byte TrendModeReadInterval { get; set; }

        [XmlElement("TrendModeTrig1")]
        public int TrendModeTrig1 { get; set; }

        [XmlElement("TrendModeTrig2")]
        public int TrendModeTrig2 { get; set; }
    }
}
