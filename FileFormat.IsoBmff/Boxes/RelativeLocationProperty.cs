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
    /// The RelativeLocationProperty descriptive item property is used to describe the horizontal and
    /// vertical position of the reconstructed image of the associated image item relative to the reconstructed
    /// image of the related image item identified through the 'tbas' item reference.
    /// </summary>
    public class RelativeLocationProperty : FullBox
    {
        /// <summary>
        /// Specifies the horizontal offset in pixels of the left-most pixel column of the reconstructed image 
        /// of the associated image item in the reconstructed image of the related image item. The left-most 
        /// pixel column of the reconstructed image of the related image item has a horizontal offset equal to 0.
        /// </summary>
        public uint horizontal_offset;

        /// <summary>
        /// Specifies the vertical offset in pixels of the top-most pixel row of the reconstructed image 
        /// of the associated image item in the reconstructed image of the related image item. The top-most
        /// pixel row of the reconstructed image of the related image item has a vertical offset equal to 0.
        /// </summary>
        public uint vertical_offset;

        /// <summary>
        /// Text summary of the box.
        /// </summary>
        public new string ToString => $"{type} horiz offset: {horizontal_offset} vert offset: {vertical_offset}";

        /// <summary>
        /// Create the box object from the bitstream and box size.
        /// </summary>
        /// <param name="stream">File stream.</param>
        /// <param name="size">Box size in bytes.</param>
        public RelativeLocationProperty(BitStreamReader stream, ulong size) : base(stream, BoxType.rloc, size)
        {
            horizontal_offset = (uint)stream.Read(32);
            vertical_offset = (uint)stream.Read(32);
        }
    }
}
