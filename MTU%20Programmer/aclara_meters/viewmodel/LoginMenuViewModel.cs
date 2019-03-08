using System;
using System.Windows.Input;
using Xamarin.Forms;
using aclara_meters.Models;
using aclara_meters.Helpers;
using aclara_meters.view;
using Acr.UserDialogs;
using System.Threading.Tasks;

using MTUComm;
using System.Linq;
using System.Collections.Generic;

namespace aclara_meters.viewmodel
{
    public class LoginMenuViewModel : ViewModelBase
    {
        #region Commands
        public INavigation Navigation { get; set; }
        public ICommand LoginCommand { get; set; }
        public ICommand LoadCommand { get; set; }
        #endregion

        #region Properties
        private User _user = new User();

        public User User
        {
            get { return _user; }
            set { SetProperty(ref _user, value); }
        }

        private string _message;

        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }
        #endregion

        IUserDialogs dialogs_save;

        public LoginMenuViewModel(IUserDialogs dialogs)
        {
            dialogs_save = dialogs;
            LoginCommand = new Command(Login);
            LoadCommand = new Command(Load);
            Task.Run(async () =>
            {
                await Task.Delay(550); Device.BeginInvokeOnMainThread(() =>
                {
                    LoadCommand.Execute(null);
                });
            });

        }

        public void Load()
        {
            if (FormsApp.credentialsService.DoCredentialsExist())
            {

                FormsApp.loggger.logLogin(FormsApp.credentialsService.UserName);
                //Application.Current.MainPage.Navigation.PushAsync(new AclaraViewMainMenu(dialogs_save),false);
                Application.Current.MainPage=new NavigationPage(new AclaraViewMainMenu(dialogs_save));
                //Application.Current.MainPage.Navigation.PushAsync(new AclaraViewGlobalUIController(), false);
            }   
        }

        bool AreCredentialsCorrect(string username, string password)
        {
            /*
            string testData = @"<Users Encrypted=""true""> <user> <name>INSTALL1</name> <pass>6A6C60B602D435E7</pass> </user> <user> <name>INSTALL2</name> <pass>6A6C60B602D435E7</pass> </user> <user> <name>Bob</name> <pass>9BFA1831B2529666F9DF84C0136247AD</pass> </user> <user> <name>test</name> <pass>test</pass> </user> </Users>";

            XmlSerializer serializer = new XmlSerializer(typeof(XML.XmlElementList.Users));
            // testData is your xml string
            using (TextReader reader = new StringReader(testData))
            {
                //Configuration result = (Configuration)serializer.Deserialize(reader);

                XML.XmlElementList.Users result =  (XML.XmlElementList.Users)serializer.Deserialize(reader);
            }

            */

            // string[] arr = XDocument.Load(@"User.xml").Descendants("Users").Select(element => element.Value).ToArray();
            // Path where the file should be saved once downloaded (locally)
            //string pathLocalFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "User.txt");
            //string[] arr = XDocument.Load(pathLocalFile).Descendants("Users").Select(element => element.Value).ToArray();

            /*

            XmlSerializer serializer = new XmlSerializer(typeof(XML.XmlElementList.Users));
            
            // testData is your xml string
            using (TextReader reader = new StringReader(XDocument.Load(pathLocalFile).ToString()))
            {
                //Configuration result = (Configuration)serializer.Deserialize(reader);
                XDocument xd = XDocument.Load(reader);
                String jsonresp = xd.Root;
                XML.XmlElementList.Users users = JsonConvert.DeserializeObject<XML.XmlElementList.Users>


                XML.XmlElementList.Users result = (XML.XmlElementList.Users)serializer.Deserialize(reader);
            }
            */
            
            Xml.User[] dbUsers = Configuration.GetInstance ().users;
            
            IEnumerable<Xml.User> coincidences = dbUsers.Where ( user => user.Name.Equals ( username ) );
            if ( coincidences.Count () == 1 )
            {
                Xml.User dbUser = coincidences.ToList<Xml.User> ()[ 0 ];
                
                string dbPassword = dbUser.Pass;
                
                if ( dbUser.Encrypted )
                {
                    // DESENCRIPTAR LA CONTRASEÑA
                    // dbPassword = ...
                }
                
                return string.Equals ( dbPassword, password );
            }
            
            return false;
        }

        public async void Login()
        {
            IsBusy = true;
            Title = string.Empty;
            try
            {
                if (User.Email != null)
                {
                    if (User.Password != null)
                    {
                        string userName = User.Email;
                        string password = User.Password;

                        #region Credentials length Validation

                        if (userName.Length < FormsApp.config.global.UserIdMinLength || userName.Length > FormsApp.config.global.UserIdMaxLength)
                        {
                            IsBusy = false;
                            Message = "The field UserName must be with a minimum length of " + FormsApp.config.global.UserIdMinLength+ " and a maximum length of " + FormsApp.config.global.UserIdMaxLength;
                            return;
                        }

                        if(password.Length < FormsApp.config.global.PasswordMinLength || password.Length > FormsApp.config.global.PasswordMaxLength)
                        {
                            IsBusy = false;
                            Message = "The field Password must be with a minimum length of " + FormsApp.config.global.PasswordMinLength + " and a maximum length of " + FormsApp.config.global.PasswordMaxLength;
                            return;
                        }

                        #endregion

                        var isValid = AreCredentialsCorrect(userName, password);

                        if (isValid)
                        {
                            bool doCredentialsExist = FormsApp.credentialsService.DoCredentialsExist();
                            if (!doCredentialsExist)
                            {
                                FormsApp.credentialsService.SaveCredentials(userName, password);
                        
                            }
                            Settings.IsLoggedIn = true;
                            Settings.SavedUserName = User.Email;

                            FormsApp.loggger.logLogin(FormsApp.credentialsService.UserName);
                            //await Application.Current.MainPage.Navigation.PushAsync(new AclaraViewMainMenu(dialogs_save), false);
                            Application.Current.MainPage = new NavigationPage(new AclaraViewMainMenu(dialogs_save));
                            //await Application.Current.MainPage.Navigation.PushAsync(new AclaraViewGlobalUIController(), false);
                        }
                        else
                        {
                            Message = "Wrong username or password";
                        }

                        IsBusy = false;
                    }
                    else
                    {
                        IsBusy = false;
                        Message = "Password required";
                    }
                }
                else
                {
                    IsBusy = false;
                    Message = "Email required";
                }

            }
            catch (Exception e)
            {
                IsBusy = false;
                await Application.Current.MainPage.DisplayAlert("Connection error", e.Message, "Ok");
            }
        }
    }
}