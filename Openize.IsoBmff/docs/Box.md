# Openize.IsoBmff.Box

Structure for storing data in IsoBmff files.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**ToString** | **string** | Text summary of the box. | 

## Static methods

Name | Type | Description | Parameters
------------ | ------------- | ------------- | -------------
**ParceBox** | **Box** | Read next box from stream. | BitStreamReader <b>stream</b> - File stream reader.
**SetExternalConstructor** | **void** | Add external constructor for unimplemented box type. | BoxType <b>type</b> - Box type.<br />ExternalBoxConstructor <b>parser</b> - External box constructor.
**UintToString** | **string** | Convert uint value to string with ASCII coding. | uint <b>value</b> - Unsigned integer.

## Fields

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**size** | **ulong** | An integer that specifies the number of bytes in this box, including all its fields and contained boxes; if size is 1 then the actual size is in the field largesize; if size is 0, then this box is the last one in the file, and its contents extend to the end of the file. | 
**type** | **BoxType** | Identifies the box type; standard boxes use a compact type, which is normally four printable characters, to permit ease of identification, and is shown so in the boxes below. User extensions use an extended type; in this case, the type field is set to 'uuid'. | 

## Delegates

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**ExternalBoxConstructor** | **Box** | External box constructor for unimplemented box types. | BitStreamReader <b>stream</b> - Stream reader.<br />ulong <b>size</b> - Box size in bytes.

## Constructors

Name | Description | Parameters
------------ | ------------- | ------------- | -------------
**Box** | Create the box object from the bitstream. | BitStreamReader <b>stream</b> - File stream.
**Box** | Create the box object from the box type and box size in bytes.<br />This constructor doesn't read data from the stream. | BoxType <b>boxtype</b> - Box type integer.<br />ulong <b>size</b> - Box size in bytes.
**Box** | Create the box object from the bitstream and box type. | BitStreamReader <b>stream</b> - File stream.<br />BoxType <b>boxtype</b> - Box type integer.

[[Back to API_README]](API_README.md)



