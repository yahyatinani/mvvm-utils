using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ObservableRangeCollection
{
    public abstract class ObservableRangeCollectionBase<T> : ObservableCollection<T>
    {
        public void ReplaceRange( IEnumerable<T> range )
        {
            CheckReentrancy();

            var toAddItems = ToList( range );
            Items.Clear();

            AddAndRaiseEvents( toAddItems );
        }

        public void AddRange( IEnumerable<T> range )
        {
            CheckReentrancy();

            var toAddItems = ToList( range );

            AddAndRaiseEvents( toAddItems );
        }

        protected abstract void AddAndRaiseEvents( List<T> toAddItems );

        private static List<T> ToList( IEnumerable<T> range )
        {
            if ( range == null ) throw new NullRange();

            return range is List<T> list ? list : new List<T>( range );
        }

        public class NullRange : Exception
        {
        }
    }

    public class ObservableRangeCollection<T> : ObservableRangeCollectionBase<T>
    {
        protected override void AddAndRaiseEvents( List<T> toAddItems )
        {
            if ( IsEmpty( toAddItems ) ) return;

            var eventArgs = IsEmpty() ? ResetEventArgs() : ActionEventArgs( toAddItems );

            foreach ( var item in toAddItems ) Items.Add( item );

            RaiseEvents( eventArgs );
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
    }
}