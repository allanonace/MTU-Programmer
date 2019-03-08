namespace MTUComm.Observer
{
    public interface IObserver
    {
        dynamic EventOnNext      ( params object[] parameters );
        dynamic EventOnCompleted ( params object[] parameters );
        dynamic EventOnError     ( params object[] parameters );
    }
}
