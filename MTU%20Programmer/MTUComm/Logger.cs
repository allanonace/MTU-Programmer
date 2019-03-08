using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Xml;

namespace MTUComm
{
    public class Logger
    {
        private String abs_path = "";
        public  String fixed_name = "";
        private Configuration config;

        public Logger(Configuration config, string outFileName = "" )
        {
            this.config = config;
            fixed_name = outFileName;
            abs_path = config.GetBasePath();
        }

        private Boolean isFixedName()
        {
            if(fixed_name.Length > 0)
            {
                return true;
            }
            return false;
        }

        private String getFileName()
        {
            if (isFixedName())
            {
                return fixed_name;
            }
            else
            {
                return DateTime.Now.ToString("MMddyyyyHH")+"Log.xml";
            }
        }

        private string getBaseFileHandler()
        {
            string base_stream = "<?xml version=\"1.0\" encoding=\"ASCII\"?>";
            base_stream += "<StarSystem>";
            base_stream += "    <AppInfo>";
            base_stream += "        <AppName>" + config.getApplicationName() + "</AppName>";
            base_stream += "        <Version>" + config.GetApplicationVersion() + "</Version>";
            base_stream += "        <Date>" + DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm") + "</Date>";
            base_stream += "        <UTCOffset>" + TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).ToString() + "</UTCOffset>";
            base_stream += "        <UnitId>" + config.GetDeviceUUID() + "</UnitId>";
            base_stream += "        <AppType>" + (isFixedName() ? "Scripting" : "Interactive") + "</AppType>";
            base_stream += "    </AppInfo>";
            base_stream += "    <Message />";
            base_stream += "    <Mtus />";
            base_stream += "    <Warning />";
            base_stream += "    <Error />";
            base_stream += "</StarSystem>";
            return base_stream;
        }

