# Openize.IsoBmff.RelativeLocationProperty

The RelativeLocationProperty descriptive item property is used to describe the horizontal and vertical position of the reconstructed image of the associated image item relative to the reconstructed image of the related image item identified through the 'tbas' item reference.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**ToString** | **string** | Text summary of the box. | 

## Fields

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**horizontal_offset** | **uint** | Specifies the horizontal offset in pixels of the left-most pixel column of the reconstructed image of the associated image item in the reconstructed image of the related image item. The left-most pixel column of the reconstructed image of the related image item has a horizontal offset equal to 0. | 
**horizontal_offset** | **uint** | Specifies the vertical offset in pixels of the top-most pixel row of the reconstructed image of the associated image item in the reconstructed image of the related image item. The top-most pixel row of the reconstructed image of the related image item has a vertical offset equal to 0. | 

## Constructors

Name | Description | Parameters
------------ | ------------- | ------------- | -------------
**RelativeLocationProperty** | Create the box object from the bitstream and box size. | BitStreamReader <b>stream</b> - File stream.<br />ulong <b>size</b> - Box size in bytes.

[[Back to API_README]](API_README.md)