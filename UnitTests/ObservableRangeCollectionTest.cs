using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using NUnit.Framework;
using static System.Collections.Specialized.NotifyCollectionChangedAction;
using static NUnit.Framework.Assert;
using static UnitTests.ObservableRangeCollection<UnitTests.TestEntity>;

namespace UnitTests
{
    public abstract class ObservableRangeCollectionContext
    {
        private ObservableRangeCollection<TestEntity> _collection;

        [SetUp]
        public void ObservableRangeCollectionContextSetup()
        {
            _collection = new ObservableRangeCollection<TestEntity>();
        }

        private void AssertCollectionSizeIs( int expectedSize )
        {
            AreEqual( expectedSize, _collection.Count );
        }

        private void AddRange( params TestEntity[] range )
        {
            _collection.AddRange( range );
        }

        [TestFixture]
        public class ObservableRangeCollectionTest : ObservableRangeCollectionContext
        {
            private void AssertCollectionContains( TestEntity entity )
            {
                if ( entity == null ) throw new ArgumentNullException( nameof( entity ) );

                True( _collection.Contains( entity ) );
            }

            [Test]
            public void ObservableRangeCollection_ShouldBeInstanceOfObservableCollection()
            {
                IsInstanceOf<ObservableCollection<TestEntity>>( _collection );
            }

            [Test]
            public void WhenAddingNullRange_AddRangeThrowsNullRange()
            {
                Throws<NullRange>( () => AddRange( null ) );
            }

            [Test]
            public void WhenAddingEmptyRange_CollectionShouldNotChange()
            {
                AddRange();

                AssertCollectionSizeIs( 0 );
            }

            [Test]
            public void WhenRangeOneIsAdded_CollectionShouldContainRangeOne()
            {
                var one = new[] { new TestEntity() };

                AddRange( one );

                foreach ( var entity in one ) AssertCollectionContains( entity );
            }

            [Test]
            public void WhenRangeOneAndTwoAreAdded_CollectionShouldContainRangeOneAndTwo()
            {
                var one = new[] { new TestEntity() };
                var two = new[] { new TestEntity(), new TestEntity() };

                AddRange( one );
                AddRange( two );

                AssertCollectionSizeIs( 3 );
                foreach ( var entity in one ) AssertCollectionContains( entity );
                foreach ( var entity in two ) AssertCollectionContains( entity );
            }
        }

        public class ObservableRangeCollectionEventContext : ObservableRangeCollectionContext
        {
            private readonly AutoResetEvent _eventNotifier = new AutoResetEvent( false );

            [SetUp]
            public void ObservableRangeCollectionEventContextSetup()
            {
                _eventNotifier.Reset();
            }

            private void AssertAnEventRaised()
            {
                True( _eventNotifier.WaitOne( 1500 ) );
            }

            private void AssertNoEventRaised()
            {
                False( _eventNotifier.WaitOne( 1500 ) );
            }

            [TestFixture]
            public class ObservableRangeCollectionChangedEventsTest : ObservableRangeCollectionEventContext
            {
                private NotifyCollectionChangedEventArgs _eventArgs;
                private int _collectionCountWhenEventRaised;

                [SetUp]
                public void ObservableRangeCollectionEventsTestSetup()
                {
                    _eventArgs = null;
                    _collectionCountWhenEventRaised = 0;
                    _collection.CollectionChanged += OnCollectionChanged();

                    NotifyCollectionChangedEventHandler OnCollectionChanged()
                    {
                        return ( sender, args ) =>
                        {
                            _collectionCountWhenEventRaised = _collection.Count;
                            _eventArgs = args;
                            _eventNotifier.Set();
                        };
                    }
                }

                private void AssertCollectionChangedEventActionIs(
                    NotifyCollectionChangedAction collectionChangedAction )
                {
                    AreEqual( collectionChangedAction, _eventArgs.Action );
                }

                [Test]
                public void WhenAddingRangeOneToEmptyCollection_ShouldRaiseCollectionChangedWithResetAction()
                {
                    var one = new[] { new TestEntity() };

                    AddRange( one );

                    AssertAnEventRaised();
                    AssertCollectionChangedEventActionIs( Reset );
                    AssertCollectionSizeIs( _collectionCountWhenEventRaised );
                }

