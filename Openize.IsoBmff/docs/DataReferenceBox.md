# Openize.IsoBmff.DataReferenceBox

The data reference object contains a table of data references (normally URLs) that declare the location(s) of the media data used within the presentation.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**ToString** | **string** | Text summary of the box. | 
**Children** | **ObservableCollection<Box>** | Observable collection of the nested boxes. | 

## Fields

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**entry_count** | **uint** | The count of data references. | 
**entries** | **List<DataEntryUrlBox>** | The list of data references. | 

## Constructors

Name | Description | Parameters
------------ | ------------- | ------------- | -------------
**DataReferenceBox** | Create the box object from the bitstream. | BitStreamReader <b>stream</b> - File stream.

[[Back to API_README]](API_README.md)