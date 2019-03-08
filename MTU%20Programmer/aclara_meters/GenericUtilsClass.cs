using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MTUComm;
using Plugin.DeviceInfo;
using Renci.SshNet;
using Xamarin.Forms;

namespace aclara_meters
{
    public class GenericUtilsClass
    {

        public static async Task UploadFilesTask()
        {

            var res = await Application.Current.MainPage.DisplayAlert("Alert", "Detected pending log files. Do you want to Upload them?", "Ok", "Cancel");

            if (res)
            {
                if( GenericUtilsClass.UploadLogFiles() )
                {
                    Application.Current.MainPage.DisplayAlert("Alert", "Log files successfully uploaded", "Ok");
                }
                else
                {
                    Application.Current.MainPage.DisplayAlert("Error", "Log files could not be uploaded", "Ok");
                }
            }
        }


        public static async Task UploadFilesTaskSettings()
        {
        
            if (GenericUtilsClass.UploadLogFiles())
            {
                Application.Current.MainPage.DisplayAlert("Alert", "Log files successfully uploaded", "Ok");
            }
            else
            {
                Application.Current.MainPage.DisplayAlert("Error", "Log files could not be uploaded", "Ok");
            }
            
        }



        public static bool UploadLogFiles()
        {

            string ftp_username = FormsApp.config.global.ftpUserName;
            string ftp_password = FormsApp.config.global.ftpPassword;
            string ftp_remoteHost = FormsApp.config.global.ftpRemoteHost;
            string ftp_remotePath = FormsApp.config.global.ftpRemotePath; //For the logs...


            string host = FormsApp.config.global.ftpRemoteHost;
            string username = FormsApp.config.global.ftpUserName;
            string password = FormsApp.config.global.ftpPassword;

            //string pathRemoteFile = "/home/aclara/"; // prueba_archivo.xml";

            //TODO: UUID MOVIL EN PATH REMOTE FILE
            string pathRemoteFile = "/home/aclara" + FormsApp.config.global.ftpRemotePath + CrossDeviceInfo.Current.Id + "/"; // prueba_archivo.xml";

            // Path where the file should be saved once downloaded (locally)
            string path = Mobile.GetPath();

            //string name = "ReadMtuResult.xml";
            //string filename = Path.Combine(xml_documents, name);
            using (SftpClient sftp = new SftpClient(host, username, password))
            {
                try
                {
                    sftp.Connect();

                    if (!sftp.Exists(pathRemoteFile))
                    {
                        sftp.CreateDirectory(pathRemoteFile);
                    }
                    //TODO

                    List<string> saved_array_files = new List<string>();

                    try
                    {
                        var lines = File.ReadAllLines(Path.Combine(path, "SavedLogsList.txt"));
                        foreach (var line in lines)
                        {
                            saved_array_files.Add(line);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.StackTrace);
                    }

                    List<FileInfo> local_array_files = new List<FileInfo>();
                    DirectoryInfo info = new DirectoryInfo(path);
                    FileInfo[] files = info.GetFiles().OrderBy(p => p.LastWriteTimeUtc).ToArray();

                    foreach (FileInfo file in files)
                    {

                        if (file.Name.Contains("Log.xml") || file.Name.Contains("Result"))
                        {
                            Console.WriteLine(file.Name + " Last Write time: " + file.LastWriteTimeUtc.ToString());
                            bool enc = false;
                            foreach (string fileFtp in saved_array_files)
                            {
                                if (fileFtp.Equals(file.Name))
                                {
                                    enc = true;
                                }
                            }

                            if (!enc)
                            {

                                if (file.Name.Contains("Result"))
                                {
                                    local_array_files.Add(file);
                                }
                                else
                                {
                                    string dayfix = file.Name.Split('.')[0].Replace("Log", "");
                                    DateTime date = DateTime.ParseExact(dayfix, "MMddyyyyHH", CultureInfo.InvariantCulture).ToUniversalTime();
                                    TimeSpan diff = date - DateTime.UtcNow;
                                    int hours = (int)diff.TotalHours;
                                    if (hours < 0)
                                    {
                                        local_array_files.Add(file);
                                    }
                                }
                            }
                        }
                    }

                    if (local_array_files.Count > 0)
                    {
                        foreach (FileInfo file in local_array_files)
                        {
                            var fileStream = new FileStream(file.FullName, FileMode.Open);
                            if (fileStream != null)
                            {
                                sftp.UploadFile(fileStream, Path.Combine(pathRemoteFile, file.Name), null);
                            }
                            long cont = fileStream.Length;
                            fileStream.Close();

                            #region Create copy of deleted files to another dir

                            string url_to_copy = Path.Combine(path, "log_copies");
                            if (!Directory.Exists(url_to_copy))
                                Directory.CreateDirectory(url_to_copy);

                            File.Copy(Path.Combine(path, file.Name), Path.Combine(url_to_copy, file.Name), true);


                            #endregion

                            File.Delete(file.FullName);
                        }

                        try
                        {
                            using (TextWriter tw = new StreamWriter(Path.Combine(path, "SavedLogsList.txt")))
                            {
                                foreach (string fileFtp in saved_array_files)
                                {
                                    tw.WriteLine(fileFtp);
                                }
                                foreach (FileInfo s in local_array_files)
                                    tw.WriteLine(s.Name);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.StackTrace);
                        }
                    }


                    sftp.Disconnect();

                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("An exception has been caught " + e.ToString());
                }
            }

            return false;
        }


    }
}
