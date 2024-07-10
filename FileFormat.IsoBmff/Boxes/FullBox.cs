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
using System.Collections;

namespace FileFormat.IsoBmff
{
    /// <summary>
    /// Structure for storing data in IsoBmff files with specified box version and flags.
    /// </summary>
    public class FullBox : Box
    {
        /// <summary>
        /// An integer that specifies the version of this format of the box.
        /// </summary>
        protected byte version;

        /// <summary>
        /// A map of flags.
        /// </summary>
        protected BitArray flags = new BitArray(24);

        /// <summary>
        /// Create the box object from the bitstream.
        /// </summary>
        /// <param name="stream">File stream.</param>
        public FullBox(BitStreamReader stream) : 
            base(stream)
        {
            version = (byte)stream.Read(8);

            for (int i = 0; i < 24; i++)
                flags[i] = stream.ReadFlag();
        }

        /// <summary>
        /// Create the box object from the bitstream and box type.
        /// </summary>
        /// <param name="stream">File stream.</param>
        /// <param name="boxtype">Box type integer.</param>
        public FullBox(BitStreamReader stream, BoxType boxtype) : 
            base(stream, boxtype)
        {
            version = (byte)stream.Read(8);

            for (int i = 0; i < 24; i++)
                flags[i] = stream.ReadFlag();
        }

        /// <summary>
        /// Create the box object from the bitstream, box type and size.
        /// </summary>
        /// <param name="stream">File stream.</param>
        /// <param name="boxtype">Box type integer.</param>
        /// <param name="size">Box size in bytes.</param>
        public FullBox(BitStreamReader stream, BoxType boxtype, ulong size) :
            base(boxtype, size)
        {
            version = (byte)stream.Read(8);

            for (int i = 0; i < 24; i++)
                flags[i] = stream.ReadFlag();
        }

        /// <summary>
        /// Create the box object from the box type, size, version and flags.
        /// This constructor doesn't read data from the stream.
        /// </summary>
        /// <param name="boxtype">Box type integer.</param>
        /// <param name="size">Box size in bytes.</param>
        /// <param name="version">The version of this format of the box.</param>
        /// <param name="flags">The map of flags.</param>
        public FullBox(BoxType boxtype, ulong size, byte version, int flags) :
            base(boxtype, size)
        {
            this.version = version;
            this.flags = new BitArray(new int[] { flags });
        }
    }
}
