# Openize.IsoBmff.ItemDataBox

This box contains the data of metadata items that use the construction method indicating that an item’s data extents are stored within this box.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**ToString** | **string** | Text summary of the box. | 

## Fields

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**data** | **byte[]** | The contained meta data in raw format. | 

## Constructors

Name | Description | Parameters
------------ | ------------- | ------------- | -------------
**ItemDataBox** | Create the box object from the bitstream, box size and start position. | BitStreamReader <b>stream</b> - File stream.<br />ulong <b>size</b> - Box size in bytes.<br />ulong <b>startPos</b> - Start position in bits.

[[Back to API_README]](API_README.md)