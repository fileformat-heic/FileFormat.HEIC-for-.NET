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
    /// Contains colour information about the image.
    /// If colour information is supplied in both this box, and also in the video bitstream,
    /// this box takes precedence, and over‐rides the information in the bitstream.
    /// </summary>
    public class ColourInformationBox : Box
    {
        /// <summary>
        /// An indication of the type of colour information supplied.
        /// </summary>
        public uint colour_type;

        /// <summary>
        /// Indicates the chromaticity coordinates of the source primaries.
        /// </summary>
        public ushort colour_primaries;

        /// <summary>
        /// Indicates the reference opto-electronic transfer characteristic.
        /// </summary>
        public ushort transfer_characteristics;

        /// <summary>
        /// Describes the matrix coefficients used in deriving luma and chroma signals from the green, blue, and red.
        /// </summary>
        public ushort matrix_coefficients;

        /// <summary>
        /// Indicates the black level and range of the luma and chroma signals as derived from E′Y, E′PB, and E′PR or E′R, E′G, and E′B real-valued component signals.
        /// </summary>
        public bool full_range_flag;

        /// <summary>
        /// Text summary of the box.
        /// </summary>
        public new string ToString => $"{type} Color type: {UintToString(colour_type)}";

        /// <summary>
        /// Create the box object from the bitstream and box size.
        /// </summary>
        /// <param name="stream">File stream.</param>
        /// <param name="size">Box size in bytes.</param>
        public ColourInformationBox(BitStreamReader stream, ulong size) : base(BoxType.colr, size)
        {
            colour_type = (uint)stream.Read(32);

            if (colour_type == 0x6e636c78) /* nclx; on-screen colours */
            {
                colour_primaries = (ushort)stream.Read(16);
                transfer_characteristics = (ushort)stream.Read(16);
                matrix_coefficients = (ushort)stream.Read(16);
                full_range_flag = stream.ReadFlag();
                stream.SkipBits(7);
            }
            else if (colour_type == 0x72494343) // rICC
            {
                //restricted ICC profile
                //ICC_profile; 
            }
            else if (colour_type == 0x70726f66) // prof
            {
                //unrestricted ICC profile
                //ICC_profile; 
            }
        }
    }
}
