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
    /// This box contains the identifier of the primary item.
    /// </summary>
    public class PrimaryItemBox : FullBox
    {
        /// <summary>
        /// The identifier of the primary item.
        /// </summary>
        public uint item_ID;

        /// <summary>
        /// Text summary of the box.
        /// </summary>
        public new string ToString => $"{type} \nitem_ID: {item_ID}";

        /// <summary>
        /// Create the box object from the bitstream and box size.
        /// </summary>
        /// <param name="stream">File stream.</param>
        /// <param name="size">Box size in bytes.</param>
        public PrimaryItemBox(BitStreamReader stream, ulong size) : base(stream, BoxType.pitm, size)
        {
            item_ID = (uint)stream.Read(version == 0 ? 16 : 32);
        }
    }
}
