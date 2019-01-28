using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using NUnit.Framework;
using ObservableRangeCollection;
using static System.Collections.Specialized.NotifyCollectionChangedAction;
using static NUnit.Framework.Assert;
using static ObservableRangeCollection.ObservableRangeCollectionBase<UnitTests.TestEntity>;

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

        private void AddAndRaiseEvents( params TestEntity[] range )
        {
            _collection.AddAndRaiseEvents( new List<TestEntity>( range ) );
        }

        private void AddRange( params TestEntity[] range )
        {
            _collection.AddRange( range );
        }

        private void ReplaceRange( params TestEntity[] range )
        {
            _collection.ReplaceRange( range );
        }

        [TestFixture]
        public class ObservableRangeCollectionTest : ObservableRangeCollectionContext
        {
            private ObservableRangeCollectionSpy _collectionSpy;

            [SetUp]
            public void ObservableRangeCollectionTestSetUp()
            {
                _collectionSpy = new ObservableRangeCollectionSpy();
            }

            private void AssertCollectionContains( TestEntity entity )
            {
                if ( entity == null ) throw new ArgumentNullException( nameof( entity ) );

                True( _collection.Contains( entity ) );
            }

            #region AddAndRaiseEventsTests

            [Test]
            public void WhenAddingEmptyRange_CollectionShouldNotChange()
            {
                AddAndRaiseEvents();

                AssertCollectionSizeIs( 0 );
            }

            [Test]
            public void WhenRangeOneIsAdded_CollectionShouldContainRangeOne()
            {
                var one = new[] { new TestEntity() };

                AddAndRaiseEvents( one );

                foreach ( var entity in one ) AssertCollectionContains( entity );
            }

            [Test]
            public void WhenRangeOneAndTwoAreAdded_CollectionShouldContainRangeOneAndTwo()
            {
                var rangeOne = new[] { new TestEntity() };
                var rangeTwo = new[] { new TestEntity(), new TestEntity() };

                AddAndRaiseEvents( rangeOne );
                AddAndRaiseEvents( rangeTwo );

                AssertCollectionSizeIs( 3 );
                foreach ( var entity in rangeOne ) AssertCollectionContains( entity );
                foreach ( var entity in rangeTwo ) AssertCollectionContains( entity );
            }

            #endregion

            #region AddRangeTests

            [Test]
            public void WhenAddingNullRange_AddRangeThrowsNullRange()
            {
                Throws<NullRange>( () => AddRange( null ) );
            }

            [Test]
            public void AddRangeShouldCall_AddAndRaiseEvents()
            {
                var testEntities = new[] { new TestEntity() };

                _collectionSpy.AddRange( testEntities );

                True( _collectionSpy.IsAddAndRaiseEventsCalled );
                AreEqual( testEntities, _collectionSpy.ToAddItems );
            }

            #endregion

            [Test]
            public void ReplaceItemsShouldClearRangeOneAndCallAddAndRaiseEventsToAddRangeTwo()
            {
                var rangeOne = new TestEntity();
                var rangeTwo = new List<TestEntity> { new TestEntity(), new TestEntity() };
                _collectionSpy.Add( rangeOne );

                _collectionSpy.ReplaceItems( rangeTwo );

                False( _collectionSpy.Contains( rangeOne ) );
                True( _collectionSpy.IsAddAndRaiseEventsCalled );
                AreEqual( rangeTwo, _collectionSpy.ToAddItems );
                AreEqual( 0, _collectionSpy.CountMock, "Clear() got called after AddAndRaiseEvents()!" );
            }

            #region ReplaceRangeTests

            [Test]
            public void WhenRangeIsNull_ReplaceRangeShouldThrowNullRangeWithoutChangingCollection()
            {
                var oldRange = new TestEntity();
                _collection.Add( oldRange );

                Throws<NullRange>( () => ReplaceRange( null ) );
                AreEqual( oldRange, _collection[0] );
            }

            [Test]
            public void ReplaceRangeShouldCallReplaceItems()
            {
                var testEntities = new List<TestEntity> { new TestEntity(), new TestEntity() };

                _collectionSpy.ReplaceRange( testEntities );

                True( _collectionSpy.IsReplaceItemsCalled );
                AreEqual( testEntities, _collectionSpy.ToAddItems );
            }

            #endregion

            #region ReplaceTests

            [Test]
            public void WhenItemIsNull_ReplaceThrowNullItem()
            {
                var oldRange = new TestEntity();
                _collection.Add( oldRange );

                Throws<NullItem>( () => _collection.Replace( null ) );
                AreEqual( oldRange, _collection[0] );
            }

            [Test]
            public void ReplaceShouldCallReplaceItems()
            {
                var item = new TestEntity();

                _collectionSpy.Replace( item );

                True( _collectionSpy.IsReplaceItemsCalled );
                AreEqual( item, _collectionSpy.ReplacedItem );
            }

            #endregion

            private class ObservableRangeCollectionSpy : ObservableRangeCollection<TestEntity>
            {
                public List<TestEntity> ToAddItems { get; private set; }

                public int CountMock { get; private set; } = -1;

                protected internal override void AddAndRaiseEvents( List<TestEntity> toAddItems )
                {
                    CountMock = Count;
                    ToAddItems = toAddItems;
                    IsAddAndRaiseEventsCalled = true;
                }

                protected internal override void ReplaceItems( List<TestEntity> items )
                {
                    base.ReplaceItems( items );
                    IsReplaceItemsCalled = true;
                    ReplacedItem = items[0];
                }

                public bool IsAddAndRaiseEventsCalled { get; private set; }

                public bool IsReplaceItemsCalled { get; private set; }

                public TestEntity ReplacedItem { get; private set; }
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

                    AddAndRaiseEvents( one );

                    AssertAnEventRaised();
                    AssertCollectionChangedEventActionIs( Reset );
                    AssertCollectionSizeIs( _collectionCountWhenEventRaised );
                }

                [Test]
                public void WhenAddingRangeOneToNonEmptyCollection_ShouldRaiseCollectionChangedWithAddAction()
                {
                    AddAndRaiseEvents( new TestEntity(), new TestEntity() );
                    var one = new[] { new TestEntity() };

                    AddAndRaiseEvents( one );

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
                    AddAndRaiseEvents();

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

                    AddAndRaiseEvents( new TestEntity(), new TestEntity(), new TestEntity() );

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

                [Test]
                public void WhenCollectionChangedHasMultiSubsAndIsBeingModified_ReplaceRangeThrowsInvalidOperation()
                {
                    _collection.CollectionChanged += OnCollectionChanged;
                    _collection.CollectionChanged += ( sender, args ) => { };

                    ReplaceRange( new TestEntity(), new TestEntity(), new TestEntity() );

                    void OnCollectionChanged( object sender, NotifyCollectionChangedEventArgs args )
                    {
                        _collection.CollectionChanged -= OnCollectionChanged;

                        Throws<InvalidOperationException>( () => ReplaceRange( new TestEntity() ) );
                    }
                }

                [Test]
                public void WhenCollectionChangedHasMultiSubsAndIsBeingModified_ReplaceThrowsInvalidOperation()
                {
                    _collection.CollectionChanged += OnCollectionChanged;
                    _collection.CollectionChanged += ( sender, args ) => { };

                    _collection.Replace( new TestEntity() );

                    void OnCollectionChanged( object sender, NotifyCollectionChangedEventArgs args )
                    {
                        _collection.CollectionChanged -= OnCollectionChanged;

                        Throws<InvalidOperationException>( () => _collection.Replace( new TestEntity() ) );
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
                    AddAndRaiseEvents( new TestEntity() );

                    AssertAnEventRaised();
                    AreEqual( 2, _propertiesNames.Count );
                    AreEqual( "Count", _propertiesNames[0] );
                    AreEqual( "Items", _propertiesNames[1] );
                }

                [Test]
                public void WhenAddingEmptyRange_NoEventsShouldBeRaised()
                {
                    AddAndRaiseEvents();

                    AssertNoEventRaised();
                }
            }
        }
    }
}