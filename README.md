# Openize.Heic

Openize.Heic is an open source SDK implementing the ISO/IEC 23008-12:2017 HEIF file format decoder.

It is written from scratch and has a plain C# API to enable a simple integration into other software.

## Supported features

Openize.Heic has support for:
* HEIC coded static images;
  * I slices;
  * 4:2:0, 4:2:2 and 4:4:4 chroma subsampling.
* HEIC coded animations that use several I‑slices;
* multiple images in a file;
* alpha channels, depth maps, thumbnails, auxiliary images;
* correct color transform according to embedded color profiles;
* image transformations (crop, mirror, rotate), overlay images.

Openize.Heic doesn't support:
* HDR images;
* reading EXIF and XMP metadata;
* color transform according to EXIF contained color profiles;
* HEIC coded animations that use P and B‑slices;
* deblocking filter.

## Usage examples
### Read .heic file to int array with Argb32 data
```C#
using (var fs = new FileStream("filename.heic", FileMode.Open))
{
    HeicImage image = HeicImage.Load(fs);
    int[] pixels = image.GetInt32Array(Heic.Decoder.PixelFormat.Argb32);
}
```

### Convert .heic file to .png
```C#
using (var fs = new FileStream("filename.heic", FileMode.Open))
{
    HeicImage image = HeicImage.Load(fs);
     
    var pixels = image.GetByteArray(Heic.Decoder.PixelFormat.Bgra32);
    var width = (int)image.Width;
    var height = (int)image.Height;
     
    var wbitmap = new WriteableBitmap(width, height, 72, 72, PixelFormats.Bgra32, null);
    var rect = new Int32Rect(0, 0, width, height);
    wbitmap.WritePixels(rect, pixels, 4 * width, 0);
    
    using FileStream saveStream = new FileStream("output.png", FileMode.OpenOrCreate);
    PngBitmapEncoder encoder = new PngBitmapEncoder();
    encoder.Frames.Add(BitmapFrame.Create(wbitmap));
    encoder.Save(saveStream);
}
```

### Convert .heic file to .jpg
```C#
using (var fs = new FileStream("filename.heic", FileMode.Open))
{
    HeicImage image = HeicImage.Load(fs);
     
    var pixels = image.GetByteArray(Heic.Decoder.PixelFormat.Bgra32);
    var width = (int)image.Width;
    var height = (int)image.Height;
     
    var wbitmap = new WriteableBitmap(width, height, 72, 72, PixelFormats.Bgra32, null);
    var rect = new Int32Rect(0, 0, width, height);
    wbitmap.WritePixels(rect, pixels, 4 * width, 0);
    
    using FileStream saveStream = new FileStream("output.jpg", FileMode.OpenOrCreate);
    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
    encoder.Frames.Add(BitmapFrame.Create(wbitmap));
    encoder.Save(saveStream);
}
```

### Convert .heic collection to a set of .png files
```C#
using (var fs = new FileStream("filename.heic", FileMode.Open))
{
    HeicImage image = HeicImage.Load(fs);

    foreach (var key in image.Frames.Keys)
    {
        var width = (int)image.Frames[key].Width;
        var height = (int)image.Frames[key].Height;
        var pixels = image.Frames[key].GetByteArray(Openize.Heic.Decoder.PixelFormat.Bgra32);

        var wbitmap = new WriteableBitmap(width, height, 72, 72, PixelFormats.Bgra32, null);
        var rect = new Int32Rect(0, 0, width, height);
        wbitmap.WritePixels(rect, pixels, 4 * width, 0);

        using FileStream saveStream = new FileStream("output"+key+".png", FileMode.OpenOrCreate);
        PngBitmapEncoder encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(wbitmap));
        encoder.Save(saveStream);
    }
}
```

## Documentation

All public classes, methods and properties are documented in corresponding API_README:
* [/Openize.Heic.Decoder/docs/API_README.md](/Openize.Heic.Decoder/docs/API_README.md) for Openize.Heic.Decoder;
* [/Openize.IsoBmff/docs/API_README.md](/Openize.IsoBmff/docs/API_README.md) for Openize.IsoBmff.

