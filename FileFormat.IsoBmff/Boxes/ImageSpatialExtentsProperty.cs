/*
 * FileFormat.IsoBmff
 * Copyright (c) 2024 Openize Pty Ltd. 
 *
 * This file is part of FileFormat.IsoBmff.
 *
 * FileFormat.IsoBmff is available under MIT license, which is
 * available along with FileFormat.IsoBmff sources.
 */

using FileFormat.IsoBmff.IO;

namespace FileFormat.IsoBmff
{
    /// <summary>
    /// The ImageSpatialExtentsProperty documents the width and height of the associated image item.
    /// Every image item shall be associated with one property of this type, prior to the association of all
    /// transformative properties.
    /// </summary>
    public class ImageSpatialExtentsProperty : FullBox
    {
        /// <summary>
        /// The width of the reconstructed image in pixels.
        /// </summary>
        public uint image_width;

        /// <summary>
        /// The height of the reconstructed image in pixels.
        /// </summary>
        public uint image_height;

        /// <summary>
        /// Text summary of the box.
        /// </summary>
        public new string ToString => $"{type} width: {image_width} height: {image_height}";

        /// <summary>
        /// Create the box object from the bitstream and box size.
        /// </summary>
        /// <param name="stream">File stream.</param>
        /// <param name="size">Box size in bytes.</param>
        public ImageSpatialExtentsProperty(BitStreamReader stream, ulong size) : base(stream, BoxType.ispe, size)
        {
            image_width = (uint)stream.Read(32);
            image_height = (uint)stream.Read(32);
        }
    }
}
