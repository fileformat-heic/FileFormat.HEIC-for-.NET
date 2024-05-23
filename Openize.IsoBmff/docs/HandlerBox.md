# Openize.IsoBmff.HandlerBox

This box declares media type of the track, and thus the process by which the media‐data in the track is presented.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**ToString** | **string** | Text summary of the box. | 

## Fields

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**handler_type** | **uint** | When present in a media box, contains a value as defined in clause 12, or a value from a derived specification, or registration.<br />When present in a meta box, contains an appropriate value to indicate the format of the meta box contents.<br />The value 'null' can be used in the primary meta box to indicate that it is merely being used to hold resources. | 
**name** | **string** | A null‐terminated string in UTF‐8 characters which gives a human‐readable name for the track type (for debugging and inspection purposes). | 

## Constructors

Name | Description | Parameters
------------ | ------------- | ------------- | -------------
**HandlerBox** | Create the box object from the bitstream. | BitStreamReader <b>stream</b> - File stream.

[[Back to API_README]](API_README.md)