/*
 * Openize.IsoBmff
 * Copyright (c) 2024 Openize Pty Ltd. 
 *
 * This file is part of Openize.IsoBmff.
 *
 * Openize.IsoBmff is available under MIT license, which is
 * available along with Openize.IsoBmff sources.
 */

using Openize.IsoBmff.IO;

namespace Openize.IsoBmff
{
    /// <summary>
    /// The image mirroring item property mirrors the image about either a vertical or horizontal axis.
    /// </summary>
    public class ImageMirror : Box
    {
        /// <summary>
        /// Specifies a vertical (axis = 0) or horizontal (axis = 1) axis for the mirroring operation.
        /// </summary>
        public byte axis;

        /// <summary>
        /// Text summary of the box.
        /// </summary>
        public new string ToString => $"{type}: " + (axis == 0 ? "verticaly" : "horizontal") + " mirrored";

        /// <summary>
        /// Create the box object from the bitstream and box size.
        /// </summary>
        /// <param name="stream">File stream.</param>
        /// <param name="size">Box size in bytes.</param>
        public ImageMirror(BitStreamReader stream, ulong size) : base(BoxType.imir, size)
        {
            stream.SkipBits(7);
            axis = (byte)stream.Read(1);
        }
    }
}
