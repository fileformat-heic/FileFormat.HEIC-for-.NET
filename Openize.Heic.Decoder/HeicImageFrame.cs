﻿/*
 * Openize.HEIC 
 * Copyright (c) 2024 Openize Pty Ltd. 
 *
 * This file is part of Openize.HEIC.
 *
 * Openize.HEIC is available under Openize license, which is
 * available along with Openize.HEIC sources.
 */

using Openize.Heic.Decoder.IO;
using Openize.IsoBmff;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Openize.Heic.Decoder
{
    /// <summary>
    /// Heic image frame class.
    /// Contains hevc coded data or meta data.
    /// </summary>
    public class HeicImageFrame
    {
        #region Private Fields

        private uint id;
        private IlocItem locationBox;
        private HeicImage parent;
        private BitStreamWithNalSupport stream;
        private bool cashed;

        private uint ispeWidth;
        private uint ispeHeight;

        #endregion

        #region Internal Fields

        /// <summary>
        /// Hevc decoder configuration information from Isobmff container. 
        /// </summary>
        internal HEVCDecoderConfigurationRecord hvcConfig;

        /// <summary>
        /// Raw YUV pixel data.
        /// Multidimantional array: chroma or luma index, then two-dimentional array with x and y navigation.
        /// </summary>
        internal ushort[][,] rawPixels;

        #endregion

        #region Public Properties

        /// <summary>
        /// Type of an image frame content.
        /// </summary>
        public ImageFrameType ImageType { get; }

        /// <summary>
        /// Width of the image frame in pixels.
        /// </summary>
        public uint Width {
            get => imageRotationAngle % 2 == 0 ? ispeWidth : ispeHeight;
        }

        /// <summary>
        /// Height of the image frame in pixels.
        /// </summary>
        public uint Height
        {
            get => imageRotationAngle % 2 == 0 ? ispeHeight : ispeWidth;
        }

        /// <summary>
        /// Indicates the presence of transparency of transparency layer.
        /// </summary>
        /// <returns>True if frame is linked with alpha data frame, false otherwise.</returns>
        public bool HasAlpha => alphaReference != null;

        /// <summary>
        /// Indicates the fact that frame contains only transform data and is inherited from another frame.
        /// </summary>
        /// <returns>True if frame is derived, false otherwise.</returns>
        public bool IsDerived { get; }

        /// <summary>
        /// 
        /// </summary>
        public BoxType? DerivativeType { get; private set; }

        /// <summary>
        /// Number of channels with color data.
        /// </summary>
        public byte NumberOfChannels { get; set; }

        /// <summary>
        /// Bits per channel with color data.
        /// </summary>
        public byte[] BitsPerChannel { get; set; }

        #endregion

        #region Private Properties

        /// <summary>
        /// Identificator of the layer with alpha data.
        /// </summary>
        private uint? alphaReference { get; set; } = null;

        /// <summary>
        /// Image rotation angle data in anti-clockwise direction in units of 90 degrees.
        /// <para>To get the degree units this value has to be multiplied by 90.</para>
        /// </summary>
        private byte imageRotationAngle { get; set; }

        /// <summary>
        /// Specifies a vertical (axis = 1) or horizontal (axis = 2) axis for the mirroring operation.
        /// </summary>
        private byte imageMirrorAxis { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Create Heic image frame object.
        /// </summary>
        /// <param name="stream">File stream.</param>
        /// <param name="parent">Parent image.</param>
        /// <param name="id">Frame identificator.</param>
        /// <param name="properties">Frame properties described in container.</param>
        internal HeicImageFrame(BitStreamWithNalSupport stream, HeicImage parent, uint id, List<Box> properties)
        {
            ImageType = (ImageFrameType)parent.Header.GetInfoBoxById(id).item_type;

            IsDerived = ImageType == ImageFrameType.iden || ImageType == ImageFrameType.iovl || ImageType == ImageFrameType.grid;

            locationBox = parent.Header.GetLocationBoxById(id);

            this.id = id;
            this.stream = stream;
            this.parent = parent;
            cashed = false;

            LoadProperties(stream, properties);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get pixel data in the format of byte array.
        /// <para>Each three or four bytes (the count depends on the pixel format) refer to one pixel left to right top to bottom line by line.</para>
        /// </summary>
        /// <param name="pixelFormat">Pixel format that defines the order of colors and the presence of alpha byte.</param>
        /// <param name="boundsRectangle">Bounds of the requested area.</param>
        /// <returns>Byte array</returns>
        public byte[] GetByteArray(PixelFormat pixelFormat, Rectangle boundsRectangle = default)
        {
            boundsRectangle = ValidateBounds(boundsRectangle);

            byte[,,] threeDim = GetMultidimArray(boundsRectangle);
            threeDim = ApplyPixelFormat(threeDim, pixelFormat);

            int bpp = pixelFormat == PixelFormat.Rgb24 ? 3 : 4;
            byte[] output = new byte[boundsRectangle.Height * boundsRectangle.Width * bpp];

            int index = 0;
            for (int row = boundsRectangle.Top; row < boundsRectangle.Bottom; row++)
            {
                for (int col = boundsRectangle.Left; col < boundsRectangle.Right; col++)
                {
                    output[index++] = threeDim[col, row, 0];
                    output[index++] = threeDim[col, row, 1];
                    output[index++] = threeDim[col, row, 2];
                    if (bpp == 4) output[index++] = threeDim[col, row, 3];
                }
            }

            return output;
        }

        /// <summary>
        /// Get pixel data in the format of integer array. 
        /// <para>Each int value refers to one pixel left to right top to bottom line by line.</para>
        /// </summary>
        /// <param name="pixelFormat">Pixel format that defines the order of colors.</param>
        /// <param name="boundsRectangle">Bounds of the requested area.</param>
        /// <returns>Integer array</returns>
        public int[] GetInt32Array(PixelFormat pixelFormat, Rectangle boundsRectangle = default)
        {
            boundsRectangle = ValidateBounds(boundsRectangle);

            byte[,,] threeDim = GetMultidimArray(boundsRectangle);
            threeDim = ApplyPixelFormat(threeDim, pixelFormat);

            int[] output = new int[boundsRectangle.Height * boundsRectangle.Width];

            int index = 0;
            for (int row = boundsRectangle.Top; row < boundsRectangle.Bottom; row++)
            {
                for (int col = boundsRectangle.Left; col < boundsRectangle.Right; col++)
                {
                    output[index++] =
                        threeDim[col, row, 0] << 24 |
                        threeDim[col, row, 1] << 16 |
                        threeDim[col, row, 2] << 8 |
                        threeDim[col, row, 3] << 0;
                }
            }

            return output;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Add layer reference to the frame.
        /// Used to link reference layers that are reverce-linked (alpha layer, depth map, hdr).
        /// </summary>
        /// <param name="id">Layer identificator.</param>
        /// <param name="type">Layer type.</param>
        internal void AddLayerReference(uint id, HeicFrameReferenceType type)
        {
            switch (type)
            {
                case HeicFrameReferenceType.Alpha:
                    alphaReference = id;
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region Private Methods

        private byte[,,] GetMultidimArray(Rectangle boundsRectangle)
        {
            byte[,,] rgba;
            if (!IsDerived)
            {
                if (!cashed)
                    LoadHvcRawPixels();

                DerivativeType = parent.Header.GetDerivedType(id);

                var converter = new YuvConverter(this);
                rgba = converter.GetRgbaByteArray();
            }
            else
            {
                byte[] databox = locationBox.construction_method == 1 ?
                    parent.Header.GetItemDataBoxContent(
                        locationBox.base_offset + locationBox.extents[0].offset,
                        locationBox.extents[0].length) :
                    new byte[0];

                switch (ImageType)
                {
                    case ImageFrameType.iden:
                        var derived = parent.Header.GetDerivedList(id);
                        HeicImageFrame frame = parent.Frames[derived[0]];
                        rgba = frame.GetMultidimArray(new Rectangle(0, 0, (int)frame.Width, (int)frame.Height));
                        break;
                    case ImageFrameType.iovl:
                        rgba = GetByteArrayForOverlay(databox);
                        break;
                    case ImageFrameType.grid:
                        rgba = GetByteArrayForGrid(databox);
                        break;
                    default:
                        throw new Exception("Unknown derived image.");
                }
            }

            rgba = AddAlphaLayer(rgba, boundsRectangle);

            return TransformImage(rgba);
        }

        private Rectangle ValidateBounds(Rectangle boundsRectangle)
        {
            if (boundsRectangle == default)
                return new Rectangle(0, 0, (int)Width, (int)Height);

            if (boundsRectangle.Left < 0 || boundsRectangle.Top < 0 ||
                boundsRectangle.Right > (int)Width || boundsRectangle.Bottom > (int)Height)
                throw new ArgumentOutOfRangeException("The specification of selection area cannot exceed the size of an image.",
                    new ArgumentOutOfRangeException("boundsRectangle"));

            return boundsRectangle;
        }

        private byte[,,] AddAlphaLayer(byte[,,] pixels, Rectangle boundsRectangle)
        {
            if (alphaReference == null)
                return pixels;

            var alpha = parent.Frames[alphaReference ?? 0].GetMultidimArray(boundsRectangle);

            for (uint row = 0; row < Height; row++)
            {
                for (uint col = 0; col < Width; col++)
                {
                    pixels[col, row, 3] = alpha[col, row, 0];
                }
            }
            return pixels;
        }

        private byte[,,] TransformImage(byte[,,] pixels)
        {
            if (imageRotationAngle == 0 && imageMirrorAxis == 0)
                return pixels;

            byte[,,] rotated = new byte[Width, Height, 4];

            uint oldCol, oldRow;

            for (uint newRow = 0; newRow < Height; newRow++)
            {
                for (uint newCol = 0; newCol < Width; newCol++)
                {
                    oldCol = imageRotationAngle switch
                    {
                        3 => newRow,
                        2 => Height - newCol - 1,
                        1 => Height - newRow - 1,
                        0 => newCol
                    };

                    oldRow = imageRotationAngle switch
                    {
                        3 => Width - newCol - 1,
                        2 => Width - newRow - 1,
                        1 => newCol,
                        0 => newRow
                    };

                    if (imageMirrorAxis == 1) // vertical
                        oldRow = Height - oldRow;
                    else if (imageMirrorAxis == 2) // horisontal
                        oldCol = Width - oldCol;

                    for (int i = 0; i < 4; i++)
                        rotated[newCol, newRow, i] = pixels[oldCol, oldRow, i];
                }
            }
            return rotated;
        }

        private byte[,,] ApplyPixelFormat(byte[,,] rgba, PixelFormat pixelFormat)
        {
            if (pixelFormat == PixelFormat.Rgba32 || pixelFormat == PixelFormat.Rgb24)
                return rgba;

            byte r, g, b, a;

            for (uint row = 0; row < Height; row++)
            {
                for (uint col = 0; col < Width; col++)
                {
                    r = rgba[col, row, 0];
                    g = rgba[col, row, 1];
                    b = rgba[col, row, 2];
                    a = rgba[col, row, 3];

                    switch (pixelFormat)
                    {
                        case PixelFormat.Argb32:
                            rgba[col, row, 0] = a;
                            rgba[col, row, 1] = r;
                            rgba[col, row, 2] = g;
                            rgba[col, row, 3] = b;
                            break;
                        case PixelFormat.Bgra32:
                            rgba[col, row, 0] = b;
                            rgba[col, row, 1] = g;
                            rgba[col, row, 2] = r;
                            rgba[col, row, 3] = a;
                            break;
                    }
                }
            }
            return rgba;
        }

        private ushort[][,] LoadHvcRawPixels()
        {
            if (ImageType != ImageFrameType.hvc1)
                return null;

            stream.CurrentImageId = id;
            stream.SetBytePosition(locationBox.base_offset + locationBox.extents[0].offset);
            NalUnit.ParseUnit(stream);
            rawPixels = stream.Context.Pictures[id].pixels;
            cashed = true;
            stream.DeleteImageContext(id);
            return rawPixels;
        }

        private byte[,,] GetByteArrayForGrid(byte[] databox)
        {
            int gridFieldLength = ((databox[1] & 1) + 1) * 2;

            byte rows = databox[2];
            byte columns = databox[3];

            var localWidth = (uint)GetByteFromIdatField(databox, 4, gridFieldLength);
            var localHeight = (uint)GetByteFromIdatField(databox, 4 + gridFieldLength, gridFieldLength);

            int index = 0;
            var derived = parent.Header.GetDerivedList(id);

            HeicImageFrame frame;
            byte[,,] framePixels;
            byte[,,] output = new byte[localWidth, localHeight, 4];

            for (int i = 0; i <= rows; i++)
            {
                for (int j = 0; j <= columns; j++)
                {
                    index = i * (columns + 1) + j;
                    frame = parent.Frames[derived[index]];
                    framePixels = frame.GetMultidimArray(new Rectangle(0, 0, (int)frame.Width, (int)frame.Height));

                    for (int k = 0; k < frame.Height; k++)
                    {
                        for (int l = 0; l < frame.Width; l++)
                        {
                            if (j * frame.Width + l < localWidth && i * frame.Height + k < localHeight)
                            {
                                output[j * frame.Width + l, i * frame.Height + k, 0] = framePixels[l, k, 0];
                                output[j * frame.Width + l, i * frame.Height + k, 1] = framePixels[l, k, 1];
                                output[j * frame.Width + l, i * frame.Height + k, 2] = framePixels[l, k, 2];
                                output[j * frame.Width + l, i * frame.Height + k, 3] = framePixels[l, k, 3];
                            }
                        }
                    }
                }
            }
            return output;
        }

        private byte[,,] GetByteArrayForOverlay(byte[] databox)
        {
            var derived = parent.Header.GetDerivedList(id);

            int iovlFieldLength = ((databox[1] & 1) + 1) * 2;
            int index = 2;

            ushort[] canvas_fill_value = new ushort[4];
            for (int j = 0; j < 4; j++)
            {
                canvas_fill_value[j] = (ushort)GetByteFromIdatField(databox, index, 2);
                index += 2;
            }

            uint outputWidth = (uint)GetByteFromIdatField(databox, index, iovlFieldLength);
            uint outputHeight = (uint)GetByteFromIdatField(databox, index + iovlFieldLength, iovlFieldLength);
            index += 2 * iovlFieldLength;

            int[] horizontal_offset = new int[derived.Length];
            int[] vertical_offset = new int[derived.Length];

            for (int i = 0; i < derived.Length; i++)
            {
                horizontal_offset[i] = GetByteFromIdatField(databox, index, iovlFieldLength);
                vertical_offset[i] = GetByteFromIdatField(databox, index + iovlFieldLength, iovlFieldLength);
                index += 2 * iovlFieldLength;
            }

            HeicImageFrame frame;
            byte[,,] framePixels;
            byte[,,] output = new byte[outputWidth, outputHeight, 4];

            for (uint row = 0; row < Height; row++)
            {
                for (uint col = 0; col < Width; col++)
                {
                    output[col, row, 3] = 255;
                }
            }

            for (int i = 0; i < derived.Length; i++)
            {
                frame = parent.Frames[derived[i]];
                framePixels = frame.GetMultidimArray(new Rectangle(0, 0, (int)frame.Width, (int)frame.Height));

                for (int k = 0; k < frame.Height; k++)
                {
                    for (int l = 0; l < frame.Width; l++)
                    {
                        if (horizontal_offset[i] + l < outputWidth && vertical_offset[i] + k < outputHeight)
                        {
                            output[horizontal_offset[i] + l, vertical_offset[i] + k, 0] = framePixels[l, k, 0];
                            output[horizontal_offset[i] + l, vertical_offset[i] + k, 1] = framePixels[l, k, 1];
                            output[horizontal_offset[i] + l, vertical_offset[i] + k, 2] = framePixels[l, k, 2];
                            output[horizontal_offset[i] + l, vertical_offset[i] + k, 3] = framePixels[l, k, 3];
                        }
                    }
                }
            }
            return output;
        }

        private int GetByteFromIdatField(byte[] arr, int offset, int length)
        {
            int value = arr[offset];

            for (int i = 1; i < length; i++)
            {
                value = (value << 8) | arr[offset + i];
            }

            return value;
        }

        private void LoadProperties(BitStreamWithNalSupport stream, List<Box> properties)
        {
            foreach (var item in properties)
            {
                switch ((uint)item.type)
                {
                    case 0x68766343: // hvcC
                        HEVCConfigurationBox config = item as HEVCConfigurationBox;
                        stream.CreateNewImageContext(id);
                        stream.SetBytePosition((long)config.offset);
                        hvcConfig = new HEVCDecoderConfigurationRecord(stream);
                        config.record = hvcConfig; // gui
                        break;
                    case 0x69737065: // ispe
                        var ispe = item as ImageSpatialExtentsProperty;
                        ispeWidth = ispe.image_width;
                        ispeHeight = ispe.image_height;
                        break;
                    case 0x70617370: // pasp
                        var pasp = item as PixelAspectRatioBox;
                        break;
                    case 0x636f6c72: // colr
                        var colr = item as ColourInformationBox;
                        break;
                    case 0x70697869: // pixi
                        var pixi = item as PixelInformationProperty;
                        NumberOfChannels = pixi.num_channels;
                        BitsPerChannel = pixi.bits_per_channel;
                        break;
                    case 0x726c6f63: // rloc
                        var rloc = item as RelativeLocationProperty;
                        break;
                    case 0x61757843: // auxC
                        var auxC = item as AuxiliaryTypeProperty;

                        HeicFrameReferenceType type = HeicFrameReferenceType.Undefined;

                        switch (auxC.aux_type)
                        {
                            case "urn:mpeg:hevc:2015:auxid:1\0":
                                type = HeicFrameReferenceType.Alpha;
                                break;
                            case "urn:mpeg:hevc:2015:auxid:2\0":
                                type = HeicFrameReferenceType.DepthMap;
                                break;
                            case "urn:com:apple:photo:2020:aux:hdrgainmap\0":
                                type = HeicFrameReferenceType.Hdr;
                                break;
                        }

                        var derived = parent.Header.GetDerivedList(id);
                        foreach (var derivedId in derived)
                        {
                            parent.Frames[derivedId].AddLayerReference(id, type);
                        }
                        break;
                    case 0x636c6170: // clap
                        var clap = item as CleanApertureBox;
                        break;
                    case 0x69726f74: // irot
                        var irot = item as ImageRotation;
                        imageRotationAngle = irot.angle;
                        break;
                    case 0x6c73656c: // lsel
                        var lsel = item as LayerSelectorProperty;
                        break;
                    case 0x696d6972: // imir
                        var imir = item as ImageMirror;
                        imageMirrorAxis = (byte)(imir.axis + 1);
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion
    }
}