# Changelog

All notable changes to this project will be documented in this file.

## 1.1.0 - 2019-08-05
### Added
- ObservableRangeCollection API methods :
  - GetRange
### Changed

- Fix the name of the indexer passed to PropertyChangedEventArgs.
- Fix a bug in RemoveRangewithRemove when the removed items indices are in descending order.
- Fix a test of RemoveRangeWithRemoveAction.
- Add some more tests.
- Add some necessary documentation about the exceptions being thrown to help the clients handle them properly.


## 1.0.0 - 2019-01-30 

### Added

- ObservableRangeCollection API methods :
  - AddRange
  - ReplaceRange
  - Replace
  - RemoveRange
  - RemoveRangeWithRemoveAction

