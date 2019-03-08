using Xamarin.Forms;
using Xml;

namespace MTUComm
{
    public class PageLinker
    {
        private const string BTN_TXT = "Ok";
    
        private static PageLinker instance;
        private static Page currentPage;
    
        public static Page CurrentPage
        {
            get { return currentPage;  }
            set { currentPage = value; }
        }
    
        private PageLinker () {}
        
        private static PageLinker GetInstance ()
        {
            if ( instance == null )
                instance = new PageLinker ();
                
            return instance;
        }

        private void _ShowAlert (
            string title,
            string message,
            string btnText )
        {
            if ( currentPage != null )
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    currentPage.DisplayAlert ( title, message, btnText );
                });
            }
        }

        public static void ShowAlert (
            string title,
            string message,
            string btnText )
        {
            GetInstance ()._ShowAlert ( title, message, btnText );
        }
        
        public static void ShowAlert (
            string title,
            Error  error,
            string btnText = BTN_TXT )
        {
            if ( error.Id > -1 )
                GetInstance ()._ShowAlert (
                    title, "Error " + error.Id + ": " + error.Message, btnText );
            else
                GetInstance ()._ShowAlert (
                    title, error.Message, btnText );
        }
    }
}
