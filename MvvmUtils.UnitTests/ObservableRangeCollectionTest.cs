using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using MvvmUtils;
using static System.Collections.Specialized.NotifyCollectionChangedAction;
using static NUnit.Framework.Assert;
using static MvvmUtils.ObservableRangeCollectionBase<UnitTests.TestEntity>;

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
                var rangeOne = new[] { new TestEntity() };

                AddAndRaiseEvents( rangeOne );

                foreach ( var entity in rangeOne ) That( _collection, Does.Contain( entity ) );
            }

            [Test]
            public void WhenRangeOneAndTwoAreAdded_CollectionShouldContainRangeOneAndTwo()
            {
                var rangeOne = new[] { new TestEntity() };
                var rangeTwo = new[] { new TestEntity(), new TestEntity() };

                AddAndRaiseEvents( rangeOne );
                AddAndRaiseEvents( rangeTwo );

                That( _collection.Count, Is.EqualTo( 3 ) );
                foreach ( var entity in rangeOne ) That( _collection, Does.Contain( entity ) );
                foreach ( var entity in rangeTwo ) That( _collection, Does.Contain( entity ) );
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

            #region ReplaceRangeTests

            [Test]
            public void ReplaceRangeShouldClearRangeOneAndCallAddAndRaiseEventsToAddRangeTwo()
            {
                var rangeOne = new TestEntity();
                var rangeTwo = new List<TestEntity> { new TestEntity(), new TestEntity() };
                _collectionSpy.Add( rangeOne );

                _collectionSpy.ReplaceRange( rangeTwo );

                That( _collectionSpy, Does.Not.Contain( rangeOne ) );
                That( _collectionSpy.IsAddAndRaiseEventsCalled, Is.True );
                AreEqual( rangeTwo, _collectionSpy.ToAddItems );
                AreEqual( 0, _collectionSpy.CountMock, "Clear() got called after AddAndRaiseEvents()!" );
            }

            [Test]
            public void WhenRangeIsNull_ReplaceRangeShouldThrowNullRangeWithoutChangingCollection()
            {
                var range = new[] { new TestEntity(), new TestEntity(), new TestEntity() };
                AddRange( range );

                Throws<NullRange>( () => ReplaceRange( null ) );
                for ( var i = 0; i < _collection.Count; i++ ) That( _collection[i], Is.EqualTo( range[i] ) );
            }

            #endregion

            #region ReplaceTests

            [Test]
            public void WhenItemIsNull_ReplaceThrowNullItem()
            {
                var range = new[] { new TestEntity(), new TestEntity(), new TestEntity() };
                AddRange( range );


                Throws<NullItem>( () => _collection.Replace( null ) );
                for ( var i = 0; i < _collection.Count; i++ ) That( _collection[i], Is.EqualTo( range[i] ) );
            }

            [Test]
            public void ReplaceShouldReplaceRangeOneWithItem()
            {
                var rangeOne = new[] { new TestEntity(), new TestEntity(), new TestEntity() };
                var item = new TestEntity();
                AddRange( rangeOne );

                _collection.Replace( item );

                That( _collection.Count, Is.EqualTo( 1 ) );
                That( _collection[0], Is.EqualTo( item ) );
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
                Throws<NullRange>( () => _collection.RemoveRangeWithRemoveAction( null ) );
            }

            [Test]
            [TestCase( new[] { 0, 1, 2, 3 }, new[] { 0, 1, 2, 3 } )]
            [TestCase( new[] { 0, 1, 2, 3 }, new[] { 1, 2, 3 }, 0 )]
            [TestCase( new[] { 0, 1, 2, 3 }, new[] { 0, 2, 3 }, 1 )]
            [TestCase( new[] { 0, 1, 2, 3 }, new[] { 2, 3 }, 0, 1 )]
            [TestCase( new[] { 0, 1, 2, 3 }, new int[] { }, 0, 1, 2, 3 )]
            [TestCase( new[] { 0, 1 }, new[] { 0, 1 }, 2, 3 )]
            [TestCase( new[] { 0, 0, 1, 1, 3, 3, 3, 2 }, new[] { 0, 1, 1, 3, 3 }, 0, 3, 2 )]
            [TestCase( new[] { 0, 1, 2, 3 }, new[] { 3 }, 0, 2, 1 )]
            [TestCase( new[] { 0, 0, 1, 1, 3, 3, 3, 2 }, new[] { 1, 1, 3, 3 }, 0, 0, 2 )]
            [TestCase( new[] { 0, 1, 2, 3 }, new[] { 0, 3 }, 1, 1, 2 )]
            public void RemoveRangeWithRemove_ShouldRemoveRange(
                int[] toAddIndices, int[] whatLeftIndices, params int[] toRemoveIndices )
            {
                var range = new[] { new TestEntity(), new TestEntity(), new TestEntity(), new TestEntity(), };
                var toAddItems = toAddIndices.Select( index => range[index] ).ToArray();
                var toRemoveItems = toRemoveIndices.Select( index => range[index] ).ToList();
                var whatLeftItems = whatLeftIndices.Select( index => range[index] ).ToList();
                AddRange( toAddItems );

                _collection.RemoveRangeWithRemoveAction( toRemoveItems );

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

                public bool IsAddAndRaiseEventsCalled { get; private set; }
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
                private const string EVENT_RAISED_BEFORE_OPERATION = "Event was raised before operation is " +
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
                    That( _collection.Count, Is.EqualTo( _collectionSizeAtEventRaise ), EVENT_RAISED_BEFORE_OPERATION );
                }

                #region AddAndRaiseEventsTests

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

                #endregion

                #region AddRangeTests

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

                #endregion

                #region ReplaceRangeTests

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
                public void ReplaceRange_ShouldOnlyRaiseOneCollectionChangedEvent()
                {
                    var eventRaisesCount = 0;
                    _collection.CollectionChanged += ( sender, args ) =>
                    {
                        ++eventRaisesCount;

                        _eventNotifier.Set();
                    };

                    _collection.ReplaceRange( new[] { new TestEntity() } );

                    AssertThatEventWasRaised();
                    AreEqual( 1, eventRaisesCount, "More than one CollectionChanged event was raised!" );
                }

                #endregion

                #region ReplaceTests

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

                [Test]
                public void Replace_ShouldOnlyRaiseOneCollectionChangedEvent()
                {
                    var eventRaisesCount = 0;
                    _collection.CollectionChanged += ( sender, args ) =>
                    {
                        ++eventRaisesCount;

                        _eventNotifier.Set();
                    };

                    _collection.Replace( new TestEntity() );

                    AssertThatEventWasRaised();
                    AreEqual( 1, eventRaisesCount, "More than one CollectionChanged event was raised!" );
                }

                #endregion

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

                #region RemoveRangeWithRemove

                [Test]
                public void AfterItemsRemoved_RemoveRangeWithRemoveShouldRaiseOnlyOneCollectionChangedEvent()
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

                    _collection.RemoveRangeWithRemoveAction( toRemove );

                    AssertThatEventWasRaised();
                    That( eventsRaisedCount, Is.EqualTo( 1 ) );
                }

                [Test]
                [TestCase( new[] { 0, 1, 2, 3 }, new[] { 0 }, 0 )]
                [TestCase( new[] { 0, 1, 2, 3 }, new[] { 0, 2, 3 }, 0, 2, 3 )]
                [TestCase( new[] { 0, 1, 2 }, new[] { 0, 2 }, 0, 2, 3 )]
                public void RemoveRangeWithRemove_ShouldRaiseRemoveActionWithRemovedItems(
                    int[] toAddIndices, int[] removedItemsIndices, params int[] toRemoveIndices )
                {
                    var range = new[] { new TestEntity(), new TestEntity(), new TestEntity(), new TestEntity() };
                    var toAddItems = toAddIndices.Select( index => range[index] ).ToArray();
                    var toRemoveItems = toRemoveIndices.Select( index => range[index] ).ToList();
                    var expectedRemovedItems = removedItemsIndices.Select( index => range[index] ).ToList();
                    AddRange( toAddItems );
                    _collection.CollectionChanged += OnCollectionChanged();

                    _collection.RemoveRangeWithRemoveAction( toRemoveItems );
                    var removedItems = _eventArgs.OldItems;

                    AssertThatEventWasRaised();
                    AssertThatChangeEventActionIs( Remove );
                    AssertThatEventWasRaisedAfterOperationIsDone();
                    That( removedItems.Count, Is.EqualTo( expectedRemovedItems.Count ) );
                    for ( var i = 0; i < expectedRemovedItems.Count; i++ )
                        That( expectedRemovedItems[i], Is.EqualTo( removedItems[i] ) );
                }

                [Test]
                public void WhenRemovingConsecutiveRange_ShouldRaiseRemoveActionWithStartingIndex()
                {
                    var entity1 = new TestEntity();
                    var entity2 = new TestEntity();
                    var entity3 = new TestEntity();
                    var entity4 = new TestEntity();
                    var entity5 = new TestEntity();
                    var entity6 = new TestEntity();
                    AddRange( entity1, entity2, entity3, entity4, entity5, entity6 );
                    _collection.CollectionChanged += OnCollectionChanged();

                    _collection.RemoveRangeWithRemoveAction( new[] { entity4, entity3, entity2 } );

                    AssertThatEventWasRaised();
                    var startingIndex = _eventArgs.OldStartingIndex;
                    That( startingIndex, Is.EqualTo( 1 ) );
                }

                [Test]
                public void WhenRemovingNoneConsecutiveRange_ShouldRaiseRemoveActionWithMinusOneStartingIndex()
                {
                    var entity1 = new TestEntity();
                    var entity2 = new TestEntity();
                    var entity3 = new TestEntity();
                    var entity4 = new TestEntity();
                    var entity5 = new TestEntity();
                    var entity6 = new TestEntity();
                    AddRange( entity1, entity2, entity3, entity4, entity5, entity6 );
                    _collection.CollectionChanged += OnCollectionChanged();

                    _collection.RemoveRangeWithRemoveAction( new[] { entity1, entity5, entity4 } );

                    That( _eventArgs.OldStartingIndex, Is.EqualTo( -1 ) );
                }

                [Test]
                public void WhenRangeIsEmpty_RemoveRangeWithRemoveShouldNotRaiseAnyEvents()
                {
                    _collection.CollectionChanged += OnCollectionChanged();

                    _collection.RemoveRangeWithRemoveAction( new List<TestEntity>() );

                    AssertThatNoEventsWereRaised();
                }

                [Test]
                public void WhenCollectionIsEmpty_RemoveRangeWithRemoveShouldNotRaiseAnyEvents()
                {
                    _collection.CollectionChanged += OnCollectionChanged();

                    _collection.RemoveRangeWithRemoveAction( new List<TestEntity> { new TestEntity() } );

                    AssertThatNoEventsWereRaised();
                }

                [Test]
                public void WhenRemovingNoneExistingRange_RemoveRangeWithRemoveShouldNotRaiseAnyEvents()
                {
                    _collection.AddRange( new List<TestEntity> { new TestEntity(), new TestEntity() } );
                    _collection.CollectionChanged += OnCollectionChanged();

                    _collection.RemoveRangeWithRemoveAction( new List<TestEntity> { new TestEntity() } );

                    AssertThatNoEventsWereRaised();
                }

                [Test]
                public void WhenRemovingNoneDistinctConsecutiveRange()
                {
                    var entity1 = new TestEntity();
                    var entity2 = new TestEntity();
                    var entity3 = new TestEntity();
                    var entity4 = new TestEntity();
                    var entity5 = new TestEntity();
                    var entity6 = new TestEntity();
                    AddRange( entity1, entity2, entity3, entity4, entity5, entity6 );
                    var leftRange = new List<TestEntity> { entity3, entity4, entity5, entity6 };
                    _collection.CollectionChanged += OnCollectionChanged();

                    _collection.RemoveRangeWithRemoveAction( new[] { entity1, entity1, entity2 } );

                    That( _collection.Count, Is.EqualTo( 4 ) );
                    That( _eventArgs.OldItems.Count, Is.EqualTo( 2 ) );
                    for ( var i = 0; i < leftRange.Count; i++ )
                        That( _collection[i], Is.EqualTo( leftRange[i] ) );
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
                    _collection.RemoveRangeWithRemoveAction( range );

                    void CollectionChanged( object sender, NotifyCollectionChangedEventArgs args )
                    {
                        _collection.CollectionChanged -= CollectionChanged;

                        Throws<InvalidOperationException>( () => _collection.RemoveRangeWithRemoveAction( range ) );
                    }
                }

                #endregion
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

                #region AddAndRaiseEventsTests

                [Test]
                public void WhenAddingRange_ShouldRaisePropertyChanged()
                {
                    SubscribeToPropertyChangedEvent();

                    AddAndRaiseEvents( new TestEntity() );

                    AssertThatEventWasRaised();
                    AreEqual( 2, _propertiesNames.Count );
                    AreEqual( "Count", _propertiesNames[0] );
                    AreEqual( "Item[]", _propertiesNames[1] );
                }

                [Test]
                public void WhenAddingEmptyRange_NoEventsShouldBeRaised()
                {
                    SubscribeToPropertyChangedEvent();

                    AddAndRaiseEvents();

                    AssertThatNoEventsWereRaised();
                }

                #endregion

                #region RemoveRangeTests

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
                    AreEqual( "Item[]", _propertiesNames[1] );
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
                public void WhenRangeDoesNotExist_RemoveRangeShouldNotRaiseEvent()
                {
                    AddRange( new TestEntity(), new TestEntity(), new TestEntity() );
                    SubscribeToPropertyChangedEvent();

                    _collection.RemoveRange( new[] { new TestEntity(), new TestEntity() } );

                    AssertThatNoEventsWereRaised();
                }

                #endregion

                #region RemoveRangeWithRemoveActionTests

                [Test]
                public void RemoveRangeWithRemove_ShouldRaisePropertyChanged()
                {
                    var entity = new TestEntity();
                    var toRemove = new[] { entity };
                    AddRange( entity, new TestEntity(), new TestEntity() );
                    SubscribeToPropertyChangedEvent();

                    _collection.RemoveRangeWithRemoveAction( toRemove );

                    AssertThatEventWasRaised();
                    AreEqual( 2, _propertiesNames.Count );
                    AreEqual( "Count", _propertiesNames[0] );
                    AreEqual( "Item[]", _propertiesNames[1] );
                }

                [Test]
                public void WhenCollectionIsEmpty_RemoveRangeWithRemoveShouldNotRaiseEvent()
                {
                    SubscribeToPropertyChangedEvent();

                    _collection.RemoveRangeWithRemoveAction( new[] { new TestEntity(), new TestEntity() } );

                    AssertThatNoEventsWereRaised();
                }

                #endregion

                [Test]
                public void ReplaceShouldRaiseOnePropertyChangedEvent()
                {
                    var entity = new TestEntity();
                    AddRange( new TestEntity(), new TestEntity() );
                    SubscribeToPropertyChangedEvent();

                    _collection.Replace( entity );

                    AssertThatEventWasRaised();
                    AreEqual( 2, _propertiesNames.Count, "More than one PropertyChanged event was raised!" );
                    AreEqual( "Count", _propertiesNames[0] );
                    AreEqual( "Item[]", _propertiesNames[1] );
                }


                [Test]
                public void ReplaceRangeShouldRaiseOnePropertyChangedEvent()
                {
                    var entity = new TestEntity();
                    AddRange( new TestEntity(), new TestEntity() );
                    SubscribeToPropertyChangedEvent();

                    _collection.ReplaceRange( new[] { entity } );

                    AssertThatEventWasRaised();
                    AreEqual( 2, _propertiesNames.Count, "More than one PropertyChanged event was raised!" );
                    AreEqual( "Count", _propertiesNames[0] );
                    AreEqual( "Item[]", _propertiesNames[1] );
                }
            }
        }
    }
}