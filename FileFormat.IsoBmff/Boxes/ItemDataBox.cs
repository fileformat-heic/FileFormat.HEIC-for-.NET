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
using System;
using System.Text;

namespace FileFormat.IsoBmff
{
    /// <summary>
    /// This box contains the data of metadata items that use the construction method indicating that an item’s 
    /// data extents are stored within this box.
    /// </summary>
    public class ItemDataBox : Box
    {
        /// <summary>
        /// The contained meta data in raw format.
        /// </summary>
        public byte[] data;

        /// <summary>
        /// Text summary of the box.
        /// </summary>
        public new string ToString
        {
            get
            {
                StringBuilder hexSB = new StringBuilder();
                foreach (byte b in data)
                    hexSB.Append(Convert.ToString(b, 16).PadLeft(2, '0') + " ");
                return $"{type} {hexSB}";
            }
        }

        /// <summary>
        /// Create the box object from the bitstream, box size and start position.
        /// </summary>
        /// <param name="stream">File stream.</param>
        /// <param name="size">Box size in bytes.</param>
        /// <param name="startPos">Start position in bits.</param>
        public ItemDataBox(BitStreamReader stream, ulong size, ulong startPos) : base(BoxType.idat, size)
        {
            var count = size - (stream.GetBitPosition() - startPos) / 8;
            data = new byte[count];
            for (ulong i = 0; i < count; i++)
                data[i] = (byte)stream.Read(8);
        }
    }
}
