# Openize.IsoBmff.ItemPropertyContainerBox

Contains an implicitly indexed list of item properties.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**ToString** | **string** | Text summary of the box. | 
**Children** | **ObservableCollection<Box>** | Observable collection of the nested boxes. | 

## Methods

Name | Type | Description | Parameters
------------ | ------------- | ------------- | -------------
**GetPropertyByIndex** | **Box** | Returns property by index. | int <b>id</b> - Property index.

## Fields

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**items** | **Dictionary<int, Box>** | Dictionary of properties. | 

## Constructors

Name | Description | Parameters
------------ | ------------- | ------------- | -------------
**ItemPropertyContainerBox** | Create the box object from the bitstream and start position. | BitStreamReader <b>stream</b> - File stream.<br />ulong <b>startPos</b> - Start position in bits.

[[Back to API_README]](API_README.md)