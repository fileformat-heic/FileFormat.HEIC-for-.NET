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
    /// This box identifies the specifications to which this file complies. 
    /// </summary>
    public class FileTypeBox : Box
    {
        /// <summary>
        /// A brand identifier.
        /// <para>Each brand is a printable four‐character code, registered with ISO, that identifies a precise specification.</para>
        /// </summary>
        public uint major_brand;

        /// <summary>
        /// An informative integer for the minor version of the major brand.
        /// <para>Each brand is a printable four‐character code, registered with ISO, that identifies a precise specification.</para>
        /// </summary>
        public uint minor_version;

        /// <summary>
        /// A list of compatible brands.
        /// <para>Each brand is a printable four‐character code, registered with ISO, that identifies a precise specification.</para>
        /// </summary>
        public uint[] compatible_brands;

        /// <summary>
        /// Text summary of the box.
        /// </summary>
        public new string ToString
        {
            get
            {
                var str = "";
                foreach (var b in compatible_brands)
                    str += UintToString(b) + " ";

                return $"{type}\nmajor: {UintToString(major_brand)} \nminor: {UintToString(minor_version)} \ncompatible: {str}";
            }
        }

        /// <summary>
        /// Create the box object from the bitstream and box size.
        /// </summary>
        /// <param name="stream">File stream.</param>
        /// <param name="size">Box size in bytes.</param>
        public FileTypeBox(BitStreamReader stream, ulong size) : base(BoxType.ftyp, size)
        {
            major_brand = (uint)stream.Read(32);
            minor_version = (uint)stream.Read(32);
            
            compatible_brands = new uint[size/4 - 4];
            for (ulong i = 4; i < size/4; i++)
                compatible_brands[i - 4] = (uint)stream.Read(32);
        }

        /// <summary>
        /// Checks if specified brand is supported.
        /// </summary>
        /// <param name="brand">The specified brand.</param>
        /// <returns>True if brand is supported, false otherwise.</returns>
        public bool IsBrandSupported(uint brand)
        {
            if (major_brand == brand)
                return true;

            if (minor_version == brand) 
                return true;

            for (int i = 0; i < compatible_brands.Length; i++)
                if (compatible_brands[i] == brand)
                    return true;

            return false;
        }
    }
}
