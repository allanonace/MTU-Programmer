using TYPE = MTUComm.Observer.Broadcaster.TYPE;

namespace MTUComm.Observer
{
    public interface IObservable
    {
        bool Subscribe        ( string eventId, IObserver observer, bool removeOnFinish = true, string sufixToUse = "" );
        void Publish          ( string eventId, TYPE eventType, params object[] parameters );
        void PublishInCascade ( string eventId, TYPE eventType, params object[] parameters );
        void Unsubscribe      ( string eventId, IObserver observer );
        void Clear            ();
    }
}
