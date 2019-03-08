using System;
using System.Collections.Generic;
using System.Linq;

// Observer pattern using broadcaster approach applying singleton
// TODO: Use reflection to allow register multiple events in the same instance/class ( ie. OnCompleted_EventId )
namespace MTUComm.Observer
{
    public class Broadcaster// : IObservable
    {
        #region Nested class

        private class Event
        {
            private const string METHOD_ID_NEXT      = "EventOnNext_";
            private const string METHOD_ID_COMPLETED = "EventOnCompleted_";
            private const string METHOD_ID_ERROR     = "EventOnError_";

            public IObserver observer                 { get; private set; }
            public bool removeOnFinish                { get; private set; }
            public Func<object[],dynamic> OnNext      { get; private set; }
            public Func<object[],dynamic> OnCompleted { get; private set; }
            public Func<object[],dynamic> OnError     { get; private set; }

            public Event ( IObserver observer, bool removeOnFinish, string sufixToUse )
            {
                this.observer       = observer;
                this.removeOnFinish = removeOnFinish;

                if ( string.IsNullOrEmpty ( sufixToUse ) )
                {
                    this.OnNext      = ( prs ) => this.observer.EventOnNext      ( prs );
                    this.OnCompleted = ( prs ) => this.observer.EventOnCompleted ( prs );
                    this.OnError     = ( prs ) => this.observer.EventOnError     ( prs );
                }
                else
                {
                    Type type = this.observer.GetType ();
                    type.GetMethod ( METHOD_ID_NEXT      + sufixToUse, new Type[] { typeof( object[] ) } );
                    type.GetMethod ( METHOD_ID_COMPLETED + sufixToUse, new Type[] { typeof( object[] ) } );
                    type.GetMethod ( METHOD_ID_ERROR     + sufixToUse, new Type[] { typeof( object[] ) } );
                }
            }
        }

        #endregion

        #region Attributes

        public enum TYPE { NEXT, COMPLETED, ERROR }

        private static Dictionary<string,List<Event>> events;

        #endregion

        #region Initialization

        static Broadcaster ()
        {
            events = new Dictionary<string,List<Event>> ();
        }

        private Broadcaster ()
        {

        }

        #endregion

        /// <summary>
        /// Subscribe an observer to an specified event
        /// </summary>
        /// <returns>The subscribe</returns>
        /// <param name="eventId">Event identifier</param>
        /// <param name="observer">Observer instance</param>
        /// <param name="removeOnFinish">If set to <c>true</c> the pair event-observer will be removed on finish</param>
        /// <param name="sufixToUse">Sufix to use to support multiple event methods in the same class</param>
        public static bool Subscribe ( string eventId, IObserver observer, bool removeOnFinish = true, string sufixToUse = "" )
        {
            // Create list for the new event
            if ( ! events.ContainsKey ( eventId ) )
                events.Add ( eventId, new List<Event> () );

            List<Event> list = events[ eventId ];

            // Search for observer and if it is not registered, not
            // allowing to register more than one time same object to same event
            if ( ! list.Any ( evnt => evnt.observer == observer ) )
            {
                list.Add ( new Event ( observer, removeOnFinish, sufixToUse ) );
                return true;
            }
            return false;
        }

        public static void Unsubscribe ( string eventId, IObserver observer )
        {
            if ( events.ContainsKey ( eventId ) )
            {
                Event evnt = null;

                try
                {
                    evnt = events[ eventId ].Single ( e => e.observer == observer );
                }
                catch ( Exception e )
                {
                    // Observer is not registered for selected event
                    return;
                }

                events[ eventId ].Remove ( evnt );
            }
        }

        public static void UnsubscribeAll ( string eventId )
        {
            if ( events.ContainsKey ( eventId ) )
                events.Remove ( eventId );
        }

        /// <summary>
        /// Invoke event method for registered observers, all at the "same" time
        /// and not taking into account if methods return some value or not
        /// </summary>
        /// <param name="eventId">Event identifier</param>
        /// <param name="eventType">Event type ( NEXT, COMPLETED and ERROR )</param>
        /// <param name="parameters">Parameters that inside each event method have to been casted</param>
        public static void Publish ( string eventId, TYPE eventType, params object[] parameters )
        {
            if ( events.ContainsKey ( eventId ) )
            {
                List<Event> list = events[ eventId ];

                // First invoke methods
                foreach ( Event evnt in list )
                {
                    switch ( eventType )
                    {
                        case TYPE.NEXT     : evnt.OnNext      ( parameters ); break;
                        case TYPE.COMPLETED: evnt.OnCompleted ( parameters ); break;
                        case TYPE.ERROR    : evnt.OnError     ( parameters ); break;
                    }
                }

                // Then remove oberservers
                if ( eventType != TYPE.NEXT )
                {
                    Event evnt;
                    for ( int i = list.Count - 1; i >= 0; i-- )
                    {
                        evnt = list[ i ];
                        if ( evnt.removeOnFinish )
                            list.RemoveAt ( i );
                    }
                }
            }
        }

        /// <summary>
        /// Invoke event method for registered observers in inverse insertion order,
        /// linking the output of one method with the input of the next
        /// </summary>
        /// <param name="eventId">Event identifier</param>
        /// <param name="eventType">Event type ( NEXT, COMPLETED and ERROR )</param>
        /// <param name="parameters">Parameters that inside each event method have to been casted</param>
        public static void PublishInCascade ( string eventId, TYPE eventType, params object[] parameters )
        {
            if ( events.ContainsKey ( eventId ) )
            {
                List<Event>   list = events[ eventId ];
                List<dynamic> ps   = new List<dynamic> ( parameters );

                // First invoke methods
                Event evnt;
                dynamic rvalue = null;
                for ( int i = list.Count - 1; i >= 0; i-- )
                {
                    evnt = list[ i ];

                    switch ( eventType )
                    {
                        case TYPE.NEXT     : rvalue = evnt.OnNext      ( ps.ToArray () ); break;
                        case TYPE.COMPLETED: rvalue = evnt.OnCompleted ( ps.ToArray () ); break;
                        case TYPE.ERROR    : rvalue = evnt.OnError     ( ps.ToArray () ); break;
                    }

                    if ( rvalue != null )
                        ps.Add ( rvalue );
                }
            }
        }

        /// <summary>
        /// Remove all registered events and observers information
        /// </summary>
        public static void Clear ()
        {
            foreach ( string key in events.Keys )
            {
                events[ key ].Clear ();
                events[ key ] = null;
            }
            events.Clear ();
            events = null;
        }
    }
}
