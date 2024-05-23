# Openize.IsoBmff.ItemProtectionBox

The item protection box provides an array of item protection information, for use by the Item Information Box.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**ToString** | **string** | Text summary of the box. | 

## Fields

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**protection_count** | **ushort** | Count of protection informarion schemas. | 
**protection_information** | **ProtectionSchemeInfoBox[]** | Array of protecyion informarion schemas. | 

## Constructors

Name | Description | Parameters
------------ | ------------- | ------------- | -------------
**ItemProtectionBox** | Create the box object from the bitstream and box size. | BitStreamReader <b>stream</b> - File stream.<br />ulong <b>size</b> - Box size in bytes.

[[Back to API_README]](API_README.md)