using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo( "UnitTests" )]

namespace ObservableRangeCollection
{
    public abstract class ObservableRangeCollectionBase<T> : ObservableCollection<T>
    {
        public void Replace( T item )
        {
            CheckReentrancy();

            ReplaceItems( WrapItem( item ) );
        }

        private static List<T> WrapItem( T item )
        {
            if ( item == null ) throw new NullItem();

            return new List<T> { item };
        }

        public void ReplaceRange( IEnumerable<T> range )
        {
            CheckReentrancy();

            ReplaceItems( ToList( range ) );
        }

        public void AddRange( IEnumerable<T> range )
        {
            CheckReentrancy();

            AddAndRaiseEvents( ToList( range ) );
        }

        private static List<T> ToList( IEnumerable<T> range )
        {
            if ( range == null ) throw new NullRange();

            return range is List<T> list ? list : new List<T>( range );
        }

        protected internal abstract void ReplaceItems( List<T> items );

        protected internal abstract void AddAndRaiseEvents( List<T> toAddItems );

        public class NullRange : Exception
        {
        }

        public class NullItem : Exception
        {
        }
    }

    public class ObservableRangeCollection<T> : ObservableRangeCollectionBase<T>
    {
        protected internal override void ReplaceItems( List<T> items )
        {
            Items.Clear();
            AddAndRaiseEvents( items );
        }

        protected internal override void AddAndRaiseEvents( List<T> toAddItems )
        {
            if ( IsEmpty( toAddItems ) ) return;

            var eventArgs = IsEmpty() ? ResetEventArgs() : AddEventArgs( toAddItems );

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

        private NotifyCollectionChangedEventArgs AddEventArgs( IList toAddItems )
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