# Openize.IsoBmff.GroupsListBox

An entity group is a grouping of items, which may also group tracks. The entities in an entity group share a particular characteristic or have a particular relationship, as indicated by the grouping type.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**ToString** | **string** | Text summary of the box. | 
**Children** | **ObservableCollection<Box>** | Observable collection of the nested boxes. | 

## Fields

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**boxes** | **List<Box>** | List of nested boxes. | 

## Constructors

Name | Description | Parameters
------------ | ------------- | ------------- | -------------
**GroupsListBox** | Create the box object from the bitstream and box size. | BitStreamReader <b>stream</b> - File stream.<br />ulong <b>size</b> - Box size in bytes.

[[Back to API_README]](API_README.md)