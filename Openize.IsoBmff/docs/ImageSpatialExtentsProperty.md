# Openize.IsoBmff.ImageSpatialExtentsProperty

The ImageSpatialExtentsProperty documents the width and height of the associated image item.
Every image item shall be associated with one property of this type, prior to the association of all transformative properties.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**ToString** | **string** | Text summary of the box. | 

## Fields

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**image_width** | **uint** | The width of the reconstructed image in pixels. | 
**image_height** | **uint** | The height of the reconstructed image in pixels. | 

## Constructors

Name | Description | Parameters
------------ | ------------- | ------------- | -------------
**ImageSpatialExtentsProperty** | Create the box object from the bitstream and box size. | BitStreamReader <b>stream</b> - File stream.<br />ulong <b>size</b> - Box size in bytes.

[[Back to API_README]](API_README.md)