# Openize.IsoBmff.ItemReferenceBox

Contains all the linking of one item to others via typed references.
All references for one item of a specific type are collected into a single item type reference box.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**ToString** | **string** | Text summary of the box. | 
**Children** | **ObservableCollection<SingleItemTypeReferenceBox>** | Observable collection of the references. | 

## Fields

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**references** | **List<SingleItemTypeReferenceBox>** | List of references. | 

## Constructors

Name | Description | Parameters
------------ | ------------- | ------------- | -------------
**ItemReferenceBox** | Create the box object from the bitstream, box size and start position. | BitStreamReader <b>stream</b> - File stream.<br />ulong <b>size</b> - Box size in bytes.<br />ulong <b>startPos</b> - Start position in bits.

[[Back to API_README]](API_README.md)