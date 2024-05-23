# Openize.Heic.Decoder.ImageFrameType

Type of an image frame.

## Enumeration

Name | Value | Description | Notes
------------ | ------------- | ------------- | -------------
**hvc1** | 0x68766331 | HEVC coded image frame.| 
**iden** | 0x6964656e | Identity transformation.<br />Cropping and/or rotation by 90, 180, or 270 degrees, imposed through the respective transformative properties.| 
**iovl** | 0x696f766c | Image Overlay.<br />Overlaying any number of input images in indicated order and locations onto the canvas of the output image. | 
**grid** | 0x67726964 | Image Grid.<br />Reconstructing a grid of input images of the same width and height. | 

[[Back to API_README]](API_README.md)