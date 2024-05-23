# Openize.IsoBmff.ColourInformationBox

Contains colour information about the image.
If colour information is supplied in both this box, and also in the video bitstream, this box takes precedence, and over‐rides the information in the bitstream.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**ToString** | **string** | Text summary of the box. | 

## Fields

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**colour_type** | **uint** | An indication of the type of colour information supplied. | 
**colour_primaries** | **ushort** | Indicates the chromaticity coordinates of the source primaries. | 
**transfer_characteristics** | **ushort** | Indicates the reference opto-electronic transfer characteristic. | 
**matrix_coefficients** | **ushort** | Describes the matrix coefficients used in deriving luma and chroma signals from the green, blue, and red. | 
**full_range_flag** | **bool** | Indicates the black level and range of the luma and chroma signals as derived from E′Y, E′PB, and E′PR or E′R, E′G, and E′B real-valued component signals. | 

## Constructors

Name | Description | Parameters
------------ | ------------- | ------------- | -------------
**ColourInformationBox** | Create the box object from the bitstream and box size. | BitStreamReader <b>stream</b> - File stream.<br />ulong <b>size</b> - Box size in bytes.

[[Back to API_README]](API_README.md)