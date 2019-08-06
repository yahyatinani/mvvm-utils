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
        private const string INDEXER_NAME = "Item[]";
        private const string COUNT_PROPERTY_NAME = nameof( Count );
        private const NotifyCollectionChangedAction RESET_ACTION = NotifyCollectionChangedAction.Reset;
        private const NotifyCollectionChangedAction REMOVE_ACTION = NotifyCollectionChangedAction.Remove;
        protected const NotifyCollectionChangedAction ADD_ACTION = NotifyCollectionChangedAction.Add;

        /// <exception cref="NullItem">If the given item is null.</exception>
        public void Replace( T item )
        {
            if ( item == null ) throw new NullItem();

            ClearWithoutRaisingEvents();
            Add( item );
        }

        /// <exception cref="NullRange">If the given range is null.</exception>
        public void ReplaceRange( IEnumerable<T> range )
        {
            CheckReentrancy();

            if ( range == null ) throw new NullRange();

            ClearWithoutRaisingEvents();
            AddAndRaiseEvents( ToList( range ) );
        }

        private void ClearWithoutRaisingEvents()
        {
            Items.Clear();
        }

        /// <exception cref="NullRange">If the given range is null.</exception>
        public void AddRange( IEnumerable<T> range )
        {
            CheckReentrancy();

            if ( range == null ) throw new NullRange();

            AddAndRaiseEvents( ToList( range ) );
        }

        /// <exception cref="NullRange">If the given range is null.</exception>
        public void RemoveRange( IEnumerable<T> range )
        {
            CheckReentrancy();

            if ( range == null ) throw new NullRange();
            if ( IsEmpty() ) return;

            var toRemoveRange = ToList( range );
            var isCollectionChanged = false;
            foreach ( var item in toRemoveRange ) isCollectionChanged = Items.Remove( item );

            if ( isCollectionChanged ) RaiseEvents( ResetEventArgs() );
        }

        protected static NotifyCollectionChangedEventArgs ResetEventArgs()
        {
            return new NotifyCollectionChangedEventArgs( RESET_ACTION );
        }

        /// <exception cref="NullRange">If the given range is null.</exception>
        public void RemoveRangeWithRemoveAction( IEnumerable<T> range )
        {
            CheckReentrancy();
            if ( range == null ) throw new NullRange();

            var toRemoveRange = ToList( range );
            var indices = new List<int>();
            for ( var i = 0; i < toRemoveRange.Count; i++ )
            {
                var item = toRemoveRange[i];
                var index = Items.IndexOf( item );
                if ( index < 0 )
                {
                    toRemoveRange.Remove( item );
                    i--;
                }
                else
                {
                    Items.Remove( item );
                    indices.Add( index );
                }
            }

            if ( IsEmpty( indices ) ) return;

            RaiseEvents( RemoveEventArgs( toRemoveRange, FindStartingIndex( indices ) ) );
        }

        private static List<T> ToList( IEnumerable<T> range )
        {
            return range is List<T> list ? list : new List<T>( range );
        }

        protected static bool IsEmpty( ICollection collection )
        {
            return collection.Count == 0;
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
            OnPropertyChanged( new PropertyChangedEventArgs( COUNT_PROPERTY_NAME ) );
            OnPropertyChanged( new PropertyChangedEventArgs( INDEXER_NAME ) );
            OnCollectionChanged( eventArgs );
        }

        protected internal abstract void AddAndRaiseEvents( List<T> toAddItems );

        /// <exception cref="NegativeIndex">index is less than 0.</exception>
        /// <exception cref="NegativeCount">count is less than 0.</exception>
        /// <exception cref="InvalidIndexCountRange">index and count do not denote a valid range of elements in the
        /// ObservableRangeCollection&lt;T&gt;.</exception>
        public List<T> GetRange( int index, int count )
        {
            if ( index < 0 )
                throw new NegativeIndex();

            if ( count < 0 )
                throw new NegativeCount();

            if ( index + count > Count )
                throw new InvalidIndexCountRange();

            var requestedRange = new List<T>();
            for ( var i = index; i < count; i++ ) requestedRange.Add( Items[i] );

            return requestedRange;
        }

        public class NullRange : Exception
        {
        }

        public class NullItem : Exception
        {
        }

        public class NegativeIndex : Exception
        {
        }

        public class InvalidIndexCountRange : Exception
        {
        }

        public class NegativeCount : Exception
        {
        }
    }

    public class ObservableRangeCollection<T> : ObservableRangeCollectionBase<T>
    {
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