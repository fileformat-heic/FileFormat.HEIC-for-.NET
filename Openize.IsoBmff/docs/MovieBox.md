# Openize.IsoBmff.MovieBox

The metadata for a presentation is stored in the single Movie Box which occurs at the top‐level of a file. 

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**ToString** | **string** | Text summary of the box. | 
**Children** | **ObservableCollection<Box>** | Observable collection of the nested boxes. | 

## Fields

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**boxes** | **List<Box>** | List of nested boxes. | 

## Constructors

Name | Description | Parameters
------------ | ------------- | ------------- | -------------
**MovieBox** | Create the box object from the bitstream, box size and start position. | BitStreamReader <b>stream</b> - File stream.<br />ulong <b>size</b> - Box size in bytes.<br />ulong <b>startPos</b> - Start position in bits.

[[Back to API_README]](API_README.md)