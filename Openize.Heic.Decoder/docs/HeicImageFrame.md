# Openize.Heic.Decoder.HeicImageFrame

Heic image frame class.
Contains hevc coded data or meta data.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**ImageType** | **ImageFrameType** | Type of an image frame content. | 
**Width** | **uint** | Width of the image frame in pixels. | 
**Height** | **uint** | Height of the image frame in pixels. | 
**HasAlpha** | **bool** | Indicates the presence of transparency of transparency layer.<br />True if frame is linked with alpha data frame, false otherwise. | 
**IsDerived** | **bool** | Indicates the fact that frame contains only transform data and is inherited from another frame.<br />True if frame is derived, false otherwise. | 
**NumberOfChannels** | **byte** | Number of channels with color data. | 
**BitsPerChannel** | **byte[]** | Bits per channel with color data. | 

## Methods

Name | Type | Description | Parameters
------------ | ------------- | ------------- | -------------
**GetByteArray** | **byte[]** | Get pixel data in the format of byte array.<br />Each three or four bytes (the count depends on the pixel format) refer to one pixel left to right top to bottom line by line. | PixelFormat <b>pixelFormat</b> - Pixel format that defines the order of colors and the presence of alpha byte.<br />Rectangle <b>boundsRectangle</b> - Bounds of the requested area.
**GetInt32Array** | **int[]** | Get pixel data in the format of integer array.<br />Each int value refers to one pixel left to right top to bottom line by line. | PixelFormat <b>pixelFormat</b> - Pixel format that defines the order of colors.<br />Rectangle <b>boundsRectangle</b> - Bounds of the requested area.

## Fields

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**hvcConfig** | **HEVCDecoderConfigurationRecord** | Hevc decoder configuration information from Isobmff container. | 
**rawPixels** | **ushort[][,]** | Raw YUV pixel data. <br />Multidimantional array: chroma or luma index, then two-dimentional array with x and y navigation. | 

[[Back to API_README]](API_README.md)