                [Test]
                public void WhenAddingRangeOneToNonEmptyCollection_ShouldRaiseCollectionChangedWithAddAction()
                {
                    AddRange( new TestEntity(), new TestEntity() );
                    var one = new[] { new TestEntity() };

                    AddRange( one );

                    AssertAnEventRaised();
                    AssertCollectionChangedEventActionIs( Add );
                    AssertCollectionSizeIs( _collectionCountWhenEventRaised );
                    IsNull( _eventArgs.OldItems );
                    AreEqual( 2, _eventArgs.NewStartingIndex );
                    for ( var i = 0; i < one.Length; i++ )
                        AreEqual( one[i], _eventArgs.NewItems[i] );
                }

                [Test]
                public void WhenAddingEmptyRange_NoEventsShouldBeRaised()
                {
                    AddRange();

                    AssertNoEventRaised();
                }

                [Test]
                public void WhenAddingRange_ShouldOnlyRaiseOneCollectionChangedEvent()
                {
                    var eventRaisesCount = 0;
                    _collection.CollectionChanged += ( sender, args ) =>
                    {
                        ++eventRaisesCount;

                        _eventNotifier.Set();
                    };

                    AddRange( new TestEntity(), new TestEntity(), new TestEntity() );

                    AssertAnEventRaised();
                    AreEqual( 1, eventRaisesCount );
                }

                [Test]
                public void WhenCollectionChangedHasMultiSubsAndIsBeingModified_AddRangeThrowsInvalidOperation()
                {
                    _collection.CollectionChanged += OnCollectionChanged;
                    _collection.CollectionChanged += ( sender, args ) => { };

                    AddRange( new TestEntity(), new TestEntity(), new TestEntity() );

                    void OnCollectionChanged( object sender, NotifyCollectionChangedEventArgs args )
                    {
                        _collection.CollectionChanged -= OnCollectionChanged;

                        Throws<InvalidOperationException>( () => AddRange( new TestEntity() ) );
                    }
                }
            }

            [TestFixture]
            public class ObservableRangeCollectionPropertyChangedEventTest : ObservableRangeCollectionEventContext
            {
                private List<string> _propertiesNames;

                [SetUp]
                public void ObservableRangeCollectionPropertyChangedEventSetup()
                {
                    _propertiesNames = new List<string>();
                    ( (INotifyPropertyChanged) _collection ).PropertyChanged += ( sender, args ) =>
                    {
                        if ( args != null ) _propertiesNames.Add( args.PropertyName );
                        _eventNotifier.Set();
                    };
                }

                [Test]
                public void WhenAddingRange_ShouldRaisePropertyChanged()
                {
                    AddRange( new TestEntity() );

                    AssertAnEventRaised();
                    AreEqual( 2, _propertiesNames.Count );
                    AreEqual( "Count", _propertiesNames[0] );
                    AreEqual( "Items", _propertiesNames[1] );
                }

                [Test]
                public void WhenAddingEmptyRange_NoEventsShouldBeRaised()
                {
                    AddRange();

                    AssertNoEventRaised();
                }
            }
        }
    }

    public class ObservableRangeCollection<T> : ObservableCollection<T>
    {
        public void AddRange( IEnumerable<T> range )
        {
            CheckReentrancy();

            if ( range == null ) throw new NullRange();

            var toAddItems = ToList( range );

            if ( IsEmpty( toAddItems ) ) return;

            var eventArgs = IsEmpty() ? ResetEventArgs() : ActionEventArgs( toAddItems );

            foreach ( var item in toAddItems ) Items.Add( item );

            OnPropertyChanged( new PropertyChangedEventArgs( nameof( Count ) ) );
            OnPropertyChanged( new PropertyChangedEventArgs( nameof( Items ) ) );
            OnCollectionChanged( eventArgs );
        }

        private bool IsEmpty()
        {
            return Count == 0;
        }

        private static NotifyCollectionChangedEventArgs ResetEventArgs()
        {
            return new NotifyCollectionChangedEventArgs( Reset );
        }

        private NotifyCollectionChangedEventArgs ActionEventArgs( IList toAddItems )
        {
            return new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, toAddItems,
                Count );
        }

        private static bool IsEmpty( ICollection toAddItems )
        {
            return toAddItems.Count == 0;
        }

        private static List<T> ToList( IEnumerable<T> range )
        {
            return range is List<T> list ? list : new List<T>( range );
        }

        public class NullRange : Exception
        {
        }
    }
}