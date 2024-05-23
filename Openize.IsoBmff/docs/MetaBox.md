# Openize.IsoBmff.MetaBox

A common base structure that contains general metadata.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**ToString** | **string** | Text summary of the box. | 
**Children** | **ObservableCollection<Box>** | Observable collection of the nested boxes. | 
**hdlr** | **HandlerBox** | Handler box. | 
**pitm** | **PrimaryItemBox** | Primary item box. | 
**iloc** | **ItemLocationBox** | Item location box. | 
**iinf** | **ItemInfoBox** | Item info box. | 
**iprp** | **ItemPropertiesBox** | Item properties box. | 
**iref** | **ItemReferenceBox** | Item reference box. | 
**idat** | **ItemDataBox** | Item data box. | 

## Methods

Name | Type | Description | Parameters
------------ | ------------- | ------------- | -------------
**TryGetBox** | **Box** | Try to get specified box. Return null if required box not available. | BoxType <b>type</b> - Box type.

## Fields

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**boxes** | **List<Box>** | List of nested boxes. | 

## Constructors

Name | Description | Parameters
------------ | ------------- | ------------- | -------------
**MetaBox** | Create the box object from the bitstream, box size and start position. | BitStreamReader <b>stream</b> - File stream.<br />ulong <b>size</b> - Box size in bytes.<br />ulong <b>startPos</b> - Start position in bits.

[[Back to API_README]](API_README.md)