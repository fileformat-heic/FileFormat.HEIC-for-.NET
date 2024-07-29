# FileFormat.Heic.Decoder.HeicImage

Heic image class.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Header** | **HeicHeader** | Heic image header. Grants convinient access to IsoBmff container meta data. | 
**Frames** | **Dictionary<uint, HeicImageFrame>** | Dictionary of public Heic image frames with access by identifier. | 
**AllFrames** | **Dictionary<uint, HeicImageFrame>** | Dictionary of all Heic image frames with access by identifier. | 
**DefaultFrame** | **HeicImageFrame** | Returns the default image frame, which is specified in meta data. | 
**Width** | **uint** | Width of the default image frame in pixels. | 
**Height** | **uint** | Height of the default image frame in pixels. | 

## Methods

Name | Type | Description | Parameters | Notes
------------ | ------------- | ------------- | ------------- | -------------
**Load** | **HeicImage** | Reads the file meta data and creates a class object for further decoding of the file contents. | Stream <b>stream</b> - File stream. | This operation does not decode pixels.<br />Use the default frame methods GetByteArray or GetInt32Array afterwards in order to decode pixels.
**CanLoad** | **bool** | Checks if the stream can be read as a heic image.<br />Returns true if file header contains heic signarure, false otherwise | Stream <b>stream</b> - File stream. | 
**GetByteArray** | **byte[]** | Get pixel data of the default image frame in the format of byte array.<br />Each three or four bytes (the count depends on the pixel format) refer to one pixel left to right top to bottom line by line.<br />Returns null if frame does not contain image data. | PixelFormat <b>pixelFormat</b> - Pixel format that defines the order of colors and the presence of alpha byte.<br />Rectangle <b>boundsRectangle</b> - Bounds of the requested area.
**GetInt32Array** | **int[]** | Get pixel data of the default image frame in the format of integer array.<br />Each int value refers to one pixel left to right top to bottom line by line.<br />Returns null if frame does not contain image data. | PixelFormat <b>pixelFormat</b> - Pixel format that defines the order of colors.<br />Rectangle <b>boundsRectangle</b> - Bounds of the requested area.

[[Back to API_README]](API_README.md)