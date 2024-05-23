# Openize.IsoBmff.CleanApertureBox

The clean aperture transformative item property defines a cropping transformation of the input image.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**ToString** | **string** | Text summary of the box. | 

## Fields

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**cleanApertureWidthN** | **uint** | A numerator of the fractional number which defines the exact clean aperture width, in counted pixels, of the image. | 
**cleanApertureWidthD** | **uint** | A denominator of the fractional number which defines the exact clean aperture width, in counted pixels, of the image. | 
**cleanApertureHeightN** | **uint** | A numerator of the fractional number which defines the exact clean aperture height, in counted pixels, of the image. | 
**cleanApertureHeightD** | **uint** | A denominator of the fractional number which defines the exact clean aperture height, in counted pixels, of the image. | 
**horizOffN** | **int** | A numerator of the fractional number which defines the horizontal offset of clean aperture centre minus(width‐1)/2. Typically 0. | 
**horizOffD** | **uint** | A denominator of the fractional number which defines the horizontal offset of clean aperture centre minus(width‐1)/2. Typically 0. | 
**vertOffN** | **int** | A numerator of the fractional number which defines the vertical offset of clean aperture centre minus(height‐1)/2. Typically 0. | 
**vertOffD** | **uint** | A denominator of the fractional number which defines the vertical offset of clean aperture centre minus(height‐1)/2. Typically 0. | 

## Constructors

Name | Description | Parameters
------------ | ------------- | ------------- | -------------
**CleanApertureBox** | Create the box object from the bitstream and box size. | BitStreamReader <b>stream</b> - File stream.<br />ulong <b>size</b> - Box size in bytes.

[[Back to API_README]](API_README.md)