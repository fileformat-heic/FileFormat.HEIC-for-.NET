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
using System;
using System.Collections.Generic;

namespace Openize.IsoBmff
{
    /// <summary>
    /// Contains text description of the image.
    /// </summary>
    public class UserDescriptionBox : FullBox
    {
        /// <summary>
        /// List of text descriptions of the image.
        /// </summary>
        public List<string> description;

        /// <summary>
        /// Text summary of the box.
        /// </summary>
        public new string ToString => $"{type} description:\n{String.Join("\n", description.ToArray())}";

        /// <summary>
        /// Create the box object from the bitstream, box size and start position.
        /// </summary>
        /// <param name="stream">File stream.</param>
        /// <param name="size">Box size in bytes.</param>
        /// <param name="startPos">Start position in bits.</param>
        public UserDescriptionBox(BitStreamReader stream, ulong size, ulong startPos) : base(stream, BoxType.udes, size)
        {
            description = new List<string>();

            while (stream.GetBitPosition() < startPos + size * 8)
            {
                description.Add(stream.ReadString());
            }
        }
    }
}