### HeicImage

#### Methods
Name | Type | Description | Parameters | Notes
------------ | ------------- | ------------- | ------------- | -------------
**Load** | **HeicImage** | Reads the file meta data and creates a class object for further decoding of the file contents. | `Stream stream` - File stream. | This operation does not decode pixels.<br />Use the default frame methods GetByteArray or GetInt32Array afterwards in order to decode pixels.
**CanLoad** | **bool** | Checks if the stream can be read as a heic image.<br />Returns true if file header contains heic signarure, false otherwise | `Stream stream` - File stream. | 

#### Properties
Name | Type | Description
------------ | ------------- | ------------- 
**Frames** | **Dictionary<uint, HeicImageFrame>** | Dictionary of public Heic image frames with access by identifier. 
**AllFrames** | **Dictionary<uint, HeicImageFrame>** | Dictionary of all Heic image frames with access by identifier. 
**DefaultImage** | **HeicImageFrame** | Returns the default image frame, which is specified in meta data. 

### HeicImageFrame

#### Methods
Name | Type | Description | Parameters
------------ | ------------- | ------------- | -------------
**GetByteArray** | **byte[]** | Get pixel data in the format of byte array.<br />Each three or four bytes (the count depends on the pixel format) refer to one pixel left to right top to bottom line by line. | `PixelFormat pixelFormat` - Pixel format that defines the order of colors and the presence of alpha byte.<br />`Rectangle boundsRectangle` - Bounds of the requested area.
**GetInt32Array** | **int[]** | Get pixel data in the format of integer array.<br />Each int value refers to one pixel left to right top to bottom line by line. | `PixelFormat pixelFormat` - Pixel format that defines the order of colors.<br />`Rectangle boundsRectangle` - Bounds of the requested area.
**GetTextData** | **string** | Get frame text data.<br />Exists only for mime frame types. | 

### Properties
Name | Type | Description
------------ | ------------- | ------------- 
**ImageType** | **ImageFrameType** | Type of an image frame content.
**Width** | **uint** | Width of the image frame in pixels. 
**Height** | **uint** | Height of the image frame in pixels.
**HasAlpha** | **bool** | Indicates the presence of transparency of transparency layer.<br />True if frame is linked with alpha data frame, false otherwise.
**IsHidden** | **bool** | Indicates the fact that frame is marked as hidden.<br />True if frame is hidden, false otherwise.
**IsImage** | **bool** | Indicates the fact that frame contains image data.<br />True if frame is image, false otherwise.
**IsDerived** | **bool** | Indicates the fact that frame contains image transform data and is inherited from another frame(-s).<br />True if frame is derived, false otherwise.
**DerivativeType** | **BoxType?** | Indicates the type of derivative content if the frame is derived.
**AuxiliaryReferenceType** | **AuxiliaryReferenceType** | Indicates the type of auxiliary reference layer if the frame type is auxiliary.
**NumberOfChannels** | **byte** | Number of channels with color data.
**BitsPerChannel** | **byte[]** | Bits per channel with color data.

## License
Openize.HEIC is available under [Openize License](LICENSE).
> [!CAUTION]
> Openize does not and cannot grant You a patent license for the utilization of HEVC/H.265 image compression/decompression technologies.

Openize.HEIC uses Openize.IsoBmff that is distributed under [MIT License](/Openize.IsoBmff/LICENSE).

## OSS Notice
Sample files used for tests and located in the "./Openize.Heic.Tests/TestsData/samples/nokia" folder belong to Nokia Technologies and are used according to [Nokia High-Efficiency Image File Format (HEIF) License](https://github.com/nokiatech/heif/blob/master/LICENSE.TXT)

> Licensed Field means the non-commercial purposes of evaluation, testing and academic research in each non-commercial case to use, run, modify (in a way that still complies with the Specification) and copy the Software to (a) generate, using one or more encoded pictures as inputs, a file complying with the Specification and including the one or more encoded pictures that were given as inputs; and/or (b) read a file complying with the Specification, resulting into one or more encoded pictures included in the file as outputs.
