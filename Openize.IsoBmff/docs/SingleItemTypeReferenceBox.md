# Openize.IsoBmff.SingleItemTypeReferenceBox

Collects all references for one item of a specific type.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**ToString** | **string** | Text summary of the box. | 

## Fields

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**from_item_ID** | **uint** | The ID of the item that refers to other items. | 
**reference_count** | **uint** | The number of references. | 
**to_item_ID** | **uint[]** | The array of the IDs of the item referred to. | 

## Constructors

Name | Description | Parameters
------------ | ------------- | ------------- | -------------
**SingleItemTypeReferenceBox** | Create the box object from the bitstream and box size. | BitStreamReader <b>stream</b> - File stream.<br />ulong <b>size</b> - Box size in bytes.

[[Back to API_README]](API_README.md)