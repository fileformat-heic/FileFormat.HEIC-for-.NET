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
    /// The clean aperture transformative item property defines a cropping transformation of the input image.
    /// </summary>
    public class CleanApertureBox : FullBox
    {
        /// <summary>
        /// A numerator of the fractional number which defines the exact clean aperture width, in counted pixels, of the image.
        /// </summary>
        public uint cleanApertureWidthN;

        /// <summary>
        /// A denominator of the fractional number which defines the exact clean aperture width, in counted pixels, of the image.
        /// </summary>
        public uint cleanApertureWidthD;

        /// <summary>
        /// A numerator of the fractional number which defines the exact clean aperture height, in counted pixels, of the image.
        /// </summary>
        public uint cleanApertureHeightN;

        /// <summary>
        /// A denominator of the fractional number which defines the exact clean aperture height, in counted pixels, of the image.
        /// </summary>
        public uint cleanApertureHeightD;

        /// <summary>
        /// A numerator of the fractional number which defines the horizontal offset of clean aperture centre minus(width‐1)/2. Typically 0.	
        /// </summary>
        public int horizOffN;

        /// <summary>
        /// A denominator of the fractional number which defines the horizontal offset of clean aperture centre minus(width‐1)/2. Typically 0.	
        /// </summary>
        public uint horizOffD;

        /// <summary>
        /// A numerator of the fractional number which defines the vertical offset of clean aperture centre minus(height‐1)/2. Typically 0.
        /// </summary>
        public int vertOffN;

        /// <summary>
        /// A denominator of the fractional number which defines the vertical offset of clean aperture centre minus(height‐1)/2. Typically 0.
        /// </summary>
        public uint vertOffD;

        /// <summary>
        /// Text summary of the box.
        /// </summary>
        public new string ToString => $"{type} size: {size}";

        /// <summary>
        /// Create the box object from the bitstream and box size.
        /// </summary>
        /// <param name="stream">File stream.</param>
        /// <param name="size">Box size in bytes.</param>
        public CleanApertureBox(BitStreamReader stream, ulong size) : base(stream, BoxType.clap, size)
        {
            cleanApertureWidthN = (uint)stream.Read(32);
            cleanApertureWidthD = (uint)stream.Read(32);
            cleanApertureHeightN = (uint)stream.Read(32);
            cleanApertureHeightD = (uint)stream.Read(32);
            horizOffN = stream.Read(32);
            horizOffD = (uint)stream.Read(32);
            vertOffN = stream.Read(32);
            vertOffD = (uint)stream.Read(32);
        }
    }
}
