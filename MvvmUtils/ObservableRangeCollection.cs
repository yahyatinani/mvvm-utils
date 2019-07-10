using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo( "MvvmUtils.UnitTests" )]

namespace MvvmUtils
{
    public abstract class ObservableRangeCollectionBase<T> : ObservableCollection<T>
    {
        private const NotifyCollectionChangedAction RESET_ACTION = NotifyCollectionChangedAction.Reset;
        private const NotifyCollectionChangedAction REMOVE_ACTION = NotifyCollectionChangedAction.Remove;

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
            if ( IsCollectionOrRangeEmpty( toRemoveRange ) ) return;

            foreach ( var item in toRemoveRange ) Items.Remove( item );

            RaiseEvents( ResetEventArgs() );
        }

        protected static NotifyCollectionChangedEventArgs ResetEventArgs()
        {
            return new NotifyCollectionChangedEventArgs( RESET_ACTION );
        }

        public void RemoveRangeWithRemoveAction( IEnumerable<T> range )
        {
            CheckReentrancy();

            var toRemoveRange = ToList( range );
            if ( IsCollectionOrRangeEmpty( toRemoveRange ) ) return;

            var indices = new List<int>();

            for ( var i = 0; i < toRemoveRange.Count; i++ )
            {
                var index = Items.IndexOf( toRemoveRange[i] );

                if ( index < 0 )
                    toRemoveRange.RemoveAt( i-- );
                else
                    indices.Add( index );
            }

            if ( IsEmpty( indices ) ) return;

            indices.Sort();
            for ( var i = indices.Count - 1; i >= 0; i-- ) Items.RemoveAt( indices[i] );

            RaiseEvents( RemoveEventArgs( toRemoveRange, FindStartingIndex( indices ) ) );
        }

        private static List<T> ToList( IEnumerable<T> range )
        {
            if ( range == null ) throw new NullRange();

            return range is List<T> list ? list : new List<T>( range );
        }

        private bool IsCollectionOrRangeEmpty( ICollection toRemoveRange )
        {
            return IsEmpty( toRemoveRange ) || IsEmpty();
        }

        protected static bool IsEmpty( ICollection toAddItems )
        {
            return toAddItems.Count == 0;
        }

        protected bool IsEmpty()
        {
            return Count == 0;
        }

        private static int FindStartingIndex( List<int> indices )
        {
            return Utilities.IsConsecutive( indices ) ? indices[0] : -1;
        }

        private static NotifyCollectionChangedEventArgs RemoveEventArgs( IList toRemoveRange, int startingIndex )
        {
            return new NotifyCollectionChangedEventArgs( REMOVE_ACTION, toRemoveRange, startingIndex );
        }

        protected void RaiseEvents( NotifyCollectionChangedEventArgs eventArgs )
        {
            OnPropertyChanged( new PropertyChangedEventArgs( nameof( Count ) ) );
            OnPropertyChanged( new PropertyChangedEventArgs( nameof( Items ) ) );
            OnCollectionChanged( eventArgs );
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
        private const NotifyCollectionChangedAction ADD_ACTION = NotifyCollectionChangedAction.Add;

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
            return new NotifyCollectionChangedEventArgs( ADD_ACTION, toAddItems, Count );
        }
    }
}