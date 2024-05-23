# Openize.IsoBmff.ItemInfoBox

The item information box provides extra information about file entries.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**ToString** | **string** | Text summary of the box. | 
**Children** | **ObservableCollection<ItemInfoEntry>** | Observable collection of entries of extra information. | 

## Fields

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**entry_count** | **uint** | A count of the number of entries in the info entry array. | 
**item_infos** | **ItemInfoEntry[]** | Array of entries of extra information, each entry is formatted as a box.<br />This array is sorted by increasing item_ID in the entry records. | 

## Constructors

Name | Description | Parameters
------------ | ------------- | ------------- | -------------
**ItemInfoBox** | Create the box object from the bitstream and box size. | BitStreamReader <b>stream</b> - File stream.<br />ulong <b>size</b> - Box size in bytes.

[[Back to API_README]](API_README.md)