        public string CreateFileIfNotExist ( bool append = true )
        {
            string file_name = getFileName();
            String filename_clean = Path.GetFileName(file_name);
            String rel_path = file_name.Replace(filename_clean, "");

            if (rel_path.Length > 1 && rel_path.StartsWith("/"))
            {
                rel_path = rel_path.Substring(1);
            }

            string full_new_path = Path.Combine(abs_path, rel_path);

            if (!Directory.Exists(full_new_path))
            {
                Directory.CreateDirectory(full_new_path);
            }
                    

            string uri = Path.Combine(full_new_path, filename_clean);

            if ( ! File.Exists ( uri ) )
            { 
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(uri, append ))
                {
                    file.WriteLine(getBaseFileHandler());
                }
            }
            else if ( ! append )
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(uri, false ))
                {
                    file.WriteLine(getBaseFileHandler());
                }
            }
            else
            {
                try
                {
                    XDocument.Load(uri);
                }
                catch ( Exception e )
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(uri, false ))
                    {
                        file.WriteLine(getBaseFileHandler());
                    }
                }
            }

            return uri;
        }

        private string getEventBaseFileHandler()
        {
            string base_stream = "<?xml version=\"1.0\" encoding=\"ASCII\"?>";
            base_stream += "<Log>";
            base_stream += "    <Transfer/>";
            base_stream += "</Log>";
            return base_stream;
        }

        public string CreateEventFileIfNotExist ( string mtu_id )
        {
            string file_name = "MTUID"+ mtu_id+ "-" + System.DateTime.Now.ToString("MMddyyyyHH") + "-" + DateTime.Now.Ticks.ToString() + "DataLog.xml"; 
            string uri = Path.Combine(abs_path, file_name);

            if (!File.Exists(uri))
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(uri, true))
                {
                    file.WriteLine(getEventBaseFileHandler());
                }
            }

            return uri;
        }

        private XElement getRootElement()
        {
            String uri = CreateFileIfNotExist();
            XDocument doc =  XDocument.Load(uri);
            XElement root = new XElement("StarSystem");

            return root;
        }

        public void addAtrribute(XElement node, String attname, String att_value)
        {
            if ( att_value != null && att_value.Length > 0 )
                node.Add(new XAttribute(attname, att_value));
        }

        public void logLogin(String username)
        {
            String uri = CreateFileIfNotExist();
            XDocument doc = XDocument.Load(uri);

            XElement error = new XElement("AppMessage");

            logParameter(error, new Parameter("Date", null, DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm:ss")));

            XElement message = new XElement("Message", "User Login: "+ username);
            error.Add(message);

            doc.Root.Element("Message").Add(error);
            doc.Save(uri);
        }

        /// <summary>
        /// Write errors in log file, using Errors singleton class
        /// that contains all catched errors since the last writting
        /// <Error>
        ///   <AppError>
        ///     <Date></Date>
        ///     <Message ErrorId="n">...</Message>
        ///   </AppError>
        /// </Error>
        /// </summary>
        public void LogError ()
        {
            String    uri    = CreateFileIfNotExist ();
            XDocument doc    = XDocument.Load ( uri );
            XElement  erNode = doc.Root.Element ( "Error" );
            string    time   = DateTime.UtcNow.ToString ( "MM/dd/yyyy HH:mm:ss" );

            foreach ( Error e in Errors.GetErrorsToLog () )
            {
                XElement error = new XElement ( "AppError" );

                logParameter(error, new Parameter ( "Date", null, time ) );
            
                XElement message = new XElement ( "Message", e.Message );
                
                if ( Errors.ShowId &&
                     e.Id > -1 )
                    addAtrribute ( message, "ErrorId", e.Id.ToString () );
                    
                if ( e.Port > 1 )
                    addAtrribute ( message, "Port", e.Port.ToString () );
                
                error.Add ( message );
                
                erNode.Add ( error );
            }

            doc.Save ( uri );
        }

        public string logReadResultString(Action ref_action, ActionResult result)
        {
            XDocument doc = XDocument.Parse(getBaseFileHandler());
            try
            {
                logReadResult(doc.Root.Element("Mtus"), ref_action, result, Int32.Parse(result.getParameterByTag("MtuType").Value));
            }
            catch ( Exception e )
            {
                logFullResult ( doc.Root.Element ( "Mtus" ), ref_action, result );
            }
            return doc.ToString();
        }

        public void logFullResult(XElement parent, Action ref_action, ActionResult result)
        {
            XElement action = new XElement("Action");

            addAtrribute(action, "display", ref_action.DisplayText);
            addAtrribute(action, "type", ref_action.LogText);
            addAtrribute(action, "reason", ref_action.Reason);

            foreach (Parameter parameter in result.getParameters())
            {
                logParameter(action, parameter, parameter.getLogTag());
            }

            parent.Add(action);
        }

        public void logReadResult(Action ref_action, ActionResult result, Mtu mtuType)
        {
            String uri = CreateFileIfNotExist();
            XDocument doc = XDocument.Load(uri);

            logReadResult(doc.Root.Element("Mtus"), ref_action, result, mtuType.Id);
            doc.Save(uri);
        }

        public void logReadDataResult(Action ref_action, ActionResult result, Mtu mtuType)
        {
            String uri = CreateFileIfNotExist();
            XDocument doc = XDocument.Load(uri);

            logReadDataResult(doc.Root.Element("Mtus"), ref_action, result, mtuType.Id);
            doc.Save(uri);
        }

        public void logReadDataResult(XElement parent, Action ref_action, ActionResult result, int mtu_type_id)
        {
            XElement action = new XElement("Action");

            addAtrribute(action, "display", ref_action.DisplayText);
            addAtrribute(action, "type", ref_action.LogText);
            addAtrribute(action, "reason", ref_action.Reason);

            InterfaceParameters[] parameters = config.getLogInterfaceFields(mtu_type_id, "DataRead");
            foreach (InterfaceParameters parameter in parameters)
            {
                if (parameter.Name == "Port")
                {

                    ActionResult[] ports = result.getPorts();
                    for (int i = 0; i < ports.Length; i++)
                    {
                        logPort(i, action, ports[i], parameter.Parameters.ToArray());
                    }
                }
                else
                {
                    logComplexParameter(action, result, parameter);
                }

            }

            parent.Add(action);
        }

        public void logReadResult(XElement parent, Action ref_action, ActionResult result, int mtu_type_id)
        {
            XElement action = new XElement("Action");

            addAtrribute(action, "display", ref_action.DisplayText);
            addAtrribute(action, "type", ref_action.LogText);
            addAtrribute(action, "reason", ref_action.Reason);

            InterfaceParameters[] parameters = config.getLogInterfaceFields(mtu_type_id, "ReadMTU");
            foreach (InterfaceParameters parameter in parameters)
            {
                if(parameter.Name == "Port") {

                    ActionResult[] ports = result.getPorts();
                    for (int i = 0; i < ports.Length; i++)
                    {
                        logPort(i, action, ports[i], parameter.Parameters.ToArray());
                    }
                }
                else
                {
                    logComplexParameter(action, result, parameter);
                }
                
            }

                /*logParameter(action, new Parameter("Date", "Date/Time", DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm:ss")));

                if (ref_action.getUser() != null)
                {

                    logParameter(action, new Parameter("User", "User", ref_action.getUser()));
                }

                foreach(Parameter parameter in result.getParameters())
                {
                    logParameter(action, parameter);
                }

                ActionResult[] ports = result.getPorts();
                for (int i= 0; i < ports.Length; i++)
                {
                    logPort(i, action, ports[i]);
                }
                */

                parent.Add(action);
        }

        public void logPort(int portnumber, XElement parent, ActionResult result, InterfaceParameters[] parameters)
        {
            XElement port = new XElement("Port");

            addAtrribute(port, "display", "Port "+ (portnumber+1).ToString());
            addAtrribute(port, "number", (portnumber + 1).ToString());

            foreach (InterfaceParameters parameter in parameters)
            {
                logComplexParameter(port, result, parameter);
            }

            parent.Add(port);
        }

        public void logTurnOffResult(Action ref_action, Mtu Mtu )
        {
            String uri = CreateFileIfNotExist();
            XDocument doc = XDocument.Load(uri);

            logTurnOffResult(doc.Root.Element("Mtus"), ref_action.DisplayText, ref_action.LogText, ref_action.user, ( uint )Mtu.Id );
            doc.Save(uri);
        }

        public void logTurnOffResult(XElement parent, string display, string type, string user, uint MtuId)
        {
            XElement action = new XElement("Action");

            addAtrribute(action, "display", display);
            addAtrribute(action, "type", type);

            logParameter(action, new Parameter("Date", "Date/Time", DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm:ss")));

            if (user != null)
            {

                logParameter(action, new Parameter("User", "User", user));
            }

            logParameter(action, new Parameter("MtuId", "MTU ID", MtuId.ToString()));

            parent.Add(action);
        }

        public void logTurnOnResult(Action ref_action, uint MtuId)
        {
            String uri = CreateFileIfNotExist();
            XDocument doc = XDocument.Load(uri);
            
            logTurnOnResult(doc.Root.Element("Mtus"), ref_action.DisplayText, ref_action.LogText, ref_action.user, MtuId);
            doc.Save(uri);
        }

        public void logTurnOnResult(XElement parent, string display, string type, string user, uint MtuId)
        {
            XElement action = new XElement("Action");

            addAtrribute(action, "display", display);
            addAtrribute(action, "type", type);

            logParameter(action, new Parameter("Date", "Date/Time", DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm:ss")));

            if (user != null)
            {

                logParameter(action, new Parameter("User", "User", user));
            }

            logParameter(action, new Parameter("MtuId", "MTU ID", MtuId.ToString()));

            parent.Add(action);
        }

        public void logCancel(Action ref_action, String cancel, String reason)
        {
            String uri = CreateFileIfNotExist();
            XDocument doc = XDocument.Load(uri);

            XElement action = new XElement("Action");

            addAtrribute(action, "display", ref_action.DisplayText);
            addAtrribute(action, "type", ref_action.LogText);
            addAtrribute(action, "reason", ref_action.Reason);

            logParameter(action, new Parameter("Date", "Date/Time", DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm:ss")));
            logParameter(action, new Parameter("User", "User", ref_action.user));

            logParameter(action, new Parameter("Cancel", "Cancel Action", cancel));
            logParameter(action, new Parameter("Reason", "Cancel Reason", reason));

            doc.Root.Element("Mtus").Add(action);
            doc.Save(uri);

        }

        public void logAction(Action ref_action)
        {
            String uri = CreateFileIfNotExist();
            XDocument doc = XDocument.Load(uri);

            logAction(doc.Root.Element("Mtus"), ref_action);
            doc.Save(uri);
    
        }

        public void logAction(XElement parent, Action ref_action)
        {

            XElement action = new XElement("Action");

            addAtrribute(action, "display", ref_action.DisplayText);
            addAtrribute(action, "type", ref_action.LogText);
            addAtrribute(action, "reason", ref_action.Reason);


            if(ref_action.user != null)
            {
                logParameter(action, new Parameter("Date", "Date/Time", DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm:ss")));
                logParameter(action, new Parameter("User", "User", ref_action.user));
            }

            Parameter[] parameters = ref_action.GetParameters();
            foreach (Parameter parameter in parameters)
            {
                if (parameter.doesGenerateLog()) {
                    logParameter(action, parameter);
                }
                
            }


            Action[] sub_actions = ref_action.GetSubActions();
            foreach(Action subaction in sub_actions)
            {
                logAction(action, subaction);
            }

            parent.Add(action);

        }

        private void logDataResultEntry(XElement events, LogDataEntry entry)
        {
            XElement read_event = new XElement("MeterReadEvent");

            addAtrribute(read_event, "FormatVersion", entry.FormatVersion.ToString());

            read_event.Add(new XElement("TimeStamp", entry.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss")));
            read_event.Add(new XElement("MeterRead", entry.MeterRead.ToString()));
            read_event.Add(new XElement("ErrorStatus", entry.ErrorStatus.ToString()));
            read_event.Add(new XElement("ReadInterval", "PT"+entry.ReadInterval.ToString()+"M"));
            read_event.Add(new XElement("PortNumber", "PORT"+(entry.PortNumber + 1 ).ToString()));
            read_event.Add(new XElement("IsDailyRead", entry.IsDailyRead.ToString()));
            read_event.Add(new XElement("IsTopOfHourRead", entry.IsTopOfHourRead.ToString()));
            read_event.Add(new XElement("ReadReason", entry.ReasonForRead.ToString()));
            read_event.Add(new XElement("IsSynchronized", entry.IsSynchronized.ToString()));


            events.Add(read_event);
        }

        public string logReadDataResultEntries(string mtu_id, DateTime start, DateTime end, List<LogDataEntry> Entries)
        {
            String path = CreateEventFileIfNotExist(mtu_id);
            XDocument doc = XDocument.Load(path);

            XElement transfer  = doc.Root.Element("Transfer");


            XElement events = new XElement("Events");

            addAtrribute(events, "FilterMode", "Match");
            addAtrribute(events, "FilterValue", "MeterRead");
            addAtrribute(events, "RangeStart", start.ToString("yyyy-MM-dd HH:mm:ss"));
            addAtrribute(events, "RangeStop", end.ToString("yyyy-MM-dd HH:mm:ss"));

            foreach (LogDataEntry entry in Entries)
            {
                logDataResultEntry(events, entry);
            }

            transfer.Add(events);

            doc.Save(path);

            return path;
        }

        public void logComplexParameter(XElement parent, ActionResult result, InterfaceParameters parameter)
        {
            Parameter param = null;

            if (parameter.Source != null)
            {
                try
                {
                    param = result.getParameterByTag(parameter.Source.Split(new char[] { '.' })[1]);
                }
                catch (Exception e) { }

            }
            if (param == null)
            {
                param = result.getParameterByTag(parameter.Name);
            }

            if (param != null)
            {
                logParameter(parent, param, parameter.Name);
            }
        }

        public void logParameter(XElement parent, Parameter parameter)
        {
            logParameter(parent, parameter, parameter.getLogTag());
        }

        public void logParameter(XElement parent, Parameter parameter, string TagName)
        {
            XElement xml_parameter = new XElement(TagName, parameter.Value );
            addAtrribute(xml_parameter, "display", parameter.getLogDisplay());
            if (parameter.Optional)
            {
                addAtrribute(xml_parameter, "option", "1");
            }
            parent.Add(xml_parameter);
        }
    }
}
