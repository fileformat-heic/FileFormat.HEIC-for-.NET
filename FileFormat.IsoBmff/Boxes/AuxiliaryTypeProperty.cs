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

namespace FileFormat.IsoBmff
{
    /// <summary>
    /// AuxiliaryTypeProperty box includes a URN identifying the type of the auxiliary image.
    /// AuxiliaryTypeProperty may additionally include other fields, as required by the URN.
    /// </summary>
    public class AuxiliaryTypeProperty : FullBox
    {
        /// <summary>
        /// A null-terminated UTF-8 character string of the Uniform Resource Name (URN) used to identify the type of the associated auxiliary image item.
        /// </summary>
        public string aux_type;

        /// <summary>
        /// Zero or more bytes until the end of the box. The semantics of these bytes depend on the value of aux_type.
        /// </summary>
        public byte[] aux_subtype;

        /// <summary>
        /// Text summary of the box.
        /// </summary>
        public new string ToString => $"{type} type: {aux_type}";

        /// <summary>
        /// Create the box object from the bitstream and box size.
        /// </summary>
        /// <param name="stream">File stream.</param>
        /// <param name="size">Box size in bytes.</param>
        public AuxiliaryTypeProperty(BitStreamReader stream, ulong size) : base(stream, BoxType.auxC, size)
        {
            aux_type = stream.ReadString();

            var count = size - (ulong)aux_type.Length - 12;
            aux_subtype = new byte[count];
            for (ulong i = 0; i < count; i++)
                aux_subtype[i] = (byte)stream.Read(8);
        }
    }
}
