using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
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

                That( _collection, Is.Empty );
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

                That( _collection.Count, Is.EqualTo( 3 ) );
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

            #region RemoveRange

            [Test]
            public void WhenRemovingNullRange_RemoveRangeThrowsNullRange()
            {
                Throws<NullRange>( () => _collection.RemoveRange( null ) );
            }

            [Test]
            [TestCase( new[] { 0, 1, 2, 3 }, new[] { 0, 1, 2, 3 } )]
            [TestCase( new[] { 0, 1, 2, 3 }, new[] { 1, 2, 3 }, 0 )]
            [TestCase( new[] { 0, 1, 2, 3 }, new[] { 0, 2, 3 }, 1 )]
            [TestCase( new[] { 0, 1, 2, 3 }, new[] { 2, 3 }, 0, 1 )]
            [TestCase( new[] { 0, 1, 2, 3 }, new int[] { }, 0, 1, 2, 3 )]
            [TestCase( new[] { 0, 1 }, new[] { 0, 1 }, 2, 3 )]
            [TestCase( new[] { 0, 0, 1, 1, 3, 3, 3, 2 }, new[] { 0, 1, 1, 3, 3 }, 0, 3, 2 )]
            public void RemoveRange_ShouldRemoveRange( int[] toAddIndices, int[] whatLeftIndices,
                params int[] toRemoveIndices )
            {
                var range = new[] { new TestEntity(), new TestEntity(), new TestEntity(), new TestEntity(), };
                var toAddItems = toAddIndices.Select( index => range[index] ).ToArray();
                var toRemoveItems = toRemoveIndices.Select( index => range[index] ).ToList();
                var whatLeftItems = whatLeftIndices.Select( index => range[index] ).ToList();
                AddRange( toAddItems );

                _collection.RemoveRange( toRemoveItems );

                for ( var i = 0; i < whatLeftItems.Count; i++ )
                    AreEqual( whatLeftItems[i], _collection[i] );
            }

            #endregion

            #region RemoveRangeWithRemove

            [Test]
            public void WhenRemovingNullRange_RemoveRangeWithRemoveThrowsNullRange()
            {
                Throws<NullRange>( () => _collection.RemoveRangeWithRemove( null ) );
            }

            [Test]
            [TestCase( new[] { 0, 1, 2, 3 }, new[] { 0, 1, 2, 3 } )]
            [TestCase( new[] { 0, 1, 2, 3 }, new[] { 1, 2, 3 }, 0 )]
            [TestCase( new[] { 0, 1, 2, 3 }, new[] { 0, 2, 3 }, 1 )]
            [TestCase( new[] { 0, 1, 2, 3 }, new[] { 2, 3 }, 0, 1 )]
            [TestCase( new[] { 0, 1, 2, 3 }, new int[] { }, 0, 1, 2, 3 )]
            [TestCase( new[] { 0, 1 }, new[] { 0, 1 }, 2, 3 )]
            [TestCase( new[] { 0, 0, 1, 1, 3, 3, 3, 2 }, new[] { 0, 1, 1, 3, 3 }, 0, 3, 2 )]
            public void RemoveRangeWithRemove_ShouldRemoveRange( int[] toAddIndices, int[] whatLeftIndices,
                params int[] toRemoveIndices )
            {
                var range = new[] { new TestEntity(), new TestEntity(), new TestEntity(), new TestEntity(), };
                var toAddItems = toAddIndices.Select( index => range[index] ).ToArray();
                var toRemoveItems = toRemoveIndices.Select( index => range[index] ).ToList();
                var whatLeftItems = whatLeftIndices.Select( index => range[index] ).ToList();
                AddRange( toAddItems );

                _collection.RemoveRangeWithRemove( toRemoveItems );

                for ( var i = 0; i < whatLeftItems.Count; i++ )
                    AreEqual( whatLeftItems[i], _collection[i] );
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

            private void AssertThatEventWasRaised()
            {
                True( _eventNotifier.WaitOne( 200 ), "No event was raised!" );
            }

            private void AssertThatNoEventsWereRaised()
            {
                False( _eventNotifier.WaitOne( 200 ), "An event was raised!" );
            }

            [TestFixture]
            public class ObservableRangeCollectionChangedEventsTest : ObservableRangeCollectionEventContext
            {
                private const string EVENT_RAISED_BEFORE_OPERATION_IS_DONE = "Event was raised before operation is " +
                                                                             "done.";

                private NotifyCollectionChangedEventArgs _eventArgs;
                private int _collectionSizeAtEventRaise;

                [SetUp]
                public void ObservableRangeCollectionEventsTestSetup()
                {
                    _eventArgs = null;
                    _collectionSizeAtEventRaise = 0;
                }

                private NotifyCollectionChangedEventHandler OnCollectionChanged()
                {
                    return ( sender, args ) =>
                    {
                        _collectionSizeAtEventRaise = _collection.Count;
                        _eventArgs = args;
                        _eventNotifier.Set();
                    };
                }

                private void AssertThatChangeEventActionIs( NotifyCollectionChangedAction changeAction )
                {
                    AreEqual( changeAction, _eventArgs.Action );
                }

                private void AssertThatEventWasRaisedAfterOperationIsDone()
                {
                    That( _collection.Count,
                        Is.EqualTo( _collectionSizeAtEventRaise ),
                        EVENT_RAISED_BEFORE_OPERATION_IS_DONE );
                }

                [Test]
                public void WhenAddingRangeToEmptyCollection_ShouldRaiseCollectionChangedWithResetAction()
                {
                    _collection.CollectionChanged += OnCollectionChanged();
                    AddAndRaiseEvents( new TestEntity(), new TestEntity() );

                    AssertThatEventWasRaised();
                    AssertThatChangeEventActionIs( Reset );
                    AssertThatEventWasRaisedAfterOperationIsDone();
                }

                [Test]
                public void WhenAddingRangeToNonEmptyCollection_ShouldRaiseCollectionChangedWithAddAction()
                {
                    _collection.CollectionChanged += OnCollectionChanged();
                    AddAndRaiseEvents( new TestEntity(), new TestEntity() );
                    var range = new[] { new TestEntity(), new TestEntity() };

                    AddAndRaiseEvents( range );

                    AssertThatEventWasRaised();
                    AssertThatChangeEventActionIs( Add );
                    AssertThatEventWasRaisedAfterOperationIsDone();
                    IsNull( _eventArgs.OldItems );
                    AreEqual( 2, _eventArgs.NewStartingIndex );
                    for ( var i = 0; i < range.Length; i++ )
                        AreEqual( range[i], _eventArgs.NewItems[i] );
                }

                [Test]
                public void WhenAddingEmptyRange_NoEventsShouldBeRaised()
                {
                    AddAndRaiseEvents();

                    AssertThatNoEventsWereRaised();
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

                    AssertThatEventWasRaised();
                    AreEqual( 1, eventRaisesCount );
                }

                [Test]
                public void WhenCollectionChangedHasMultiSubsAndIsBeingModified_AddRangeThrowsInvalidOperation()
                {
                    _collection.CollectionChanged += OnCollectionChanged();
                    _collection.CollectionChanged += CollectionChanged;
                    _collection.CollectionChanged += ( sender, args ) => { };

                    AddRange( new TestEntity(), new TestEntity(), new TestEntity() );

                    void CollectionChanged( object sender, NotifyCollectionChangedEventArgs args )
                    {
                        _collection.CollectionChanged -= CollectionChanged;

                        Throws<InvalidOperationException>( () => AddRange( new TestEntity() ) );
                    }
                }

                [Test]
                public void WhenCollectionChangedHasMultiSubsAndIsBeingModified_ReplaceRangeThrowsInvalidOperation()
                {
                    _collection.CollectionChanged += OnCollectionChanged();
                    _collection.CollectionChanged += CollectionChanged;
                    _collection.CollectionChanged += ( sender, args ) => { };

                    ReplaceRange( new TestEntity(), new TestEntity(), new TestEntity() );

                    void CollectionChanged( object sender, NotifyCollectionChangedEventArgs args )
                    {
                        _collection.CollectionChanged -= CollectionChanged;

                        Throws<InvalidOperationException>( () => ReplaceRange( new TestEntity() ) );
                    }
                }

                [Test]
                public void WhenCollectionChangedHasMultiSubsAndIsBeingModified_ReplaceThrowsInvalidOperation()
                {
                    _collection.CollectionChanged += OnCollectionChanged();
                    _collection.CollectionChanged += CollectionChanged;
                    _collection.CollectionChanged += ( sender, args ) => { };

                    _collection.Replace( new TestEntity() );

                    void CollectionChanged( object sender, NotifyCollectionChangedEventArgs args )
                    {
                        _collection.CollectionChanged -= CollectionChanged;

                        Throws<InvalidOperationException>( () => _collection.Replace( new TestEntity() ) );
                    }
                }

                #region RemoveRangeTests

                [Test]
                public void WhenItemsRemoved_RemoveRangeShouldRaiseOnlyOneCollectionChangedEvent()
                {
                    var eventsRaisedCount = 0;
                    var entity1 = new TestEntity();
                    var entity2 = new TestEntity();
                    var toRemove = new[] { entity1, entity2 };
                    AddRange( entity1, entity2, new TestEntity() );
                    _collection.CollectionChanged += ( sender, args ) =>
                    {
                        ++eventsRaisedCount;
                        _eventNotifier.Set();
                    };

                    _collection.RemoveRange( toRemove );

                    AssertThatEventWasRaised();
                    That( eventsRaisedCount, Is.EqualTo( 1 ) );
                }

                [Test]
                public void WhenItemsRemoved_RemoveRangeShouldRaiseCollectionChangedWithResetAction()
                {
                    var entity = new TestEntity();
                    var toRemove = new[] { entity };
                    AddRange( entity, new TestEntity(), new TestEntity() );
                    _collection.CollectionChanged += OnCollectionChanged();

                    _collection.RemoveRange( toRemove );

                    AssertThatEventWasRaised();
                    AssertThatChangeEventActionIs( Reset );
                    AssertThatEventWasRaisedAfterOperationIsDone();
                }

                [Test]
                public void WhenRangeIsEmpty_RemoveRangeShouldNotRaiseAnyEvents()
                {
                    AddRange( new TestEntity(), new TestEntity(), new TestEntity() );
                    _collection.CollectionChanged += OnCollectionChanged();

                    _collection.RemoveRange( new List<TestEntity>() );

                    AssertThatNoEventsWereRaised();
                }


                [Test]
                public void WhenCollectionIsEmpty_RemoveRangeShouldNotRaiseAnyEvents()
                {
                    _collection.CollectionChanged += OnCollectionChanged();

                    _collection.RemoveRange( new[] { new TestEntity(), new TestEntity(), } );

                    AssertThatNoEventsWereRaised();
                }

                #endregion

                [Test]
                public void WhenItemsRemoved_RemoveRangeWithRemoveShouldRaiseOnlyOneCollectionChangedEvent()
                {
                    var eventsRaisedCount = 0;
                    var entity1 = new TestEntity();
                    var entity2 = new TestEntity();
                    var toRemove = new[] { entity1, entity2 };
                    AddRange( entity1, entity2, new TestEntity() );
                    _collection.CollectionChanged += ( sender, args ) =>
                    {
                        ++eventsRaisedCount;
                        _eventNotifier.Set();
                    };

                    _collection.RemoveRangeWithRemove( toRemove );

                    AssertThatEventWasRaised();
                    That( eventsRaisedCount, Is.EqualTo( 1 ) );
                }

                [Test]
                [TestCase( new[] { 0, 1, 2, 3 }, new[] { 0 }, 0 )]
                [TestCase( new[] { 0, 1, 2, 3 }, new[] { 0, 2, 3 }, 0, 2, 3 )]
                [TestCase( new[] { 0, 1, 2 }, new[] { 0, 2 }, 0, 2, 3 )]
                [TestCase( new[] { 0 }, new int[] { }, 1, 2, 3 )]
                [TestCase( new[] { 0 }, new int[] { }, 1 )]
                public void RemoveRangeWithRemove_ShouldRaiseRemoveActionWithRemovedItems( int[] toAddIndices,
                    int[] removedItemsIndices, params int[] toRemoveIndices )
                {
                    var range = new[] { new TestEntity(), new TestEntity(), new TestEntity(), new TestEntity() };
                    var toAddItems = toAddIndices.Select( index => range[index] ).ToArray();
                    var toRemoveItems = toRemoveIndices.Select( index => range[index] ).ToList();
                    var expectedRemovedItems = removedItemsIndices.Select( index => range[index] ).ToList();
                    AddRange( toAddItems );
                    _collection.CollectionChanged += OnCollectionChanged();

                    _collection.RemoveRangeWithRemove( toRemoveItems );
                    var removed = _eventArgs.OldItems;

                    AssertThatEventWasRaised();
                    AssertThatChangeEventActionIs( Remove );
                    AssertThatEventWasRaisedAfterOperationIsDone();
                    for ( var i = 0; i < removed.Count; i++ )
                        That( expectedRemovedItems[i], Is.EqualTo( removed[i] ) );
                }

                [Test]
                public void WhenRangeIsEmpty_RemoveRangeWithRemoveShouldNotRaiseAnyEvents()
                {
                    _collection.CollectionChanged += OnCollectionChanged();

                    _collection.RemoveRangeWithRemove( new List<TestEntity>() );

                    AssertThatNoEventsWereRaised();
                }

                [Test]
                public void WhenCollectionIsEmpty_RemoveRangeWithRemoveShouldNotRaiseAnyEvents()
                {
                    _collection.CollectionChanged += OnCollectionChanged();

                    _collection.RemoveRangeWithRemove( new List<TestEntity> { new TestEntity() } );

                    AssertThatNoEventsWereRaised();
                }

                [Test]
                [TestCase( 0, 0 )]
                [TestCase( 1, 1 )]
                [TestCase( 2, 3, 2, 5, 4 )]
                [TestCase( 1, 1, 2, 3 )]
                public void WhenRemovingConsecutiveRange_ShouldRaiseRemoveActionWithFirstIndex( int expectedIndex,
                    params int[] toRemoveIndices )
                {
                    var range = new[]
                    {
                        new TestEntity(), new TestEntity(), new TestEntity(), new TestEntity(), new TestEntity(),
                        new TestEntity(),
                    };
                    var toRemoveItems = toRemoveIndices.Select( index => range[index] ).ToList();
                    AddRange( range );
                    _collection.CollectionChanged += OnCollectionChanged();

                    _collection.RemoveRangeWithRemove( toRemoveItems );
                    var startingIndex = _eventArgs.OldStartingIndex;

                    AssertThatEventWasRaised();
                    That( startingIndex, Is.EqualTo( expectedIndex ) );
                }

                [Test]
                [TestCase( new[] { 0 }, 1, 5, 4 )]
                [TestCase( new[] { 0, 1, 2, 3, 4, 5 }, 1, 5, 4 )]
                public void WhenRemovingNonConsecutiveOrExistingRange_ShouldRaiseRemoveActionWithMinusOneIndex(
                    int[] toAddIndices, params int[] toRemoveIndices )
                {
                    var range = new[]
                    {
                        new TestEntity(), new TestEntity(), new TestEntity(), new TestEntity(), new TestEntity(),
                        new TestEntity(),
                    };
                    var toRemoveItems = toRemoveIndices.Select( index => range[index] ).ToList();
                    var toAddItems = toAddIndices.Select( index => range[index] ).ToArray();
                    AddRange( toAddItems );
                    _collection.CollectionChanged += OnCollectionChanged();

                    _collection.RemoveRangeWithRemove( toRemoveItems );
                    var startingIndex = _eventArgs.OldStartingIndex;

                    AssertThatEventWasRaised();
                    That( startingIndex, Is.EqualTo( -1 ) );
                }

                [Test]
                public void WhenCollectionChangedHasMultiSubsAndIsBeingModified_RemoveRangeThrowsInvalidOperation()
                {
                    var range = new[] { new TestEntity(), new TestEntity(), new TestEntity() };
                    _collection.CollectionChanged += OnCollectionChanged();
                    _collection.CollectionChanged += CollectionChanged;
                    _collection.CollectionChanged += ( sender, args ) => { };

                    AddRange( range );
                    _collection.RemoveRange( range );

                    void CollectionChanged( object sender, NotifyCollectionChangedEventArgs args )
                    {
                        _collection.CollectionChanged -= CollectionChanged;

                        Throws<InvalidOperationException>( () => _collection.RemoveRange( range ) );
                    }
                }

                [Test]
                public void WhenCollectionChangedHasMultiSubs_RemoveRangeRemoveThrowsInvalidOperation()
                {
                    var range = new[] { new TestEntity(), new TestEntity(), new TestEntity() };
                    _collection.CollectionChanged += OnCollectionChanged();
                    _collection.CollectionChanged += CollectionChanged;
                    _collection.CollectionChanged += ( sender, args ) => { };

                    AddRange( range );
                    _collection.RemoveRangeWithRemove( range );

                    void CollectionChanged( object sender, NotifyCollectionChangedEventArgs args )
                    {
                        _collection.CollectionChanged -= CollectionChanged;

                        Throws<InvalidOperationException>( () => _collection.RemoveRangeWithRemove( range ) );
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
                }

                [Test]
                public void WhenAddingRange_ShouldRaisePropertyChanged()
                {
                    SubscribeToPropertyChangedEvent();

                    AddAndRaiseEvents( new TestEntity() );

                    AssertThatEventWasRaised();
                    AreEqual( 2, _propertiesNames.Count );
                    AreEqual( "Count", _propertiesNames[0] );
                    AreEqual( "Items", _propertiesNames[1] );
                }

                [Test]
                public void WhenAddingEmptyRange_NoEventsShouldBeRaised()
                {
                    SubscribeToPropertyChangedEvent();

                    AddAndRaiseEvents();

                    AssertThatNoEventsWereRaised();
                }

                [Test]
                public void RemoveRange_ShouldRaisePropertyChanged()
                {
                    var entity = new TestEntity();
                    var toRemove = new[] { entity };
                    AddRange( entity, new TestEntity(), new TestEntity() );
                    SubscribeToPropertyChangedEvent();

                    _collection.RemoveRange( toRemove );

                    AssertThatEventWasRaised();
                    AreEqual( 2, _propertiesNames.Count );
                    AreEqual( "Count", _propertiesNames[0] );
                    AreEqual( "Items", _propertiesNames[1] );
                }

                [Test]
                public void WhenRemovingEmptyRange_NoEventsShouldBeRaised()
                {
                    AddRange( new TestEntity(), new TestEntity(), new TestEntity() );
                    SubscribeToPropertyChangedEvent();

                    _collection.RemoveRange( new List<TestEntity>() );

                    AssertThatNoEventsWereRaised();
                }

                [Test]
                public void WhenCollectionIsEmpty_RemoveRangeShouldNotRaiseEvent()
                {
                    SubscribeToPropertyChangedEvent();

                    _collection.RemoveRange( new[] { new TestEntity(), new TestEntity() } );

                    AssertThatNoEventsWereRaised();
                }

                [Test]
                public void RemoveRangeWithRemove_ShouldRaisePropertyChanged()
                {
                    var entity = new TestEntity();
                    var toRemove = new[] { entity };
                    AddRange( entity, new TestEntity(), new TestEntity() );
                    SubscribeToPropertyChangedEvent();

                    _collection.RemoveRangeWithRemove( toRemove );

                    AssertThatEventWasRaised();
                    AreEqual( 2, _propertiesNames.Count );
                    AreEqual( "Count", _propertiesNames[0] );
                    AreEqual( "Items", _propertiesNames[1] );
                }

                [Test]
                public void WhenCollectionIsEmpty_RemoveRangeWithRemoveShouldNotRaiseEvent()
                {
                    SubscribeToPropertyChangedEvent();

                    _collection.RemoveRangeWithRemove( new[] { new TestEntity(), new TestEntity() } );

                    AssertThatNoEventsWereRaised();
                }

                private void SubscribeToPropertyChangedEvent()
                {
                    ( (INotifyPropertyChanged) _collection ).PropertyChanged += OnPropertyChanged();

                    PropertyChangedEventHandler OnPropertyChanged()
                    {
                        return ( sender, args ) =>
                        {
                            if ( args != null ) _propertiesNames.Add( args.PropertyName );
                            _eventNotifier.Set();
                        };
                    }
                }
            }
        }
    }
}