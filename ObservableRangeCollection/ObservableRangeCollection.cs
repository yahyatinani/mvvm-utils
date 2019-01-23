using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ObservableRangeCollection
{
    public class ObservableRangeCollection<T> : ObservableCollection<T>
    {
        public void AddRange( IEnumerable<T> range )
        {
            CheckReentrancy();

            var toAddItems = ToList( range );

            if ( IsEmpty( toAddItems ) ) return;

            var eventArgs = IsEmpty() ? ResetEventArgs() : ActionEventArgs( toAddItems );

            foreach ( var item in toAddItems ) Items.Add( item );

            RaiseEvents( eventArgs );
        }

        private static List<T> ToList( IEnumerable<T> range )
        {
            if ( range == null ) throw new NullRange();

            return range is List<T> list ? list : new List<T>( range );
        }

        private static bool IsEmpty( ICollection toAddItems )
        {
            return toAddItems.Count == 0;
        }

        private bool IsEmpty()
        {
            return Count == 0;
        }

        private static NotifyCollectionChangedEventArgs ResetEventArgs()
        {
            return new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset );
        }

        private NotifyCollectionChangedEventArgs ActionEventArgs( IList toAddItems )
        {
            return new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, toAddItems,
                Count );
        }

        private void RaiseEvents( NotifyCollectionChangedEventArgs eventArgs )
        {
            OnPropertyChanged( new PropertyChangedEventArgs( nameof( Count ) ) );
            OnPropertyChanged( new PropertyChangedEventArgs( nameof( Items ) ) );
            OnCollectionChanged( eventArgs );
        }

        public class NullRange : Exception
        {
        }
    }
}