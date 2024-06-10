# Openize.IsoBmff.ItemInfoEntry

This box provides extra information about one entry.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**ToString** | **string** | Text summary of the box. | 

## Fields

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**item_ID** | **uint** | Contains either 0 for the primary resource (e.g., the XML contained in an 'xml ' box) or the ID of the item for which the following information is defined. | 
**item_protection_index** | **ushort** | Contains either 0 for an unprotected item, or the one‐based index into the item protection  box defining the protection applied to this item(the first box in the item protection box has the index 1). | 
**item_type** | **uint** | A 32‐bit value, typically 4 printable characters, that is a defined valid item type indicator, such as 'mime'. | 
**item_name** | **string** | A null‐terminated string in UTF‐8 characters containing a symbolic name of the item (source file for file delivery transmissions). | 
**content_type** | **string** | A null‐terminated string in UTF‐8 characters with the MIME type of the item.<br />If the item is content encoded (see below), then the content type refers to the item after content decoding. | 
**item_uri_type** | **string** | A string that is an absolute URI, that is used as a type indicator. | 
**content_encoding** | **string** | An optional null‐terminated string in UTF‐8 characters used to indicate that the binary file is encoded and needs to be decoded before interpreted. The values are as defined for Content‐Encoding for HTTP/1.1. | 
**item_hidden** | **bool** | A bool value, that shows if item is hidden. | 

## Constructors

Name | Description | Parameters
------------ | ------------- | ------------- | -------------
**ItemInfoEntry** | Create the box object from the bitstream. | BitStreamReader <b>stream</b> - File stream.

[[Back to API_README]](API_README.md)