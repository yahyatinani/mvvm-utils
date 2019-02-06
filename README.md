# MVVM Utils

|      | Build                                                        | Release                                                      | Version                                                      | Coverage                                                     | Downloads                                          | License                                                      |
| ---- | ------------------------------------------------------------ | ------------------------------------------------------------ | ------------------------------------------------------------ | ------------------------------------------------------------ | -------------------------------------------------- | ------------------------------------------------------------ |
| Live | [![Build status](https://dev.azure.com/yahyatinani/MVVM%20Utils/_apis/build/status/MVVM%20Utils%20CI)](https://dev.azure.com/yahyatinani/MVVM%20Utils/_build/latest?definitionId=6) | ![Release status](https://vsrm.dev.azure.com/yahyatinani/_apis/public/Release/badge/d40226bf-0034-4e1e-99a1-f45477642b2b/2/5) | [![NuGet](https://img.shields.io/nuget/v/MvvmUtils.svg?label=NuGet)](https://www.nuget.org/packages/MvvmUtils/) | ![](https://img.shields.io/azure-devops/coverage/yahyatinani/MVVM Utils/6.svg) | ![](https://img.shields.io/nuget/dt/MvvmUtils.svg) | ![](https://img.shields.io/github/license/whyrising/mvvm-utils.svg) |
| Beta | [![Build status](https://dev.azure.com/yahyatinani/MVVM%20Utils/_apis/build/status/MVVM%20Utils%20CI)](https://dev.azure.com/yahyatinani/MVVM%20Utils/_build/latest?definitionId=6) | ![Release status](https://vsrm.dev.azure.com/yahyatinani/_apis/public/Release/badge/d40226bf-0034-4e1e-99a1-f45477642b2b/2/3) | [![NuGet](https://img.shields.io/nuget/v/MvvmUtils.svg?label=NuGet)](https://www.nuget.org/packages/MvvmUtils/) | ![](https://img.shields.io/azure-devops/coverage/yahyatinani/MVVM Utils/6.svg) |                                                    |                                                              |

## Brief

MVVM Utils is intended to save developers' time by providing a bunch of handy classes that replace some of the repetitive code that we may have to write in a MVVM application.

## Utils

### ObservableRangeCollection

It's an ObservableCollection that can add, remove and replace items but in a better optimized way, by working with ranges, so instead of notifying the UI every time an item is added or removed, It notifies the UI only once, after the intended operation is done.

API:

- **AddRange** : If the collection is empty, It does raise OnCollectionChanged  with NotifyCollectionChangedAction.Reset, otherwise, It raises OnCollectionChanged with NotifyCollectionChangedAction.Add along with the added items and the starting index.
- **RemoveRange** : It does raise OnCollectionChanged with NotifyCollectionChangedAction.Reset.
- **RemoveRangeWithRemoveAction** : It checks if the removed range is consecutive, if so, the startingIndex is determined, if it is not the case, the starting index is set to -1, then it does raise OnCollectionChanged with NotifyCollectionChangedAction.Remove along with the starting index and the removed items.
- **ReplaceRange**
- **Replace**

*Note: this ObservableCollection is more of a remake, inspired by [jamesmontemagno](https://github.com/jamesmontemagno/mvvm-helpers/tree/master/MvvmHelpers) :   ), I tried to rewrite it my way, and hopefully did some improvements XD.

## Wanna Join Me?

You can report bugs, ask for more features, suggestions, optimizations or even send some pull requests, you know what to do :   ).