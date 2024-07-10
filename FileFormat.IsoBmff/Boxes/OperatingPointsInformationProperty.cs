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

namespace FileFormat.IsoBmff
{
    /// <summary>
    /// The 'oinf' property informs about the different operating points provided by a bitstream and their
    /// constitution.Each operating point is related to an output layer set and a combination of a profile, level
    /// and tier.
    /// </summary>
    public class OperatingPointsInformationProperty : FullBox
    {
        /// <summary>
        /// Text summary of the box.
        /// </summary>
        public new string ToString => $"{type} OMMITED";

        /// <summary>
        /// Create the box object from the bitstream and box size.
        /// </summary>
        /// <param name="stream">File stream.</param>
        /// <param name="size">Box size in bytes.</param>
        public OperatingPointsInformationProperty(BitStreamReader stream, ulong size) : base(stream, BoxType.oinf, size)
        {
            throw new NotImplementedException();
        }
    }
}
