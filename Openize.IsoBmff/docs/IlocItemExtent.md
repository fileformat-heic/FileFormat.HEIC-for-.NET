# Openize.IsoBmff.IlocItemExtent

Data class for organised storage on location data extents.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**ToString** | **string** | Text summary of the box. | 

## Fields

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**index** | **uint** | An index as defined for the constructionmethod. | 
**offset** | **uint** | The absolute offset, in bytes from the data origin of the container, of this extent data.<br />If offset_size is 0, extent_offset takes the value 0. | 
**length** | **uint** | The absolute length in bytes of this metadata item extent.<br />If length_size is 0, extent_length takes the value 0.<br />If the value is 0, then length of the extent is the length of the entire referenced container. | 

[[Back to API_README]](API_README.md)