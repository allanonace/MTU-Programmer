using System;
using System.Linq;
using Xamarin.Auth;

namespace aclara_meters
{
	public class CredentialsService : ICredentialsService
    {
        public string UserName
        {
            get
            {
                var account = AccountStore.Create().FindAccountsForService(FormsApp.AppName).FirstOrDefault();
                return (account != null) ? account.Username : null;
            }
        }

        public string Password
        {
            get
            {
                var account = AccountStore.Create().FindAccountsForService(FormsApp.AppName).FirstOrDefault();
                return (account != null) ? account.Properties["Password"] : null;
            }
        }

        public void SaveCredentials(string userName, string password)
        {
            if (!string.IsNullOrWhiteSpace(userName) && !string.IsNullOrWhiteSpace(password))
            {
                Account account = new Account
                {
                    Username = userName
                };
                account.Properties.Add("Password", password);
                AccountStore.Create().Save(account, FormsApp.AppName);
            }

        }

        public void DeleteCredentials()
        {
            var account = AccountStore.Create().FindAccountsForService(FormsApp.AppName).FirstOrDefault();
            if (account != null)
            {
                AccountStore.Create().Delete(account, FormsApp.AppName);
            }
        }

        public bool DoCredentialsExist()
        {
            return AccountStore.Create().FindAccountsForService(FormsApp.AppName).Any() ? true : false;
        }
    }
}
