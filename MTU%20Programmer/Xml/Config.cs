using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Xml
{
    public class Config
    {
        public ErrorList GetErrors (string path)
        {
            ErrorList errors;
        
            try
            {
                
                XmlSerializer s = new XmlSerializer(typeof(ErrorList));
            
                using (StreamReader streamReader = new StreamReader(path))
                {
                    string fileContent = NormalizeBooleans(streamReader.ReadToEnd());
                    using (StringReader reader = new StringReader(fileContent))
                    {
                        errors = (ErrorList)s.Deserialize(reader);
                    }
                }
            }
            catch (Exception e)
            {
                throw new DemandConfLoadException("Error loading Errors file");
            }
 
            return errors;
        }
    
        public UserList GetUsers (string path)
        {
            UserList users;
        
            try
            {
                
                XmlSerializer s = new XmlSerializer(typeof(UserList));
            
                using (StreamReader streamReader = new StreamReader(path))
                {
                    string fileContent = NormalizeBooleans(streamReader.ReadToEnd());
                    using (StringReader reader = new StringReader(fileContent))
                    {
                        users = (UserList)s.Deserialize(reader);
                    }
                }
            }
            catch (Exception e)
            {
                throw new DemandConfLoadException("Error loading Users file");
            }
 
            return users;
        }
    
        public DemandConf GetDemandConf(string path)
        {
            DemandConf demandConf;
            XmlSerializer s = new XmlSerializer(typeof(DemandConf));

            try
            {
                using (StreamReader streamReader = new StreamReader(path))
                {
                    string fileContent = NormalizeBooleans(streamReader.ReadToEnd());
                    using (StringReader reader = new StringReader(fileContent))
                    {
                        demandConf = (DemandConf)s.Deserialize(reader);
                    }
                }
            }
            catch (Exception e)
            {
                throw new DemandConfLoadException("Error loading DemandConf file");
            }
 
            return demandConf;
        }

        public MeterTypes GetMeters(string path)
        {
            MeterTypes meterTypes;
            XmlSerializer s = new XmlSerializer(typeof(MeterTypes));

            try
            {
                using (StreamReader streamReader = new StreamReader(path))
                {
                    string fileContent = NormalizeBooleans(streamReader.ReadToEnd());
                    using (StringReader reader = new StringReader(fileContent))
                    {
                        meterTypes = (MeterTypes)s.Deserialize(reader);
                    }
                }
            }
            catch (Exception e)
            {
                throw new MeterLoadException("Error loading Meter file");
            }

            return meterTypes;
        }
 
        public MtuTypes GetMtu(string path)
        {
            MtuTypes mtuTypes = new MtuTypes();

            XmlSerializer s = new XmlSerializer(typeof(MtuTypes));

            try
            {
                using (StreamReader streamReader = new StreamReader(path))
                {
                    string fileContent = NormalizeBooleans(streamReader.ReadToEnd());
                    using (StringReader reader = new StringReader(fileContent))
                    {
                        mtuTypes = (MtuTypes)s.Deserialize(reader);
                    }
                }
            }
            
            catch (Exception e ) 
            {
                throw new MtuLoadException("Error loading Mtu file: " + e.Message );
            }

            return mtuTypes;
        }

        public AlarmList GetAlarms(string path)
        {
            AlarmList alarms;
            XmlSerializer s = new XmlSerializer(typeof(AlarmList));

            try
            {
                using (StreamReader streamReader = new StreamReader(path))
                {
                    string fileContent = NormalizeBooleans(streamReader.ReadToEnd());
                    using (StringReader reader = new StringReader(fileContent))
                    {
                        alarms = (AlarmList)s.Deserialize(reader);
                    }
                }
            }
            catch (Exception e)
            {
                throw new AlarmLoadException("Error loading Alarm file");
            }

            return alarms;
        }

        public InterfaceConfig GetInterfaces(string path)
        {
            InterfaceConfig interfaces;
            XmlSerializer s = new XmlSerializer(typeof(InterfaceConfig));

            try
            {
                using (StreamReader streamReader = new StreamReader(path))
                {
                    string fileContent = NormalizeBooleans(streamReader.ReadToEnd());
                    using (StringReader reader = new StringReader(fileContent))
                    {
                        interfaces = (InterfaceConfig)s.Deserialize(reader);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                throw new InterfaceLoadException("Error loading Interfaces file");
            }

            return interfaces;
        }

        public Global GetGlobal(string path)
        {
            Global global;
            XmlSerializer s = new XmlSerializer(typeof(Global));

            try
            {
                using (StreamReader streamReader = new StreamReader(path))
                {
                    string fileContent = NormalizeBooleans(streamReader.ReadToEnd());
                    using (StringReader reader = new StringReader(fileContent))
                    {
                        global = (Global)s.Deserialize(reader);
                    }
                }
            }
            catch (Exception e )
            {
                throw new GlobalLoadException("Error loading Global file");
            }

            return global;
        }

        public static string NormalizeBooleans(string input)
        {
            string       pattern = "(?:\"|>)(?i)(#)(?:\"|<)";
            List<string> words   = new List<string> () { "true", "false" };

            return Regex.Replace ( input,
                pattern.Replace ( "#", String.Join ( "|", words ) ),
                entry => entry.Value.ToLower() );
        }
    }
}
