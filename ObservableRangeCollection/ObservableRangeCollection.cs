using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
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

        public void RemoveRange( IEnumerable<T> range )
        {
            CheckReentrancy();
            var toRemoveRange = ToList( range );
            if ( AreCollectionOrRangeEmpty( toRemoveRange ) ) return;

            foreach ( var item in toRemoveRange ) Items.Remove( item );

            RaiseEvents( ResetEventArgs() );
        }

        private static List<T> ToList( IEnumerable<T> range )
        {
            if ( range == null ) throw new NullRange();

            return range is List<T> list ? list : new List<T>( range );
        }

        protected static bool IsEmpty( ICollection toAddItems )
        {
            return toAddItems.Count == 0;
        }

        protected static NotifyCollectionChangedEventArgs ResetEventArgs()
        {
            return new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset );
        }

        protected void RaiseEvents( NotifyCollectionChangedEventArgs eventArgs )
        {
            OnPropertyChanged( new PropertyChangedEventArgs( nameof( Count ) ) );
            OnPropertyChanged( new PropertyChangedEventArgs( nameof( Items ) ) );
            OnCollectionChanged( eventArgs );
        }

        public void RemoveRangeWithRemove( IEnumerable<T> range )
        {
            CheckReentrancy();
            var toRemoveRange = ToList( range );
            if ( AreCollectionOrRangeEmpty( toRemoveRange ) ) return;

            var indices = new List<int>();
            for ( var i = 0; i < toRemoveRange.Count; i++ )
            {
                var index = Items.IndexOf( toRemoveRange[i] );
                if ( index < 0 )
                {
                    toRemoveRange.RemoveAt( i-- );
                    continue;
                }

                indices.Add( index );
            }

            for ( var i = 0; i < indices.Count; i++ ) Items.RemoveAt( indices[i] - i );

            RaiseEvents( RemoveEventArgs( toRemoveRange, DetermineStartingIndex( indices ) ) );
        }

        private static NotifyCollectionChangedEventArgs RemoveEventArgs( IList toRemoveRange, int startingIndex )
        {
            return new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Remove, toRemoveRange,
                startingIndex );
        }

        private bool AreCollectionOrRangeEmpty( ICollection toRemoveRange )
        {
            return IsEmpty( toRemoveRange ) || IsEmpty();
        }

        private static int DetermineStartingIndex( List<int> indices )
        {
            return IsConsecutive( indices ) ? indices[0] : -1;
        }

        private static bool IsConsecutive( List<int> indices )
        {
            indices.Sort();

            return !indices.Select( ( i, j ) => i - j ).Distinct().Skip( 1 ).Any() && !IsEmpty( indices );
        }

        protected internal abstract void ReplaceItems( List<T> items );

        protected bool IsEmpty()
        {
            return Count == 0;
        }

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

        private NotifyCollectionChangedEventArgs AddEventArgs( IList toAddItems )
        {
            const NotifyCollectionChangedAction addAction = NotifyCollectionChangedAction.Add;

            return new NotifyCollectionChangedEventArgs( addAction, toAddItems, Count );
        }
    }
}