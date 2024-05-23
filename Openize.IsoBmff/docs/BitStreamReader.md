# Openize.IsoBmff.BitStreamReader

The BitStreamReader class is designed to read bits from a specified stream.
It reads a minimal amount of bytes from the stream into an intermediate buffer and then reads the bits from the buffer, returning the read value.
If there is still enough data in the buffer, the data is read from it.

## Methods

Name | Type | Description | Parameters
------------ | ------------- | ------------- | -------------
**GetBitPosition** | **ulong** | Gets the current position within the bitstream.<br />The bitstream position is x8 of stream position, adjusted according to the number of bits read from the latest byte. | 
**SetBytePosition** | **void** | Sets the current position within the bitstream. | long <b>bytePosition</b> - The new byte position within the bitstream.
**ByteAligned** | **bool** | Indicates if the current position in the bitstream is on a byte boundary.<br />Returns true if the current position in the bitstream is on a byte boundary, false otherwise. | 
**MoreData** | **bool** | Indicates if there are more data in the bitstream.<br />True if there are more data in the bitstream, false otherwise. | 
**FillBufferFromStream** | **int** | Fill reader buffer with data from stream.<br />Returns the tolal amount of bytes read into the buffer. | 
**Read** | **int** | Reads the specified number of bits from the stream.<br />Returns the integer value. | int <b>bitCount</b> - The required number of bits to read.
**ReadString** | **string** | Reads bytes as ASCII characters until '\0'.<br />Returns the string value. | 
**ReadFlag** | **bool** | Reads one bit and returns true if it is 1, otherwise false.<br />Returns the boolean value. | 
**Peek** | **int** | Peeks the specified number of bits from the stream.<br />This method does not change the position of the underlying stream, state of the reader remains unchanged.<br />Returns the integer value. | int <b>bitCount</b> - The required number of bits to read.
**SkipBits** | **void** | Skip the specified number of bits in the stream. | int <b>bitCount</b> - Number of bits to skip.
**GetBit** | **int** | Reads bit at the specified position.<br />This method does not change the position of the underlying stream, state of the reader remains unchanged.<br />This method is approximately 50% slower than the Read method. | long <b>position</b> - The position of stream in bits to read.

## Fields

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**stream** | **Stream** | File stream. | 
**state** | **BitReaderState** | Bit reader state. | 

## Constructors

Name | Description | Parameters
------------ | ------------- | ------------- | -------------
**BitStreamReader** | The constructor takes a Stream object and an optional buffer size as parameters. | Stream <b>stream</b> - The source stream.<br />int <b>bufferSize</b> - The buffer size.

[[Back to API_README]](API_README.md)