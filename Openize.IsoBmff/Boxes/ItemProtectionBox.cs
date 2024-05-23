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

namespace Openize.IsoBmff
{
    /// <summary>
    /// The item protection box provides an array of item protection information, for use by the Item Information Box.
    /// </summary>
    public class ItemProtectionBox : FullBox
    {
        /// <summary>
        /// Count of protection informarion schemas.
        /// </summary>
        ushort protection_count;

        /// <summary>
        /// Array of protecyion informarion schemas. 
        /// </summary>
        ProtectionSchemeInfoBox[] protection_information;

        /// <summary>
        /// Text summary of the box.
        /// </summary>
        public new string ToString => $"{type}";

        /// <summary>
        /// Create the box object from the bitstream and box size.
        /// </summary>
        /// <param name="stream">File stream.</param>
        /// <param name="size">Box size in bytes.</param>
        public ItemProtectionBox(BitStreamReader stream, ulong size) : base (stream, BoxType.ipro, size)
        {
            protection_count = (ushort)stream.Read(16);
            protection_information = new ProtectionSchemeInfoBox[protection_count];

            for (int i = 1; i <= protection_count; i++)
                protection_information[i-1] = new ProtectionSchemeInfoBox(stream);
        }
    }

    /// <summary>
    /// The Protection Scheme Information Box contains all the information required both to understand the 
    /// encryption transform applied and its parameters, and also to find other information such as the kind
    /// and location of the key management system.
    /// </summary>
    public class ProtectionSchemeInfoBox : Box
    {
        public ProtectionSchemeInfoBox(BitStreamReader stream) : base (stream, BoxType.sinf)
        {
            throw new NotImplementedException();

            //OriginalFormatBox(fmt) original_format;
            //SchemeTypeBox scheme_type_box; // optional
            //SchemeInformationBox info; // optional
        }
    }
}
