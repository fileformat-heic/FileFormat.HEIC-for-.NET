# Openize.Heic.Decoder.HeicImage

Heic image class.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Header** | **HeicHeader** | Heic image header. Grants convinient access to IsoBmff container meta data. | 
**Frames** | **Dictionary<uint, HeicImageFrame>** | Dictionary of public Heic image frames with access by identifier. | 
**AllFrames** | **Dictionary<uint, HeicImageFrame>** | Dictionary of all Heic image frames with access by identifier. | 
**DefaultImage** | **HeicImageFrame** | Returns the default image frame, which is specified in meta data. | 
**Width** | **uint** | Width of the default image frame in pixels. | 
**Height** | **uint** | Height of the default image frame in pixels. | 

## Methods

Name | Type | Description | Parameters | Notes
------------ | ------------- | ------------- | ------------- | -------------
**Load** | **HeicImage** | Reads the file meta data and creates a class object for further decoding of the file contents. | Stream <b>stream</b> - File stream. | This operation does not decode pixels.<br />Use the default frame methods GetByteArray or GetInt32Array afterwards in order to decode pixels.
**CanLoad** | **bool** | Checks if the stream can be read as a heic image.<br />Returns true if file header contains heic signarure, false otherwise | Stream <b>stream</b> - File stream. | 

[[Back to API_README]](API_README.md)