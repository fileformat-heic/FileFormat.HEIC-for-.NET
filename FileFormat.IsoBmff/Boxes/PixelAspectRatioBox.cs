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
    /// Contains the pixel aspect ratio information.
    /// </summary>
    public class PixelAspectRatioBox : Box
    {
        /// <summary>
        /// Define the relative height of a pixel.
        /// </summary>
        public uint hSpacing;

        /// <summary>
        /// Define the relative width of a pixel.
        /// </summary>
        public uint vSpacing;

        /// <summary>
        /// Text summary of the box.
        /// </summary>
        public new string ToString => $"{type} hSpacing: {hSpacing} vSpacing: {vSpacing}";

        /// <summary>
        /// Create the box object from the bitstream and box size.
        /// </summary>
        /// <param name="stream">File stream.</param>
        /// <param name="size">Box size in bytes.</param>
        public PixelAspectRatioBox(BitStreamReader stream, ulong size) : base(BoxType.pasp, size)
        {
            hSpacing = (uint)stream.Read(32);
            vSpacing = (uint)stream.Read(32);
        }
    }
}
