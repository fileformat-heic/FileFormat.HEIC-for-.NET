# Openize.Heic.Decoder.IO.BitStreamWithNalSupport

The BitStreamWithNalSupport class is designed to read bits from a specified stream.
It allows to ignore specified byte sequences while reading.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**CurrentImageId** | **uint** | Dictionary of images context information. | 

## Methods

Name | Type | Description | Parameters
------------ | ------------- | ------------- | -------------
**CreateNewImageContext** | **void** | Creates an image context object. | uint <b>imageId</b> - Image identificator.
**DeleteImageContext** | **void** | Deletes the image context object by id. | uint <b>imageId</b> - Image identificator.
**TurnOnNalUnitMode** | **void** | Turns on Nal Unit reader mode which ignores specified by standart byte sequences. | 
**TurnOffNulUnitMode** | **void** | Turns off Nal Unit reader mode. | 
**Read** | **int** | Reads the specified number of bits from the stream. | int <b>bitCount</b> - The required number of bits to read.
**ReadString** | **string** | Reads bytes as ASCII characters until '\0'. | 
**ReadFlag** | **bool** | Reads one bit and returns true if it is 1, otherwise false. | 
**SkipBits** | **void** | Skip the specified number of bits in the stream. | int <b>bitsNumber</b> - Number of bits to skip.
**ReadUev** | **uint** | Read an unsigned integer 0-th order Exp-Golomb-coded syntax element with the left bit first. | 
**ReadSev** | **int** | Read an signed integer 0-th order Exp-Golomb-coded syntax element with the left bit first. | 

## Constructors

Name | Description | Parameters
------------ | ------------- | ------------- | -------------
**BitStreamWithNalSupport** | Creates a class object with a stream object and an optional buffer size as parameters. | Stream <b>stream</b> - The source stream.<br />int <b>bufferSize</b> = 4 - The buffer size. 

[[Back to API_README]](API_README.md)