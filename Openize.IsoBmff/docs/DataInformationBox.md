# Openize.IsoBmff.DataInformationBox

The data information box contains objects that declare the location of the media information in a track.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**ToString** | **string** | Text summary of the box. | 
**Children** | **ObservableCollection<Box>** | Observable collection of the nested boxes. | 

## Fields

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**dref** | **DataReferenceBox** | The data reference object contains a table of data references (normally URLs) that declare the location(s) of the media data used within the presentation. | 

## Constructors

Name | Description | Parameters
------------ | ------------- | ------------- | -------------
**DataInformationBox** | Create the box object from the bitstream and box size. | BitStreamReader <b>stream</b> - File stream.<br />ulong <b>size</b> - Box size in bytes.

[[Back to API_README]](API_README.md)