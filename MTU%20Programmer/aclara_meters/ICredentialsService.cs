using System;
namespace aclara_meters
{
	public interface ICredentialsService
    {
        string UserName { get; }

        string Password { get; }

        void SaveCredentials (string userName, string password);

        void DeleteCredentials ();

        bool DoCredentialsExist ();
    }
}
